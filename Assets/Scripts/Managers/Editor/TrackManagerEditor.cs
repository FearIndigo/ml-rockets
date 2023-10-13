using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FearIndigo.Managers.Editor
{
    [CustomEditor(typeof(TrackManager), true)]
    public class TrackManagerEditor : UnityEditor.Editor
    {
        private Label _seedLabel;
        private TrackManager _trackManager;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            _trackManager = serializedObject.targetObject as TrackManager;
            if (_trackManager)
            {
                _seedLabel = UIHelper.Create<Label>();
                UpdateSeedLabel();
                container.Add(
                    _seedLabel,
                    UIHelper.Create<Button>("Randomize Seed", OnRandomizeSeed),
                    UIHelper.Create<Button>("Generate Track", OnGenerateTrack),
                    UIHelper.Create<Button>("Generate Random Track", OnGenerateRandomTrack)
                );
            }

            return container;
        }

        private void OnRandomizeSeed()
        {
            if(!_trackManager) return;
            _trackManager.RandomizeSeed();
            UpdateSeedLabel();
        }
        
        private void OnGenerateTrack()
        {
            if(!_trackManager) return;
            _trackManager.GenerateTrack();
        }
        
        private void OnGenerateRandomTrack()
        {
            if(!_trackManager) return;
            _trackManager.GenerateRandomTrack();
            UpdateSeedLabel();
        }

        private void UpdateSeedLabel()
        {
            if(!_trackManager || !_trackManager.trackConfig) return;
            _seedLabel.text = $"Seed: {_trackManager.trackConfig.data.seed}";
        }
    }
}