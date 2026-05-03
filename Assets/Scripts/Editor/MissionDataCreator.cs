#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Menu: ADL > Create Mission Assets
// Creates the three MissionData ScriptableObjects in Assets/Data/
public static class MissionDataCreator
{
    [MenuItem("ADL/Create Mission Assets")]
    static void Create()
    {
        CreateMission("brush", "Morning Routine",  "Brush your teeth",
            new[] { "toothbrush", "toothpaste", "cup" });

        CreateMission("leave", "Leaving the House", "Get ready to go out",
            new[] { "shoe", "keys", "bag", "glasses" });

        CreateMission("meds", "Medication Time", "Take your medicine",
            new[] { "medication", "bottle", "banana" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Mission assets created in Assets/Data/");
    }

    static void CreateMission(string id, string title, string subtitle, string[] targets)
    {
        string path = string.Format("Assets/Data/{0}_mission.asset", id);
        var existing = AssetDatabase.LoadAssetAtPath<MissionData>(path);
        if (existing != null)
        {
            existing.missionId   = id;
            existing.title       = title;
            existing.subtitle    = subtitle;
            existing.targetItems = targets;
            EditorUtility.SetDirty(existing);
            return;
        }
        var asset = ScriptableObject.CreateInstance<MissionData>();
        asset.missionId   = id;
        asset.title       = title;
        asset.subtitle    = subtitle;
        asset.targetItems = targets;
        AssetDatabase.CreateAsset(asset, path);
    }
}
#endif
