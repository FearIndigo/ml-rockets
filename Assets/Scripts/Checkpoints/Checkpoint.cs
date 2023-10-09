using System;
using FearIndigo.Managers;
using FearIndigo.Ship;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class Checkpoint : MonoBehaviour
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
        public Color activeColor;
        public Color nextActiveColor;
        public Color inactiveColor;
        public Renderer checkpointRenderer;

        private GameManager _gameManager;

        public void Init(GameManager gameManager, int id, float2 position)
        {
            _gameManager = gameManager;
            checkpointID = id;
            transform.localPosition = new Vector3(position.x, position.y, 0);
            SetActive(State.Inactive);
        }

        public void SetActive(State newState)
        {
            state = newState;

            checkpointRenderer.material.color = state switch
            {
                State.Inactive => inactiveColor,
                State.NextActive => nextActiveColor,
                State.Active => activeColor,
            };
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(state != State.Active) return;

            if (other.gameObject.TryGetComponent<ShipController>(out var shipController))
            {
                _gameManager.SetActiveCheckpoint(checkpointID + 1);
            }
        }
    }
}