using System.Collections;
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

    [Header("Rects")]
    public RectTransform moveRoot;

    public RectTransform layoutRoot;

    [Header("Positioning")]
    public Vector2 screenOffset = new Vector2(18, 18);

    [Header("Orientation Position References")]
    public RectTransform landscapeReference;
    public RectTransform portraitReference;

    [Header("Landscape Override X")]
    public bool overrideLandscapeX = true;

    public float landscapeTargetX = -1351f;

    [Header("Animation (Smooth)")]
    public float slideDuration = 0.75f;
    public Ease showEase = Ease.OutQuint;
    public Ease hideEase = Ease.InQuint;

    public float slideFromOffset = 520f;

    [Range(0.1f, 1f)] public float fadeShowRatio = 0.85f;
    [Range(0.1f, 1f)] public float fadeHideRatio = 0.60f;

    [Header("Layout Rebuild (Resize Popup)")]
    public bool forceRebuildLayout = true;
    public bool rebuildAtEndOfFrame = true;

    [Header("Debug")]
    public bool debugLog = false;

    private Canvas rootCanvas;
    private Vector2 targetAnchoredPos;

    private Sequence seq;
    private Coroutine rebuildCo;

    private bool IsLandscape => Screen.width > Screen.height;

    public bool IsOpen
    {
        get
        {
            return canvasGroup != null &&
                   canvasGroup.alpha > 0.9f &&
                   canvasGroup.blocksRaycasts;
        }
    }

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (moveRoot == null) moveRoot = GetComponent<RectTransform>();
        if (layoutRoot == null) layoutRoot = moveRoot;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
            closeButton.gameObject.SetActive(false);
        }

        HideInstant();
    }

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
            else iconImage.gameObject.SetActive(false);
        }

        if (forceRebuildLayout)
        {
            ForceRebuildNow();

            if (rebuildAtEndOfFrame)
            {
                if (rebuildCo != null) StopCoroutine(rebuildCo);
                rebuildCo = StartCoroutine(RebuildEndOfFrame());
            }
        }

        ResolveTargetPosition(screenPos);

        if (debugLog)
        {
            Debug.Log($"[PartInfoPopupUI] IsLandscape={IsLandscape} targetAnchoredPos={targetAnchoredPos}");
        }

        PlayShowAnimation();
    }

    public void Hide() => PlayHideAnimation();

    private Vector2 GetAnchoredPosFromReference(RectTransform reference)
    {
        if (reference == null || moveRoot == null || rootCanvas == null)
            return Vector2.zero;

        RectTransform parentRect = moveRoot.parent as RectTransform;
        if (parentRect == null)
            return moveRoot.anchoredPosition;

        Camera uiCam =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, reference.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPoint,
            uiCam,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private void ResolveTargetPosition(Vector2 screenPos)
    {
        if (moveRoot == null || rootCanvas == null) return;

        bool landscape = IsLandscape;

        if (landscape && landscapeReference != null)
        {
            targetAnchoredPos = GetAnchoredPosFromReference(landscapeReference);

            if (overrideLandscapeX)
                targetAnchoredPos.x = landscapeTargetX;

            return;
        }

        if (!landscape && portraitReference != null)
        {
            targetAnchoredPos = GetAnchoredPosFromReference(portraitReference);
            return;
        }

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Camera uiCam =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos + screenOffset,
            uiCam,
            out Vector2 localPos
        );

        targetAnchoredPos = localPos;
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

        if (moveRoot != null)
        {
            moveRoot.anchoredPosition = new Vector2(Screen.width, 0);
        }
    }

    void PlayShowAnimation()
    {
        if (moveRoot == null || canvasGroup == null) return;

        KillSeq();

        Vector2 endPos;
        if (IsLandscape)
            endPos = new Vector2(landscapeTargetX, moveRoot.anchoredPosition.y);
        else
            endPos = new Vector2(0f, moveRoot.anchoredPosition.y);

        float dir = IsLandscape ? -1f : 1f;
        Vector2 fromPos = endPos + new Vector2(slideFromOffset * dir, 0);

        moveRoot.anchoredPosition = fromPos;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        seq = DOTween.Sequence();
        seq.Join(moveRoot.DOAnchorPos(endPos, slideDuration).SetEase(showEase));
        seq.Join(canvasGroup.DOFade(1f, slideDuration * fadeShowRatio).SetEase(Ease.OutSine));
    }

    void PlayHideAnimation()
    {
        if (moveRoot == null || canvasGroup == null) return;

        KillSeq();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Vector2 endPos;
        if (IsLandscape)
            endPos = new Vector2(landscapeTargetX, moveRoot.anchoredPosition.y);
        else
            endPos = new Vector2(0f, moveRoot.anchoredPosition.y);

        float dir = IsLandscape ? -1f : 1f;
        Vector2 toPos = endPos + new Vector2(slideFromOffset * dir, 0);

        seq = DOTween.Sequence();

        seq.Join(moveRoot.DOAnchorPos(toPos, slideDuration).SetEase(hideEase));
        seq.Join(canvasGroup.DOFade(0f, slideDuration * fadeHideRatio).SetEase(Ease.InSine));

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

        if (moveRoot != null) moveRoot.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
    }

    public void OnBackgroundClicked()
    {
        if (clickOutsideToClose)
            Hide();
    }

    void ForceRebuildNow()
    {
        if (titleText != null) titleText.ForceMeshUpdate();
        if (descText != null) descText.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();

        if (layoutRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

        Canvas.ForceUpdateCanvases();
    }

    IEnumerator RebuildEndOfFrame()
    {
        yield return null;
        ForceRebuildNow();
        rebuildCo = null;
    }
}