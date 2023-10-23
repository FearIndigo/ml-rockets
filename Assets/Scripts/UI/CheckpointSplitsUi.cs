using FearIndigo.Managers;
using FearIndigo.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.UI
{
    public class CheckpointSplitsUi : MonoBehaviour
    {
        public MainSceneManager mainSceneManager;
        public GameManager gameManager;
        public VisualElement container;
        
        private void OnEnable()
        {
            container = UIHelper.Create("checkpoint-splits", "flex-row");
            mainSceneManager.uiDocument.rootVisualElement.Add(container);
            
            gameManager.timerManager.OnCheckpointSplitsUpdated += UpdateCheckpointSplits;

            UpdateCheckpointSplits();
        }

        private void OnDisable()
        {
            mainSceneManager?.uiDocument?.rootVisualElement?.Remove(container);
            
            gameManager.timerManager.OnCheckpointSplitsUpdated -= UpdateCheckpointSplits;
        }

        public void UpdateCheckpointSplits()
        {
            container.Clear();
            var shipCheckpointSplits = gameManager.timerManager.shipCheckpointSplits;

            foreach (var checkpointSplits in shipCheckpointSplits.Values)
            {
                var splitsContainer = UIHelper.Create("flex-column", "margin-right");

                foreach (var checkpointSplit in checkpointSplits)
                {
                    splitsContainer.Add(
                        UIHelper.Create("flex-row", "margin-bottom-small").Add(
                                UIHelper.Create<TextElement>($"{checkpointSplit.Key + 1}:", "splits-key"),
                                UIHelper.Create<TextElement>(checkpointSplit.Value == 0f ? "" : $"{checkpointSplit.Value:F2}", "splits-value")));
                }
                
                container.Add(splitsContainer);
            }
        }
    }
}