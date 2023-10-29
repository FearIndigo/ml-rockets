using FearIndigo.Audio;
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
        [HideInInspector] public int index;
        
        [Header("Physics")]
        public float linearThrust;
        public float angularThrust;
        public float drag;
        public float angularDrag;

        [Header("Graphics")]
        public SpriteRenderer spriteRenderer;
        public TrailRenderer trail;
        public ParticleSystem thrustParticles;

        [Header("Colors")]
        public Color aiColor;
        public Color humanColor;
        
        [Header("Audio")]
        public AudioEvent crashedAudioEvent;
        public AudioEvent thrustAudio;
        
        [Header("AI")]
        public float stepPunishment = -0.0005f;
        public float crashedPunishment = -1f;
        public float allCheckpointsReward = 1f;
        public float finishLineReward = 1f;
        public int maxStepsBetweenCheckpoints;
        public float pFactor = 2f;
        public float maxVelocityObservation = 80f;
        public float maxAngularVelocityObservation = 350f;
        public float maxDistanceObservation = 200f;
        public CustomRayPerceptionSensorComponent2D raySensor;

        private GameManager _gameManager;
        private BehaviorParameters _behaviorParameters;
        private int _stepsSinceLastCheckpoint;
        private AudioSource _thrustSource;

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

        public void Init(int shipIndex, bool useHeuristics)
        {
            index = shipIndex;
            enabled = true;
            
            _behaviorParameters.BehaviorType = useHeuristics ? BehaviorType.HeuristicOnly : BehaviorType.Default;

            spriteRenderer.color = useHeuristics switch
            {
                true => humanColor,
                false => aiColor
            };
        }
        
        public override void OnEpisodeBegin()
        {
            _stepsSinceLastCheckpoint = 0;

            stepPunishment = Academy.Instance.IsCommunicatorOn
                ? Academy.Instance.EnvironmentParameters.GetWithDefault("step_punishment", stepPunishment)
                : stepPunishment;
            
            allCheckpointsReward = Academy.Instance.IsCommunicatorOn
                ? Academy.Instance.EnvironmentParameters.GetWithDefault("all_checkpoints_reward", allCheckpointsReward)
                : allCheckpointsReward;
        }

        private GameObject GetRaySensorDetectableObject()
        {
            return _gameManager.checkpointManager.GetCheckpoint(this, 0).gameObject;
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
            // Active checkpoint observation (3 float)
            ObserveCheckpointAhead(0, sensor);
            // Active checkpoint +1 observation (3 float)
            ObserveCheckpointAhead(+1, sensor);
            // Active checkpoint +2 observation (3 float)
            ObserveCheckpointAhead(+2, sensor);

            // 15 total
        }

        /// <summary>
        /// <para>
        /// Observe if checkpoint is finish line, direction to checkpoint and distance to checkpoint.
        /// </para>
        /// </summary>
        /// <param name="activeCheckpointOffset"></param>
        /// <param name="sensor"></param>
        private void ObserveCheckpointAhead(int activeCheckpointOffset, VectorSensor sensor)
        {
            sensor.AddObservation(_gameManager.checkpointManager.GetCheckpoint(this, activeCheckpointOffset) is FinishLine);
            sensor.AddObservation(Normalize(_gameManager.checkpointManager.GetCheckpointDirection(this, activeCheckpointOffset), maxDistanceObservation));
            
            // 3 Total
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
            var deltaTime = Time.fixedDeltaTime;
            var discreteActions = actions.DiscreteActions;

            var thrust = discreteActions[0] == 1;
            if (thrust)
            {
                velocity += (Vector2)(Quaternion.Euler(0,0,rb.rotation) * Vector3.up * (linearThrust * deltaTime));
            }
            PlayThrust(thrust);

            var torque = discreteActions[1] switch
            {
                0 => 1f, // Rotate left
                1 => 0, // No rotation
                2 => -1f, // Rotate right
                _ => 0 // default = No rotation
            };
            angularVelocity += torque * angularThrust * deltaTime;

            velocity += Physics2D.gravity * deltaTime;
            velocity *= Mathf.Clamp01(1f - drag * deltaTime);
            angularVelocity *= Mathf.Clamp01(1f - angularDrag * deltaTime);
            
            rb.MovePosition(rb.position + velocity * deltaTime);
            rb.MoveRotation(rb.rotation + angularVelocity * deltaTime);

            AddReward(stepPunishment);
            
            _stepsSinceLastCheckpoint++;
            if (Academy.Instance.IsCommunicatorOn && _stepsSinceLastCheckpoint >= maxStepsBetweenCheckpoints)
            {
                SetReward(_gameManager.checkpointManager.GetActiveCheckpointId(this) / (float) _gameManager.checkpointManager.checkpoints.Count - 1f);
                EpisodeInterrupted();
                _gameManager.Reset();
            }
        }

        public void PlayThrust(bool play)
        {
            if (play)
            {
                thrustParticles.Play();
                if(!_thrustSource)
                    _thrustSource = AudioSourcePool.Get();
                if (!_thrustSource.isPlaying)
                    thrustAudio.Play(_thrustSource, transform.position);
                else
                    _thrustSource.transform.position = transform.position;
            }
            else
            {
                thrustParticles.Stop();

                if (_thrustSource)
                {
                    _thrustSource.Stop();
                    AudioSourcePool.Release(_thrustSource);
                    _thrustSource = null;
                }
            }
        }

        /// <summary>
        /// Set reward and reset _stepsSinceLastCheckpoint.
        /// </summary>
        public void CheckpointAcquired(bool isFinishLine = false)
        {
            if (isFinishLine)
            {
                SetReward(finishLineReward);
            }
            else
            {
                SetReward(allCheckpointsReward / (_gameManager.checkpointManager.checkpoints.Count - 1f));
            }
            
            _stepsSinceLastCheckpoint = 0;
        }

        /// <summary>
        /// Set punishment and stop ship.
        /// </summary>
        public void Crashed()
        {
            crashedAudioEvent.Play(transform.position);
            SetReward(crashedPunishment);
            StopShip();
        }

        /// <summary>
        /// Stop the ship moving etc.
        /// </summary>
        public void StopShip()
        {
            EndEpisode();
            PlayThrust(false);

            _gameManager.shipManager.ShipStopped(this);
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
            thrustParticles.Clear();
        }
    }
}
