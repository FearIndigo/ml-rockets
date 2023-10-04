using System;
using FearIndigo.Splines;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace FearIndigo.Managers
{
    [BurstCompile]
    public class TrackManager : MonoBehaviour
    {
        public class GenerationFailedException : Exception 
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
        
        [Header("Spline")]
        public Spline trackSpline;

        [Header("Config")]
        public float2 trackBounds;
        public float minTrackWidth;
        public float maxTrackWidth;
        
        [Header("Generation")]
        public uint randomSeed;
        public int numRandomPoints;
        public float maxXDisplacement;
        public float maxYDisplacement;
        public float minPointDistance;
        public float maxPointDistance;
        public float minCornerAngle;
        public int maxIterations;
        public int maxConvexHullIterations;
        public int maxDisplacementIterations;

        private Random _rng;

        /// <summary>
        /// <para>
        /// Generate a new random seed.
        /// </para>
        /// </summary>
        public void RandomizeSeed()
        {
            randomSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        }

        ///<summary>
        /// <para>
        /// Randomize seed before generating track
        /// </para>
        /// </summary>
        public void GenerateRandomTrack()
        {
            RandomizeSeed();
            GenerateTrack();
        }
        
        /// <summary>
        /// <para>
        /// 1) Generate a set of random points.
        /// 2) Get their convex hull.
        /// 3) Compute the midpoint of each pair of points in the convex hull and displace it by a random amount.
        /// 4) Given both a minimum angle and a minimum distance threshold, push apart the set of points resulting from 2
        ///    and 3 to try to guarantee that:
        ///      a) there are not two adjacent points whose distance is less than the minimum distance threshold and
        ///      b) there is no set of three adjacent points where the angle of the vectors that connect them is less
        ///         than the minimum angle threshold.
        /// 5) Obtain the final layout by interpolating all the points obtained after step 4 with splines forcing them to
        ///    pass through all the input points.
        /// </para>
        /// </summary>
        [BurstCompile]
        public void GenerateTrack()
        {
            _rng = Random.CreateFromIndex(randomSeed);
            try
            {
                NativeArray<float2> points;
                var valid = false;
                var iteration = 0;
                do
                {
                    iteration++;
                    if (iteration > maxIterations)
                    {
                        throw new GenerationFailedException("Took too long to generate valid map!");
                    }

                    points = GenerateRandomPoints();
                    points = GetConvexHullVertices(points);
                    if (CheckConstraints(points)) continue;
                    points = GetDisplacedMidpoints(points);
                    valid = CheckConstraints(points);
                } while (!valid);
                
                UpdateSplineFromPoints(points);
            }
            catch(GenerationFailedException e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// <para>
        /// Generate an array of random points.
        /// </para>
        /// </summary>
        [BurstCompile]
        private NativeArray<float2> GenerateRandomPoints()
        {
            var points = new NativeArray<float2>(numRandomPoints, Allocator.Temp);
            var halfBounds = trackBounds / 2f;
            for (var i = 0; i < numRandomPoints; i++)
            {
                points[i] = _rng.NextFloat2(-halfBounds, halfBounds);
            }
            return points;
        }

        /// <summary>
        /// <para>
        /// Get a list of vertices for a convex hull that wraps an array of points.
        /// </para>
        /// </summary>
        /// <param name="points"></param>
        [BurstCompile]
        private NativeArray<float2> GetConvexHullVertices(NativeArray<float2> points)
        {
            var vertices = new NativeArray<float2>(points.Length + 1, Allocator.Temp);
            var lowestYIndex = GetLowestYIndex(points);
            vertices[0] = points[lowestYIndex];
            var verticesCount = 1;
            var collinearPoints = new NativeArray<float2>(points.Length, Allocator.Temp);
            var collinearPointsCount = 0;
            var current = points[lowestYIndex];
            var iteration = 0;
            while (true)
            {
                iteration++;
                if (iteration > maxConvexHullIterations)
                {
                    throw new GenerationFailedException("Took too many iterations to find convex hull!");
                }
                var nextTarget = points[0];
                for (var i = 1; i < points.Length; i++)
                {
                    if (points[i].Equals(current))
                        continue;
                    var x1 = current.x - nextTarget.x;
                    var x2 = current.x - points[i].x;
                    var y1 = current.y - nextTarget.y;
                    var y2 = current.y - points[i].y;
                    var val = (y2 * x1) - (y1 * x2);
                    if (val > 0)
                    {
                        nextTarget = points[i];
                        collinearPoints = new NativeArray<float2>(points.Length, Allocator.Temp);
                        collinearPointsCount = 0;
                    }
                    else if (val == 0)
                    {
                        var currentToNext = nextTarget - current;
                        var currentToPoint = points[i] - current;
                        if (currentToNext.x * currentToNext.x + currentToNext.y * currentToNext.y <
                            currentToPoint.x * currentToPoint.x + currentToPoint.y * currentToPoint.y)
                        {
                            collinearPoints[collinearPointsCount] = nextTarget;
                            collinearPointsCount++;
                            nextTarget = points[i];
                        }
                        else
                        {
                            collinearPoints[collinearPointsCount] = points[i];
                            collinearPointsCount++;
                        }
                    }
                }

                foreach (var collinearPoint in collinearPoints.GetSubArray(0, collinearPointsCount))
                {
                    vertices[verticesCount] = collinearPoint;
                    verticesCount++;
                }
                if (nextTarget.Equals(points[lowestYIndex]))
                    break;
                vertices[verticesCount] = nextTarget;
                verticesCount++;
                current = nextTarget;
            }
            return vertices.GetSubArray(0, verticesCount);
        }

        /// <summary>
        /// <para>
        /// Gets the index of the point with the lowest y value.
        /// </para>
        /// </summary>
        /// <param name="points"></param>
        [BurstCompile]
        private int GetLowestYIndex(NativeArray<float2> points)
        {
            var index = 0;
            var lowestY = float.PositiveInfinity;
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                if(point.y > lowestY) continue;
                index = i;
                lowestY = point.y;
            }
            return index;
        }
        
        /// <summary>
        /// <para>
        /// Compute the midpoint of each pair of points and displace it by a random amount while ensuring displaced
        /// points are still within track bounds.
        /// </para>
        /// </summary>
        /// <param name="points"></param>
        [BurstCompile]
        private NativeArray<float2> GetDisplacedMidpoints(NativeArray<float2> points)
        {
            var displacedMidpoints = new NativeArray<float2>(points.Length, Allocator.Temp);
            var halfBounds = trackBounds / 2f;
            for (var i = 0; i < points.Length; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Length];
                var midpoint = p0 + (p1 - p0) / 2f;
                float2 displacedMidpoint;
                var iteration = 0;
                do
                {
                    iteration++;
                    if (iteration > maxDisplacementIterations)
                    {
                        throw new GenerationFailedException("Took too many iterations to find displaced midpoint!");
                    }

                    displacedMidpoint = midpoint + _rng.NextFloat2(
                                            new float2(-maxXDisplacement, -maxYDisplacement),
                                            new float2(maxXDisplacement, maxYDisplacement));
                } while (displacedMidpoint.x < -halfBounds.x || displacedMidpoint.x > halfBounds.x ||
                         displacedMidpoint.y < -halfBounds.y || displacedMidpoint.y > halfBounds.y);
                displacedMidpoints[i] = displacedMidpoint;
            }
            return CombineArrays(points, displacedMidpoints);
        }

        /// <summary>
        /// <para>
        /// Combine arrays of same length into one array, taking one element from each array at a time.
        /// </para>
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        [BurstCompile]
        private NativeArray<T> CombineArrays<T>(NativeArray<T> array1, NativeArray<T> array2) where T : struct
        {
            if (array1.Length != array2.Length)
            {
                throw new Exception("Cannot combine arrays of different lengths!");
            }
            
            var combinedPoints = new NativeArray<T>(array1.Length * 2, Allocator.Temp);
            for (var i = 0; i < array1.Length; i++)
            {
                var index = i * 2;
                combinedPoints[index] = array1[i];
                combinedPoints[index + 1] = array2[i];
            }
            return combinedPoints;
        }

        /// <summary>
        /// <para>
        /// Given both a minimum angle and a minimum distance threshold, confirm that:
        /// a) there are not two adjacent points whose distance is less than the minimum distance threshold and
        /// b) there is no set of three adjacent points where the angle of the vectors that connect them is less
        /// than the minimum angle threshold.
        /// </para>
        /// </summary>
        /// <param name="points">Array of points to check constraints.</param>
        [BurstCompile]
        private bool CheckConstraints(NativeArray<float2> points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Length];
                var p2 = points[(i + 2) % points.Length];

                var distSqr = math.distancesq(p0, p1);
                if (distSqr < minPointDistance * minPointDistance || distSqr > maxPointDistance * maxPointDistance)
                    return false;

                if (GetAngle(p0 - p1, p2 - p1) < minCornerAngle) return false;
            }

            return true;
        }
        
        /// <summary>
        /// <para>
        /// Get the angle in degrees between 'from' and 'to' vectors.
        /// </para>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        [BurstCompile]
        public float GetAngle(float2 from, float2 to)
        {
            return math.degrees(math.acos(math.dot(math.normalize(from), math.normalize(to))));
        }
        
        /// <summary>
        /// <para>
        /// Set knots on spline from an array of points.
        /// </para>
        /// </summary>
        /// <param name="points">Points spline should pass through.</param>
        [BurstCompile]
        private void UpdateSplineFromPoints(NativeArray<float2> points)
        {
            var knots = new NativeArray<Knot>(points.Length, Allocator.Temp);
            for (var i = 0; i < points.Length; i++)
            {
                knots[i] = new Knot(points[i], _rng.NextFloat(minTrackWidth, maxTrackWidth));
            }
            trackSpline.SetKnots(knots);
        }

        private void OnDrawGizmos()
        {
            var halfBounds = trackBounds / 2f;
            var points = new Vector3[]
            {
                new Vector3(-halfBounds.x, -halfBounds.y, 0),
                new Vector3(-halfBounds.x, halfBounds.y, 0),
                new Vector3(halfBounds.x, halfBounds.y, 0),
                new Vector3(halfBounds.x, -halfBounds.y, 0)
            };

            for (var i = 0; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
            }
        }
    }
}

