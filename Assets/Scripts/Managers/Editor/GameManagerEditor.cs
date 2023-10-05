using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FearIndigo.Managers.Editor
{
    [CustomEditor(typeof(GameManager), true)]
    public class GameManagerEditor : UnityEditor.Editor
    {
        private Label _seedLabel;
        private GameManager _gameManager;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            _gameManager = serializedObject.targetObject as GameManager;
            if (_gameManager)
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
            if(!_gameManager) return;
            _gameManager.RandomizeSeed();
            UpdateSeedLabel();
        }
        
        private void OnGenerateTrack()
        {
            if(!_gameManager) return;
            _gameManager.GenerateTrack();
        }
        
        private void OnGenerateRandomTrack()
        {
            if(!_gameManager) return;
            _gameManager.GenerateRandomTrack();
            UpdateSeedLabel();
        }

        private void UpdateSeedLabel()
        {
            if(!_gameManager) return;
            _seedLabel.text = $"Seed: {_gameManager.trackConfig.data.seed}";
        }
    }
}