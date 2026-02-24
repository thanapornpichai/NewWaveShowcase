using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AutoResizeTextBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector2 padding = new Vector2(40f, 30f);
    [SerializeField] private float maxWidth = 600f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetText(string message)
    {
        text.text = message;
        Resize();
    }

    private void Resize()
    {
        text.ForceMeshUpdate();

        Vector2 preferredSize = text.GetPreferredValues(maxWidth, 0f);

        float width = Mathf.Min(preferredSize.x, maxWidth);
        float height = preferredSize.y;

        rect.sizeDelta = new Vector2(
            width + padding.x,
            height + padding.y
        );
    }
}