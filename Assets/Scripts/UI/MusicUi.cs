using FearIndigo.Managers;
using FearIndigo.Ship;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class MusicUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public AudioSource musicAudioSource;
        public VisualElement container;
        public Toggle enableMusicToggle;
        public Toggle enableTouchControlsToggle;
        public bool musicEnabled = true;
        
        private void OnEnable()
        {
            container = UIHelper.Create("music-toggle");

            enableMusicToggle = UIHelper.Create<Toggle>();
            enableMusicToggle.label = "Enable Music";
            enableMusicToggle.RegisterValueChangedCallback(OnToggleChanged);
            enableMusicToggle.SetValueWithoutNotify(musicEnabled);

            enableTouchControlsToggle = UIHelper.Create<Toggle>();
            enableTouchControlsToggle.label = "Enable Touch Controls";
            enableTouchControlsToggle.RegisterValueChangedCallback(OnTouchToggleChanged);
            enableTouchControlsToggle.SetValueWithoutNotify(ShipInput.instance.useTouchInput);
            
            container.Add(
                enableMusicToggle,
                enableTouchControlsToggle
            );
            
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
        }

        private void OnDisable()
        {
            enableMusicToggle.UnregisterValueChangedCallback(OnToggleChanged);
            enableTouchControlsToggle.UnregisterValueChangedCallback(OnTouchToggleChanged);
            
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            musicEnabled = evt.newValue;
            if (musicEnabled)
            {
                musicAudioSource.Play();
            }
            else
            {
                musicAudioSource.Stop();
            }
        }
        
        private void OnTouchToggleChanged(ChangeEvent<bool> evt)
        {
            ShipInput.instance.useTouchInput = evt.newValue;
        }
    }
}