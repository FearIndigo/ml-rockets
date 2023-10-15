using Unity.MLAgents.Sensors;
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
        public CameraSensorComponent sensor;
        public float pixelWidth = 2f;
        
        private Camera _camera;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            
            transform.parent = null;
            transform.rotation = Quaternion.identity;
        }

        private void Start()
        {
            _camera.aspect = sensor.Width / (float)sensor.Height;
            _camera.orthographicSize = pixelWidth * (sensor.Width / 2f);
        }

        private void FixedUpdate()
        {
            transform.position = (Vector3)target.position + targetOffset;
        }
    }
}
