using System;
using UnityEngine;

namespace FearIndigo.Utility
{
    [Serializable]
    public struct UnityLayer
    {
        public int layerIndex;

        public UnityLayer(int layerIndex)
        {
            this.layerIndex = layerIndex;
        }
        
        public int LayerMask => 1 << layerIndex;

        public static implicit operator int(UnityLayer unityLayer) => unityLayer.layerIndex;
        public static implicit operator LayerMask(UnityLayer unityLayer) => unityLayer.LayerMask;
    }
}