using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
public class DiceTextController : MonoBehaviour
{

    TMP_Text text;
    RectTransform rectTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //get components in awake to avoid errors
        text = transform.GetComponent<TMP_Text>();
        rectTransform = transform.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPosition(Vector3[] verts)
    {
        //calculate normal
        Vector3 normal = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]).normalized;

        //average all vertices in faces to find center
        Vector3 center = new Vector3();
        foreach(Vector3 vert in verts)
        {
            center += vert;
        }
        center /= verts.Length;

        //add a lil bit of normal to make the text not overlap with the face
        rectTransform.localPosition = center + normal * 0.03f;
    }
    public void SetPosition(Vector3[] verts, int vertex)
    {
        //calculate normal
        Vector3 normal = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]).normalized;

        //average all vertices in faces to find center
        Vector3 center = new Vector3();
        foreach(Vector3 vert in verts)
        {
            center += vert;
        }
        center /= verts.Length;

        Vector3 edgePos = verts[vertex];        

        //add a lil bit of normal to make the text not overlap with the face, + offset to bias it to a certian vertex for d4
        rectTransform.localPosition = (edgePos + center) / 2 + normal * 0.03f;
    }

    public void SetRotation(Vector3[] verts, int rotNum)
    {
        //calculate normal and set forward to -normal so text facing correct direction
        Vector3 normal = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]).normalized;
        rectTransform.forward = -normal;

        //rotate the transform a lil so it is oriented correctly for d4
        rectTransform.Rotate(0, 0, (rotNum % 3) * -120f + (rotNum >= 9 ? 90 : 0));
    }
    public void SetRotation(Vector3[] verts)
    {
        //calculate normal and set forward to -normal so text faces correct direction
        Vector3 normal = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]).normalized;
        rectTransform.forward = -normal;
    }

    public void SetText(String txt)
    {
        //set text to the txt
        text.text = txt;
    }
}
