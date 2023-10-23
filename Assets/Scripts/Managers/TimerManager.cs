using System;
using System.Collections.Generic;
using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Managers
{
    public class TimerManager : SubManager
    {
        public float timer;
        
        [HideInInspector] public Dictionary<ShipController, Dictionary<int, float>> shipCheckpointSplits = new Dictionary<ShipController, Dictionary<int, float>>();
        [HideInInspector] public Action OnCheckpointSplitsUpdated;
        
        public void Reset()
        {
            shipCheckpointSplits.Clear();
            foreach (var ship in GameManager.shipManager.ships)
            {
                var dictionary = new Dictionary<int, float>();
                foreach (var checkpoint in GameManager.checkpointManager.checkpoints)
                {
                    dictionary.Add(checkpoint.checkpointId, 0f);
                }
                shipCheckpointSplits.Add(ship, dictionary);
            }
            timer = 0;
            OnCheckpointSplitsUpdated?.Invoke();
        }
        
        public void FixedUpdate()
        {
            UpdateTimer();
        }

        ///<summary>
        /// <para>
        /// Increase time on timer.
        /// </para>
        /// </summary>
        private void UpdateTimer()
        {
            timer += Time.deltaTime;
        }
        
        /// <summary>
        /// <para>
        /// Set the timer split for the checkpointId.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="checkpointId"></param>
        public void UpdateCheckpointSplit(ShipController ship, int checkpointId)
        {
            if (!shipCheckpointSplits.TryGetValue(ship, out var checkpointSplits))
            {
                checkpointSplits = new Dictionary<int, float>();
                shipCheckpointSplits.Add(ship, checkpointSplits);
            }
            checkpointSplits[checkpointId] = GameManager.timerManager.timer;
            
            OnCheckpointSplitsUpdated?.Invoke();
        }
    }
}

