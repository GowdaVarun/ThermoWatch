using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DataCenterGuard
{
    /// <summary>
    /// Simple orbit camera: hold right mouse to rotate, scroll to zoom.
    /// Uses the new Input System package.
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        public Vector3 target = new Vector3(0f, 1.2f, 0f);

        [Header("View")]
        public float distance = 15f;
        public float yaw = 32f;
        public float pitch = 27f;
        public float minPitch = 8f;
        public float maxPitch = 78f;
        public float minDistance = 5f;
        public float maxDistance = 28f;

        [Header("Control")]
        public float rotateSpeed = 0.22f;
        public float zoomSpeed = 0.02f;

        private void Start()
        {
            ApplyNow();
        }

        private void LateUpdate()
        {
            HandleInput();
            ApplyNow();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    yaw += delta.x * rotateSpeed;
                    pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, minPitch, maxPitch);
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                ResetView();
#endif
        }

        public void ResetView()
        {
            distance = 15f;
            yaw = 32f;
            pitch = 27f;
            ApplyNow();
        }

        public void ApplyNow()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = rotation;
            transform.position = target - rotation * Vector3.forward * distance;
        }
    }
}
