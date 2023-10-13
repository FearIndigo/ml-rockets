using System;
using UnityEngine;

namespace FearIndigo.Utility
{
    [Serializable]
    public struct UnityLayer
    {
        public int layerIndex;

        public static implicit operator int(UnityLayer unityLayer) => unityLayer.layerIndex;
        public static implicit operator LayerMask(UnityLayer unityLayer) => (1 << unityLayer);
    }
}