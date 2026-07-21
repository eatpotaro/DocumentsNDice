using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class Brush : MonoBehaviour
{
    private int r;
    private int g;
    private int b;
    private int size;
    public enum BrushType
    {
        draw,
        erase,
        fill
    }

    public BrushType brushType;
    public TMP_Text colourText;
    public TMP_Text toolText;
    public Image colourImage;
    public Color GetColour()
    {
        return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
    }
    public void SetR(Single value)
    {
        r = ((int)value);
    }    
    public void SetG(Single value)
    {
        g = ((int)value);
    }    
    public void SetB(Single value)
    {
        b = ((int)value);
    }
    public void SetSize(Single value)
    {
        size = (int)value;
    }
    public int GetSize()
    {
        return size;
    }
    public void UpdateText()
    {
        colourText.text = $"R:{r}\nG:{g}\nB:{b}\nSize:{size}";
        colourImage.color = GetColour();
    }
    public void SetBrushType(int type)
    {
        switch(type)
        {
            default:
                {
                    brushType = BrushType.draw;
                    toolText.text = $"Current Tool:\nDraw";
                    break;
                }
            case 1:
                {
                    brushType = BrushType.erase;
                    toolText.text = $"Current Tool:\nErase";
                    break;                    
                }
            case 2:
                {
                    brushType = BrushType.fill;
                    toolText.text = $"Current Tool:\nFill";
                    break;                    
                }
        }
    }
    void Start()
    {
        size = 1;
        UpdateText();
        SetBrushType(0);
    }

    public static List<Pixel> DrawPixels(int x, int y, int size, Color color, Texture2D previousFrame)
    {
        List<Pixel> pixels = new List<Pixel>();
        List<(int, int)> pos = new List<(int, int)>();
        for(int i = (x - size); i < x + size; i++)
        {
            for(int j = (y - size); j < y + size; j++)
            {
                if(pos.Contains((i, j)))
                {
                    continue;
                }

                if(Math.Pow(i - x, 2) + Math.Pow(j - y, 2) < Math.Pow(size, 2))
                {
                    Pixel current = new Pixel(i, j, color);
                    current.prevColor = previousFrame.GetPixel(i, j);
                    pixels.Add(current);

                    pos.Add((i, j));
                }
            }
        }
        return pixels;
    }

    public static List<Pixel> DrawFill(int x, int y, Color color, Texture2D previousFrame)
    {
        List<Pixel> pixels = new List<Pixel>();
        List<(int, int)> pos = new List<(int, int)>();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        Color startColor = previousFrame.GetPixel(x, y);
        pos.Add((x, y));
        visited.Add((x, y));

        while(pos.Count > 0)
        {
            (int i, int j) = pos[0];
            pos.RemoveAt(0);

            if(i < 0 || i >= previousFrame.width || j < 0 || j >= previousFrame.height)
            {
                continue;
            }

            if(previousFrame.GetPixel(i, j) == startColor)
            {
                pixels.Add(new Pixel(i, j, color, startColor));
                
                if(!visited.Contains((i-1, j)))
                {
                    pos.Add((i-1, j));     
                    visited.Add((i-1, j));               
                }

                if(!visited.Contains((i+1, j)))
                {
                    pos.Add((i+1, j));  
                    visited.Add((i+1, j));                   
                }

                if(!visited.Contains((i, j-1)))
                {
                    pos.Add((i, j-1));     
                    visited.Add((i, j-1));                
                }

                if(!visited.Contains((i, j+1)))
                {
                    pos.Add((i, j+1));  
                    visited.Add((i, j+1));                   
                }
            }
        }
        return pixels;
    }
}
