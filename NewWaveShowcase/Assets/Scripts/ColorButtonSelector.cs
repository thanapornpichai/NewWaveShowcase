using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButtonSelector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ModelColorCustomizer customizer;
    [SerializeField] private string partId = "Body";

    [Header("Buttons")]
    [SerializeField] private List<Button> colorButtons = new List<Button>();

    [Header("Start Selected (Color ID)")]
    [SerializeField] private string startColorId = "Default";

    [Header("Scale")]
    [SerializeField] private float selectedScale = 1.5f;
    [SerializeField] private float normalScale = 1f;

    [Header("Animate (Optional)")]
    [SerializeField] private bool useDotween = true;
    [SerializeField] private float tweenSeconds = 0.2f;

    [Header("Runtime")]
    [SerializeField] private Button currentSelected;

    private void Awake()
    {
        if (colorButtons == null) colorButtons = new List<Button>();
    }

    private void Start()
    {
        ResetAllButtonScaleImmediate();

        SelectStartButtonById();
    }

    public void OnClickButtonOnly(Button clickedButton)
    {
        if (clickedButton == null) return;
        SelectButton(clickedButton);
    }

    public void OnClickColor(Button clickedButton, Color color)
    {
        if (clickedButton == null) return;

        if (customizer != null && !string.IsNullOrEmpty(partId))
            customizer.SetPartColor(partId, color);

        SelectButton(clickedButton);
    }

    public void SelectButton(Button clickedButton)
    {
        currentSelected = clickedButton;

        for (int i = 0; i < colorButtons.Count; i++)
        {
            var b = colorButtons[i];
            if (b == null) continue;

            float target = (b == clickedButton) ? selectedScale : normalScale;
            ApplyScale(b.transform, target);
        }
    }

    public void ResetAllButtonScaleImmediate()
    {
        currentSelected = null;

        for (int i = 0; i < colorButtons.Count; i++)
        {
            var b = colorButtons[i];
            if (b == null) continue;

            b.transform.localScale = Vector3.one * normalScale;
        }
    }

    private void SelectStartButtonById()
    {
        if (string.IsNullOrEmpty(startColorId)) return;

        for (int i = 0; i < colorButtons.Count; i++)
        {
            var b = colorButtons[i];
            if (b == null) continue;

            var hook = b.GetComponent<ColorButtonHook>();
            if (hook == null) continue;

            if (hook.ColorIdEquals(startColorId))
            {
                SelectButton(b);
                return;
            }
        }

        if (colorButtons.Count > 0 && colorButtons[0] != null)
            SelectButton(colorButtons[0]);
    }

    private void ApplyScale(Transform t, float targetScale)
    {
        if (t == null) return;

        if (!useDotween || tweenSeconds <= 0f)
        {
            t.localScale = Vector3.one * targetScale;
            return;
        }

        if (!TryDoTweenScale(t, targetScale, tweenSeconds))
        {
            t.localScale = Vector3.one * targetScale;
        }
    }

    private bool TryDoTweenScale(Transform t, float targetScale, float duration)
    {
        try
        {
            var extType = System.Type.GetType("DG.Tweening.ShortcutExtensions, DOTween");
            if (extType == null) return false;

            var method = extType.GetMethod("DOScale", new System.Type[]
            {
                typeof(Transform),
                typeof(float),
                typeof(float)
            });

            if (method == null) return false;

            TryDoTweenKill(t);

            method.Invoke(null, new object[] { t, targetScale, duration });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void TryDoTweenKill(Transform t)
    {
        try
        {
            var extType = System.Type.GetType("DG.Tweening.ShortcutExtensions, DOTween");
            if (extType == null) return;

            var method = extType.GetMethod("DOKill", new System.Type[]
            {
                typeof(Transform),
                typeof(bool)
            });

            if (method == null) return;

            method.Invoke(null, new object[] { t, false });
        }
        catch
        {
            
        }
    }
}