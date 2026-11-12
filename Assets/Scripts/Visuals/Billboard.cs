using UnityEngine;

namespace DataCenterGuard
{
    /// <summary>
    /// Keeps a world-space label facing the camera.
    /// </summary>
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        private Camera cachedCamera;

        private void LateUpdate()
        {
            FaceCameraNow();
        }

        /// <summary>
        /// Applies the billboard rotation immediately. Also used by the scene builder
        /// so saved editor previews show readable text.
        /// </summary>
        public void FaceCameraNow()
        {
            if (cachedCamera == null)
                cachedCamera = Camera.main;
            if (cachedCamera == null)
                return;

            // Keep the TextMesh front (-Z face) toward the camera.
            transform.rotation = cachedCamera.transform.rotation;
        }
    }
}
