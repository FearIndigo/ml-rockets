using Unity.MLAgents;
using UnityEngine;

namespace FearIndigo.Sensors
{
    [RequireComponent(typeof(Camera))]
    public class CameraSensorCamera : MonoBehaviour
    {
        [Header("Target")]
        public Rigidbody2D target;
        public Vector3 targetOffset;

        [Header("Sensor")]
        public CustomRenderTextureSensorComponent sensor;
        public float pixelWidth = 1.5f;
        
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.aspect = sensor.TextureSize.x / (float)sensor.TextureSize.y;
            _camera.orthographicSize = pixelWidth * (sensor.TextureSize.x / 2f);
            _camera.targetTexture = sensor.RenderTexture;

            transform.parent = null;
            transform.rotation = Quaternion.identity;

            Academy.Instance.AgentPreStep += AgentPreStep;
        }
        
        void OnDestroy()
        {
            if (Academy.IsInitialized)
            {
                Academy.Instance.AgentPreStep -= AgentPreStep;
            }
        }

        private void AgentPreStep(int i)
        {
            if (!target)
            {
                Destroy(gameObject);
                return;
            }
            
            transform.position = (Vector3)target.position + targetOffset;

            RenderCamera();
        }

        private void RenderCamera()
        {
            var prevRt = RenderTexture.active;
            RenderTexture.active = _camera.targetTexture;
            _camera.Render();
            RenderTexture.active = prevRt;
        }
    }
}
