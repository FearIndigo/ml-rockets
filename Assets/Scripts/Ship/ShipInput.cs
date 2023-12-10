using FearIndigo.Singleton;
using UnityEngine;

namespace FearIndigo.Ship
{
    public class ShipInput : SingletonGO<ShipInput>
    {
        public bool useTouchInput;

        public bool thrust;
        public bool left;
        public bool right;

        private void FixedUpdate()
        {
            if(useTouchInput) return;
            
            thrust = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            right = Input.GetKey(KeyCode.D);
        }
    }
}