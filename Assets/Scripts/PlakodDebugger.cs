using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlakodDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"[Plakod] === Connected devices ({InputSystem.devices.Count}) ===");
        foreach (var d in InputSystem.devices)
            Debug.Log($"[Plakod] Device: '{d.displayName}'  type: {d.GetType().Name}  layout: {d.layout}");
    }

    void Update()
    {
        foreach (var device in InputSystem.devices)
            foreach (var control in device.allControls)
                if (control is ButtonControl btn && btn.wasPressedThisFrame)
                    Debug.Log($"[Plakod] Device: {device.displayName} | Button: {control.path}");
    }
}
