using UnityEngine;

namespace LWOS
{
    public class InputManager : MonoBehaviour
    {
        public Platform platform = Platform.PC;

        public bool GetButton(string button)
        {
            switch (platform)
            {
                case Platform.PC:
                    return Input.GetButton(button);
                case Platform.MOBILE:
                    return CrossPlatformInputManager.GetButton(button);
            }

            return Input.GetButton(button);
        }

        public bool GetButtonDown(string button)
        {
            switch (platform)
            {
                case Platform.PC:
                    return Input.GetButtonDown(button);
                case Platform.MOBILE:
                    return CrossPlatformInputManager.GetButtonDown(button);
            }

            return Input.GetButtonDown(button);
        }

        public bool GetButtonUp(string button)
        {
            switch (platform)
            {
                case Platform.PC:
                    return Input.GetButtonUp(button);
                case Platform.MOBILE:
                    return CrossPlatformInputManager.GetButtonUp(button);
            }

            return Input.GetButtonUp(button);
        }

        public float GetAxis(string button)
        {
            switch (platform)
            {
                case Platform.PC:
                    return Input.GetAxis(button);
                case Platform.MOBILE:
                    return CrossPlatformInputManager.GetAxis(button);
            }

            return Input.GetAxis(button);
        }

        private static InputManager instance;
        public static InputManager Instance
        {
            get
            {
                if (instance == null) { instance = FindObjectOfType<InputManager>(); }
                return instance;
            }
        }
    }
}
