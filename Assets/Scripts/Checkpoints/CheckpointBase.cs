using System;
using FearIndigo.Audio;
using FearIndigo.Managers;
using FearIndigo.Ship;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public abstract class CheckpointBase : MonoBehaviour
    {
        [Serializable]
        public enum State
        {
            Inactive,
            NextActive,
            Active
        }
        
        public int checkpointId;
        public State state;
        public AudioEvent acquiredAudioEvent;

        protected GameManager GameManager;

        private void Awake()
        {
            GameManager = GetComponentInParent<GameManager>();
        }

        /// <summary>
        /// <para>
        /// Initialize checkpoint.
        /// </para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public void Init(int id, float2 position)
        {
            checkpointId = id;
            transform.localPosition = new Vector3(position.x, position.y, 0);
            SetState(State.Inactive);
        }

        /// <summary>
        /// <para>
        /// Set the current checkpoint state.
        /// </para>
        /// </summary>
        /// <param name="newState"></param>
        public void SetState(State newState)
        {
            state = newState;

            OnStateChanged();
        }

        /// <summary>
        /// <para>
        /// Called when the checkpoint state is changed.
        /// </para>
        /// </summary>
        protected virtual void OnStateChanged() {}

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<ShipController>(out var ship) &&
                GameManager.checkpointManager.GetActiveCheckpointId(ship) == checkpointId)
            {
                OnCheckpointAcquired(ship);
            }
        }

        /// <summary>
        /// <para>
        /// Called when the checkpoint has been acquired by a ship.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        protected virtual void OnCheckpointAcquired(ShipController ship)
        {
            GameManager.checkpointManager.SetActiveCheckpoint(ship, checkpointId + 1);
            GameManager.timerManager.UpdateCheckpointSplit(ship, checkpointId);
            ship.CheckpointAcquired();

            acquiredAudioEvent.Play(ship.transform.position);
        }
    }
}