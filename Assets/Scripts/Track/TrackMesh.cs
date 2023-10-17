using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackMesh : MonoBehaviour
    {
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<ShipController>(out var ship))
            {
                ship.Crashed();
            }
        }
    }
}
