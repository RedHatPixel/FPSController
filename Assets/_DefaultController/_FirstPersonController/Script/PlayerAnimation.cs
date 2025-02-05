using System;
using System.Linq;
using UnityEngine;

namespace DefaultController.FirstPersonController
{
    public class PlayerAnimation : MonoBehaviour
    {

        #region Class Variables
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private PlayerLocomotion _playerLocomotion;
        private PlayerAction _playerAction;
        private PlayerState _playerState;
        private PlayerController _playerController;

        // Locomotion
        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int isIdlingHash = Animator.StringToHash("isIdling");
        private static int isGroundedHash = Animator.StringToHash("isGrounded");
        private static int isFallingHash = Animator.StringToHash("isFalling");
        private static int isJumpingHash = Animator.StringToHash("isJumping");

        // Actions
        private static int isAttackingHash = Animator.StringToHash("isAttacking");
        private static int isGatheringHash = Animator.StringToHash("isGathering");
        private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
        private int[] actionHashes;

        // Camera/Rotation
        private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
        private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

        private Vector3 _currentBlendInput = Vector3.zero;
        private float _sprintMaxBlendValue = 1.5f;
        private float _walkMaxBlendValue = 1.0f;
        private float _stealthMaxBlendValue = 0.7f;
        #endregion

        #region Startup
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            _playerState = GetComponentInParent<PlayerState>();
            _playerController = GetComponentInParent<PlayerController>();
            _playerAction = GetComponentInParent<PlayerAction>();

            _playerAction.Interact += PlayerAction_InteractPressed;
            _playerAction.AttackPressed += PlayerAction_AttackPressed;
            actionHashes = new int[] { isGatheringHash };
        }

        private void PlayerAction_AttackPressed(object sender, EventArgs e)
        {
            _animator.SetBool(isAttackingHash, true);
        }

        private void PlayerAction_InteractPressed(object sender, EventArgs e)
        {
            _animator.SetBool(isGatheringHash, true);
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            UpdateAnimationState();
            UpdateActionState();
        }

        private void UpdateActionState()
        {
            if (_playerLocomotion.MovementInput != Vector2.zero ||
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping ||
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling)
            {
                _animator.SetBool(isGatheringHash, false);
            }
        }

        private void UpdateAnimationState()
        {
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isStealthing = _playerState.CurrentPlayerMovementState == PlayerMovementState.Stealthing;
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isGrounded = _playerState.InGroundedState();
            bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

            Vector2 inputTarget = isSprinting ? _playerLocomotion.MovementInput * _sprintMaxBlendValue :
                                  isStealthing ? _playerLocomotion.MovementInput * _stealthMaxBlendValue :
                                                    _playerLocomotion.MovementInput * _walkMaxBlendValue;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            _animator.SetBool(isGroundedHash, isGrounded);
            _animator.SetBool(isIdlingHash, isIdling);
            _animator.SetBool(isFallingHash, isFalling);
            _animator.SetBool(isJumpingHash, isJumping);
            _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);

            _animator.SetBool(isPlayingActionHash, isPlayingAction);
            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
            _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
        }
        #endregion

        #region Animation Events
        public void disableGathering()
        {
            _animator.SetBool(isGatheringHash, false);
        }

        public void disableAttacking()
        {
            _animator.SetBool(isAttackingHash, false);
        }
        #endregion
    }
}
