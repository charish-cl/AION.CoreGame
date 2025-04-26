
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

    //这里的Key是左下角的坐标乘以10000，保证唯一性
    public long Key;
    public MapCell(Vector2 position, MapCellType cellType)
    {
        this.Position = position;
        this.cellType = cellType;
        this.Key = (long)(position.x * 10000 + position.y);
    }


}
