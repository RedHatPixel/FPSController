using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultController
{

    [DefaultExecutionOrder(-2)]
    public class PlayerAction : MonoBehaviour, InputSystem.IPlayerActionActions
    {
        #region Class Variables
        public event EventHandler AttackPressed;
        public event EventHandler Interact;
        #endregion

        #region Callbacks
        private void OnEnable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot enable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerAction.Enable();
            InputManager.Instance.InputSystem.PlayerAction.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.Instance?.InputSystem == null)
            {
                Debug.LogError("Player controls is not initialized - cannot disable");
                return;
            }

            InputManager.Instance.InputSystem.PlayerAction.Disable();
            InputManager.Instance.InputSystem.PlayerAction.RemoveCallbacks(this);
        }
        #endregion

        #region Interface
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                Interact?.Invoke(this, EventArgs.Empty);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                AttackPressed?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}