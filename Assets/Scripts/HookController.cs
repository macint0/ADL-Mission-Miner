using UnityEngine;

// Attach to the pivot GameObject (top-center of screen).
// Requires: hookHead child with CircleCollider2D (trigger) + Kinematic Rigidbody2D, tagged "Hook".
// LineRenderer on this same GameObject draws the rope.
//
// Control flow:
//   Button 1 press  → lock aim angle         (Swinging → AimLocked)
//   Button 2 hold   → charge power           (AimLocked → Charging)
//   Button 2 release → fire                  (Charging → Firing)
//   Auto-fire after maxChargeDuration         (prevents stalling)
[RequireComponent(typeof(LineRenderer))]
public class HookController : MonoBehaviour
{
    public enum HookState { Swinging, AimLocked, Charging, Firing, Retrieving }

    [Header("Swing")]
    public float swingFrequency = 0.55f;
    public float swingAmplitude = 32f;

    [Header("Charge")]
    public float sweetSpotMin     = 0.8f;  // seconds
    public float sweetSpotMax     = 2.5f;
    public float maxChargeDuration = 3f;
    public float minHookRange     = 1.5f;
    public float maxHookRange     = 11f;

    [Header("Speed")]
    public float hookFireSpeed  = 15f;
    public float retrieveSpeed  = 9f;
    public float restLength     = 0.8f;

    [Header("Visual")]
    [Tooltip("World-unit offset from hookHead pivot to the hook tip. Leave 0 to auto-derive from sprite bounds.")]
    public float hookTipOffset = 0f;

    [Header("References")]
    public Transform    pivot;
    public Transform    hookHead;
    public ChargeMeterUI chargeMeter;

    public HookState State      { get; private set; }
    public float     ChargeTime { get; private set; }
    public bool      InSweetSpot => ChargeTime >= sweetSpotMin && ChargeTime <= sweetSpotMax;

    float           _swingTimer;
    float           _currentLength;
    float           _ropeAngle;      // degrees, drives hook sprite rotation
    Vector2         _lockedDir;
    float           _shotRange;
    CollectibleItem _heldItem;
    LineRenderer    _line;
    Rigidbody2D     _hookRb;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _hookRb = hookHead != null ? hookHead.GetComponent<Rigidbody2D>() : null;
        _currentLength = restLength;
        State = HookState.Swinging;

        if (hookHead == null) return;

        // If HookHead is still at default scale the 512px sprite is 5.12 world units tall —
        // half the screen. Force it to 0.15 so the sprite is ~0.77 units and tip offset is sane.
        if (hookHead.localScale.y > 0.9f)
            hookHead.localScale = Vector3.one * 0.15f;

