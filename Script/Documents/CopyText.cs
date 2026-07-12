using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

public class CopyText : MonoBehaviour
{
    public TMP_Text target;
    public TMP_Text original;

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        target.text = original.text;
    }
}
