using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class OpenMenuUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public VisualElement container;

        private void OnEnable()
        {
            container = UIHelper.Create("open-menu");
            container.Add(
                UIHelper.Create<Button>("Menu", () => mainSceneManager.OpenPauseMenu(true), "button"),
                UIHelper.Create<Button>("Reset", mainSceneManager.ResetCurrentTrack, "button"));
            
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
        }

        private void OnDisable()
        {
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
        }
    }
}