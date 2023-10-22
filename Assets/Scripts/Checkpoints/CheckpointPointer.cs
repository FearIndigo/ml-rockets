using FearIndigo.Managers;
using FearIndigo.Ship;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class CheckpointPointer : MonoBehaviour
    {
        public bool hideIfActiveIsFinishLine;
        public int activeCheckpointOffset;
        public SpriteRenderer spriteRenderer;
        public float2 fadeDistance;
        public float maxAlpha;

        private Color _color;
        private ShipController _ship;
        private GameManager _gameManager;

        private void Start()
        {
            _ship = GetComponentInParent<ShipController>();
            _gameManager = GetComponentInParent<GameManager>();
            _color = spriteRenderer.color;
        }

        private void LateUpdate()
        {
            if (_ship != _gameManager.shipManager.MainShip || hideIfActiveIsFinishLine && _gameManager.checkpointManager.GetCheckpoint(_ship, 0) is FinishLine)
            {
                _color.a = 0f;
            }
            else
            {
                var checkpointDirection = _gameManager.checkpointManager.GetCheckpointDirection(_ship, activeCheckpointOffset);
                transform.rotation = Quaternion.LookRotation(checkpointDirection, Vector3.back);
                _color.a = math.clamp(math.remap(fadeDistance.x, fadeDistance.y, 0, maxAlpha, checkpointDirection.magnitude), 0, maxAlpha);
            }
            spriteRenderer.color = _color;
        }
    }
}