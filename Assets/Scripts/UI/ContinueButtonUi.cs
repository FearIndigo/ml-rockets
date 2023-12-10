using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class ContinueButtonUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public VisualElement container;

        private void OnEnable()
        {
            if (!mainSceneManager.roundOver)
            {
                enabled = false;
                return;
            }

            container = UIHelper.Create("continue-button");
            container.Add(UIHelper.Create<Button>("Continue", mainSceneManager.Continue, "button"));
            
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
        }

        private void OnDisable()
        {
            if(container == null) return;
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
            container = null;
        }
    }
}