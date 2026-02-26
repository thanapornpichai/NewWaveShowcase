using UnityEngine;

public class OrientationPositionSwitcher : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform target;

    [Header("Position References")]
    [SerializeField] private RectTransform landscapeReference;
    [SerializeField] private RectTransform portraitReference;

    private bool isLandscape;

    private void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        UpdateOrientation(true);
    }

    private void Update()
    {
        UpdateOrientation(false);
    }

    private void UpdateOrientation(bool force)
    {
        bool currentLandscape = Screen.width > Screen.height;

        if (force || currentLandscape != isLandscape)
        {
            isLandscape = currentLandscape;

            if (isLandscape && landscapeReference != null)
            {
                ApplyPosition(landscapeReference);
            }
            else if (!isLandscape && portraitReference != null)
            {
                ApplyPosition(portraitReference);
            }
        }
    }

    private void ApplyPosition(RectTransform reference)
    {
        target.anchoredPosition = reference.anchoredPosition;
        target.localScale = reference.localScale;
        target.localRotation = reference.localRotation;
    }
}