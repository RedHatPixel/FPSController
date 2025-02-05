using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerLocomotion : MonoBehaviour, InputSystem.IPlayerLocomotionActions
    {

        #region Class Variables
        [Header("Input Settings")]
        [SerializeField] private bool holdToSprint = true;
        [SerializeField] private bool holdToStealth = true;

        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintToggled { get; private set; }
        public bool StealthToggled { get; private set; }
        public event EventHandler JumpPressed;
        #endregion

        #region Callbacks
        private void OnEnable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot enable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerLocomotion.Enable();
            InputManager.Instance.InputSystem.PlayerLocomotion.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot disable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerLocomotion.Disable();
            InputManager.Instance.InputSystem.PlayerLocomotion.RemoveCallbacks(this);
        }
        #endregion

        #region Interface
        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed && !StealthToggled)
                SprintToggled = holdToSprint || !SprintToggled;
            else if (context.canceled)
                SprintToggled = !holdToSprint && SprintToggled;
        }

        public void OnStealth(InputAction.CallbackContext context)
        {
            if (context.performed && !SprintToggled)
                StealthToggled = holdToStealth || !StealthToggled;
            else if (context.canceled)
                StealthToggled = !holdToStealth && StealthToggled;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                JumpPressed?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}