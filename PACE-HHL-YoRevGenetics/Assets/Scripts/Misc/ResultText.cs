using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultText : MonoBehaviour
{
    TextInfo textInfo;

    [SerializeField] TMP_Text resultText;

    private void Awake()
    {
        textInfo = new CultureInfo("en-US", false).TextInfo;
    }

    public void SetResultText(string result)
    {
        result ??= string.Empty;
        resultText.text = textInfo.ToTitleCase(result);
    }

    public void ResetResultText() => SetResultText("");
}
