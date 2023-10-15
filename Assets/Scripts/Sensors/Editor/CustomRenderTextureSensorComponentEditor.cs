using UnityEditor;
using UnityEngine;

namespace FearIndigo.Sensors.Editor
{
    [CustomEditor(typeof(CustomRenderTextureSensorComponent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    internal class CustomRenderTextureSensorComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            // Drawing the RenderTextureComponent
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.PropertyField(so.FindProperty("m_TextureSize"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_SensorName"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_Grayscale"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_ObservationStacks"), true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(so.FindProperty("m_Compression"), true);

            var requireSensorUpdate = EditorGUI.EndChangeCheck();
            so.ApplyModifiedProperties();

            if (requireSensorUpdate)
            {
                UpdateSensor();
            }
        }

        void UpdateSensor()
        {
            var sensorComponent = serializedObject.targetObject as CustomRenderTextureSensorComponent;
            sensorComponent?.UpdateSensor();
        }
    }
}