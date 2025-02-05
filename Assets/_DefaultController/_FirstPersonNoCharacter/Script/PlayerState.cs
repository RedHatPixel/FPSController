using UnityEngine;

namespace DefaultController.FirstPersonNoCharacter
{
    public class PlayerState : MonoBehaviour
    {
        [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;

        public void SetPlayerMovementState(PlayerMovementState playerMovementState)
        {
            CurrentPlayerMovementState = playerMovementState;
        }

        public bool InGroundedState()
        {
            return IsStateGroundedState(CurrentPlayerMovementState);
        }

        public bool IsStateGroundedState(PlayerMovementState movementState)
        {
            return movementState == PlayerMovementState.Idling ||
                   movementState == PlayerMovementState.Stealthing ||
                   movementState == PlayerMovementState.Walking ||
                   movementState == PlayerMovementState.Sprinting;
        }

        public bool isStateFalling()
        {
            return CurrentPlayerMovementState == PlayerMovementState.Falling;
        }
    }

    public enum PlayerMovementState
    {
        Idling = 0,
        Stealthing = 1,
        Walking = 2,
        Sprinting = 3,
        Jumping = 4,
        Falling = 5,
    }
}