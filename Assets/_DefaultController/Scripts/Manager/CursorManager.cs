using UnityEngine;

namespace DefaultController
{
    [DefaultExecutionOrder(-3)]
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

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
            HideCursor();
        }

        public void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                ShowCursor();
            else
                HideCursor();
        }
    }
}