        // Load hook sprite first so bounds are available below.
        var sr = hookHead.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            var spr = Resources.Load<Sprite>("Items/hook");
            if (spr != null) sr.sprite = spr;
        }

        // Derive tip offset from actual sprite height × world scale. No cap needed now that
        // hookHead scale is controlled above.
        if (hookTipOffset <= 0f && sr != null && sr.sprite != null)
        {
            float localH = sr.sprite.bounds.size.y;
            hookTipOffset = localH * Mathf.Abs(hookHead.lossyScale.y) * 0.9f;
        }

        // Place the trigger collider at the tip (local offset = world offset / scale).
        var col = hookHead.GetComponent<CircleCollider2D>();
        if (col != null && hookHead.lossyScale.y != 0f)
            col.offset = new Vector2(0f, -(hookTipOffset / Mathf.Abs(hookHead.lossyScale.y)));
    }

    void Fire()
    {
        _shotRange = Mathf.Lerp(minHookRange, maxHookRange, ChargePower());
        State = HookState.Firing;
        if (chargeMeter != null) chargeMeter.Hide();
    }

    void Update()
    {
        var input = InputReader.Instance;

        switch (State)
        {
            case HookState.Swinging:
                _swingTimer += Time.deltaTime;
                float a = Mathf.Sin(_swingTimer * 2f * Mathf.PI * swingFrequency) * swingAmplitude;
                _ropeAngle = a;
                SetHookByAngle(a);
                // Hold B1 → lock aim
                if (input != null && input.Button1Held)
                {
                    _lockedDir = AngleToDir(a);
                    _ropeAngle = a;
                    State = HookState.AimLocked;
                }
                break;

            case HookState.AimLocked:
                // Release B1 without charging → back to swing
                if (input == null || !input.Button1Held) { ReturnToSwing(); break; }
                // Hold B2 → start charging
                if (input.Button2Held)
                {
                    ChargeTime = 0f;
                    State = HookState.Charging;
                    if (chargeMeter != null) chargeMeter.Show();
                }
                break;

            case HookState.Charging:
                ChargeTime = Mathf.Min(ChargeTime + Time.deltaTime, maxChargeDuration);
                if (chargeMeter != null) chargeMeter.Refresh(ChargeTime, sweetSpotMin, sweetSpotMax, maxChargeDuration);
                // Release B1 (pinch) → fire
                if (input != null && input.Button1Released) { Fire(); break; }
                // Release B2 (wrist) → cancel back to aim locked
                if (input != null && input.Button2Released)
                {
                    if (chargeMeter != null) chargeMeter.Hide();
                    ChargeTime = 0f;
                    State = HookState.AimLocked;
                    break;
                }
                // Auto-fire when meter maxes out
                if (ChargeTime >= maxChargeDuration) Fire();
                break;

            case HookState.Firing:
                _currentLength += hookFireSpeed * Time.deltaTime;
                SetHookByDir(_lockedDir, _currentLength);
                if (_currentLength >= _shotRange) State = HookState.Retrieving;
                break;

            case HookState.Retrieving:
                _currentLength -= retrieveSpeed * Time.deltaTime;
                SetHookByDir(_lockedDir, _currentLength);
                if (_heldItem != null) _heldItem.transform.position = hookHead.position + (Vector3)(_lockedDir * hookTipOffset);
                if (_currentLength <= restLength)
                {
                    _currentLength = restLength;
                    DeliverItem();
                    ReturnToSwing();
                }
                break;
        }

        if (_line != null && pivot != null && hookHead != null)
        {
            _line.SetPosition(0, pivot.position);
            _line.SetPosition(1, hookHead.position);
        }

        // Rotate hook sprite to align with rope (pivot=Top, hangs downward at 0°).
        if (hookHead != null)
            hookHead.rotation = Quaternion.Euler(0f, 0f, _ropeAngle);
    }

    public void GrabItem(CollectibleItem item)
    {
        if (State != HookState.Firing) return;
        _heldItem = item;
        State = HookState.Retrieving;
    }

    void DeliverItem()
    {
        if (_heldItem == null) return;
        MissionManager.Instance?.EvaluateItem(_heldItem, InSweetSpot);
        _heldItem.gameObject.SetActive(false);
        _heldItem = null;
    }

    void ReturnToSwing()
    {
        _currentLength = restLength;
        _swingTimer = 0f;
        State = HookState.Swinging;
    }

    // Range ramps 0→1 across the full sweet-spot window, then caps.
    // This lets the player aim at any distance by timing the release.
    float ChargePower() => Mathf.Clamp01(ChargeTime / sweetSpotMax);

    void SetHookByAngle(float angleDeg)
    {
        _currentLength = restLength;
        MoveHook((Vector2)pivot.position + AngleToDir(angleDeg) * restLength);
    }

    void SetHookByDir(Vector2 dir, float len) => MoveHook((Vector2)pivot.position + dir * len);

    void MoveHook(Vector2 target)
    {
        if (_hookRb != null) _hookRb.MovePosition(target);
        else if (hookHead != null) hookHead.position = target;
    }

    Vector2 AngleToDir(float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(r), -Mathf.Cos(r));
    }
}
