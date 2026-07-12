using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class BasicDocument : Document
{
    public TMP_InputField BodyText;

    public override string Serialize()
    {
        return $"B{GetTitle()}\n{BodyText.text}";
    }
    public override string GetDocType()
    {
        return "B";
    }

    public override void Load(int key, string serialised)
    {
        string[] parts = serialised.Substring(1).Split("\n");
        title.text = parts[0];
        BodyText.text = parts[1];
        DocID = key;
    }
}
