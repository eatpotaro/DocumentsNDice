using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCreator : MonoBehaviour
{
    public TMP_InputField IpField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ValidateIpField()
    {
        string startText = IpField.text;
        string[] parts = startText.Split(new char[]{'.'});
        int[] finalAddress = new int[4];

        for(int i = 0; i < parts.Length; i++)
        {
            int newpart = 0;
            bool success = int.TryParse(parts[i], out newpart);

            if(!success)
            {
                finalAddress[i] = 0;
                continue;
            }

            if(newpart < 0 || newpart > 255)
            {
                finalAddress[i] = 0;  
                continue;
            }

            finalAddress[i] = newpart;
        }

        IpField.text = $"{finalAddress[0]}.{finalAddress[1]}.{finalAddress[2]}.{finalAddress[3]}";
    }
}
