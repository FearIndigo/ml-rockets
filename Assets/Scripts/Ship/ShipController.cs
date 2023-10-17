using FearIndigo.Checkpoints;
using FearIndigo.Managers;
using FearIndigo.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
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
        public float drag;
        public float angularDrag;

        [Header("Graphics")]
        public TrailRenderer trail;
        public ParticleSystem thrustParticles;

        [Header("AI")]
        public float stepPunishment = -0.001f;
        public float pFactor = 2f;
        public float maxVelocityObservation = 80f;
        public float maxAngularVelocityObservation = 350f;
        public float maxDistanceObservation = 200f;
        public CustomRayPerceptionSensorComponent2D raySensor;

        private GameManager _gameManager;
        private BehaviorParameters _behaviorParameters;

        [HideInInspector] public Vector2 velocity;
        [HideInInspector] public float angularVelocity;
        [HideInInspector] public Rigidbody2D rb;

        protected override void Awake()
        {
            base.Awake();
            
            _gameManager = GetComponentInParent<GameManager>();
            _behaviorParameters = GetComponent<BehaviorParameters>();
            
            rb = GetComponent<Rigidbody2D>();

            raySensor.DetectableObject = GetRaySensorDetectableObject;
        }

        private GameObject GetRaySensorDetectableObject()
        {
            return _gameManager.checkpointManager.GetCheckpoint(this, 0).gameObject;
        }
        
        public void SetBehaviourType(BehaviorType behaviorType)
        {
            _behaviorParameters.BehaviorType = behaviorType;
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
            // Ship velocity (2 float)
            sensor.AddObservation(Normalize(velocity, maxVelocityObservation));
            // Ship angular velocity (1 float)
            sensor.AddObservation(Normalize(angularVelocity, maxAngularVelocityObservation));
            // Ship orientation (1 float)
            sensor.AddObservation(NormalizeRotation(Quaternion.Euler(0,0,rb.rotation).normalized.eulerAngles.z));
            // Active checkpoint -1 direction (2 float)
            sensor.AddObservation(Normalize(_gameManager.checkpointManager.GetCheckpointDirection(this, -1), maxDistanceObservation));
            // Active checkpoint +0 direction (2 float)
            sensor.AddObservation(Normalize(_gameManager.checkpointManager.GetCheckpointDirection(this, 0), maxDistanceObservation));
            // Active checkpoint +1 direction (2 float) [zero value if active checkpoint is finish line]
            sensor.AddObservation(_gameManager.checkpointManager.GetCheckpoint(this, 0) is FinishLine
                ? Vector2.zero
                : Normalize(_gameManager.checkpointManager.GetCheckpointDirection(this, +1), maxDistanceObservation));

            // 10 total
        }

        private float NormalizeRotation(float input)
        {
            return input / 180f - 1f;
        }

        private float Normalize(float input, float max)
        {
            return Mathf.Sign(input) * (1f - Mathf.Pow(1f - Mathf.Clamp01(Mathf.Abs(input) / max), pFactor));
        }

        private Vector2 Normalize(Vector2 input, float max)
        {
            return new Vector2(Normalize(input.x, max), Normalize(input.y, max));
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
                velocity += (Vector2)(Quaternion.Euler(0,0,rb.rotation) * Vector3.up * (linearThrust * Time.deltaTime));
                thrustParticles.Play();
            }
            else
            {
                thrustParticles.Stop();
            }

            var torque = discreteActions[1] switch
            {
                0 => 1f, // Rotate left
                1 => 0, // No rotation
                2 => -1f, // Rotate right
                _ => 0 // default = No rotation
            };
            angularVelocity += torque * angularThrust * Time.deltaTime;

            velocity += Physics2D.gravity * Time.fixedDeltaTime;
            velocity *= Mathf.Clamp01(1f - drag * Time.fixedDeltaTime);
            angularVelocity *= Mathf.Clamp01(1f - angularDrag * Time.fixedDeltaTime);
            
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation + angularVelocity * Time.fixedDeltaTime);
            
            AddReward(stepPunishment);
        }

        /// <summary>
        /// <para>
        /// Teleport ship to position.
        /// </para>
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector2 position)
        {
            var t = transform;
            t.localPosition = position;
            t.rotation = Quaternion.identity;
            rb.position = t.position;
            rb.rotation = 0f;
            velocity = Vector2.zero;
            angularVelocity = 0f;
            trail.Clear();
        }
    }
}
