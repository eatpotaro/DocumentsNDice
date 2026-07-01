using UnityEngine;

[CreateAssetMenu(fileName = "DiceData", menuName = "Scriptable Objects/DiceData")]
public class DiceData : ScriptableObject
{
    public enum ValueReadMode
    {
        highestVertex,
        highestFace
    };

    public enum FaceAssignMode
    {
        perEdge,
        perFace
    }

    public int numFaces;
    public int verticesPerFace;
    public ValueReadMode readMode;
    public FaceAssignMode faceAssignMode;
}
