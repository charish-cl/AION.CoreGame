using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class MapCreate : SerializedMonoBehaviour
{
    [LabelText("障碍物")]
    public Tilemap obstacle;
    
    [LabelText("道路")]
    public Tilemap roads;
    
    public bool showGizmos = true;
    
    public Grid grid;

    public Vector2 cellSize
    {
        get
        {
            if (grid == null)
            {
                return Vector2.one; 
            }
            return grid.cellSize;
        }
    }
    private void OnEnable()
    {
        grid = GetComponent<Grid>();
    }

 
    public Dictionary<MapCellType,Color> DrawColor = new Dictionary<MapCellType, Color>()
    {
        {MapCellType.Obstacle,Color.red},
        {MapCellType.Road,Color.green},
    };

    public List<MapCell> ObstacleMapCells = new List<MapCell>();

    public List<MapCell> CanWalkMapCells = new List<MapCell>();
    [Button]
    public void CreateMap()
    {
        ObstacleMapCells.Clear();
        // 获取所有障碍物
        ObstacleMapCells = GetMapCellsByTilemap(obstacle, MapCellType.Obstacle);

        //获取所有的道路
        List<MapCell> roadsMapCells = GetMapCellsByTilemap(roads, MapCellType.Road);
        
        //去除重合的
        
        var dictionary = ObstacleMapCells.ToDictionary(e=>e.Position.x * 10000+ e.Position.y );
        CanWalkMapCells = roadsMapCells.Where(e => !dictionary.ContainsKey(e.Position.x* 10000+ e.Position.y)).ToList();
        
        //用二进制保存行走信息
    }

    [Button("保存地图",ButtonSizes.Large)]
    private void SaveMap()
    {
        //TODO:保存行走信息
        StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/MapData.txt");
        for (int i = 0; i < CanWalkMapCells.Count; i++)
        {
            writer.Write(CanWalkMapCells[i].Position.x + " " + CanWalkMapCells[i].Position.y + " ");
        }
        writer.Close();
    }
    public void LoadMap()
    {
        //TODO:加载行走信息
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/MapData.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] strs = line.Split(' ');
            Vector2 pos = new Vector2(int.Parse(strs[0]), int.Parse(strs[1]));
            CanWalkMapCells.Add(new MapCell(pos, MapCellType.Road));
        }
        reader.Close();
    }

    public List<MapCell> GetMapCellsByTilemap(Tilemap tilemap, MapCellType cellType = MapCellType.Obstacle)
    {
        List<MapCell> mapCells = new List<MapCell>();
        BoundsInt bounds = tilemap.cellBounds;
        for(int x = bounds.xMin; x < bounds.xMax; x++){
            for(int y = bounds.yMin; y < bounds.yMax; y++){     
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {                    
                    mapCells.Add(new MapCell(new Vector2(x,y), cellType = cellType));
                }
            }
        }
        return mapCells;
    }
    public Color GetColor(MapCellType cellType)
    {
        if (DrawColor.ContainsKey(cellType))
        {
            return DrawColor[cellType];
        }
        return Color.white;
    }
    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }
        if (ObstacleMapCells == null) return;
        // foreach (var cell in ObstacleMapCells)
        // {
        //     FDraw.GimzoDrawRectangle(GetColor(cell.cellType),cell.Position, cellSize.x, cellSize.y);
        // }
        // foreach (var cell in CanWalkMapCells)
        // {
        //     FDraw.GimzoDrawRectangle(GetColor(cell.cellType), cell.Position, cellSize.x, cellSize.y);
        // }
    }



}
