using UnityEngine;

namespace DefaultController.FirstPersonController
{

    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _Camera;
        [SerializeField] private PlayerState playerState;
        [SerializeField] private Transform cameraHost;

        [Header("Offset Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 0.07f, 0.09f);

        [Header("View Scopping Settings")]
        [SerializeField] private bool enableViewScopping = true;

        [Space]
        [SerializeField, Range(40f, 100f)] private float maxFOV = 70f;
        [SerializeField, Range(40f, 100f)] private float normalFOV = 60f;
        [Space]
        [SerializeField, Range(10f, 100f)] private float fallingFOVSpeed = 10f;
        [SerializeField, Range(10f, 100f)] private float runningFOVSpeed = 60f;
        [SerializeField, Range(10f, 100f)] private float normalFOVSpeed = 40f;
        [SerializeField, Range(0f, 50f)] private float fallingFOVDelay = 1f;

        private float fallingTimer = 0f;
        private bool hasResetView = false;
        private bool isFalling = false;

        private void Awake()
        {
            _Camera = GetComponent<Camera>();
            playerState = GetComponentInParent<PlayerState>();
        }

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

        private void LateUpdate()
        {
            UpdateCameraView();
        }

        private void UpdateCameraView()
        {
            if (!enableViewScopping)
                if (!hasResetView)
                {
                    _Camera.fieldOfView = normalFOV;
                    hasResetView = true;
                }

            hasResetView = false;
            if (playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting)
                _Camera.fieldOfView = Mathf.MoveTowards(_Camera.fieldOfView, maxFOV, Time.deltaTime * runningFOVSpeed);
            else if (playerState.CurrentPlayerMovementState == PlayerMovementState.Falling)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    fallingTimer = 0f;
                }

                fallingTimer += Time.deltaTime;
                if (fallingTimer >= fallingFOVDelay)
                {
                    _Camera.fieldOfView = Mathf.MoveTowards(_Camera.fieldOfView, maxFOV, Time.deltaTime * fallingFOVSpeed);
                }
                _Camera.fieldOfView = Mathf.MoveTowards(_Camera.fieldOfView, maxFOV, Time.deltaTime * fallingFOVSpeed);
            }
            else if (playerState.CurrentPlayerMovementState != PlayerMovementState.Jumping)
                _Camera.fieldOfView = Mathf.MoveTowards(_Camera.fieldOfView, normalFOV, Time.deltaTime * normalFOVSpeed);
        }
    }
}