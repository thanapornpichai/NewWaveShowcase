using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ColorButtonHook : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ColorButtonSelector selector;

    [Header("ID")]
    [SerializeField] private string colorId = "Default";

    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        if (_btn != null)
        {
            _btn.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (_btn != null)
        {
            _btn.onClick.RemoveListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (selector == null) return;

        selector.OnClickButtonOnly(_btn);
    }

    public bool ColorIdEquals(string id)
    {
        return string.Equals(colorId, id, System.StringComparison.OrdinalIgnoreCase);
    }
}