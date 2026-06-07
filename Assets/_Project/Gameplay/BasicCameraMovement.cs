using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Gameplay
{
    /// <summary>
    /// Basic free-fly camera movement using the new Unity Input System
    /// </summary>
    public class BasicCameraMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float acceleration = 50f;
        public float accSprintMultiplier = 4f;
        public float dampingCoefficient = 5f;

        [Header("Look")]
        public float lookSensitivity = 0.1f;

        [Header("Cursor")]
        public bool focusOnEnable = true;

        private Vector3 _velocity;

        static bool Focused
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                Cursor.lockState = value
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;

                Cursor.visible = !value;
            }
        }

        void OnEnable()
        {
            if (focusOnEnable)
                Focused = true;
        }

        void OnDisable()
        {
            Focused = false;
        }

        void Update()
        {
            // Focus handling
            if (Focused)
                UpdateInput();
            else if (Mouse.current.leftButton.wasPressedThisFrame)
                Focused = true;

            // Damping
            _velocity = Vector3.Lerp(
                _velocity,
                Vector3.zero,
                dampingCoefficient * Time.deltaTime
            );

            // Apply movement
            transform.position += _velocity * Time.deltaTime;
        }

        void UpdateInput()
        {
            // Movement
            _velocity += GetAccelerationVector() * Time.deltaTime;

            // Mouse look
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            mouseDelta *= lookSensitivity;

            Quaternion rotation = transform.rotation;

            Quaternion horiz = Quaternion.AngleAxis(
                mouseDelta.x,
                Vector3.up
            );

            Quaternion vert = Quaternion.AngleAxis(
                -mouseDelta.y,
                Vector3.right
            );

            transform.rotation = horiz * rotation * vert;

            // Unlock cursor
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Focused = false;
        }

        Vector3 GetAccelerationVector()
        {
            Vector3 moveInput = Vector3.zero;

            Keyboard kb = Keyboard.current;

            if (kb.wKey.isPressed)
                moveInput += Vector3.forward;

            if (kb.sKey.isPressed)
                moveInput += Vector3.back;

            if (kb.aKey.isPressed)
                moveInput += Vector3.left;

            if (kb.dKey.isPressed)
                moveInput += Vector3.right;

            if (kb.eKey.isPressed)
                moveInput += Vector3.up;

            if (kb.qKey.isPressed)
                moveInput += Vector3.down;

            Vector3 direction =
                transform.TransformVector(moveInput.normalized);

            float currentAcceleration = acceleration;

            if (kb.leftCtrlKey.isPressed)
                currentAcceleration *= accSprintMultiplier;

            return direction * currentAcceleration;
        }
    }
}