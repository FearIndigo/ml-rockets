using System;
using FearIndigo.Managers;
using FearIndigo.Ship;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class TouchInputUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        
        public VisualElement container;
        public TextElement thrustButton;
        public TextElement leftButton;
        public TextElement rightButton;

        private void OnEnable()
        {
            if (!ShipInput.instance.useTouchInput)
            {
                enabled = false;
                return;
            }
            
            container = UIHelper.Create("touch-input");
            container.pickingMode = PickingMode.Ignore;
            
            thrustButton = UIHelper.Create<TextElement>("Thrust", "thrust-input");
            leftButton = UIHelper.Create<TextElement>("Left", "turn-input");
            rightButton = UIHelper.Create<TextElement>("Right", "turn-input");
            var turnContainer = UIHelper.Create("turn-container").Add(
                leftButton,
                rightButton
            );
            
            RegisterCallbacks(thrustButton, UpdateThrust);
            RegisterCallbacks(leftButton, UpdateLeft);
            RegisterCallbacks(rightButton, UpdateRight);
            
            mainSceneManager.uiDocument.rootVisualElement.Add(
                container.Add(
                    thrustButton,
                    turnContainer
                ));
        }

        private void OnDisable()
        {
            if(container == null) return;

            UnregisterCallbacks(thrustButton, UpdateThrust);
            UnregisterCallbacks(leftButton, UpdateLeft);
            UnregisterCallbacks(rightButton, UpdateRight);
            
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
            container = null;
        }

        private void RegisterCallbacks(VisualElement element, Action<bool> callback)
        {
            element.RegisterCallback<PointerDownEvent>((evt) => callback(true));
            element.RegisterCallback<PointerUpEvent>((evt) => callback(false));
            element.RegisterCallback<PointerLeaveEvent>((evt) => callback(false));
        }
        
        private void UnregisterCallbacks(VisualElement element, Action<bool> callback)
        {
            element.UnregisterCallback<PointerDownEvent>((evt) => callback(true));
            element.UnregisterCallback<PointerUpEvent>((evt) => callback(false));
            element.UnregisterCallback<PointerLeaveEvent>((evt) => callback(false));
        }

        private void UpdateThrust(bool input)
        {
            ShipInput.instance.thrust = input;
        }
        
        private void UpdateLeft(bool input)
        {
            ShipInput.instance.left = input;
        }
        
        private void UpdateRight(bool input)
        {
            ShipInput.instance.right = input;
        }
    }
}