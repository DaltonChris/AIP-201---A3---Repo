using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector2Int StartPos { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2Int EntryPos { get; set; }
    public Room(Vector2Int startPosition, int width, int height)
    {
        StartPos = startPosition;
        Width = width;
        Height = height;
    }
}
