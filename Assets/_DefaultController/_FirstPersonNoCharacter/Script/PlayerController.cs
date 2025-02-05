using System;
using UnityEngine;

namespace DefaultController.FirstPersonNoCharacter
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerLocomotion))]
    [RequireComponent(typeof(PlayerState))]
    public class PlayerController : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;
        public float lateralAcceleration { get; private set; } = 0f;
        public float clampLateralMagnitude { get; private set; } = 0f;

        [Header("Base Movement")]
        [SerializeField] private float stealthAcceleration = 17f;
        [SerializeField] private float stealthSpeed = 1.5f;
        [SerializeField] private float walkAcceleration = 20f;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintAcceleration = 40f;
        [SerializeField] private float sprintSpeed = 5f;
        [SerializeField] private float inAirAcceleration = 20f;
        [SerializeField] private float drag = 15;
        [SerializeField] private float inAirDrag = 5f;
        [SerializeField] private float movingThreshold = 0.01f;

        [Header("Gravity Setting")]
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float terminalVelocity = 50f;
        [SerializeField] private float jumpSpeed = 1.0f;

        [Header("Camera Setting")]
        [SerializeField, Range(0.1f, 1.0f)] private float mouseSensitivity = 0.1f;
        [SerializeField, Range(60.0f, 90.0f)] private float clampAngle = 80.0f;
        [SerializeField, Range(1f, 20f)] private float cameraParentRotationSpeed = 10f;

        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        private PlayerLocomotion _PlayerLocomotion;
        private PlayerState _PlayerState;

        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        private bool _jumpedLastFrame = false;
        private float _verticalVelocity = 0f;
        private float _antiBump;
        private float _stepOffset;
        private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
        #endregion

        #region Startup
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _PlayerLocomotion = GetComponent<PlayerLocomotion>();
            _PlayerState = GetComponent<PlayerState>();
            if (_playerCamera == null) _playerCamera = Camera.main;

            _antiBump = sprintSpeed;
            _stepOffset = _characterController.stepOffset;
            _PlayerLocomotion.JumpPressed += PlayerInput_JumpPressed;
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            _lastMovementState = _PlayerState.CurrentPlayerMovementState;

            bool canRun = CanRun();
            bool isMovementInput = IsMovementInput();
            bool isMovingLaterally = IsMovingLaterally();
            bool isStealthing = _PlayerLocomotion.StealthToggled && isMovingLaterally;
            bool isSprinting = _PlayerLocomotion.SprintToggled && isMovingLaterally;
            bool isGrounded = IsGrounded();

            PlayerMovementState lateralState = isStealthing ? PlayerMovementState.Stealthing :
                                               canRun && isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Walking : PlayerMovementState.Idling;

            _PlayerState.SetPlayerMovementState(lateralState);

            if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f)
            {
                _PlayerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
            {
                _PlayerState.SetPlayerMovementState(PlayerMovementState.Falling);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else
                _characterController.stepOffset = _stepOffset;
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = _PlayerState.InGroundedState();

            _verticalVelocity -= gravity * Time.deltaTime;

            if (isGrounded && _verticalVelocity < 0)
                _verticalVelocity = -_antiBump;

            if (_PlayerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
                _verticalVelocity += _antiBump;

            if (_characterController.collisionFlags == CollisionFlags.Above)
                _verticalVelocity = 0f;

            if (Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
                _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
        }

        private void PlayerInput_JumpPressed(object sender, EventArgs e)
        {
            if (!_PlayerState.InGroundedState() || _PlayerState.isStateFalling()) return;
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
            _jumpedLastFrame = true;
        }

        private void HandleLateralMovement()
        {
            bool isStealthing = _PlayerState.CurrentPlayerMovementState == PlayerMovementState.Stealthing;
            bool isSprinting = _PlayerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = _PlayerState.InGroundedState();

            lateralAcceleration = !isGrounded ? inAirAcceleration :
                                        isStealthing ? stealthAcceleration :
                                        isSprinting ? sprintAcceleration :
                                        walkAcceleration;

            clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                          isStealthing ? stealthSpeed :
                                          isSprinting ? sprintSpeed :
                                          walkSpeed;

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * _PlayerLocomotion.MovementInput.x + cameraForwardXZ * _PlayerLocomotion.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            float dragMagnitude = isGrounded ? drag : inAirDrag;
            Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;

            newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
            newVelocity.y += _verticalVelocity;
            newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

            _characterController.Move(newVelocity * Time.deltaTime);
        }

        private Vector3 HandleSteepWalls(Vector3 velocity)
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= _characterController.slopeLimit;

            if (!validAngle && _verticalVelocity < 0f)
                velocity = Vector3.ProjectOnPlane(velocity, normal);

            return velocity;
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            Vector2 lookInput = _PlayerLocomotion.LookInput;

            _cameraRotation.x += mouseSensitivity * lookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - mouseSensitivity * lookInput.y, -clampAngle, clampAngle);
            _playerTargetRotation.x += transform.eulerAngles.x + mouseSensitivity * lookInput.x;

            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, cameraParentRotationSpeed * Time.deltaTime);

            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
        }
        #endregion

        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
            return lateralVelocity.magnitude > movingThreshold;
        }

        private bool IsMovementInput()
        {
            return _PlayerLocomotion.MovementInput != Vector2.zero;
        }

        private bool CanRun()
        {
            return _PlayerLocomotion.MovementInput.y >= Mathf.Abs(_PlayerLocomotion.MovementInput.x);
        }

        private bool IsGrounded()
        {
            return _PlayerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);
            bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= _characterController.slopeLimit;
            return _characterController.isGrounded && validAngle;
        }
        #endregion

        private void OnDrawGizmos()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, _characterController.transform.position.z);
            Gizmos.DrawWireSphere(spherePosition, _characterController.radius);
        }
    }
}