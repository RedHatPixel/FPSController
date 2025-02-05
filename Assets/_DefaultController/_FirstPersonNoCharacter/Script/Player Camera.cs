using UnityEngine;

namespace DefaultController.FirstPersonNoCharacter
{

    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _Camera;
        [SerializeField] private PlayerLocomotion playerLocomotion;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerState playerState;

        [Header("View Bobbing Settings")]
        [SerializeField] private bool enableViewBobbing = true;
        [Space]
        [SerializeField, Range(0f, 1f)] private float bobbingStrengthX = 0.05f;
        [SerializeField, Range(0f, 1f)] private float bobbingStrengthY = 0.1f;
        [SerializeField, Range(0.1f, 1f)] private float bobbingSpeed = .1f;

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

        private Vector3 originalPosition;
        private float fallingTimer = 0f;
        private float bobbingTimer = 0f;
        private bool hasResetLocalPosition = false;
        private bool hasResetView = false;
        private bool isFalling = false;

        private void Awake()
        {
            _Camera = GetComponent<Camera>();
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            playerState = GetComponentInParent<PlayerState>();
            playerController = GetComponentInParent<PlayerController>();
        }

        private void Start()
        {
            originalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            UpdateCameraView();
            if (enableViewBobbing) ApplyViewBobbing();
        }

        private void ApplyViewBobbing()
        {
            if (!enableViewBobbing)
                if (!hasResetLocalPosition)
                {
                    transform.localPosition = originalPosition;
                    hasResetLocalPosition = true;
                }

            hasResetLocalPosition = false;
            float waveSliceX = Mathf.Sin(bobbingTimer) * bobbingStrengthX;
            float waveSliceY = Mathf.Sin(bobbingTimer * 2) * bobbingStrengthY;

            if (playerLocomotion.MovementInput != Vector2.zero)
                transform.localPosition = originalPosition + new Vector3(waveSliceX, waveSliceY, 0);

            bobbingTimer += Time.deltaTime * playerController.lateralAcceleration * bobbingSpeed;
            if (bobbingTimer > Mathf.PI * 2)
                bobbingTimer -= Mathf.PI * 2;
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