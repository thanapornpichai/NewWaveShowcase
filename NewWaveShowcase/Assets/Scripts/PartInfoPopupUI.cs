using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Animation (Smooth)")]
    public float slideDuration = 0.75f;

    public Ease showEase = Ease.OutQuint;

    public Ease hideEase = Ease.InQuint;

    public float slideFromOffset = 520f;

    [Range(0.1f, 1f)] public float fadeShowRatio = 0.85f;
    [Range(0.1f, 1f)] public float fadeHideRatio = 0.60f;

    private Canvas rootCanvas;
    private Vector2 targetAnchoredPos;

    private Sequence seq; 

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
            closeButton.gameObject.SetActive(false);
        }

        HideInstant();
    }

    public bool IsOpen =>
        canvasGroup != null &&
        canvasGroup.alpha > 0.9f &&
        canvasGroup.blocksRaycasts;

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

            Camera uiCam =
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : rootCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos + screenOffset,
                uiCam,
                out Vector2 localPos
            );

            targetAnchoredPos = localPos;
        }

        PlayShowAnimation();
    }

    public void Hide()
    {
        PlayHideAnimation();
    }

    void HideInstant()
    {
        KillSeq();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (popupRect != null)
        {
            popupRect.anchoredPosition = new Vector2(Screen.width, 0);
        }
    }

    void PlayShowAnimation()
    {
        if (popupRect == null || canvasGroup == null) return;

        KillSeq();

        popupRect.anchoredPosition = targetAnchoredPos + new Vector2(slideFromOffset, 0);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        seq = DOTween.Sequence();

        seq.Join(
            popupRect.DOAnchorPos(targetAnchoredPos, slideDuration)
                .SetEase(showEase)
        );

        seq.Join(
            canvasGroup.DOFade(1f, slideDuration * fadeShowRatio)
                .SetEase(Ease.OutSine)
        );
    }

    void PlayHideAnimation()
    {
        if (popupRect == null || canvasGroup == null) return;

        KillSeq();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        seq = DOTween.Sequence();

        seq.Join(
            popupRect.DOAnchorPos(targetAnchoredPos + new Vector2(slideFromOffset, 0), slideDuration)
                .SetEase(hideEase)
        );

        seq.Join(
            canvasGroup.DOFade(0f, slideDuration * fadeHideRatio)
                .SetEase(Ease.InSine)
        );

        seq.OnComplete(() =>
        {
            if (closeButton != null)
                closeButton.gameObject.SetActive(false);
        });
    }

    void KillSeq()
    {
        if (seq != null && seq.IsActive())
            seq.Kill();

        seq = null;

        if (popupRect != null) popupRect.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
    }

    public void OnBackgroundClicked()
    {
        if (clickOutsideToClose)
            Hide();
    }
}