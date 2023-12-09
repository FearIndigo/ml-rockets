using FearIndigo.Managers;
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
        public Toggle muteMusicToggle;
        public bool musicEnabled = true;
        
        private void OnEnable()
        {
            container = UIHelper.Create("music-toggle");

            muteMusicToggle = UIHelper.Create<Toggle>();
            muteMusicToggle.label = "Enable Music";
            muteMusicToggle.RegisterValueChangedCallback(OnToggleChanged);
            muteMusicToggle.SetValueWithoutNotify(musicEnabled);

            container.Add(
                muteMusicToggle
            );
            
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
        }

        private void OnDisable()
        {
            muteMusicToggle.UnregisterValueChangedCallback(OnToggleChanged);
            
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
    }
}