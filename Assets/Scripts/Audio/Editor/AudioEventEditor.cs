using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.Audio.Editor
{
    [CustomEditor(typeof(AudioEvent), true)]
    public class AudioEventEditor : UnityEditor.Editor
    {
        private AudioSource _previewer;

        private void OnEnable()
        {
            _previewer = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        public void OnDisable()
        {
            DestroyImmediate(_previewer.gameObject);
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);
            
            container.Add(UIHelper.Create<Button>("Play", OnPlay));

            return container;
        }

        private void OnPlay()
        {
            (target as AudioEvent)?.Play(_previewer, Vector3.zero);
        }
    }
}