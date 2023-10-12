using System;

namespace FearIndigo.Utility
{
    [Serializable]
    public struct UnityLayer
    {
        public int layerIndex;

        public static implicit operator int(UnityLayer unityLayer) => unityLayer.layerIndex;
    }
}