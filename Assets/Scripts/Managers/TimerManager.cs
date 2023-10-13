using UnityEngine;

namespace FearIndigo.Managers
{
    public class TimerManager : SubManager
    {
        public bool timerPaused;
        public float timer;

        public void Reset()
        {
            timerPaused = false;
            timer = 0;
        }
        
        public void FixedUpdate()
        {
            UpdateTimer();
        }

        ///<summary>
        /// <para>
        /// Increase time on timer if not paused.
        /// </para>
        /// </summary>
        private void UpdateTimer()
        {
            if(timerPaused) return;
            timer += Time.deltaTime;
        }
    }
}

