using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.Managers.Editor
{
    [CustomEditor(typeof(GameManager), true)]
    public class GameManagerEditor : UnityEditor.Editor
    {
        private GameManager _gameManager;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            _gameManager = serializedObject.targetObject as GameManager;
            if (_gameManager)
            {
                container.Add(
                    UIHelper.Create<Button>("Reset", OnReset)
                );
            }

            return container;
        }

        private void OnReset()
        {
            // Can only reset when game is playing.
            if(!Application.isPlaying) return;
            if(!_gameManager) return;
            _gameManager.Reset();
        }
    }
}