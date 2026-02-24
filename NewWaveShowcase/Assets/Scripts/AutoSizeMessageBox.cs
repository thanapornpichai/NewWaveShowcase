using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AutoSizeMessageBox : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform buttonRect;

    [Header("Padding")]
    [SerializeField] private Vector2 padding = new Vector2(40, 30);
    [SerializeField] private float spacing = 20f;

    public void SetMessage(string msg)
    {
        messageText.text = msg;

        LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.rectTransform);

        Vector2 textSize = messageText.GetPreferredValues(msg);

        float bgWidth = textSize.x + padding.x;
        float bgHeight = textSize.y + padding.y;

        background.sizeDelta = new Vector2(bgWidth, bgHeight);

        Vector2 buttonPos = buttonRect.anchoredPosition;
        buttonPos.y = -bgHeight - spacing;
        buttonRect.anchoredPosition = buttonPos;
    }
}