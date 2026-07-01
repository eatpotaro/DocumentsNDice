using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class DiceParent : NetworkBehaviour
{
    public Dice dice;
    public NetworkObject netWorkObject;
}
