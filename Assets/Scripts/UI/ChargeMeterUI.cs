using UnityEngine;
using UnityEngine.UI;

// Bottom-center pill bar. HookController calls Show/Hide/Refresh directly.
// Layout: pill background → fill Image (fillAmount 0..1) → marker pip.
public class ChargeMeterUI : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;
    public Image markerPip;
    public GameObject panel;   // the root pill — toggled visible/hidden

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    // chargeTime and zone boundaries are in seconds; maxCharge is the full duration.
    public void Refresh(float chargeTime, float sweetMin, float sweetMax, float maxCharge)
    {
        float fraction = maxCharge > 0f ? chargeTime / maxCharge : 0f;
        if (fillImage != null)
        {
            fillImage.fillAmount = fraction;
            fillImage.color = ColorForTime(chargeTime, sweetMin, sweetMax);
        }
        if (markerPip != null)
        {
            var rt = markerPip.rectTransform;
            float width = ((RectTransform)rt.parent).rect.width;
            rt.anchoredPosition = new Vector2(fraction * width - width * 0.5f, rt.anchoredPosition.y);
        }
    }

    static Color ColorForTime(float t, float sweetMin, float sweetMax)
    {
        if (t < sweetMin)  return GameColors.MeterYellow;
        if (t <= sweetMax) return GameColors.MeterGreen;
        return GameColors.MeterRed;
    }
}
