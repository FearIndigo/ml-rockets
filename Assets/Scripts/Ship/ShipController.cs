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
            UpdateInput();
        }
        
        private void FixedUpdate()
        {
            ApplyInput();
        }

        /// <summary>
        /// <para>
        /// Get input for ship.
        /// </para>
        /// </summary>
        private void UpdateInput()
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W))
            {
                input.up = true;
            }

            if (Input.GetKey(KeyCode.A))
            {
                input.left = true;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input.right = true;
            }
        }

        /// <summary>
        /// <para>
        /// Apply input by adding velocity to ship.
        /// </para>
        /// </summary>
        private void ApplyInput()
        {
            if (input.up)
            {
                _rb.velocity += (Vector2)(transform.rotation * Vector3.up * (linearThrust * Time.fixedDeltaTime));
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
            _rb.angularVelocity += torque * angularThrust * Time.fixedDeltaTime;
            
            input.Reset();
        }

        /// <summary>
        /// <para>
        /// Teleport ship to position.
        /// </para>
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(float2 position)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0);
            _rb.MovePosition(transform.position);
            trail.Clear();
        }
    }
}
