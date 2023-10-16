using UnityEditor;
using UnityEngine;

namespace FearIndigo.Sensors.Editor
{
    [CustomEditor(typeof(GridSensor2DComponent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    internal class GridSensor2DComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            // Drawing the GridSensorComponent
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                // These fields affect the sensor order or observation size,
                // So can't be changed at runtime.
                EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_SensorName)), true);

                EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_CellScale)), true);
                // We only supports 2D GridSensor now so lock gridSize.y to 1
                var gridSize = so.FindProperty(nameof(GridSensor2DComponent.m_GridSize));
                var newGridSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize.vector2IntValue);
                gridSize.vector2IntValue = new Vector2Int(newGridSize.x, newGridSize.y);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_AgentRigidbody)), true);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                // detectable tags
                var detectableTags = so.FindProperty(nameof(GridSensor2DComponent.m_DetectableTags));
                var newSize = EditorGUILayout.IntField("Detectable Tags", detectableTags.arraySize);
                if (newSize != detectableTags.arraySize)
                {
                    detectableTags.arraySize = newSize;
                }
                EditorGUI.indentLevel++;
                for (var i = 0; i < detectableTags.arraySize; i++)
                {
                    var objectTag = detectableTags.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(objectTag, new GUIContent("Tag " + i), true);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_ColliderMask)), true);
            EditorGUILayout.LabelField("Sensor Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_ObservationStacks)), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_CompressionType)), true);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.LabelField("Collider and Buffer", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_InitialColliderBufferSize)), true);
                EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_MaxColliderBufferSize)), true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("Debug Gizmo", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(GridSensor2DComponent.m_ShowGizmos)), true);

            // detectable objects
            var debugColors = so.FindProperty(nameof(GridSensor2DComponent.m_DebugColors));
            var detectableObjectSize = so.FindProperty(nameof(GridSensor2DComponent.m_DetectableTags)).arraySize;
            if (detectableObjectSize != debugColors.arraySize)
            {
                debugColors.arraySize = detectableObjectSize;
            }
            EditorGUILayout.LabelField("Debug Colors");
            EditorGUI.indentLevel++;
            for (var i = 0; i < debugColors.arraySize; i++)
            {
                var debugColor = debugColors.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(debugColor, new GUIContent("Tag " + i + " Color"), true);
            }
            EditorGUI.indentLevel--;

            var requireSensorUpdate = EditorGUI.EndChangeCheck();
            so.ApplyModifiedProperties();

            if (requireSensorUpdate)
            {
                UpdateSensor();
            }
        }

        void UpdateSensor()
        {
            var sensorComponent = serializedObject.targetObject as GridSensor2DComponent;
            sensorComponent?.UpdateSensor();
        }
    }
}
