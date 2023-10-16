using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// Determines which dimensions the sensor will perform the casts in.
    /// </summary>
    public enum CustomRayPerceptionCastType
    {
        /// <summary>
        /// Cast in 2 dimensions, using Physics2D.CircleCast or Physics2D.RayCast.
        /// </summary>
        Cast2D,

        /// <summary>
        /// Cast in 3 dimensions, using Physics.SphereCast or Physics.RayCast.
        /// </summary>
        Cast3D,
    }

    /// <summary>
    /// Contains the elements that define a ray perception sensor.
    /// </summary>
    public struct CustomRayPerceptionInput
    {
        /// <summary>
        /// Length of the rays to cast. This will be scaled up or down based on the scale of the transform.
        /// </summary>
        public float RayLength;
        
        public RaycastHit[] RayHits;
        public RaycastHit2D[] RayHits2D;

        /// <summary>
        /// Function to call to get detectable game object.
        /// </summary>
        public Func<GameObject> DetectableObject;

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
        /// Starting height offset of ray from center of agent
        /// </summary>
        public float StartOffset;

        /// <summary>
        /// Ending height offset of ray from center of agent.
        /// </summary>
        public float EndOffset;

        /// <summary>
        /// Radius of the sphere to use for spherecasting.
        /// If 0 or less, rays are used instead - this may be faster, especially for complex environments.
        /// </summary>
        public float CastRadius;

        /// <summary>
        /// Whether to perform the casts in 2D or 3D.
        /// </summary>
        public CustomRayPerceptionCastType CastType;

        /// <summary>
        /// Filtering options for the casts.
        /// </summary>
        public int LayerMask;

        /// <summary>
        ///  Whether to use batched raycasts.
        /// </summary>
        public bool UseBatchedRaycasts;

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
        public (Vector3 StartPositionWorld, Vector3 EndPositionWorld) RayExtents(int rayIndex)
        {
            var angle = Angles[rayIndex];
            Vector3 startPositionLocal, endPositionLocal;
            if (CastType == CustomRayPerceptionCastType.Cast3D)
            {
                startPositionLocal = new Vector3(0, StartOffset, 0);
                endPositionLocal = PolarToCartesian3D(RayLength, angle);
                endPositionLocal.y += EndOffset;
            }
            else
            {
                // Vector2s here get converted to Vector3s (and back to Vector2s for casting)
                startPositionLocal = new Vector2();
                endPositionLocal = PolarToCartesian2D(RayLength, angle);
            }

            var position = (Vector3) AgentRigidbody.position;
            var startPositionWorld = position + startPositionLocal;
            var endPositionWorld = position + endPositionLocal;

            return (StartPositionWorld: startPositionWorld, EndPositionWorld: endPositionWorld);
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
            /// The scaled length of the ray.
            /// </summary>
            /// <remarks>
            /// If there is non-(1,1,1) scale, |EndPositionWorld - StartPositionWorld| will be different from
            /// the input rayLength.
            /// </remarks>
            public float ScaledRayLength
            {
                get
                {
                    var rayDirection = EndPositionWorld - StartPositionWorld;
                    return rayDirection.magnitude;
                }
            }

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
    public class CustomRayPerceptionSensor : ISensor
    {
        float[] m_Observations;
        ObservationSpec m_ObservationSpec;
        string m_Name;

        CustomRayPerceptionInput m_RayPerceptionInput;
        CustomRayPerceptionOutput m_RayPerceptionOutput;

        bool m_UseBatchedRaycasts;

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
        public CustomRayPerceptionSensor(string name, CustomRayPerceptionInput rayInput)
        {
            m_Name = name;
            m_RayPerceptionInput = rayInput;
            m_UseBatchedRaycasts = rayInput.UseBatchedRaycasts;

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

            if (m_UseBatchedRaycasts && m_RayPerceptionInput.CastType == CustomRayPerceptionCastType.Cast3D)
            {
                PerceiveBatchedRays(ref m_RayPerceptionOutput.RayOutputs, m_RayPerceptionInput);
            }
            else
            {
                // For each ray, do the casting and save the results.
                for (var rayIndex = 0; rayIndex < numRays; rayIndex++)
                {
                    m_RayPerceptionOutput.RayOutputs[rayIndex] = PerceiveSingleRay(m_RayPerceptionInput, rayIndex);
                }
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

            if (batched)
            {
                PerceiveBatchedRays(ref output.RayOutputs, input);
            }
            else
            {
                for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++)
                {
                    output.RayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
                }
            }

            return output;
        }

        /// <summary>
        /// Evaluate the raycast results of all the rays from the RayPerceptionInput as a batch.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rayIndex"></param>
        /// <returns></returns>
        internal static void PerceiveBatchedRays(ref CustomRayPerceptionOutput.CustomRayOutput[] batchedRaycastOutputs, CustomRayPerceptionInput input)
        {
            var numRays = input.Angles.Count;
            var results = new NativeArray<RaycastHit>(numRays, Allocator.TempJob);
            var unscaledRayLength = input.RayLength;
            var unscaledCastRadius = input.CastRadius;
            var detectableObject = input.DetectableObject?.Invoke();

            var raycastCommands = new NativeArray<RaycastCommand>(unscaledCastRadius <= 0f ? numRays : 0, Allocator.TempJob);
            var spherecastCommands = new NativeArray<SpherecastCommand>(unscaledCastRadius > 0f ? numRays : 0, Allocator.TempJob);

            // this is looped

            for (int i = 0; i < numRays; i++)
            {
                var extents = input.RayExtents(i);
                var startPositionWorld = extents.StartPositionWorld;
                var endPositionWorld = extents.EndPositionWorld;

                var rayDirection = endPositionWorld - startPositionWorld;
                // If there is non-unity scale, |rayDirection| will be different from rayLength.
                // We want to use this transformed ray length for determining cast length, hit fraction etc.
                // We also it to scale up or down the sphere or circle radii
                var scaledRayLength = rayDirection.magnitude;
                // Avoid 0/0 if unscaledRayLength is 0
                var scaledCastRadius = unscaledRayLength > 0 ?
                    unscaledCastRadius * scaledRayLength / unscaledRayLength :
                    unscaledCastRadius;

                var queryParameters = QueryParameters.Default;
                queryParameters.layerMask = input.LayerMask;

                var rayDirectionNormalized = rayDirection.normalized;

                if (scaledCastRadius > 0f)
                {
                    spherecastCommands[i] = new SpherecastCommand(startPositionWorld, scaledCastRadius, rayDirectionNormalized, queryParameters, scaledRayLength);
                }
                else
                {
                    raycastCommands[i] = new RaycastCommand(startPositionWorld, rayDirectionNormalized, queryParameters, scaledRayLength);
                }

                batchedRaycastOutputs[i] = new CustomRayPerceptionOutput.CustomRayOutput
                {
                    HasHit = false,
                    HitFraction = 1f,
                    StartPositionWorld = startPositionWorld,
                    EndPositionWorld = endPositionWorld,
                    ScaledCastRadius = scaledCastRadius
                };
            }

            if (unscaledCastRadius > 0f)
            {
                JobHandle handle = SpherecastCommand.ScheduleBatch(spherecastCommands, results, 1, 1, default(JobHandle));
                handle.Complete();
            }
            else
            {
                JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, results, 1, 1, default(JobHandle));
                handle.Complete();
            }

            for (int i = 0; i < results.Length; i++)
            {
                var castHit = results[i].collider != null;
                var hitFraction = 1.0f;
                GameObject hitObject = null;
                float scaledRayLength;
                float scaledCastRadius = batchedRaycastOutputs[i].ScaledCastRadius;
                if (scaledCastRadius > 0f)
                {
                    scaledRayLength = spherecastCommands[i].distance;
                }
                else
                {
                    scaledRayLength = raycastCommands[i].distance;
                }

                // hitFraction = castHit ? (scaledRayLength > 0 ? results[i].distance / scaledRayLength : 0.0f) : 1.0f;
                // Debug.Log(results[i].distance);
                hitFraction = castHit ? (scaledRayLength > 0 ? results[i].distance / scaledRayLength : 0.0f) : 1.0f;
                hitObject = castHit ? results[i].collider.gameObject : null;

                if (castHit)
                {
                    if (hitObject == detectableObject)
                    {
                        batchedRaycastOutputs[i].HasHit = true;
                        batchedRaycastOutputs[i].HitFraction = hitFraction;
                        break;
                    }
                }
            }

            results.Dispose();
            raycastCommands.Dispose();
            spherecastCommands.Dispose();
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
            var unscaledRayLength = input.RayLength;
            var unscaledCastRadius = input.CastRadius;
            var detectableObject = input.DetectableObject?.Invoke();

            var extents = input.RayExtents(rayIndex);
            var startPositionWorld = extents.StartPositionWorld;
            var endPositionWorld = extents.EndPositionWorld;

            var rayDirection = endPositionWorld - startPositionWorld;
            // If there is non-unity scale, |rayDirection| will be different from rayLength.
            // We want to use this transformed ray length for determining cast length, hit fraction etc.
            // We also it to scale up or down the sphere or circle radii
            var scaledRayLength = rayDirection.magnitude;
            // Avoid 0/0 if unscaledRayLength is 0
            var scaledCastRadius = unscaledRayLength > 0 ?
                unscaledCastRadius * scaledRayLength / unscaledRayLength :
                unscaledCastRadius;

            // Do the cast and assign the hit information for each detectable tag.
            int numHit;
            var hitFraction = 1.0f;
            var castHit = false;

            if (input.CastType == CustomRayPerceptionCastType.Cast3D)
            {
                if (scaledCastRadius > 0f)
                {
                    numHit = Physics.SphereCastNonAlloc(startPositionWorld, scaledCastRadius, rayDirection, input.RayHits,
                        scaledRayLength, input.LayerMask);
                }
                else
                {
                    numHit = Physics.RaycastNonAlloc(startPositionWorld, rayDirection, input.RayHits,
                        scaledRayLength, input.LayerMask);
                }

                for (var i = 0; i < numHit; i++)
                {
                    var hit = input.RayHits[i];
                    if(hit.collider.gameObject != detectableObject) continue;
                    
                    // If scaledRayLength is 0, we still could have a hit with sphere casts (maybe?).
                    // To avoid 0/0, set the fraction to 0.
                    hitFraction = scaledRayLength > 0 ? hit.distance / scaledRayLength : 0.0f;
                    castHit = true;
                    break;
                }
            }
            else
            {
                if (scaledCastRadius > 0f)
                {
                    numHit = Physics2D.CircleCastNonAlloc(startPositionWorld, scaledCastRadius, rayDirection,
                        input.RayHits2D, scaledRayLength, input.LayerMask);
                }
                else
                {
                    numHit = Physics2D.RaycastNonAlloc(startPositionWorld, rayDirection, input.RayHits2D, scaledRayLength, input.LayerMask);
                }

                for (var i = 0; i < numHit; i++)
                {
                    var hit = input.RayHits2D[i];
                    if(hit.collider.gameObject != detectableObject) continue;
                    
                    hitFraction = hit.fraction;
                    castHit = true;
                    break;
                }
            }

            var rayOutput = new CustomRayPerceptionOutput.CustomRayOutput
            {
                HasHit = castHit,
                HitFraction = hitFraction,
                StartPositionWorld = startPositionWorld,
                EndPositionWorld = endPositionWorld,
                ScaledCastRadius = scaledCastRadius
            };

            return rayOutput;
        }
    }
}
