using UnityEngine;

namespace DefaultController
{
    [DefaultExecutionOrder(-3)]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        public InputSystem InputSystem { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            InputSystem = new InputSystem();
            InputSystem.Enable();
        }

        private void OnDisable()
        {
            InputSystem.Disable();
        }
    }
}