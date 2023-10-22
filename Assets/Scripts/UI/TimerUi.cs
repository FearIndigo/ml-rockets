using System;
using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class TimerUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public GameManager gameManager;
        public TextElement timerLabel;

        private void OnEnable()
        {
            timerLabel = UIHelper.Create<TextElement>($"{gameManager.timerManager.timer:F2}", "timer-text");
            mainSceneManager.uiDocument.rootVisualElement.Add(timerLabel);
        }

        private void OnDisable()
        {
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(timerLabel);
        }
        
        private void LateUpdate()
        {
            timerLabel.text = $"{gameManager.timerManager.timer:F2}";
        }
    }
}