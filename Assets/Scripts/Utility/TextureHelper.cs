using UnityEngine;

namespace FearIndigo.Utility
{
    public static class TextureHelper
    {
        /// <summary>
        /// Safely destroy a texture. This has to be used differently in unit tests.
        /// </summary>
        /// <param name="texture"></param>
        public static void DestroyTexture(Texture2D texture)
        {
            if (Application.isEditor)
            {
                // Edit Mode tests complain if we use Destroy()
                Object.DestroyImmediate(texture);
            }
            else
            {
                Object.Destroy(texture);
            }
        }
    }
}