using System;
using System.Collections.Generic;
using System.Linq;
using FearIndigo.Utility;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// Contains the elements that define a ray perception sensor.
    /// </summary>
    public struct CustomRayPerceptionInput
    {
        /// <summary>
        /// Length of the rays to cast. This will be scaled up or down based on the scale of the transform.
        /// </summary>
        public float RayLength;
        
        public RaycastHit2D[] RayHits2D;

        /// <summary>
        /// Function to call to get detectable game objects.
        /// </summary>
        public GameObject[] DetectableObjects;

        /// <summary>
        /// Whether the orientation should follow the agent.
        /// </summary>
        public Rigidbody2D AgentRigidbody;

        /// <summary>
        /// List of angles (in degrees) used to define the rays.
        /// 90 degrees is considered "forward" relative to the game object.
        /// </summary>
        public IReadOnlyList<float> Angles;

        /// <summary>
        /// Radius of the sphere to use for spherecasting.
        /// If 0 or less, rays are used instead - this may be faster, especially for complex environments.
        /// </summary>
        public float CastRadius;

        /// <summary>
        /// Use inverse raycast.
        /// </summary>
        public bool InverseRaycast;
        
        /// <summary>
        /// Filtering options for the casts.
        /// </summary>
        public int LayerMask;

        public int LayerMaskTest;

        /// <summary>
        /// Returns the expected number of floats in the output.
        /// </summary>
        /// <returns></returns>
        public int OutputSize()
        {
            return 2 * (Angles?.Count ?? 0);
        }

        /// <summary>
        /// Get the cast start and end points for the given ray index/
        /// </summary>
        /// <param name="rayIndex"></param>
        /// <returns>A tuple of the start and end positions in world space.</returns>
        public (Vector2 StartPositionWorld, Vector2 EndPositionWorld) RayExtents(int rayIndex)
        {
            var angle = Angles[rayIndex];
            
            var endPositionLocal = PolarToCartesian2D(RayLength, angle);

            var position = AgentRigidbody.position;
            var endPositionWorld = position + endPositionLocal;

            return (StartPositionWorld: position, EndPositionWorld: endPositionWorld);
        }

        /// <summary>
        /// Converts polar coordinate to cartesian coordinate.
        /// </summary>
        static internal Vector3 PolarToCartesian3D(float radius, float angleDegrees)
        {
            var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
            var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Converts polar coordinate to cartesian coordinate.
        /// </summary>
        static internal Vector2 PolarToCartesian2D(float radius, float angleDegrees)
        {
            var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
            var y = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
            return new Vector2(x, y);
        }
    }

    /// <summary>
    /// Contains the data generated/produced from a ray perception sensor.
    /// </summary>
    public class CustomRayPerceptionOutput
    {
        /// <summary>
        /// Contains the data generated from a single ray of a ray perception sensor.
        /// </summary>
        public struct CustomRayOutput
        {
            /// <summary>
            /// Whether or not the ray hit the desired collider.
            /// </summary>
            public bool HasHit;

            /// <summary>
            /// Normalized distance to the hit object.
            /// </summary>
            public float HitFraction;

            /// <summary>
            /// Start position of the ray in world space.
            /// </summary>
            public Vector3 StartPositionWorld;

            /// <summary>
            /// End position of the ray in world space.
            /// </summary>
            public Vector3 EndPositionWorld;

            /// <summary>
            /// The scaled size of the cast.
            /// </summary>
            /// <remarks>
            /// If there is non-(1,1,1) scale, the cast radius will be also be scaled.
            /// </remarks>
            public float ScaledCastRadius;

            /// <summary>
            /// Writes the ray output information to a subset of the float array.  Each element in the rayAngles array
            /// determines a sublist of data to the observation. The sublist contains the observation data for a single cast.
            /// </summary>
            /// <param name="rayIndex"></param>
            /// <param name="buffer">Output buffer. The size must be equal to (numDetectableTags+2) * RayOutputs.Length</param>
            public void ToFloatArray(int rayIndex, float[] buffer)
            {
                var bufferOffset = 2 * rayIndex;
                buffer[bufferOffset] = HasHit ? 0f : 1f;
                buffer[bufferOffset + 1] = HitFraction;
            }
        }

        /// <summary>
        /// RayOutput for each ray that was cast.
        /// </summary>
        public CustomRayOutput[] RayOutputs;
    }

    /// <summary>
    /// A sensor implementation that supports ray cast-based observations.
    /// </summary>
    public class CustomRayPerceptionSensor2D : ISensor
    {
        float[] m_Observations;
        ObservationSpec m_ObservationSpec;
        string m_Name;

        CustomRayPerceptionInput m_RayPerceptionInput;
        CustomRayPerceptionOutput m_RayPerceptionOutput;

        /// <summary>
        /// Time.frameCount at the last time Update() was called. This is only used for display in gizmos.
        /// </summary>
        int m_DebugLastFrameCount;

        internal int DebugLastFrameCount
        {
            get { return m_DebugLastFrameCount; }
        }

        /// <summary>
        /// Creates the RayPerceptionSensor.
        /// </summary>
        /// <param name="name">The name of the sensor.</param>
        /// <param name="rayInput">The inputs for the sensor.</param>
        public CustomRayPerceptionSensor2D(string name, CustomRayPerceptionInput rayInput)
        {
            m_Name = name;
            m_RayPerceptionInput = rayInput;

            SetNumObservations(rayInput.OutputSize());

            m_DebugLastFrameCount = Time.frameCount;
            m_RayPerceptionOutput = new CustomRayPerceptionOutput();
        }

        /// <summary>
        /// The most recent raycast results.
        /// </summary>
        public CustomRayPerceptionOutput RayPerceptionOutput
        {
            get { return m_RayPerceptionOutput; }
        }

        void SetNumObservations(int numObservations)
        {
            m_ObservationSpec = ObservationSpec.Vector(numObservations);
            m_Observations = new float[numObservations];
        }

        internal void SetRayPerceptionInput(CustomRayPerceptionInput rayInput)
        {
            // Note that change the number of rays or tags doesn't directly call this,
            // but changing them and then changing another field will.
            if (m_RayPerceptionInput.OutputSize() != rayInput.OutputSize())
            {
                Debug.Log(
                    "Changing the number of tags or rays at runtime is not " +
                    "supported and may cause errors in training or inference."
                );
                // Changing the shape will probably break things downstream, but we can at least
                // keep this consistent.
                SetNumObservations(rayInput.OutputSize());
            }
            m_RayPerceptionInput = rayInput;
        }

        /// <summary>
        /// Computes the ray perception observations and saves them to the provided
        /// <see cref="ObservationWriter"/>.
        /// </summary>
        /// <param name="writer">Where the ray perception observations are written to.</param>
        /// <returns></returns>
        public int Write(ObservationWriter writer)
        {
            Array.Clear(m_Observations, 0, m_Observations.Length);
            var numRays = m_RayPerceptionInput.Angles.Count;

            // For each ray, write the information to the observation buffer
            for (var rayIndex = 0; rayIndex < numRays; rayIndex++)
            {
                m_RayPerceptionOutput.RayOutputs?[rayIndex].ToFloatArray(rayIndex, m_Observations);
            }

            // Finally, add the observations to the ObservationWriter
            writer.AddList(m_Observations);
            
            return m_Observations.Length;
        }

        /// <inheritdoc/>
        public void Update()
        {
            m_DebugLastFrameCount = Time.frameCount;
            var numRays = m_RayPerceptionInput.Angles.Count;

            if (m_RayPerceptionOutput.RayOutputs == null || m_RayPerceptionOutput.RayOutputs.Length != numRays)
            {
                m_RayPerceptionOutput.RayOutputs = new CustomRayPerceptionOutput.CustomRayOutput[numRays];
            }
            
            // For each ray, do the casting and save the results.
            for (var rayIndex = 0; rayIndex < numRays; rayIndex++)
            {
                m_RayPerceptionOutput.RayOutputs[rayIndex] = PerceiveSingleRay(m_RayPerceptionInput, rayIndex);
            }
        }

        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public ObservationSpec GetObservationSpec()
        {
            return m_ObservationSpec;
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return m_Name;
        }

        /// <inheritdoc/>
        public virtual byte[] GetCompressedObservation()
        {
            return null;
        }

        /// <inheritdoc/>
        public CompressionSpec GetCompressionSpec()
        {
            return CompressionSpec.Default();
        }

        /// <summary>
        /// Evaluates the raycasts to be used as part of an observation of an agent.
        /// </summary>
        /// <param name="input">Input defining the rays that will be cast.</param>
        /// <param name="batched">Use batched raycasts.</param>
        /// <returns>Output struct containing the raycast results.</returns>
        public static CustomRayPerceptionOutput Perceive(CustomRayPerceptionInput input, bool batched)
        {
            CustomRayPerceptionOutput output = new CustomRayPerceptionOutput();
            output.RayOutputs = new CustomRayPerceptionOutput.CustomRayOutput[input.Angles.Count];
            
            for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++)
            {
                output.RayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
            }

            return output;
        }

        /// <summary>
        /// Evaluate the raycast results of a single ray from the RayPerceptionInput.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rayIndex"></param>
        /// <returns></returns>
        internal static CustomRayPerceptionOutput.CustomRayOutput PerceiveSingleRay(
            CustomRayPerceptionInput input,
            int rayIndex
        )
        {
            var rayLength = input.RayLength;
            var castRadius = input.CastRadius;
            var detectableObjects = input.DetectableObjects;

            var extents = input.RayExtents(rayIndex);
            var startPositionWorld = extents.StartPositionWorld;
            var endPositionWorld = extents.EndPositionWorld;
            var rayDirection = endPositionWorld - startPositionWorld;

            // Do the cast and assign the hit information for each detectable tag.
            var hitFraction = 1.0f;
            var castHit = false;

            if (input.InverseRaycast)
            {
                castHit = InverseCast2D.Cast(startPositionWorld, castRadius, rayDirection, input.RayHits2D,
                    rayLength, input.LayerMask, input.LayerMaskTest);
            }
            else
            {
                if (castRadius > 0f)
                {
                    castHit = Physics2D.CircleCastNonAlloc(startPositionWorld, castRadius, rayDirection,
                        input.RayHits2D, rayLength, input.LayerMask) > 0;
                }
                else
                {
                    castHit = Physics2D.RaycastNonAlloc(startPositionWorld, rayDirection,
                        input.RayHits2D, rayLength, input.LayerMask) > 0;
                }
            }
            
            if (castHit && detectableObjects.Contains(input.RayHits2D[0].collider.gameObject))
            {
                hitFraction = input.RayHits2D[0].fraction;
            }
            else
            {
                castHit = false;
            }

            var rayOutput = new CustomRayPerceptionOutput.CustomRayOutput
            {
                HasHit = castHit,
                HitFraction = hitFraction,
                StartPositionWorld = startPositionWorld,
                EndPositionWorld = endPositionWorld,
                ScaledCastRadius = castRadius
            };

            return rayOutput;
        }
    }
}
