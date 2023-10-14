using FearIndigo.Managers;
using FearIndigo.Ship;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class CheckpointPointer : MonoBehaviour
    {
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

            if (_gameManager.shipManager.ships[0] != _ship)
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            var checkpointDirection = _gameManager.checkpointManager.GetCheckpointDirection(_ship, activeCheckpointOffset);
            transform.rotation = Quaternion.LookRotation(checkpointDirection, Vector3.back);
            _color.a = math.clamp(math.remap(fadeDistance.x, fadeDistance.y, 0, maxAlpha, checkpointDirection.magnitude), 0, maxAlpha);
            spriteRenderer.color = _color;
        }
    }
}