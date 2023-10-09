using FearIndigo.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Ship
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipController : MonoBehaviour
    {
        [Header("Physics")]
        public float linearThrust;
        public float angularThrust;

        [Header("Input")]
        public ShipInput input;

        [Header("Graphics")]
        public TrailRenderer trail;

        private Rigidbody2D _rb;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            input = new ShipInput()
            {
                up = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W),
                left = Input.GetKey(KeyCode.A),
                right = Input.GetKey(KeyCode.D)
            };
        }
        
        private void FixedUpdate()
        {
            if (input.up)
            {
                _rb.AddRelativeForce(Vector2.up * linearThrust);
            }

            var torque = 0f;
            if (input.left)
            {
                torque += 1f;
            }
            if (input.right)
            {
                torque += -1f;
            }
            
            _rb.AddTorque(torque * angularThrust);
        }

        public void Teleport(float2 position)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0);
            _rb.MovePosition(transform.position);
            trail.Clear();
        }
    }
}
