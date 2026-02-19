using UnityEngine;

public class PartInfo : MonoBehaviour
{
    [Header("ID (optional)")]
    public string partId;

    [Header("Content")]
    public string title;

    [TextArea(2, 8)]
    public string description;

    public Sprite icon;
}
