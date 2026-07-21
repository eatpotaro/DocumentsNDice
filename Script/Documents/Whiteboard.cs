using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;
using UnityEngine.Android;
using UnityEditor.PackageManager;
using UnityEngine.Rendering.Universal;

public class Whiteboard : Document, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image drawing;
    public Sprite sprite;
    public Texture2D tex;
    public Brush brush;
    public List<DrawPixelAction> DrawStack = new List<DrawPixelAction>();
    private List<Pixel> tempPixels = new List<Pixel>();
    public List<DrawPixelAction> RedoStack = new List<DrawPixelAction>();
    private int brushLength;

    public Texture2D previousFrame;

    public override string Serialize()
    {
        byte[] png = tex.EncodeToPNG();
        
        return $"W{GetTitle()}\n{Convert.ToBase64String(png)}";
    }

    public override string GetDocType()
    {
        return "W";
    }

    void Start()
    {
        base.Start();
        NetworkManager.Singleton.OnConnectionEvent += UpdateNewClient;
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            return;
        }

        NetworkManager.Singleton.OnConnectionEvent -= UpdateNewClient;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InitialiseTex();
    }
    
    public void InitialiseTex()
    {
        tex = new Texture2D(516, 234, TextureFormat.RGBA32, false);
        for(int i = 0; i < 516; i++)
        {
            for(int j = 0; j < 234; j++)
            {
                tex.SetPixel(i, j, new Color32(255, 255, 255, 255));
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        sprite = Sprite.Create(tex, new Rect(0, 0, 516, 234), Vector2.zero);

        drawing.sprite = sprite;
    }

    public override void Load(int key, string serialised)
    {
        if(serialised == "W")
        {
            return;
        }

        string[] parts = serialised.Substring(1).Split("\n");
        title.text = parts[0];

        byte[] bytes = Convert.FromBase64String(parts[1]);
        Texture2D texture = new Texture2D(516, 234);
        texture.LoadImage(bytes);    
        texture.Apply();
        tex = texture;
        tex.wrapMode = TextureWrapMode.Clamp;
        
        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        drawing.sprite = sprite;
        DocID = key;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        brushLength = 0;
        StartBrush();
    }

    public void StartBrush()
    {
        if(previousFrame == null)
        {
            previousFrame = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            previousFrame.wrapMode = TextureWrapMode.Clamp;
        }

        tempPixels = new List<Pixel>();
        previousFrame.CopyPixels(tex);
    }
    public void OnDrag(PointerEventData data)
    {
        if(brush.brushType != Brush.BrushType.draw && brush.brushType != Brush.BrushType.erase)
        {
            return;
        }

        if(tempPixels.Count >= 100)
        {
            EndBrush();
            brushLength++;
            StartBrush();
        }

        RedoStack = new List<DrawPixelAction>();

        int imageX = (int)((data.position.x - 100) * 0.3f);
        int imageY = (int)((data.position.y - 100) * 0.3f);

        int size = brush.GetSize() * 2;
        Color color = brush.GetColour();

        if(brush.brushType == Brush.BrushType.erase)
        {
            color = Color.white;
        }

        List<Pixel> pixels = Brush.DrawPixels(imageX, imageY, size, color, previousFrame);
        DrawStroke(pixels.ToArray());

        drawing.sprite = sprite;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndBrush();
    }

    public void EndBrush()
    {
        ServerDrawStrokeRPC(new DrawPixelAction(tempPixels, brushLength));
    }

    public void DrawStroke(Pixel[] pixels)
    {
        foreach(Pixel pixel in pixels)
        {
            tex.SetPixel(pixel.x, pixel.y, pixel.Color);
            tempPixels.Add(pixel);
        }
        tex.Apply();
    }
    public void DrawAltStroke(Pixel[] pixels)
    {
        foreach(Pixel pixel in pixels)
        {
            tex.SetPixel(pixel.x, pixel.y, pixel.prevColor);
            tempPixels.Add(pixel);
        }
        tex.Apply();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ServerDrawStrokeRPC(DrawPixelAction pixels)
    {
        DrawStack.Add(pixels);   
        foreach(Pixel pixel in pixels.pixels)
        {
            tex.SetPixel(pixel.x, pixel.y, pixel.Color);
        }
        tex.Apply();

        ServerApplyStrokeRPC(pixels.pixels);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if(brush.brushType != Brush.BrushType.fill)
        {
            return;
        }

        RedoStack = new List<DrawPixelAction>();

        int imageX = (int)((data.position.x - 100) * 0.3f);
        int imageY = (int)((data.position.y - 100) * 0.3f);
        
        Color color = brush.GetColour();

        if(color == previousFrame.GetPixel(imageX, imageY))
        {
            return;
        }

        List<Pixel> pixels = Brush.DrawFill(imageX, imageY, color, previousFrame);

        DrawStroke(pixels.ToArray());
        
        List<Pixel> tempPixels = new List<Pixel>();
        List<DrawPixelAction> actions = new List<DrawPixelAction>();
        for(int i = 0; i < pixels.Count; i++)
        {
            tempPixels.Add(pixels[i]);
            if(tempPixels.Count >= 100)
            {
                actions.Add(new DrawPixelAction(tempPixels, -1));
                tempPixels.Clear();
            }
        }

        if(tempPixels.Count != 0)
        {
            actions.Add(new DrawPixelAction(tempPixels, actions.Count));
        }
        actions[^1].SetLength(actions.Count);

        foreach(DrawPixelAction action in actions)
        {
            ServerDrawStrokeRPC(action);
        }

        drawing.sprite = sprite;        
    }

    public void Undo()
    {
        SendUndoRPC();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendUndoRPC()
    {
        if(DrawStack.Count == 0)
        {
            return;
        }

        int length = DrawStack[^1].GetLength();
        length = length == -1 ? 1 : length;
        DrawStack[^1].SetLength(-1);

        for(int i = 0; i <= length; i++)
        { 
            DrawAltStroke(DrawStack[^1].pixels);

            ServerApplyAltStrokeRPC(DrawStack[^1].pixels);

            RedoStack.Insert(0, DrawStack[^1]);

            DrawStack.RemoveAt(DrawStack.Count - 1);
        }

        RedoStack[0].SetLength(length);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ServerApplyStrokeRPC(Pixel[] pixels)
    {
        DrawStroke(pixels);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ServerApplyAltStrokeRPC(Pixel[] pixels)
    {
        DrawAltStroke(pixels);
    }

    public void Redo()
    {
        SendRedoRPC();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendRedoRPC()
    {
        if(RedoStack.Count == 0)
        {
            return;
        }

        int length = RedoStack[0].GetLength();
        length = length == -1 ? 1 : length;
        RedoStack[0].SetLength(-1);
        
        for(int i = 0; i <= length; i++)
        { 
            DrawStroke(RedoStack[0].pixels);
            ServerDrawStrokeRPC(RedoStack[0]);

            RedoStack.RemoveAt(0);
        }

        DrawStack[^1].SetLength(length);
    }

    public void UpdateNewClient(NetworkManager manager, ConnectionEventData data)
    {
        if(!IsServer)
        {
            return;
        }

        if(data.EventType != ConnectionEvent.ClientConnected)
        {
            return;
        }

        foreach(DrawPixelAction pixellist in DrawStack)
        {
            ApplyUpdatesRPC(data.ClientId, pixellist.pixels);
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission =RpcInvokePermission.Everyone)]
    public void ApplyUpdatesRPC(ulong client, Pixel[] pixelList)
    {
        if(NetworkManager.Singleton.LocalClientId != client)
        {
            return;
        }
         
        if(tex == null)
        {
            InitialiseTex();
        }

        DrawStroke(pixelList);
    }
}

public struct Pixel : INetworkSerializable
{
    public int x;
    public int y;
    public Color32 Color;
    public Color32 prevColor;

    public Pixel(int xnew, int ynew, Color32 colornew)
    {
        x = xnew;
        y = ynew;
        Color = colornew;
        prevColor = new Color32(255, 255, 255, 255);
    }

    public Pixel(int xnew, int ynew, Color32 colornew, Color32 colorprev)
    {
        x = xnew;
        y = ynew;
        Color = colornew;
        prevColor = colorprev;
    }

    public override string ToString()
    {
        return $"|{x}|{y}|{Color.r}|{Color.g}|{Color.b}|{prevColor.r}|{prevColor.g}|{prevColor.b}";
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref prevColor);
    }
}

public class DrawPixelAction : INetworkSerializable
{
    public Pixel[] pixels;
    public int length;

    public DrawPixelAction()
    {
        pixels = new Pixel[0];
        length = -1;
    }
    public DrawPixelAction(List<Pixel> pixelList)
    {
        pixels = pixelList.ToArray();
        length = -1;
    }

    public DrawPixelAction(List<Pixel> pixelList, int len)
    {
        pixels = pixelList.ToArray();
        length = len;
    }
    
    
    public void SetLength(int newLen)
    {
        length = newLen;
    }

    public int GetLength()
    {
        return length;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref pixels);
        serializer.SerializeValue(ref length);
    }
}
