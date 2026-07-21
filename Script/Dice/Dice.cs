using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;

public class Dice : Interactable
{
    //for creating dice
    private Mesh mesh;
    private List<Vector3[]> faces = new List<Vector3[]>();
    public DiceTextController[] diceTexts;
    public DiceData diceData;
    private NetworkVariable<float> multiplier = new();
    private NetworkVariable<int> modifier = new();
    private Transform playerCam;

    //for rolling dice
    public Rigidbody rb;
    public TMP_Text valueText;
    public static int[] naturalDice = new int[2]{20, 100};  //nice natural dice

    public override void OnNetworkSpawn()
    {
        SetText();
        modifier.OnValueChanged += (_, __) => SetText();
        multiplier.OnValueChanged += (_, __) => SetText();
    }

    public void SetText()
    {
        //get the mesh and verticies
        if(playerCam == null)
        {
            try
            {
                playerCam = Camera.main.transform;
            }
            catch
            {
                TryGetCamera();
            }
        }
        mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        rb = transform.GetComponent<Rigidbody>();

        //foreeach face make a new vector3 array for each unique vertex
        Vector3[] faceVertces = new Vector3[diceData.verticesPerFace];
        int faceVertexNum = 0;

        //run through each vertex and assign
        foreach(Vector3 vertex in mesh.vertices)
        {

            if(faceVertces.Contains<Vector3>(vertex))
            {  
                continue;
                //already contains this vertex, so skip and dont incremend the facevertexnum 
            }
            faceVertces[faceVertexNum % diceData.verticesPerFace] = vertex;

            //if the faceVertex array is full, add it to the faces array
            if((faceVertexNum + 1) % diceData.verticesPerFace == 0)
            {
                //adds a copy so changes arent linked
                faces.Add(faceVertces.ToArray<Vector3>());
                faceVertces = new Vector3[diceData.verticesPerFace];
            }

            faceVertexNum++;
        }

        //assigns the dice texts to right place
        for(int i = 0; i < diceTexts.Length; i++)
        {               
            int faceindex = i;
            int faceValue;
            switch(diceData.faceAssignMode)
            {
                default:              
                    //one text per face  
                    diceTexts[i].SetPosition(faces[faceindex]);
                    
                    faceValue = Mathf.FloorToInt((float)(faceindex + 1f + modifier.Value) * multiplier.Value);
                    diceTexts[i].SetText(faceValue.ToString());

                    diceTexts[i].SetRotation(faces[faceindex]);
                    break;
                case DiceData.FaceAssignMode.perEdge:
                    //d4
                    //hardcode values because im lazy as shit
                    int[] d4Val = new int[12]{ 4, 3, 2, 4, 1, 3, 4, 2, 1, 2, 3, 1};              
      
                    faceindex = i / 3;

                    diceTexts[i].SetPosition(faces[faceindex], i % 3);
                    faceValue = Mathf.FloorToInt((float)(d4Val[i] + modifier.Value) * multiplier.Value);

                    diceTexts[i].SetText(faceValue.ToString());
                    diceTexts[i].SetRotation(faces[faceindex], i);
                    break;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        int highestFaceID = 0;
        float bestdot = 0;
        float dotResult = 0;
        Vector3 normal;
        switch(diceData.readMode)
        {
            default:
                for(int i = 0; i < faces.Count; i++)
                {
                    //if new dot result is more negative, then replace selected face with that one
                    Vector3 posA = transform.rotation * faces[i][0];
                    Vector3 posB = transform.rotation * faces[i][1];
                    Vector3 posC = transform.rotation * faces[i][2];

                    normal = Vector3.Cross((posB - posA).normalized, (posC - posA).normalized).normalized;
                    dotResult = Vector3.Dot(normal, Vector3.up);
                    if(dotResult > bestdot)
                    {
                        highestFaceID = i;
                        bestdot = dotResult;
                    }
                }
                break;
            case DiceData.ValueReadMode.highestVertex:
                for(int i = 0; i < faces.Count; i++)
                {
                    //if new dot result is more negative, then replace selected face with that one
                    Vector3 posA = transform.rotation * faces[i][0];
                    Vector3 posB = transform.rotation * faces[i][1];
                    Vector3 posC = transform.rotation * faces[i][2];

                    normal = Vector3.Cross((posB - posA).normalized, (posC - posA).normalized).normalized;
                    dotResult = Vector3.Dot(normal, Vector3.up);
                    if(dotResult < bestdot)
                    {
                        highestFaceID = i;
                        bestdot = dotResult;
                    }
                }
                break;
        }

        int diceNum = Mathf.FloorToInt((float)(highestFaceID + 1 + modifier.Value) * multiplier.Value);

        valueText.text = diceNum.ToString();
        valueText.rectTransform.position = transform.position + Vector3.up * (diceData.numFaces == 100 ? 10f : 1.5f);
        if(playerCam != null)
        {
            valueText.rectTransform.LookAt(playerCam);
        }
        valueText.rectTransform.forward = -valueText.rectTransform.forward;

        if(!naturalDice.Contains(diceData.numFaces))
        {
            return;
        }
        
        valueText.color = Color.white;
        if(highestFaceID == 0)
        {
            valueText.color = Color.red;
            valueText.text = "Nat 1!";
        }
        else if(highestFaceID == diceData.numFaces - 1)
        {
            
            valueText.color = Color.green;
            valueText.text = $"Nat {diceData.numFaces}!";
        }
    }

    public override void OnInteract()
    {
        RollServerRPC();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RollServerRPC()
    {
        rb.AddForce(Vector3.up * 1000f);
        rb.AddTorque(Random.Range(100, 2000f), Random.Range(100, 2000f), Random.Range(100, 2000f));
    }
    public override void AltInteract()
    {
        SendDestroyRPC();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendDestroyRPC()
    {
        NetworkBehaviour.Destroy(transform.parent.gameObject);
    }

    public void SetDefaults(float multiplier = 1, int modifier = 0)
    {
        this.multiplier.Value = multiplier;
        this.modifier.Value = modifier;
    }

    public void TryGetCamera()
    {
        if(playerCam != null)
        {
            return;
        }
        try
        {
            playerCam = Camera.main.transform;
        }
        catch
        {
            Invoke(nameof(TryGetCamera), 1f);
        }
    }
}
