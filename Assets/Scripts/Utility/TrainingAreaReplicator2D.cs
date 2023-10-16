using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;

namespace FearIndigo.Utility
{
    /// <summary>
    /// The Training Ares Replicator allows for a training area object group to be replicated dynamically during runtime.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public class TrainingAreaReplicator2D : MonoBehaviour
    {
        /// <summary>
        /// The base training area to be replicated.
        /// </summary>
        public GameObject baseArea;

        /// <summary>
        /// The number of training areas to replicate.
        /// </summary>
        public int numAreas = 1;

        /// <summary>
        /// The separation between each training area.
        /// </summary>
        public float2 separation = new float2(10f, 10f);

        /// <summary>
        /// Whether to replicate in the editor or in a build only. Default = true
        /// </summary>
        public bool buildOnly = true;

        int2 m_GridSize = new(1, 1);
        int m_AreaCount;
        string m_TrainingAreaName;

        /// <summary>
        /// The size of the computed grid to pack the training areas into.
        /// </summary>
        public int2 GridSize => m_GridSize;

        /// <summary>
        /// The name of the training area.
        /// </summary>
        public string TrainingAreaName => m_TrainingAreaName;

        /// <summary>
        /// Called before the simulation begins to computed the grid size for distributing
        /// the replicated training areas and set the area name.
        /// </summary>
        public void Awake()
        {
            // Computes the Grid Size on Awake
            ComputeGridSize();
            // Sets the TrainingArea name to the name of the base area.
            m_TrainingAreaName = baseArea.name;
        }

        /// <summary>
        /// Called after Awake and before the simulation begins and adds the training areas before
        /// the Academy begins.
        /// </summary>
        public void OnEnable()
        {
            // Adds the training as replicas during OnEnable to ensure they are added before the Academy begins its work.
            if (buildOnly)
            {
#if UNITY_STANDALONE && !UNITY_EDITOR
                AddEnvironments();
#endif
                return;
            }
            AddEnvironments();
        }

        /// <summary>
        /// Computes the Grid Size for replicating the training area.
        /// </summary>
        void ComputeGridSize()
        {
            // check if running inference, if so, use the num areas set through the component,
            // otherwise, pull it from the academy
            if (Academy.Instance.IsCommunicatorOn)
                numAreas = Academy.Instance.NumAreas;

            var rootNumAreas = Mathf.Sqrt(numAreas);
            m_GridSize.x = Mathf.CeilToInt(rootNumAreas);
            m_GridSize.y = Mathf.CeilToInt(rootNumAreas);
        }

        /// <summary>
        /// Adds replicas of the training area to the scene.
        /// </summary>
        /// <exception cref="UnityAgentsException"></exception>
        void AddEnvironments()
        {
            for (int y = 0; y < m_GridSize.y; y++)
            {
                for (int x = 0; x < m_GridSize.x; x++)
                {
                    if (m_AreaCount >= numAreas) break;
                    
                    if (m_AreaCount == 0)
                    {
                        // Skip this first area since it already exists.
                        m_AreaCount = 1;
                    }
                    else if (m_AreaCount < numAreas)
                    {
                        m_AreaCount++;
                        var area = Instantiate(baseArea, new Vector3(x * separation.x, y * separation.y, 0),
                            Quaternion.identity);
                        area.name = m_TrainingAreaName;
                    }
                }
            }
        }
    }
}
