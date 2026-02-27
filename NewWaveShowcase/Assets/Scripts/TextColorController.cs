using UnityEngine;
using TMPro;

public class TextColorController : MonoBehaviour
{
    [Header("Target Text")]
    public TMP_Text targetText;

    [Header("Colors")]
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color blackColor = Color.black;
    public Color blueColor = new Color(0f, 0.4f, 1f);

    public enum TextColorState
    {
        Orange,
        Black,
        Blue
    }

    public void SetOrange()
    {
        SetColor(TextColorState.Orange);
    }

    public void SetBlack()
    {
        SetColor(TextColorState.Black);
    }

    public void SetBlue()
    {
        SetColor(TextColorState.Blue);
    }

    public void SetColor(TextColorState state)
    {
        if (targetText == null) return;

        switch (state)
        {
            case TextColorState.Orange:
                targetText.color = orangeColor;
                break;

            case TextColorState.Black:
                targetText.color = blackColor;
                break;

            case TextColorState.Blue:
                targetText.color = blueColor;
                break;
        }
    }
}