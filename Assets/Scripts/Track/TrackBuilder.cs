using System;
using System.Linq;
using FearIndigo.Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace FearIndigo.Track
{
    [BurstCompile]
    public static class TrackBuilder
    {
        private class GenerationFailedException : Exception
        {
            public GenerationFailedException()
            {
            }

            public GenerationFailedException(string message)
                : base(message)
            {
            }

            public GenerationFailedException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        /// <summary>
        /// <para>
        /// Generate the track data.
        /// </para>
        /// </summary>
        public static void GenerateTrack(ref TrackBuilderConfig config, out float2[] outputPoints, out float[] outputWidths)
        {
            var points = new NativeArray<float2>(config.numRandomPoints * 2, Allocator.TempJob);
            var widths = new NativeArray<float>(config.numRandomPoints * 2, Allocator.TempJob);
            var numPoints = new NativeArray<int>(1, Allocator.TempJob);
            var reverseDirection = new NativeArray<bool>(1, Allocator.TempJob);
            var startIndex = new NativeArray<int>(1, Allocator.TempJob);
            var trackGenerationJob = new TrackGenerationJob()
            {
                Config = config,
                Points = points,
                Widths = widths,
                NumPoints = numPoints,
                ReverseDirection = reverseDirection,
                StartIndex = startIndex
            };
            trackGenerationJob.Schedule().Complete();
            var subArrayPoints = points.GetSubArray(0, numPoints[0]);
            outputPoints = new float2[numPoints[0]];
            for (var i = 0; i < numPoints[0]; i++)
            {
                var index = (startIndex[0] + i) % numPoints[0];
                outputPoints[i] = subArrayPoints[index];
            }
            if (reverseDirection[0])
            {
                outputPoints = outputPoints.Reverse().ToArray();
            }
            outputWidths = widths.GetSubArray(0, numPoints[0]).ToArray();
            points.Dispose();
            widths.Dispose();
            numPoints.Dispose();
            reverseDirection.Dispose();
            startIndex.Dispose();
            subArrayPoints.Dispose();
        }
        
        /// <summary>
        /// <para>
        /// Job for generating track data.
        /// </para>
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct TrackGenerationJob : IJob
        {
            [ReadOnly] public TrackBuilderConfig Config;

            public NativeArray<float2> Points;
            [WriteOnly] public NativeArray<float> Widths;
            public NativeArray<int> NumPoints;
            [WriteOnly] public NativeArray<bool> ReverseDirection;
            [WriteOnly] public NativeArray<int> StartIndex;

            public void Execute()
            {
                GenerateTrack();
            }

            /// <summary>
            /// <para>
            /// Generate the track data.
            /// </para>
            /// </summary>
            [BurstCompile]
            private void GenerateTrack()
            {
                var rng = Random.CreateFromIndex(Config.seed);
                var iteration = 0;
                bool isValid;
                do
                {
                    iteration++;
                    if (iteration > Config.maxIterations)
                    {
                        throw new GenerationFailedException("Took too long to generate valid map!");
                    }
                    
                    GenerateRandomPoints(ref rng);
                    ConvertToConvexHull();
                    SubdivideAndDisplace(ref rng);
                    CheckConstraints(out isValid);
                } while (!isValid);
                
                for (var i = 0; i < NumPoints[0]; i++)
                {
                    Widths[i] = rng.NextFloat(Config.minTrackWidth, Config.maxTrackWidth);
                }

                // Change track to be counter clockwise
                ReverseDirection[0] = rng.NextBool();
                
                // Choose random start index
                StartIndex[0] = rng.NextInt(NumPoints[0]);
            }

            /// <summary>
            /// <para>
            /// Generate an array of random points.
            /// </para>
            /// <param name="rng">Random number generator.</param>
            /// </summary>
            [BurstCompile]
            private void GenerateRandomPoints(ref Random rng)
            {
                if (Config.numRandomPoints < 4)
                {
                    throw new GenerationFailedException("Need at least 4 random points!");
                }
                
                NumPoints[0] = Config.numRandomPoints;
                var halfBounds = Config.trackBounds / 2f;
                var iteration = 0;
                bool isValid;
                do
                {
                    iteration++;
                    if (iteration > Config.maxRandomPointIterations)
                    {
                        throw new GenerationFailedException("Took too long to generate random points.");
                    }

                    for (var i = 0; i < Config.numRandomPoints; i++)
                    {
                        Points[i] = rng.NextFloat2(-halfBounds, halfBounds);
                    }

                    CheckDistanceConstraint(Config.minRandomPointDistance, out isValid);
                } while (!isValid);
            }

            /// <summary>
            /// <para>
            /// Confirm that all points are at least a minimum distance away from each other.
            /// </para>
            /// </summary>
            /// <param name="minDistance"></param>
            /// <param name="isValid"></param>
            [BurstCompile]
            private void CheckDistanceConstraint(in float minDistance, out bool isValid)
            {
                for (var i0 = 0; i0 < NumPoints[0] - 1; i0++)
                {
                    for (var i1 = i0 + 1; i1 < NumPoints[0]; i1++)
                    {
                        if (math.distancesq(Points[i0], Points[i1]) < minDistance * minDistance)
                        {
                            isValid = false;
                            return;
                        }
                    }
                }
                
                isValid = true;
            }

            /// <summary>
            /// <para>
            /// Get a list of vertices for a convex hull that wraps an array of points.
            /// </para>
            /// </summary>
            [BurstCompile]
            private void ConvertToConvexHull()
            {
                if (NumPoints[0] < 3)
                {
                    throw new GenerationFailedException("Need at least 3 points to generate convex hull!");
                }

                var vertices = new NativeArray<float2>(Points.Length, Allocator.Temp);
                GetLowestYIndex(out var lowestYIndex);
                vertices[0] = Points[lowestYIndex];
                var verticesCount = 1;
                var collinearPoints = new NativeArray<float2>(NumPoints[0], Allocator.Temp);
                var collinearPointsCount = 0;
                var current = Points[lowestYIndex];
                var iteration = 0;
                while (true)
                {
                    iteration++;
                    if (iteration > Config.maxConvexHullIterations)
                    {
                        throw new GenerationFailedException("Took too many iterations to find convex hull!");
                    }

                    var nextTarget = Points[0];
                    for (var i = 1; i < NumPoints[0]; i++)
                    {
                        if (Points[i].Equals(current))
                            continue;
                        var x1 = current.x - nextTarget.x;
                        var x2 = current.x - Points[i].x;
                        var y1 = current.y - nextTarget.y;
                        var y2 = current.y - Points[i].y;
                        var val = (y2 * x1) - (y1 * x2);

                        if (val > 0)
                        {
                            nextTarget = Points[i];
                            collinearPointsCount = 0;
                        }
                        else if (val == 0)
                        {
                            if (math.distancesq(nextTarget, current) <
                                math.distancesq(Points[i], current))
                            {
                                collinearPoints[collinearPointsCount] = nextTarget;
                                collinearPointsCount++;
                                nextTarget = Points[i];
                            }
                            else
                            {
                                collinearPoints[collinearPointsCount] = Points[i];
                                collinearPointsCount++;
                            }
                        }
                    }

                    if (collinearPointsCount > 0)
                    {
                        for (var i = 0; i < collinearPointsCount; i++)
                        {
                            vertices[verticesCount] = collinearPoints[i];
                            verticesCount++;
                        }
                        collinearPointsCount = 0;
                    }

                    if (nextTarget.Equals(Points[lowestYIndex]))
                        break;

                    vertices[verticesCount] = nextTarget;
                    verticesCount++;
                    current = nextTarget;
                }

                NumPoints[0] = verticesCount;
                Points.CopyFrom(vertices);
                vertices.Dispose();
                collinearPoints.Dispose();
            }

            /// <summary>
            /// <para>
            /// Gets the index of the point with the lowest y value.
            /// </para>
            /// </summary>
            /// <param name="index"></param>
            [BurstCompile]
            private void GetLowestYIndex(out int index)
            {
                index = 0;
                var lowestY = float.PositiveInfinity;
                for (var i = 0; i < NumPoints[0]; i++)
                {
                    var point = Points[i];
                    if (point.y > lowestY) continue;
                    index = i;
                    lowestY = point.y;
                }
            }

            /// <summary>
            /// <para>
            /// Compute the midpoint of each pair of points and displace it by a random amount while ensuring displaced
            /// points are still within track bounds.
            /// </para>
            /// </summary>
            /// <param name="rng">Random number generator.</param>
            [BurstCompile]
            private void SubdivideAndDisplace(ref Random rng)
            {
                var displacedMidpoints = new NativeArray<float2>(Points.Length, Allocator.Temp);
                var halfBounds = Config.trackBounds / 2f;
                for (var i = 0; i < NumPoints[0]; i++)
                {
                    var p0 = Points[i];
                    var p1 = Points[(i + 1) % NumPoints[0]];
                    var midpoint = p0 + (p1 - p0) / 2f;
                    float2 displacedMidpoint;
                    var iteration = 0;
                    do
                    {
                        iteration++;
                        if (iteration > Config.maxDisplacementIterations)
                        {
                            throw new GenerationFailedException("Took too many iterations to find displaced midpoint!");
                        }

                        displacedMidpoint = midpoint + rng.NextFloat2(
                            new float2(-Config.maxXDisplacement, -Config.maxYDisplacement),
                            new float2(Config.maxXDisplacement, Config.maxYDisplacement));
                    } while (displacedMidpoint.x < -halfBounds.x || displacedMidpoint.x > halfBounds.x ||
                             displacedMidpoint.y < -halfBounds.y || displacedMidpoint.y > halfBounds.y);

                    displacedMidpoints[i] = displacedMidpoint;
                }

                var combinedPoints = new NativeArray<float2>(Points.Length, Allocator.Temp);
                for (var i = 0; i < NumPoints[0]; i++)
                {
                    var index = i * 2;
                    combinedPoints[index] = Points[i];
                    combinedPoints[index + 1] = displacedMidpoints[i];
                }

                Points.CopyFrom(combinedPoints);
                NumPoints[0] *= 2;

                displacedMidpoints.Dispose();
                combinedPoints.Dispose();
            }

            /// <summary>
            /// <para>
            /// Given both a minimum angle and a minimum distance threshold, confirm that:
            /// a) no points are within the minimum distance threshold to any other point and
            /// b) there is no set of three adjacent points where the angle of the vectors that connect them is less
            /// than the minimum angle threshold.
            /// </para>
            /// </summary>
            /// <param name="isValid"></param>
            [BurstCompile]
            private void CheckConstraints(out bool isValid)
            {
                CheckDistanceConstraint(Config.minPointDistance, out isValid);
                if (!isValid) return;

                for (var i = 0; i < NumPoints[0]; i++)
                {
                    var p0 = Points[i];
                    var p1 = Points[(i + 1) % NumPoints[0]];
                    var p2 = Points[(i + 2) % NumPoints[0]];

                    CheckAngleConstraint(p0 - p1, p2 - p1, Config.minCornerAngle, out isValid);
                    if (!isValid) return;
                }
            }

            /// <summary>
            /// <para>
            /// Get the angle in degrees between 'from' and 'to' vectors.
            /// </para>
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <param name="minAngle"></param>
            /// <param name="isValid"></param>
            [BurstCompile]
            private static void CheckAngleConstraint(in float2 from, in float2 to, in float minAngle, out bool isValid)
            {
                isValid = math.degrees(math.acos(math.dot(math.normalize(from), math.normalize(to)))) >= minAngle;
            }
        }
    }
}

