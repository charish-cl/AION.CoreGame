namespace GameDevKit
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class GridDirection
    {
        public readonly Vector2Int Vector;

        public void DrawLineFromDir(GridDirection direction,float length)
        {
           
        }
        private GridDirection(int x, int y)
        {
            Vector = new Vector2Int(x, y);
        }

        public static implicit operator Vector2Int(GridDirection direction)
        {
            return direction.Vector;
        }

        public static GridDirection GetDirectionFromV2I(Vector2Int vector)
        {
            return OctantDirections.DefaultIfEmpty(None).FirstOrDefault(direction => direction == vector);
        }

        public static readonly GridDirection None = new GridDirection(0, 0);
        public static readonly GridDirection North = new GridDirection(0, 1);
        public static readonly GridDirection South = new GridDirection(0, -1);
        public static readonly GridDirection East = new GridDirection(1, 0);
        public static readonly GridDirection West = new GridDirection(-1, 0);
        public static readonly GridDirection NorthEast = new GridDirection(1, 1);
        public static readonly GridDirection NorthWest = new GridDirection(-1, 1);
        public static readonly GridDirection SouthEast = new GridDirection(1, -1);
        public static readonly GridDirection SouthWest = new GridDirection(-1, -1);

        //四个方向
        public static readonly List<GridDirection> quadRantDirections
            = new List<GridDirection>
        {
            North,
            East,
            South,
            West
        };
        //八个方向
        public static readonly List<GridDirection> 
            OctantDirections = new List<GridDirection>
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        };
        public static readonly List<GridDirection> AllDirections = new List<GridDirection>
        {
            None,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        };
    }

}