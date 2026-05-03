#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Menu: ADL > Build Main Menu Scene
public static class MainMenuBuilder
{
    [MenuItem("ADL/Build Main Menu Scene")]
    static void Build()
    {
        if (!EditorUtility.DisplayDialog("Build Main Menu",
            "Clear current scene and rebuild?", "Yes", "Cancel"))
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.backgroundColor = GameColors.CreamBg;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // MainMenu script holder
        var menuGo = new GameObject("MainMenu");
        menuGo.transform.SetParent(canvasGo.transform, false);
        var menuScript = menuGo.AddComponent<MainMenu>();

        // Background panel (cream deep, full screen)
        var bg = new GameObject("Bg");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = GameColors.CreamBg;

        // Title
        AddText(canvasGo.transform, "Title", "ADL Mission Miner",
            new Vector2(0.15f, 0.62f), new Vector2(0.85f, 0.82f),
            72f, GameColors.Ink);

        // Subtitle
        AddText(canvasGo.transform, "Sub", "Grab all the glowing items!",
            new Vector2(0.2f, 0.50f), new Vector2(0.8f, 0.62f),
            30f, GameColors.InkSoft);

        // Play button
        var btn = MakeButton(canvasGo.transform, "PlayButton", "PLAY",
            new Vector2(0.35f, 0.28f), new Vector2(0.65f, 0.44f),
            GameColors.MintDeep, GameColors.Ink, 48f);
        UnityEventTools.AddPersistentListener(btn.onClick, menuScript.StartGame);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[ADL] MainMenu scene saved.");
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text,
        Vector2 ancMin, Vector2 ancMax, float size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static Button MakeButton(Transform parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax, Color bgColor, Color textColor, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();

        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var lblRt = lbl.AddComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}
#endif
