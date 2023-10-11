using FearIndigo.Managers;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class CheckpointPointer : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public float2 fadeDistance;
        public float maxAlpha;

        private Color _color;
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<GameManager>();
            _color = spriteRenderer.color;
        }

        private void LateUpdate()
        {
            var activeCheckpointDirection = _gameManager.ActiveCheckpointDirection(transform.position);
            transform.rotation = Quaternion.LookRotation(activeCheckpointDirection, Vector3.back);

            _color.a = math.clamp(math.remap(fadeDistance.x, fadeDistance.y, 0, maxAlpha, activeCheckpointDirection.magnitude), 0, maxAlpha);
            spriteRenderer.color = _color;
        }
    }
}