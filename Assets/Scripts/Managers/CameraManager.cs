using Cinemachine;

namespace FearIndigo.Managers
{
    public class CameraManager : SubManager
    {
        public CinemachineVirtualCamera virtualCamera;

        /// <summary>
        /// <para>
        /// Set the camera target.
        /// </para>
        /// </summary>
        public void UpdateCameraTarget()
        {
            if(!virtualCamera) return;
            virtualCamera.m_Follow = GameManager.shipManager.ships[0].transform;
        }
    }
}

