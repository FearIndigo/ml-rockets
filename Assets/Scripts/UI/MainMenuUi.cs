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
        public TextElement pressStartText;

        private void OnEnable()
        {
            container = UIHelper.Create("main-menu", "flex-column");
            pressStartText = UIHelper.Create<TextElement>("Press Start", "press-start");

            mainSceneManager.uiDocument.rootVisualElement.Add(
                container.Add(
                    UIHelper.Create<TextElement>("Rocket Man!", "title-text"),
                    UIHelper.Create<Button>("Single Player", mainSceneManager.SetSinglePlayerMode, "button"),
                    UIHelper.Create<Button>("AI Only", mainSceneManager.SetAiOnlyMode, "button"),
                    UIHelper.Create<Button>("Human Vs AI", mainSceneManager.SetHumanVsAiMode, "button"),
                    pressStartText));
            
            pressStartText.RegisterCallback<TransitionEndEvent>(evt => pressStartText.ToggleInClassList("scale-up"));
            mainSceneManager.uiDocument.rootVisualElement.schedule.Execute(() => pressStartText.ToggleInClassList("scale-up")).StartingIn(100);
        }

        private void OnDisable()
        {
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
        }
    }
}