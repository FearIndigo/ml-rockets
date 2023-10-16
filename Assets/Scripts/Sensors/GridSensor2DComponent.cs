using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// A SensorComponent that creates a <see cref="GridSensor2DBase"/>.
    /// </summary>
    public class GridSensor2DComponent : SensorComponent
    {
        // dummy sensor only used for debug gizmo
        GridSensor2DBase m_DebugSensor;
        List<GridSensor2DBase> m_Sensors;
        internal IGridPerception2D m_GridPerception;

        [HideInInspector, SerializeField] public string m_SensorName = "GridSensor2D";
        /// <summary>
        /// Name of the generated <see cref="GridSensor2DBase"/> object.
        /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
        /// </summary>
        public string SensorName
        {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }

        [HideInInspector, SerializeField] public Vector2 m_CellScale = new Vector2(1f, 1f);

        /// <summary>
        /// The scale of each grid cell.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public Vector2 CellScale
        {
            get { return m_CellScale; }
            set { m_CellScale = value; }
        }

        [HideInInspector, SerializeField] public Vector2Int m_GridSize = new Vector2Int(16, 16);
        /// <summary>
        /// The number of grid on each side.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public Vector2Int GridSize
        {
            get { return m_GridSize; }
            set { m_GridSize = value; }
        }

        [HideInInspector, SerializeField] public Rigidbody2D m_AgentRigidbody;
        /// <summary>
        /// The reference of the root of the agent. This is used to disambiguate objects with
        /// the same tag as the agent. Defaults to current GameObject.
        /// </summary>
        public Rigidbody2D AgentRigidbody
        {
            get { return m_AgentRigidbody; }
            set { m_AgentRigidbody = value; }
        }

        [HideInInspector, SerializeField] public string[] m_DetectableTags;
        /// <summary>
        /// List of tags that are detected.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public string[] DetectableTags
        {
            get { return m_DetectableTags; }
            set { m_DetectableTags = value; }
        }

        [HideInInspector, SerializeField] public LayerMask m_ColliderMask;
        /// <summary>
        /// The layer mask.
        /// </summary>
        public LayerMask ColliderMask
        {
            get { return m_ColliderMask; }
            set { m_ColliderMask = value; }
        }

        [HideInInspector, SerializeField] public int m_MaxColliderBufferSize = 500;
        /// <summary>
        /// The absolute max size of the Collider buffer used in the non-allocating Physics calls.  In other words
        /// the Collider buffer will never grow beyond this number even if there are more Colliders in the Grid Cell.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int MaxColliderBufferSize
        {
            get { return m_MaxColliderBufferSize; }
            set { m_MaxColliderBufferSize = value; }
        }

        [HideInInspector, SerializeField] public int m_InitialColliderBufferSize = 4;
        /// <summary>
        /// The Estimated Max Number of Colliders to expect per cell.  This number is used to
        /// pre-allocate an array of Colliders in order to take advantage of the OverlapBoxNonAlloc
        /// Physics API.  If the number of colliders found is >= InitialColliderBufferSize the array
        /// will be resized to double its current size.  The hard coded absolute size is 500.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int InitialColliderBufferSize
        {
            get { return m_InitialColliderBufferSize; }
            set { m_InitialColliderBufferSize = value; }
        }

        [HideInInspector, SerializeField] public Color[] m_DebugColors;
        /// <summary>
        /// Array of Colors used for the grid gizmos.
        /// </summary>
        public Color[] DebugColors
        {
            get { return m_DebugColors; }
            set { m_DebugColors = value; }
        }

        [HideInInspector, SerializeField] public bool m_ShowGizmos = false;
        /// <summary>
        /// Whether to show gizmos or not.
        /// </summary>
        public bool ShowGizmos
        {
            get { return m_ShowGizmos; }
            set { m_ShowGizmos = value; }
        }

        [HideInInspector, SerializeField] public SensorCompressionType m_CompressionType = SensorCompressionType.PNG;
        /// <summary>
        /// The compression type to use for the sensor.
        /// </summary>
        public SensorCompressionType CompressionType
        {
            get { return m_CompressionType; }
            set { m_CompressionType = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField]
        [Range(1, 50)]
        [Tooltip("Number of frames of observations that will be stacked before being fed to the neural network.")]
        public int m_ObservationStacks = 1;
        /// <summary>
        /// Whether to stack previous observations. Using 1 means no previous observations.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int ObservationStacks
        {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }

        /// <inheritdoc/>
        public override ISensor[] CreateSensors()
        {
            m_GridPerception = new BoxOverlap2DChecker(
                m_CellScale,
                m_GridSize,
                m_ColliderMask,
                AgentRigidbody,
                m_DetectableTags,
                m_InitialColliderBufferSize,
                m_MaxColliderBufferSize
            );

            // debug data is positive int value and will trigger data validation exception if SensorCompressionType is not None.
            m_DebugSensor = new GridSensor2DBase("DebugGridSensor2D", m_CellScale, m_GridSize, m_DetectableTags, SensorCompressionType.None);
            m_GridPerception.RegisterDebugSensor(m_DebugSensor);

            m_Sensors = GetGridSensors().ToList();
            if (m_Sensors == null || m_Sensors.Count < 1)
            {
                throw new UnityAgentsException("GridSensorComponent received no sensors. Specify at least one observation type (OneHot/Counting) to use grid sensors." +
                    "If you're overriding GridSensorComponent.GetGridSensors(), return at least one grid sensor.");
            }

            // Only one sensor needs to reference the boxOverlapChecker, so that it gets updated exactly once
            m_Sensors[0].m_GridPerception = m_GridPerception;
            foreach (var sensor in m_Sensors)
            {
                m_GridPerception.RegisterSensor(sensor);
            }

            if (ObservationStacks != 1)
            {
                var sensors = new ISensor[m_Sensors.Count];
                for (var i = 0; i < m_Sensors.Count; i++)
                {
                    sensors[i] = new StackingSensor(m_Sensors[i], ObservationStacks);
                }
                return sensors;
            }
            else
            {
                return m_Sensors.ToArray();
            }
        }

        /// <summary>
        /// Get an array of GridSensors to be added in this component.
        /// Override this method and return custom GridSensor implementations.
        /// </summary>
        /// <returns>Array of grid sensors to be added to the component.</returns>
        protected virtual GridSensor2DBase[] GetGridSensors()
        {
            List<GridSensor2DBase> sensorList = new List<GridSensor2DBase>();
            var sensor = new OneHotGridSensor2D(m_SensorName + "-OneHot", m_CellScale, m_GridSize, m_DetectableTags, m_CompressionType);
            sensorList.Add(sensor);
            return sensorList.ToArray();
        }

        /// <summary>
        /// Update fields that are safe to change on the Sensor at runtime.
        /// </summary>
        public void UpdateSensor()
        {
            if (m_Sensors != null)
            {
                m_GridPerception.ColliderMask = m_ColliderMask;
                foreach (var sensor in m_Sensors)
                {
                    sensor.CompressionType = m_CompressionType;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (m_ShowGizmos)
            {
                if (m_GridPerception == null || m_DebugSensor == null)
                {
                    return;
                }

                m_DebugSensor.ResetPerceptionBuffer();
                m_GridPerception.UpdateGizmo();
                var cellColors = m_DebugSensor.PerceptionBuffer;

                var scale = new Vector3(m_CellScale.x, m_CellScale.y, 1);
                var oldGizmoMatrix = Gizmos.matrix;
                for (var i = 0; i < m_DebugSensor.PerceptionBuffer.Length; i++)
                {
                    var cellPosition = m_GridPerception.GetCellGlobalPosition(i);
                    var cubeTransform = Matrix4x4.TRS(cellPosition, Quaternion.identity, scale);
                    Gizmos.matrix = oldGizmoMatrix * cubeTransform;
                    var colorIndex = cellColors[i] - 1;
                    var debugRayColor = Color.white;
                    if (colorIndex > -1 && m_DebugColors.Length > colorIndex)
                    {
                        debugRayColor = m_DebugColors[(int)colorIndex];
                    }
                    Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                }

                Gizmos.matrix = oldGizmoMatrix;
            }
        }
    }
}
