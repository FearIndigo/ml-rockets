using FearIndigo.Managers;
using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackMesh : MonoBehaviour
    {
        public float outOfBoundsPunishment = -1f;

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<GameManager>();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<ShipController>(out var ship))
            {
                ship.AddReward(outOfBoundsPunishment);
                _gameManager.shipManager.StopShip(ship);
            }
        }
    }
}
