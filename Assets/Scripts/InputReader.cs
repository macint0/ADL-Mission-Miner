using UnityEngine;
using UnityEngine.InputSystem;

// Button 1 — tip pinch    : joystick button 0 / Space
// Button 2 — wrist extend : joystick button 1 / F
// Hold B1 = aim locked. While B1 held, hold B2 = charging. Release B1 = fire.
[DefaultExecutionOrder(-100)]
public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    public bool Button1Held     { get; private set; }
    public bool Button1Released { get; private set; }   // true for one frame on release
    public bool Button2Held     { get; private set; }
    public bool Button2Released { get; private set; }   // true for one frame on release

    bool _prevB1, _prevB2;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        bool b1 = ReadB1();
        bool b2 = ReadB2();

        Button1Released = _prevB1 && !b1;
        Button2Released = _prevB2 && !b2;
        Button1Held = b1;
        Button2Held = b2;
        _prevB1 = b1;
        _prevB2 = b2;
    }

    bool ReadB1()
    {
        var gp = Gamepad.current;
        if (gp != null && gp.buttonEast.isPressed) return true;   // Plakod sensor 1
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.isPressed) return true;
        return false;
    }

    bool ReadB2()
    {
        var gp = Gamepad.current;
        if (gp != null && gp.buttonWest.isPressed) return true;   // Plakod sensor 2
        var kb = Keyboard.current;
        if (kb != null && kb.fKey.isPressed) return true;
        return false;
    }
}
