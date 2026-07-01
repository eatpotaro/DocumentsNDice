using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Mathematics;

public class DiceCreator : NetworkBehaviour
{
    public DiceParent[] AvailableDice; 
    public TMP_Dropdown diceTypeDropdown;
    public TMP_InputField diceModifierInput;
    public TMP_InputField diceMultiplierInput;
    public Transform playerCam;
    private int modifier;
    private float multiplier;
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnDiceServerRPC(Vector3 pos, int mod, float mult, int diceType)
    {   
        //get diceparent instance and spawn it
        DiceParent newDiceParent = Instantiate(AvailableDice[diceType]);
        newDiceParent.netWorkObject.Spawn();

        //get the dice componant and spawn it in front of player
        Dice newDice = newDiceParent.dice;
        newDice.transform.SetPositionAndRotation(pos, quaternion.identity);

        //set properties of dice
        newDice.SetDefaults(mult, mod);
    }

    public void SpawnDice()
    {
        if (!IsOwner) 
        {
            return;
        }

        Vector3 position = playerCam.position + playerCam.forward * 5f;
        SpawnDiceServerRPC(position, modifier, multiplier, diceTypeDropdown.value);
    }
    public void UpdateDiceModifier()
    {
        string diceModfierText = diceModifierInput.text;
        int diceModifierNew;
        try
        {
            diceModifierNew = int.Parse(diceModfierText);
        }
        catch
        {
            modifier = 0;
            return;
        }
        modifier = diceModifierNew;
    }
    public void UpdateDiceMultiplier()
    {
        string diceMultiplierText = diceMultiplierInput.text;
        float diceMultiplierNew;
        try
        {
            diceMultiplierNew = float.Parse(diceMultiplierText);
        }
        catch
        {
            multiplier = 1;
            return;
        }
        multiplier = diceMultiplierNew == 0f ? 1f : diceMultiplierNew;
    }
}
