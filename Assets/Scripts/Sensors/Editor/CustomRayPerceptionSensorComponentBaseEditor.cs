using UnityEngine;
using UnityEditor;

namespace FearIndigo.Sensors.Editor
{
    internal class CustomRayPerceptionSensorComponentBaseEditor : UnityEditor.Editor
    {
        bool m_RequireSensorUpdate;

        protected void OnRayPerceptionInspectorGUI(bool is3d)
        {
            var so = serializedObject;
            so.Update();

            // Drawing the RayPerceptionSensorComponent
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel++;

            // Don't allow certain fields to be modified during play mode.
            // * SensorName affects the ordering of the Agent's observations
            // * The number of rays affects the size of the observations.
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.PropertyField(so.FindProperty("m_SensorName"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_RaysPerDirection"), true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(so.FindProperty("m_InverseRaycast"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_MaxRayDegrees"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_SphereCastRadius"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_RayLength"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_RayLayerMask"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_LayerMaskTest"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_AgentRigidbody"), true);
            EditorGUILayout.PropertyField(so.FindProperty("m_DetectableObjects"), true);

            // Because the number of observation stacks affects the observation shape,
            // it is not editable during play mode.
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.PropertyField(so.FindProperty("m_ObservationStacks"), new GUIContent("Stacked Raycasts"), true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(so.FindProperty("rayHitColor"), true);
            EditorGUILayout.PropertyField(so.FindProperty("rayMissColor"), true);

            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                m_RequireSensorUpdate = true;
            }

            so.ApplyModifiedProperties();
            UpdateSensorIfDirty();
        }

        void UpdateSensorIfDirty()
        {
            if (m_RequireSensorUpdate)
            {
                var sensorComponent = serializedObject.targetObject as CustomRayPerceptionSensor2DComponent;
                sensorComponent?.UpdateSensor();
                m_RequireSensorUpdate = false;
            }
        }
    }

    [CustomEditor(typeof(CustomRayPerceptionSensor2DComponent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    internal class CustomRayPerceptionSensorComponent2DEditor : CustomRayPerceptionSensorComponentBaseEditor
    {
        public override void OnInspectorGUI()
        {
            OnRayPerceptionInspectorGUI(false);
        }
    }
}
