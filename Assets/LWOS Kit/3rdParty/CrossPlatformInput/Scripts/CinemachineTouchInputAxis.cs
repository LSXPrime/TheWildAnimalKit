using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using LWOS;

public class CinemachineTouchInputAxis : MonoBehaviour, AxisState.IInputAxisProvider
{
    private UnityEngine.UI.Slider Sensitivity;

    void Start()
    {
        Sensitivity = FindObjectOfType<GameSettings>().CameraSensitivity;
    }

    public float GetAxisValue(int axis)
    {
        return InputManager.Instance.GetAxis(axis == 0 ? "Mouse X" : "Mouse Y") * Sensitivity.value;
    }
}
