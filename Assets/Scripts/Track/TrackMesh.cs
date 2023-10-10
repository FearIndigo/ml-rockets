using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackMesh : MonoBehaviour
    {
        public float outOfBoundsReward = -1f;

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<ShipController>(out var shipController))
            {
                shipController.AddReward(outOfBoundsReward);
                shipController.EndEpisode();
            }
        }
    }
}
