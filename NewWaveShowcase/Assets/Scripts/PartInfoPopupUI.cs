using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartInfoPopupUI : MonoBehaviour
{
    [Header("UI Refs")]
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;
    public TMP_Text descText;
    public Image iconImage;

    [Header("Optional")]
    public Button closeButton;
    public bool clickOutsideToClose = true;

    [Header("Positioning")]
    public RectTransform popupRect;
    public Vector2 screenOffset = new Vector2(18, 18);

    private Canvas rootCanvas;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (closeButton != null) closeButton.onClick.AddListener(Hide);

        HideInstant();
    }

    public bool IsOpen =>
        canvasGroup != null && canvasGroup.alpha > 0.9f && canvasGroup.blocksRaycasts;

    public void Show(PartInfo info, Vector2 screenPos)
    {
        if (info == null) return;

        if (titleText != null) titleText.text = info.title;
        if (descText != null) descText.text = info.description;

        if (iconImage != null)
        {
            if (info.icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = info.icon;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (popupRect != null && rootCanvas != null)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            Camera uiCam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos + screenOffset,
                uiCam,
                out var localPos
            );

            popupRect.anchoredPosition = localPos;
        }

        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    void HideInstant() => SetVisible(false);

    void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void OnBackgroundClicked()
    {
        if (clickOutsideToClose) Hide();
    }
}
