using Unity.Cinemachine;
using UnityEngine;

namespace DefaultController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerCameraNavigation : MonoBehaviour, InputSystem.IPlayerCameraNavigationActions
    {

        #region Class Variables
        public Vector2 scrollInput { get; private set; }

        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private float _cameraZoomSpeed = 0.1f;
        [SerializeField] private float _cameraMinZoom = 1f;
        [SerializeField] private float _cameraMaxZoom = 5f;

        private CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
        #endregion

        #region Callbacks
        private void OnEnable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot enable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerCameraNavigation.Enable();
            InputManager.Instance.InputSystem.PlayerCameraNavigation.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot disable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerCameraNavigation.Disable();
            InputManager.Instance.InputSystem.PlayerCameraNavigation.RemoveCallbacks(this);
        }
        #endregion

        #region Startup
        private void Awake()
        {
            _cinemachineThirdPersonFollow = _cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        }
        #endregion

        #region Update
        private void Update()
        {
            _cinemachineThirdPersonFollow.CameraDistance = Mathf.Clamp(_cinemachineThirdPersonFollow.CameraDistance + scrollInput.y, _cameraMinZoom, _cameraMaxZoom);
        }

        private void LateUpdate()
        {
            scrollInput = Vector2.zero;
        }
        #endregion

        #region Interface
        public void OnCameraScroll(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Vector2 scroll = context.ReadValue<Vector2>();
            scrollInput = -1f * scroll.normalized * _cameraZoomSpeed;
        }
        #endregion
    }
}