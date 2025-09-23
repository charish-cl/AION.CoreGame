
using System;
using UnityEngine;

public enum MapCellType
{
    Obstacle,
    Road,
}
[Serializable]
public class MapCell
{
    //这里的Position是左下角的坐标
    public Vector2 Position;
    public MapCellType cellType;

    public MapCell(Vector2 position, MapCellType cellType)
    {
        this.Position = position;
        this.cellType = cellType;
    }
}
