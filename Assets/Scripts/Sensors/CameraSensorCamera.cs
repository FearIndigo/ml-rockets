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
        
        private Camera _camera;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.aspect = sensor.Width / (float)sensor.Height;
            
            transform.parent = null;
            transform.rotation = Quaternion.identity;
        }

        private void FixedUpdate()
        {
            transform.position = (Vector3)target.position + targetOffset;
        }
    }
}
