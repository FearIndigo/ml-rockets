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
        
        public int checkpointId;
        public State state;
        public float width;
        public float rotation;
        public float checkpointReward = 1f;

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
        /// <param name="checkpointWidth"></param>
        /// <param name="checkpointDirection"></param>
        public void Init(int id, float2 position, float checkpointWidth, float2 checkpointDirection)
        {
            checkpointId = id;
            width = checkpointWidth;
            rotation = (Mathf.Atan2(checkpointDirection.y, checkpointDirection.x) * Mathf.Rad2Deg + 270f) % 360f;
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
            GameManager.checkpointManager.UpdateCheckpointSplit(ship, checkpointId);
            ship.AddReward(checkpointReward);
        }
    }
}