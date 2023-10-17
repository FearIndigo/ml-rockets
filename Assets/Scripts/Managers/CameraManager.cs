using Cinemachine;
using UnityEngine;

namespace FearIndigo.Managers
{
    public class CameraManager : SubManager
    {
        public CinemachineVirtualCamera virtualCamera;

        public Transform CurrentTarget => virtualCamera?.m_Follow;
        
        /// <summary>
        /// <para>
        /// Set the camera target.
        /// </para>
        /// </summary>
        public void SetCameraTarget(Transform targetTransform)
        {
            if(!virtualCamera || !targetTransform) return;
            virtualCamera.m_Follow = targetTransform;
        }
    }
}

