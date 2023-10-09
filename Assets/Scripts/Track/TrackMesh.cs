using FearIndigo.Managers;
using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackMesh : MonoBehaviour
    {
        public GameManager gameManager;

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<ShipController>(out var shipController))
            {
                gameManager.Reset();
            }
        }
    }
}
