using System;
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
        
        public int checkpointID;
        public State state;

        protected GameManager GameManager;

        /// <summary>
        /// <para>
        /// Initialize checkpoint.
        /// </para>
        /// </summary>
        /// <param name="gameManager"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public void Init(GameManager gameManager, int id, float2 position)
        {
            GameManager = gameManager;
            checkpointID = id;
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
            if(state != State.Active) return;

            if (other.gameObject.TryGetComponent<ShipController>(out var shipController))
            {
                OnCheckpointAcquired(shipController);
            }
        }

        /// <summary>
        /// <para>
        /// Called when the checkpoint has been acquired by the ship.
        /// </para>
        /// </summary>
        /// <param name="shipController"></param>
        protected virtual void OnCheckpointAcquired(ShipController shipController)
        {
            GameManager.SetActiveCheckpoint(checkpointID + 1);
            GameManager.checkpointsAcquired++;
            GameManager.UpdateCheckpointSplit(checkpointID);
        }
    }
}