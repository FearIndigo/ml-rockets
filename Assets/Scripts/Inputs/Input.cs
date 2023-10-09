using System;

namespace FearIndigo.Inputs
{
    [Serializable]
    public struct ShipInput
    {
        public bool up;
        public bool left;
        public bool right;

        public void Reset()
        {
            up = false;
            left = false;
            right = false;
        }
    }
}