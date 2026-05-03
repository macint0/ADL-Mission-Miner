#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Menu: ADL > Build Game Scene
public static class GameSceneBuilder
{
    const float CAM_SIZE = 5.4f;
    const float PIVOT_Y  = 4.8f;

    [MenuItem("ADL/Build Game Scene")]
    static void BuildGameScene()
    {
        if (!EditorUtility.DisplayDialog("Build Game Scene",
            "Clear current scene and rebuild? Unsaved changes will be lost.", "Yes", "Cancel"))
            return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Camera ---
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = CAM_SIZE;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = GameColors.CreamBg;
        cam.transform.position = new Vector3(0, 0, -10);

        // --- Background ---
        var bg = new GameObject("Background");
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.color = GameColors.CreamBg;
        bgSr.sprite = CreateSolidSprite(GameColors.CreamBg);
        bg.transform.localScale = new Vector3(22f, 12f, 1f);
        bg.transform.position   = new Vector3(0, 0, 2);

        // --- InputReader ---
        var inputGo = new GameObject("InputReader");
        inputGo.AddComponent<InputReader>();

        // --- EventSystem (required for UI button clicks) ---
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();

        // --- Hook Pivot ---
        var pivotGo = new GameObject("HookPivot");
        pivotGo.transform.position = new Vector3(0, PIVOT_Y, 0);

        var lr = pivotGo.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth    = 0.04f;
        lr.endWidth      = 0.04f;
        lr.useWorldSpace = true;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = GameColors.InkSoft;
        lr.sortingOrder = 4;

        var dotGo = new GameObject("PivotDot");
        dotGo.transform.SetParent(pivotGo.transform, false);
        var dotSr = dotGo.AddComponent<SpriteRenderer>();
        dotSr.sprite = CreateSolidSprite(GameColors.InkSoft);
        dotGo.transform.localScale = Vector3.one * 0.15f;
        dotSr.sortingOrder = 6;

        var hookGo = new GameObject("HookHead");
        hookGo.tag = "Hook";
        hookGo.transform.SetParent(pivotGo.transform, false);
        hookGo.transform.localPosition = new Vector3(0, -0.8f, 0);
        hookGo.transform.localScale    = Vector3.one * 0.12f;

        var hookSr = hookGo.AddComponent<SpriteRenderer>();
        hookSr.sortingOrder = 5;
        var hookSprite = Resources.Load<Sprite>("Items/hook");
        if (hookSprite != null) hookSr.sprite = hookSprite;

        var hookRb = hookGo.AddComponent<Rigidbody2D>();
        hookRb.bodyType      = RigidbodyType2D.Kinematic;
        hookRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var hookCol = hookGo.AddComponent<CircleCollider2D>();
        hookCol.isTrigger = true;
        hookCol.radius    = 0.3f;

        var hookCtrl = pivotGo.AddComponent<HookController>();
        hookCtrl.pivot    = pivotGo.transform;
        hookCtrl.hookHead = hookGo.transform;

        // --- Item Prefab ---
        var itemPrefab = new GameObject("ItemPrefab");
        itemPrefab.AddComponent<SpriteRenderer>().sortingOrder = 3;
        itemPrefab.AddComponent<CollectibleItem>();
        var itemCol2 = itemPrefab.AddComponent<CircleCollider2D>();
        itemCol2.isTrigger = true;
        itemCol2.radius    = 0.4f;
        itemPrefab.SetActive(false);

        // --- GameManager ---
        var gameGo  = new GameObject("GameManager");
        var spawner = gameGo.AddComponent<ItemSpawner>();
        spawner.itemPrefab = itemPrefab;
        var mgr = gameGo.AddComponent<MissionManager>();
        mgr.itemSpawner = spawner;

        // --- Canvas ---
        var canvasGo = new GameObject("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var pill = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // === Timer chip — top-left ===
        var timerChip = MakePill(canvasGo.transform, "TimerChip", pill,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(165, -55), new Vector2(290, 76));
        MakeIconDot(timerChip.transform, pill, GameColors.Peach);
        ChipSmallLabel(timerChip, "TimeLabel",  "TIME",  GameColors.InkSoft);
        var timerText = ChipBigLabel(timerChip, "TimerValue", "5:00", GameColors.Ink);

        // === Mission banner — top-center ===
        var banner = MakePill(canvasGo.transform, "MissionBanner", pill,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -55), new Vector2(660, 76));
        var missionTag   = BannerTag(banner.transform, pill, "ADL MISSION");
        var missionTitle = BannerTitle(banner.transform, "Daily Tasks");

        // === Score chip — top-right ===
        var scoreChip = MakePill(canvasGo.transform, "ScoreChip", pill,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-165, -55), new Vector2(290, 76));
        MakeIconDot(scoreChip.transform, pill, GameColors.Mint);
        ChipSmallLabel(scoreChip, "ScoreLabel", "SCORE", GameColors.InkSoft);
        var scoreText = ChipBigLabel(scoreChip, "ScoreValue", "0", GameColors.Ink);

