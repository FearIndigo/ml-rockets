using UnityEngine;

namespace FearIndigo.Utility
{
    public class MaintainRotation : MonoBehaviour
    {
        public Vector3 targetRotation;
        private Quaternion _rotation;

        private void Awake()
        {
            _rotation = Quaternion.Euler(targetRotation);
        }

        private void FixedUpdate()
        {
            transform.rotation = _rotation;
        }
    }
}