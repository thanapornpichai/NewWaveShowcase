using UnityEngine;
using UnityEngine.UI;

public class PointerToggleButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button toggleButton;

    [Header("Button Icon (Optional)")]
    [SerializeField] private Image buttonIcon;

    [SerializeField] private Sprite iconOn;
    [SerializeField] private Sprite iconOff;

    [Header("Targets (Pointers)")]
    [SerializeField] private GameObject[] pointers;

    [Header("State")]
    [SerializeField] private bool startOn = false;

    public bool IsOn { get; private set; }

    private void Awake()
    {
        if (toggleButton == null)
            toggleButton = GetComponent<Button>();

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(Toggle);
            toggleButton.onClick.AddListener(Toggle);
        }

        SetState(startOn, instant: true);
    }

    public void Toggle()
    {
        SetState(!IsOn, instant: false);
    }

    public void SetOn() => SetState(true, instant: false);
    public void SetOff() => SetState(false, instant: false);

    public void SetState(bool on, bool instant)
    {
        IsOn = on;

        if (pointers != null)
        {
            for (int i = 0; i < pointers.Length; i++)
            {
                if (pointers[i] != null)
                    pointers[i].SetActive(on);
            }
        }

        if (buttonIcon != null)
        {
            if (on && iconOn != null) buttonIcon.sprite = iconOn;
            if (!on && iconOff != null) buttonIcon.sprite = iconOff;
        }
    }
}