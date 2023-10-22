using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class RandomSeedUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public GameManager gameManager;
        public VisualElement container;
        public Toggle randomSeedToggle;
        public TextField randomSeedField;
        
        private void OnEnable()
        {
            container = UIHelper.Create("random-seed");

            randomSeedToggle = UIHelper.Create<Toggle>();
            randomSeedToggle.label = "Use Random Seed";
            randomSeedToggle.RegisterValueChangedCallback(OnToggleChanged);
            randomSeedToggle.SetValueWithoutNotify(gameManager.randomSeedOnReset);

            randomSeedField = UIHelper.Create<TextField>("text-field");
            randomSeedField.label = "Seed (uint)";
            randomSeedField.style.display = gameManager.randomSeedOnReset ? DisplayStyle.None : DisplayStyle.Flex;
            randomSeedField.RegisterValueChangedCallback(OnSeedChanged);
            randomSeedField.SetValueWithoutNotify($"{gameManager.trackManager.seed}");
            
            container.Add(
                randomSeedToggle,
                randomSeedField
            );
            
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
        }

        private void OnDisable()
        {
            randomSeedToggle.UnregisterValueChangedCallback(OnToggleChanged);
            randomSeedField.UnregisterValueChangedCallback(OnSeedChanged);
            
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            gameManager.randomSeedOnReset = evt.newValue;
            randomSeedField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            randomSeedField.SetValueWithoutNotify($"{gameManager.trackManager.seed}");
        }
        
        private void OnSeedChanged(ChangeEvent<string> evt)
        {
            if (!uint.TryParse(evt.newValue, out var seed))
            {
                var value = evt.previousValue;
                if (evt.newValue == "")
                {
                    value = "0";
                }
                randomSeedField.SetValueWithoutNotify(value);
                return;
            }
            
            gameManager.trackManager.seed = seed;
            gameManager.Reset();
        }
    }
}