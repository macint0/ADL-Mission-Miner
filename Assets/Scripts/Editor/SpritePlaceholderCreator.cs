#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

// Menu: ADL > Create Placeholder Sprites
// Generates 512x512 colored placeholder PNGs in Assets/Resources/Items/
// for each item id, so the game runs before final art arrives.
public static class SpritePlaceholderCreator
{
    static readonly string[] ItemIds =
    {
        "toothbrush", "toothpaste", "cup",
        "soap", "comb", "hairbrush",
        "shoe", "keys", "bag", "glasses",
        "medication", "bottle", "banana",
        "hook", "sparkle"
    };

    static readonly Color[] Palette =
    {
        GameColors.Peach, GameColors.Mint, GameColors.Sky,
        GameColors.Rose, GameColors.Lavender, GameColors.Butter,
        GameColors.PeachDeep, GameColors.MintDeep, GameColors.SkyDeep,
        GameColors.RoseDeep, GameColors.LavDeep, GameColors.ButterDeep,
        GameColors.MeterYellow, GameColors.InkSoft, GameColors.MeterGreen,
    };

    [MenuItem("ADL/Create Placeholder Sprites")]
    static void CreateAll()
    {
        string dir = "Assets/Resources/Items";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        for (int i = 0; i < ItemIds.Length; i++)
        {
            string id   = ItemIds[i];
            string path = dir + "/" + id + ".png";
            if (File.Exists(path)) continue;

            Color col = Palette[i % Palette.Length];
            var tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            // Fill with color
            var pixels = new Color[512 * 512];
            for (int p = 0; p < pixels.Length; p++) pixels[p] = col;
            // White center label area
            for (int y = 180; y < 332; y++)
                for (int x = 80; x < 432; x++)
                    pixels[y * 512 + x] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        AssetDatabase.Refresh();

        // Set all as Sprite type
        foreach (var id in ItemIds)
        {
            string path = "Assets/Resources/Items/" + id + ".png";
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100f;
            imp.spritePivot = new Vector2(id == "hook" ? 0.5f : 0.5f, id == "hook" ? 1f : 0.5f);
            imp.SaveAndReimport();
        }

        Debug.Log("Placeholder sprites created in Assets/Resources/Items/");
    }
}
#endif
