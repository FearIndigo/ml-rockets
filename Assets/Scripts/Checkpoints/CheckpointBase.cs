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

        public string activeCheckpointTag = "Checkpoint";
        public int checkpointID;
        public State state;
        public float checkpointReward = 0.5f;

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
            tag = state == State.Active ?
                activeCheckpointTag :
                "Untagged";
            
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
            shipController.AddReward(checkpointReward);
        }
    }
}