using System;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// A custom base class to support sensor components for raycast-based sensors.
    /// </summary>
    public abstract class CustomRayPerceptionSensorComponentBase : SensorComponent
    {
        [HideInInspector, SerializeField, FormerlySerializedAs("sensorName")]
        string m_SensorName = "CustomRayPerceptionSensor";

        /// <summary>
        /// The name of the Sensor that this component wraps.
        /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
        /// </summary>
        public string SensorName
        {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }

        /// <summary>
        /// Function to call which returns object to detect.
        /// </summary>
        Func<GameObject> m_DetectableObject { get; set; }
        
        public Func<GameObject> DetectableObject
        {
            get { return m_DetectableObject; }
            set { m_DetectableObject = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField]
        private Rigidbody2D m_AgentRigidbody;
        
        public Rigidbody2D AgentRigidbody
        {
            get { return m_AgentRigidbody; }
            set { m_AgentRigidbody = value; UpdateSensor(); }
        }

        [SerializeField]
        [Tooltip("Max number of raycast hits to store in the buffer.")]
        int m_RaycastHitBufferSize;

        /// <summary>
        /// List of tags in the scene to compare against.
        /// Note that this should not be changed at runtime.
        /// </summary>
        public int RaycastHitBufferSize
        {
            get { return m_RaycastHitBufferSize; }
            set { m_RaycastHitBufferSize = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("raysPerDirection")]
        [Range(0, 50)]
        [Tooltip("Number of rays to the left and right of center.")]
        int m_RaysPerDirection = 3;

        /// <summary>
        /// Number of rays to the left and right of center.
        /// Note that this should not be changed at runtime.
        /// </summary>
        public int RaysPerDirection
        {
            get { return m_RaysPerDirection; }
            // Note: can't change at runtime
            set { m_RaysPerDirection = value; }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("maxRayDegrees")]
        [Range(0, 180)]
        [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
            "Greater than 90 degrees will go backwards.")]
        float m_MaxRayDegrees = 70;

        /// <summary>
        /// Cone size for rays. Using 90 degrees will cast rays to the left and right.
        /// Greater than 90 degrees will go backwards.
        /// </summary>
        public float MaxRayDegrees
        {
            get => m_MaxRayDegrees;
            set { m_MaxRayDegrees = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("sphereCastRadius")]
        [Range(0f, 10f)]
        [Tooltip("Radius of sphere to cast. Set to zero for raycasts.")]
        float m_SphereCastRadius = 0.5f;

        /// <summary>
        /// Radius of sphere to cast. Set to zero for raycasts.
        /// </summary>
        public float SphereCastRadius
        {
            get => m_SphereCastRadius;
            set { m_SphereCastRadius = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("rayLength")]
        [Range(1, 1000)]
        [Tooltip("Length of the rays to cast.")]
        float m_RayLength = 20f;

        /// <summary>
        /// Length of the rays to cast.
        /// </summary>
        public float RayLength
        {
            get => m_RayLength;
            set { m_RayLength = value; UpdateSensor(); }
        }

        // The value of the default layers.
        const int k_PhysicsDefaultLayers = -5;
        [HideInInspector, SerializeField, FormerlySerializedAs("rayLayerMask")]
        [Tooltip("Controls which layers the rays can hit.")]
        LayerMask m_RayLayerMask = k_PhysicsDefaultLayers;

        /// <summary>
        /// Controls which layers the rays can hit.
        /// </summary>
        public LayerMask RayLayerMask
        {
            get => m_RayLayerMask;
            set { m_RayLayerMask = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("observationStacks")]
        [Range(1, 50)]
        [Tooltip("Number of raycast results that will be stacked before being fed to the neural network.")]
        int m_ObservationStacks = 1;

        /// <summary>
        /// Whether to stack previous observations. Using 1 means no previous observations.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int ObservationStacks
        {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }

        [HideInInspector, SerializeField]
        [Tooltip("Enable to use batched raycasts and the jobs system.")]
        public bool m_UseBatchedRaycasts = false;

        /// <summary>
        /// Determines whether to use batched raycasts and the jobs system. Default = false.
        /// </summary>
        public bool UseBatchedRaycasts
        {
            get { return m_UseBatchedRaycasts; }
            set { m_UseBatchedRaycasts = value; }
        }


        /// <summary>
        /// Color to code a ray that hits another object.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Header("Debug Gizmos", order = 999)]
        internal Color rayHitColor = Color.red;

        /// <summary>
        /// Color to code a ray that avoid or misses all other objects.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        internal Color rayMissColor = Color.white;

        [NonSerialized]
        CustomRayPerceptionSensor m_RaySensor;

        /// <summary>
        /// Get the RayPerceptionSensor that was created.
        /// </summary>
        public CustomRayPerceptionSensor RaySensor
        {
            get => m_RaySensor;
        }

        /// <summary>
        /// Returns the <see cref="RayPerceptionCastType"/> for the associated raycast sensor.
        /// </summary>
        /// <returns></returns>
        public abstract CustomRayPerceptionCastType GetCastType();

        /// <summary>
        /// Returns the amount that the ray start is offset up or down by.
        /// </summary>
        /// <returns></returns>
        public virtual float GetStartVerticalOffset()
        {
            return 0f;
        }

        /// <summary>
        /// Returns the amount that the ray end is offset up or down by.
        /// </summary>
        /// <returns></returns>
        public virtual float GetEndVerticalOffset()
        {
            return 0f;
        }

        /// <summary>
        /// Returns an initialized raycast sensor.
        /// </summary>
        /// <returns></returns>
        public override ISensor[] CreateSensors()
        {
            var rayPerceptionInput = GetRayPerceptionInput();

            m_RaySensor = new CustomRayPerceptionSensor(m_SensorName, rayPerceptionInput);

            if (ObservationStacks != 1)
            {
                var stackingSensor = new StackingSensor(m_RaySensor, ObservationStacks);
                return new ISensor[] { stackingSensor };
            }

            return new ISensor[] { m_RaySensor };
        }

        /// <summary>
        /// Returns the specific ray angles given the number of rays per direction and the
        /// cone size for the rays.
        /// </summary>
        /// <param name="raysPerDirection">Number of rays to the left and right of center.</param>
        /// <param name="maxRayDegrees">
        /// Cone size for rays. Using 90 degrees will cast rays to the left and right.
        /// Greater than 90 degrees will go backwards.
        /// Orders the rays from the left-most to the right-most which makes using a convolution
        /// in the model easier.
        /// </param>
        /// <returns></returns>
        internal static float[] GetRayAngles(int raysPerDirection, float maxRayDegrees)
        {
            // Example:
            // { 90 - 3*delta, 90 - 2*delta, ..., 90, 90 + delta, ..., 90 + 3*delta }
            var anglesOut = new float[2 * raysPerDirection + 1];
            var delta = maxRayDegrees / raysPerDirection;

            for (var i = 0; i < 2 * raysPerDirection + 1; i++)
            {
                anglesOut[i] = 90 + (i - raysPerDirection) * delta;
            }

            return anglesOut;
        }

        /// <summary>
        /// Get the RayPerceptionInput that is used by the <see cref="RayPerceptionSensor"/>.
        /// </summary>
        /// <returns></returns>
        public CustomRayPerceptionInput GetRayPerceptionInput()
        {
            var rayPerceptionInput = new CustomRayPerceptionInput();
            rayPerceptionInput.RayLength = RayLength;
            rayPerceptionInput.DetectableObject = DetectableObject;
            rayPerceptionInput.AgentRigidbody = AgentRigidbody;
            rayPerceptionInput.Angles = GetRayAngles(RaysPerDirection, MaxRayDegrees);
            rayPerceptionInput.StartOffset = GetStartVerticalOffset();
            rayPerceptionInput.EndOffset = GetEndVerticalOffset();
            rayPerceptionInput.CastRadius = SphereCastRadius;
            rayPerceptionInput.CastType = GetCastType();
            rayPerceptionInput.LayerMask = RayLayerMask;
            rayPerceptionInput.UseBatchedRaycasts = UseBatchedRaycasts;

            if (rayPerceptionInput.CastType == CustomRayPerceptionCastType.Cast2D)
            {
                rayPerceptionInput.RayHits2D = new RaycastHit2D[RaycastHitBufferSize];
            }
            else
            {
                rayPerceptionInput.RayHits = new RaycastHit[RaycastHitBufferSize];
            }
            
            return rayPerceptionInput;
        }

        public void UpdateSensor()
        {
            if (m_RaySensor != null)
            {
                var rayInput = GetRayPerceptionInput();
                m_RaySensor.SetRayPerceptionInput(rayInput);
            }
        }

        internal int SensorObservationAge()
        {
            if (m_RaySensor != null)
            {
                return Time.frameCount - m_RaySensor.DebugLastFrameCount;
            }

            return 0;
        }

        void OnDrawGizmosSelected()
        {
            if (m_RaySensor?.RayPerceptionOutput?.RayOutputs != null)
            {
                // If we have cached debug info from the sensor, draw that.
                // Draw "old" observations in a lighter color.
                // Since the agent may not step every frame, this helps de-emphasize "stale" hit information.
                var alpha = Mathf.Pow(.5f, SensorObservationAge());

                foreach (var rayInfo in m_RaySensor.RayPerceptionOutput.RayOutputs)
                {
                    DrawRaycastGizmos(rayInfo, alpha);
                }
            }
            else
            {
                var rayInput = GetRayPerceptionInput();
                if (m_UseBatchedRaycasts && rayInput.CastType == CustomRayPerceptionCastType.Cast3D)
                {
                    // TODO add call to PerceiveBatchedRays()
                    var rayOutputs = new CustomRayPerceptionOutput.CustomRayOutput[rayInput.Angles.Count];
                    CustomRayPerceptionSensor.PerceiveBatchedRays(ref rayOutputs, rayInput);
                    for (var rayIndex = 0; rayIndex < rayInput.Angles.Count; rayIndex++)
                    {
                        DrawRaycastGizmos(rayOutputs[rayIndex]);
                    }
                }
                else
                {
                    for (var rayIndex = 0; rayIndex < rayInput.Angles.Count; rayIndex++)
                    {
                        var rayOutput = CustomRayPerceptionSensor.PerceiveSingleRay(rayInput, rayIndex);
                        DrawRaycastGizmos(rayOutput);
                    }
                }
            }
        }

        /// <summary>
        /// Draw the debug information from the sensor (if available).
        /// </summary>
        void DrawRaycastGizmos(CustomRayPerceptionOutput.CustomRayOutput rayOutput, float alpha = 1.0f)
        {
            var startPositionWorld = rayOutput.StartPositionWorld;
            var endPositionWorld = rayOutput.EndPositionWorld;
            var rayDirection = endPositionWorld - startPositionWorld;
            rayDirection *= rayOutput.HitFraction;

            // hit fraction ^2 will shift "far" hits closer to the hit color
            var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
            var color = Color.Lerp(rayHitColor, rayMissColor, lerpT);
            color.a *= alpha;
            Gizmos.color = color;
            Gizmos.DrawRay(startPositionWorld, rayDirection);

            // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
            if (rayOutput.HasHit)
            {
                var hitRadius = Mathf.Max(rayOutput.ScaledCastRadius, .05f);
                Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
            }
        }
    }
}
