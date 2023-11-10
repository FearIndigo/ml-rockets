using FearIndigo.Utility;
using UnityEngine;

namespace FearIndigo.Ship
{
    public class ShipCollider : MonoBehaviour
    {
        public ShipController ship;
        public UnityLayer trackLayer;
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == trackLayer)
            {
                ship.Crashed();
            }
        }
    }
}