using UnityEngine;
using UnityEngine.UI;

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

        [Header("Debug")]
        public RawImage rawImage;
        
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.aspect = sensor.TextureSize.x / (float)sensor.TextureSize.y;
            _camera.orthographicSize = pixelWidth * (sensor.TextureSize.x / 2f);
            _camera.targetTexture = sensor.RenderTexture;

            if(rawImage) rawImage.texture = sensor.RenderTexture;
            
            transform.parent = null;
            transform.rotation = Quaternion.identity;
            
            sensor.OnUpdate += OnSensorUpdate;
            sensor.OnDestroyed += OnSensorDestroyed;
        }

        private void OnSensorDestroyed()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (sensor)
            {
                sensor.OnUpdate -= OnSensorUpdate;
                sensor.OnDestroyed -= OnSensorDestroyed;
            }
        }

        private void OnSensorUpdate()
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