        // === Power meter — bottom-center ===
        var meterPanel = MakeRect(canvasGo.transform, "PowerMeterPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(520, 36));
        meterPanel.AddComponent<Image>().color = GameColors.CreamDeep;

        var fillGo  = new GameObject("Fill");
        fillGo.transform.SetParent(meterPanel.transform, false);
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color      = GameColors.MeterGreen;
        fillImg.type       = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        var pipGo  = new GameObject("Pip");
        pipGo.transform.SetParent(meterPanel.transform, false);
        var pipImg = pipGo.AddComponent<Image>();
        pipImg.color = Color.white;
        var pipRt = pipGo.GetComponent<RectTransform>();
        pipRt.sizeDelta    = new Vector2(6, 48);
        pipRt.anchorMin    = pipRt.anchorMax = new Vector2(0.5f, 0.5f);

        var chargeMeter       = meterPanel.AddComponent<ChargeMeterUI>();
        chargeMeter.fillImage = fillImg;
        chargeMeter.markerPip = pipImg;
        chargeMeter.panel     = meterPanel;
        hookCtrl.chargeMeter  = chargeMeter;

        // Win / Lose overlays
        var winScreen  = MakeOverlay(canvasGo.transform, "WinScreen",  "Well done!", GameColors.MeterGreen);
        var loseScreen = MakeOverlay(canvasGo.transform, "LoseScreen", "Time's up!", GameColors.MeterRed);
        AddOverlayButtons(winScreen.transform,  mgr);
        AddOverlayButtons(loseScreen.transform, mgr);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        // Wire MissionManager
        mgr.timerText        = timerText;
        mgr.scoreText        = scoreText;
        mgr.missionTitleText = missionTitle;
        mgr.missionTagText   = missionTag;
        mgr.winScreen        = winScreen;
        mgr.loseScreen       = loseScreen;

        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
            "Assets/Scenes/GameScene.unity");
        Debug.Log("[ADL] GameScene built and saved.");
    }

    // ------------------------------------------------------------------ pill chips

    static GameObject MakePill(Transform parent, string name, Sprite pill,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 size)
    {
        var go = MakeRect(parent, name, ancMin, ancMax, ancPos, size);
        var img   = go.AddComponent<Image>();
        img.sprite = pill;
        img.type   = Image.Type.Sliced;
        img.color  = Color.white;
        return go;
    }

    static void MakeIconDot(Transform parent, Sprite pill, Color color)
    {
        var go = new GameObject("IconDot");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f,    0f);
        rt.anchorMax = new Vector2(0.22f, 1f);
        rt.offsetMin = new Vector2(10,  10);
        rt.offsetMax = new Vector2(-4, -10);
        var img   = go.AddComponent<Image>();
        img.sprite = pill;
        img.type   = Image.Type.Sliced;
        img.color  = color;
    }

    // Small "TIME" / "SCORE" label — upper-right area of chip
    static void ChipSmallLabel(GameObject chip, string name, string text, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(chip.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, 0.50f);
        rt.anchorMax = new Vector2(0.98f, 0.95f);
        rt.offsetMin = new Vector2(4, 0);
        rt.offsetMax = new Vector2(-4, -2);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 14f;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.fontStyle = FontStyles.Bold;
    }

    // Large value label — lower-right area of chip
    static TextMeshProUGUI ChipBigLabel(GameObject chip, string name, string text, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(chip.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, 0.05f);
        rt.anchorMax = new Vector2(0.98f, 0.58f);
        rt.offsetMin = new Vector2(4,  2);
        rt.offsetMax = new Vector2(-4, 0);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 36f;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    // Butter-yellow tag pill on the left of the mission banner
    static TextMeshProUGUI BannerTag(Transform banner, Sprite pill, string text)
    {
        var tagGo = new GameObject("TagPill");
        tagGo.transform.SetParent(banner, false);
        var rt = tagGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.01f, 0.12f);
        rt.anchorMax = new Vector2(0.30f, 0.88f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img   = tagGo.AddComponent<Image>();
        img.sprite = pill;
        img.type   = Image.Type.Sliced;
        img.color  = GameColors.Butter;

        var lblGo = new GameObject("TagText");
        lblGo.transform.SetParent(tagGo.transform, false);
        var lrt = lblGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(6, 2);
        lrt.offsetMax = new Vector2(-6, -2);
        var tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 13f;
        tmp.color     = GameColors.Ink;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    // Bold mission title — right portion of banner
    static TextMeshProUGUI BannerTitle(Transform banner, string text)
    {
        var go = new GameObject("MissionTitle");
        go.transform.SetParent(banner, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.32f, 0.05f);
        rt.anchorMax = new Vector2(0.98f, 0.95f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 28f;
        tmp.color     = GameColors.Ink;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    // ------------------------------------------------------------------ generic rect

    static GameObject MakeRect(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.anchoredPosition = ancPos;
        rt.sizeDelta        = size;
        return go;
    }

    // ------------------------------------------------------------------ overlays

    static void AddOverlayButtons(Transform overlay, MissionManager mgr)
    {
        var tryBtn = MakeOverlayButton(overlay, "TryAgainBtn", "Try Again",
            new Vector2(0.2f, 0.15f), new Vector2(0.48f, 0.32f), GameColors.Butter, GameColors.Ink);
        UnityEventTools.AddPersistentListener(tryBtn.onClick, mgr.PlayAgain);

        var exitBtn = MakeOverlayButton(overlay, "ExitBtn", "Exit",
            new Vector2(0.52f, 0.15f), new Vector2(0.8f, 0.32f), GameColors.CreamDeep, GameColors.Ink);
        UnityEventTools.AddPersistentListener(exitBtn.onClick, mgr.QuitGame);
    }

    static UnityEngine.UI.Button MakeOverlayButton(Transform parent, string name, string label,
        Vector2 ancMin, Vector2 ancMax, Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin;
        rt.anchorMax = ancMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bgColor;
        var btn = go.AddComponent<UnityEngine.UI.Button>();

        var lbl    = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var lblRt  = lbl.AddComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
        var tmp    = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 36f;
        tmp.color     = textColor;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    static GameObject MakeOverlay(Transform parent, string name, string msg, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        bg.a = 0.88f;
        go.AddComponent<Image>().color = bg;

        var lbl  = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var lrt  = lbl.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.15f, 0.35f);
        lrt.anchorMax = new Vector2(0.85f, 0.65f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp  = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = msg;
        tmp.fontSize  = 64f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    // ------------------------------------------------------------------ misc

    static Sprite CreateSolidSprite(Color c)
    {
        var tex = new Texture2D(4, 4);
        var px  = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = c;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
    }
}
#endif
