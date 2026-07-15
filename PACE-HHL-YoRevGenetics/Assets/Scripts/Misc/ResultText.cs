using System.Globalization;
using TMPro;
using UnityEngine;

public class ResultText : MonoBehaviour
{
    TextInfo TextInfo
    {
        get
        {
            _textInfo ??= new CultureInfo("en-US", false).TextInfo;
            return _textInfo;
        }
    }
    TextInfo _textInfo;

    [SerializeField] TMP_Text resultText;

    public void SetResultText(string name, string result)
    {
        result ??= name ?? "";
        resultText.text = TextInfo.ToTitleCase(result);
    }

    public void SetResultText(string result) => SetResultText(null, result);

    public void ResetResultText() => SetResultText("");
}
