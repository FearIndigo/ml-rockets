using System;
using FearIndigo.Managers;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Ship
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipController : Agent
    {
        [Header("Physics")]
        public float linearThrust;
        public float angularThrust;

        [Header("Graphics")]
        public TrailRenderer trail;

        [Header("AI")]
        public float stepPunishment = -0.001f;
        public float pFactor = 2f;
        public float maxVelocityObservation = 80f;
        public float maxAngularVelocityObservation = 350f;
        public float maxDistanceObservation = 200f;

        private GameManager _gameManager;
        private Rigidbody2D _rb;

        protected override void Awake()
        {
            base.Awake();
            
            _gameManager = GetComponentInParent<GameManager>();
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Set up an Agent instance at the beginning of an episode.
        /// </summary>
        public override void OnEpisodeBegin()
        {
            _gameManager.Reset();
        }

        /// <summary>
        /// <para>
        /// Collect the vector observations of the agent for the step. The agent observation describes the current
        /// environment from the perspective of the agent.
        /// </para>
        /// </summary>
        /// <param name="sensor">The vector observations for the agent.</param>
        public override void CollectObservations(VectorSensor sensor)
        {
            var shipTransform = transform;
            var shipPosition = (Vector2)shipTransform.position;
            
            // Ship velocity (2 float)
            sensor.AddObservation(Normalise(shipTransform.InverseTransformDirection(_rb.velocity), maxVelocityObservation));
            // Ship angular velocity (1 float)
            sensor.AddObservation(Normalise(_rb.angularVelocity, maxAngularVelocityObservation));
            // Ship orientation (1 float)
            sensor.AddObservation(NormaliseRotation(shipTransform.eulerAngles.z));
            // Relative direction to active checkpoint (2 float)
            sensor.AddObservation(Normalise(shipTransform.InverseTransformDirection(_gameManager.ActiveCheckpointDirection(shipPosition)), maxDistanceObservation));
            // Relative direction to next active checkpoint (2 float)
            sensor.AddObservation(Normalise(shipTransform.InverseTransformDirection(_gameManager.NextActiveCheckpointDirection(shipPosition)), maxDistanceObservation));
            // Relative direction to previous active checkpoint (2 float)
            sensor.AddObservation(Normalise(shipTransform.InverseTransformDirection(_gameManager.PreviousActiveCheckpointDirection(shipPosition)), maxDistanceObservation));
            
            // 10 total
        }

        private float NormaliseRotation(float input)
        {
            return input / 180f - 1f;
        }

        private float Normalise(float input, float max)
        {
            return Mathf.Sign(input) * (1f - Mathf.Pow(1f - Mathf.Clamp01(Mathf.Abs(input) / max), pFactor));
        }

        private Vector2 Normalise(Vector2 input, float max)
        {
            return new Vector2(Normalise(input.x, max), Normalise(input.y, max));
        }

        /// <summary>
        /// Choose an action for this agent using a custom heuristic.
        /// </summary>
        /// <param name="actionsOut">
        /// The <see cref="ActionBuffers"/> which contain the continuous and discrete action buffers to write to.
        /// </param>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            
            // Jump
            // 0 = nothing
            // 1 = up
            discreteActionsOut[0] = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) ? 1 : 0;

            // Rotate
            // 0 = left
            // 1 = nothing
            // 2 = right
            discreteActionsOut[1] = 1;
            if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[1] -= 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[1] += 1;
            }
        }
        
        /// <summary>
        /// Specify agent behavior at every step, based on the provided action.
        /// </summary>
        /// <param name="actions">
        /// Struct containing the buffers of actions to be executed at this step.
        /// </param>
        public override void OnActionReceived(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;

            if (discreteActions[0] == 1)
            {
                _rb.velocity += (Vector2)(transform.rotation * Vector3.up * linearThrust);
            }

            var torque = discreteActions[1] switch
            {
                0 => 1f, // Rotate left
                1 => 0, // No rotation
                2 => -1f, // Rotate right
                _ => 0 // default = No rotation
            };
            _rb.angularVelocity += torque * angularThrust;
            
            AddReward(stepPunishment);
        }

        /// <summary>
        /// <para>
        /// Teleport ship to position.
        /// </para>
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(float2 position)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0);
            transform.rotation = Quaternion.identity;
            _rb.MovePosition(transform.position);
            _rb.SetRotation(0);
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0;
            trail.Clear();
        }
    }
}
