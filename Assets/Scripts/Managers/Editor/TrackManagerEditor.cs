using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FearIndigo.Managers.Editor
{
    [CustomEditor(typeof(TrackManager), true)]
    public class TrackManagerEditor : UnityEditor.Editor
    {
        private TrackManager _trackManager;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            _trackManager = serializedObject.targetObject as TrackManager;
            if (_trackManager)
            {
                container.Add(
                    UIHelper.Create<Button>("Randomize Seed", OnRandomizeSeed),
                    UIHelper.Create<Button>("Generate Track", OnGenerateTrack),
                    UIHelper.Create<Button>("Generate Random Track", OnGenerateRandomTrack)
                );
            }

            return container;
        }

        private void OnRandomizeSeed()
        {
            _trackManager?.RandomizeSeed();
        }
        
        private void OnGenerateTrack()
        {
            _trackManager?.GenerateTrack();
        }
        
        private void OnGenerateRandomTrack()
        {
            _trackManager?.GenerateRandomTrack();
        }
    }
}