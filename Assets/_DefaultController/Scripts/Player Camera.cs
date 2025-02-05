using UnityEngine;

namespace DefaultController
{

    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraHost;

        [Header("Offset Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, -3f);

        private void Start()
        {
            UpdateCameraTransform();
        }

        private void UpdateCameraTransform()
        {
            if (cameraHost == null)
            {
                Debug.Log("No Camera Host Applied, Camera stays at default position");
                return;
            }
            transform.SetParent(cameraHost);
            transform.localPosition = offset;
        }
    }
}