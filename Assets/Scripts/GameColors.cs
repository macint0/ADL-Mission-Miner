using UnityEngine;

public static class GameColors
{
    public static readonly Color CreamBg     = Hex("#FBF5EC");
    public static readonly Color CreamDeep   = Hex("#F2E9D8");
    public static readonly Color Ink         = Hex("#2E2A3A");
    public static readonly Color InkSoft     = Hex("#5A546A");
    public static readonly Color Mint        = Hex("#B8E4D2");
    public static readonly Color MintDeep    = Hex("#6FBFA0");
    public static readonly Color Peach       = Hex("#FFD2B8");
    public static readonly Color PeachDeep   = Hex("#F2A37A");
    public static readonly Color Lavender    = Hex("#D9CBEF");
    public static readonly Color LavDeep     = Hex("#A88FD1");
    public static readonly Color Butter      = Hex("#FFE9A8");
    public static readonly Color ButterDeep  = Hex("#E8C25A");
    public static readonly Color Sky         = Hex("#C7E1F2");
    public static readonly Color SkyDeep     = Hex("#7FB3D9");
    public static readonly Color Rose        = Hex("#F2C3CC");
    public static readonly Color RoseDeep    = Hex("#D98896");
    public static readonly Color MeterYellow = Hex("#F5D26B");
    public static readonly Color MeterGreen  = Hex("#7BC796");
    public static readonly Color MeterRed    = Hex("#E89090");
    public static readonly Color White       = Color.white;

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
