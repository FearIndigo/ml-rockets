using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// The grid perception strategy that uses box overlap to detect objects.
    /// </summary>
    internal class BoxOverlap2DChecker : IGridPerception2D
    {
        Vector2 m_CellScale;
        Vector2Int m_GridSize;
        LayerMask m_ColliderMask;
        Rigidbody2D m_AgentRigidbody;
        string[] m_DetectableTags;
        int m_InitialColliderBufferSize;
        int m_MaxColliderBufferSize;

        int m_NumCells;
        Vector2 m_HalfCellScale;
        Vector2 m_CellCenterOffset;
        Vector2[] m_CellLocalPositions;

        Collider2D[] m_ColliderBuffer;

        public event Action<GameObject, int> GridOverlapDetectedAll;
        public event Action<GameObject, int> GridOverlapDetectedClosest;
        public event Action<GameObject, int> GridOverlapDetectedDebug;

        public BoxOverlap2DChecker(
            Vector2 cellScale,
            Vector2Int gridSize,
            LayerMask colliderMask,
            Rigidbody2D agentRigidbody,
            string[] detectableTags,
            int initialColliderBufferSize,
            int maxColliderBufferSize)
        {
            m_CellScale = cellScale;
            m_GridSize = gridSize;
            m_ColliderMask = colliderMask;
            m_AgentRigidbody = agentRigidbody;
            m_DetectableTags = detectableTags;
            m_InitialColliderBufferSize = initialColliderBufferSize;
            m_MaxColliderBufferSize = maxColliderBufferSize;

            m_NumCells = gridSize.x * gridSize.y;
            m_HalfCellScale = new Vector2(cellScale.x / 2f, cellScale.y / 2f);
            m_CellCenterOffset = new Vector2((gridSize.x - 1f) / 2, (gridSize.y - 1f) / 2);
            m_ColliderBuffer = new Collider2D[Math.Min(m_MaxColliderBufferSize, m_InitialColliderBufferSize)];

            InitCellLocalPositions();
        }

        public LayerMask ColliderMask
        {
            get { return m_ColliderMask; }
            set { m_ColliderMask = value; }
        }

        /// <summary>
        /// Initializes the local location of the cells
        /// </summary>
        void InitCellLocalPositions()
        {
            m_CellLocalPositions = new Vector2[m_NumCells];

            for (int i = 0; i < m_NumCells; i++)
            {
                m_CellLocalPositions[i] = GetCellLocalPosition(i);
            }
        }

        public Vector2 GetCellLocalPosition(int cellIndex)
        {
            float x = (cellIndex / m_GridSize.y - m_CellCenterOffset.x) * m_CellScale.x;
            float y = (cellIndex % m_GridSize.y - m_CellCenterOffset.y) * m_CellScale.y;
            return new Vector2(x, y);
        }

        public Vector2 GetCellGlobalPosition(int cellIndex)
        {
            return m_CellLocalPositions[cellIndex] + m_AgentRigidbody.position;
        }

        public void Perceive()
        {
            for (var cellIndex = 0; cellIndex < m_NumCells; cellIndex++)
            {
                var cellCenter = GetCellGlobalPosition(cellIndex);
                var numFound = BufferResizingOverlapBoxNonAlloc(cellCenter, m_HalfCellScale);

                if (GridOverlapDetectedAll != null)
                {
                    ParseCollidersAll(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedAll);
                }
                if (GridOverlapDetectedClosest != null)
                {
                    ParseCollidersClosest(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedClosest);
                }
            }
        }

        public void UpdateGizmo()
        {
            for (var cellIndex = 0; cellIndex < m_NumCells; cellIndex++)
            {
                var cellCenter = GetCellGlobalPosition(cellIndex);
                var numFound = BufferResizingOverlapBoxNonAlloc(cellCenter, m_HalfCellScale);

                ParseCollidersClosest(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedDebug);
            }
        }

        /// <summary>
        /// This method attempts to perform the Physics2D.OverlapBoxNonAlloc and will double the size of the Collider buffer
        /// if the number of Colliders in the buffer after the call is equal to the length of the buffer.
        /// </summary>
        /// <param name="cellCenter"></param>
        /// <param name="halfCellScale"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        int BufferResizingOverlapBoxNonAlloc(Vector2 cellCenter, Vector2 halfCellScale)
        {
            int numFound;
            // Since we can only get a fixed number of results, requery
            // until we're sure we can hold them all (or until we hit the max size).
            while (true)
            {
                numFound = Physics2D.OverlapBoxNonAlloc(cellCenter, halfCellScale, 0, m_ColliderBuffer, m_ColliderMask);
                if (numFound == m_ColliderBuffer.Length && m_ColliderBuffer.Length < m_MaxColliderBufferSize)
                {
                    m_ColliderBuffer = new Collider2D[Math.Min(m_MaxColliderBufferSize, m_ColliderBuffer.Length * 2)];
                    m_InitialColliderBufferSize = m_ColliderBuffer.Length;
                }
                else
                {
                    break;
                }
            }
            return numFound;
        }

        /// <summary>
        /// Parses the array of colliders found within a cell. Finds the closest gameobject to the agent root reference within the cell
        /// </summary>
        void ParseCollidersClosest(Collider2D[] foundColliders, int numFound, int cellIndex, Vector2 cellCenter, Action<GameObject, int> detectedAction)
        {
            GameObject closestColliderGo = null;
            var minDistanceSquared = float.MaxValue;

            for (var i = 0; i < numFound; i++)
            {
                var currentColliderGo = foundColliders[i].gameObject;

                var closestColliderPoint = foundColliders[i].ClosestPoint(cellCenter);
                var currentDistanceSquared = (closestColliderPoint - m_AgentRigidbody.position).sqrMagnitude;

                if (currentDistanceSquared >= minDistanceSquared)
                {
                    continue;
                }

                // Checks if our colliders contain a detectable object
                var index = -1;
                for (var ii = 0; ii < m_DetectableTags.Length; ii++)
                {
                    if (currentColliderGo.CompareTag(m_DetectableTags[ii]))
                    {
                        index = ii;
                        break;
                    }
                }
                if (index > -1 && currentDistanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = currentDistanceSquared;
                    closestColliderGo = currentColliderGo;
                }
            }

            if (!ReferenceEquals(closestColliderGo, null))
            {
                detectedAction.Invoke(closestColliderGo, cellIndex);
            }
        }

        /// <summary>
        /// Parses all colliders in the array of colliders found within a cell.
        /// </summary>
        void ParseCollidersAll(Collider2D[] foundColliders, int numFound, int cellIndex, Vector2 cellCenter, Action<GameObject, int> detectedAction)
        {
            for (int i = 0; i < numFound; i++)
            {
                detectedAction.Invoke(foundColliders[i].gameObject, cellIndex);
            }
        }

        public void RegisterSensor(GridSensor2DBase sensor)
        {
            if (sensor.GetProcess2DCollidersMethod() == Process2DCollidersMethod.ProcessAllColliders)
            {
                GridOverlapDetectedAll += sensor.ProcessDetectedObject;
            }
            else
            {
                GridOverlapDetectedClosest += sensor.ProcessDetectedObject;
            }
        }

        public void RegisterDebugSensor(GridSensor2DBase debugSensor)
        {
            GridOverlapDetectedDebug += debugSensor.ProcessDetectedObject;
        }
    }
}
