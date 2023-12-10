using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class MainMenuUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public VisualElement container;

        private void OnEnable()
        {
            container = UIHelper.Create("main-menu", "flex-column");

            mainSceneManager.uiDocument.rootVisualElement.Add(
                container.Add(
                    UIHelper.Create<TextElement>("Rocket Man!", "title-text"),
                    UIHelper.Create<Button>("Single Player", mainSceneManager.SetSinglePlayerMode, "button"),
                    UIHelper.Create<Button>("AI Only", mainSceneManager.SetAiOnlyMode, "button"),
                    UIHelper.Create<Button>("Human Vs AI", mainSceneManager.SetHumanVsAiMode, "button"),
                    UIHelper.Create<Button>("Start", () => mainSceneManager.OpenPauseMenu(false), "press-start", "button")));
        }

        private void OnDisable()
        {
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
        }
    }
}