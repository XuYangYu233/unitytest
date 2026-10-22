using UnityEngine;
using UnityEngine.InputSystem;

namespace BoxRoom
{
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float sprintSpeed = 9f;
        public float jumpHeight = 2f;
        public float gravity = -20f;

        [Header("Look")]
        public float mouseSensitivity = 0.1f;
        public Transform playerCamera;

        [Header("Camera")]
        public float cameraHeight = 1.5f;

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;
        private bool _cursorLocked = true;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (playerCamera == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                playerCamera = cam != null ? cam.transform : null;
            }

            if (playerCamera != null)
            {
                playerCamera.localPosition = new Vector3(0f, cameraHeight, 0f);
            }

            LockCursor(true);
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (kb == null || mouse == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                LockCursor(!_cursorLocked);
            }

            if (!_cursorLocked) return;

            Vector2 look = mouse.delta.ReadValue();
            _pitch = Mathf.Clamp(_pitch - look.y * mouseSensitivity, -89f, 89f);
            transform.Rotate(Vector3.up * (look.x * mouseSensitivity));
            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            float targetSpeed = kb.shiftKey.isPressed ? sprintSpeed : moveSpeed;
            Vector2 input = ReadMovement(kb);
            Vector3 move = transform.right * input.x + transform.forward * input.y;
            if (move.sqrMagnitude > 1f) move.Normalize();

            _verticalVelocity += gravity * Time.deltaTime;
            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            if (_controller.isGrounded && kb.spaceKey.wasPressedThisFrame)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            move = move * targetSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(move * Time.deltaTime);
        }

        private static Vector2 ReadMovement(Keyboard kb)
        {
            Vector2 input = Vector2.zero;
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
            return input;
        }

        private void LockCursor(bool locked)
        {
            _cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDestroy()
        {
            if (_cursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}