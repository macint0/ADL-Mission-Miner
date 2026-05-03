using UnityEngine;

[CreateAssetMenu(menuName = "ADL/Mission Data")]
public class MissionData : ScriptableObject
{
    public string missionId;
    public string title;
    public string subtitle;
    public string[] targetItems;
}
