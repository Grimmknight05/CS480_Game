// ============================================
// SPATIAL PARTITIONING
// ============================================

/// <summary>
/// Grid-based spatial partitioning for efficient queries.
/// Useful for spawning enemies away from the player, line-of-sight checks, etc.
/// </summary>
using UnityEngine;

using System.Collections.Generic;
public class SpatialPartition<T> where T : class
{
    private Dictionary<Vector2Int, List<T>> grid;
    private float cellSize;
    private Bounds levelBounds;
    private int gridWidth;
    private int gridHeight;

    public SpatialPartition(int gridSize, float cellSize, Bounds bounds)
    {
        this.cellSize = cellSize;
        this.levelBounds = bounds;
        this.gridWidth = gridSize;
        this.gridHeight = gridSize;
        this.grid = new Dictionary<Vector2Int, List<T>>();
    }

    private Vector2Int GetGridCoordinates(Vector3 position)
    {
        float localX = position.x - levelBounds.min.x;
        float localZ = position.z - levelBounds.min.z;
        
        int gridX = Mathf.FloorToInt(localX / cellSize);
        int gridZ = Mathf.FloorToInt(localZ / cellSize);
        
        return new Vector2Int(
            Mathf.Clamp(gridX, 0, gridWidth - 1),
            Mathf.Clamp(gridZ, 0, gridHeight - 1)
        );
    }

    public void Insert(T obj, Vector3 position)
    {
        Vector2Int coords = GetGridCoordinates(position);
        
        if (!grid.ContainsKey(coords))
            grid[coords] = new List<T>();
            
        grid[coords].Add(obj);
    }

    public List<T> GetObjectsInCell(Vector3 position)
    {
        Vector2Int coords = GetGridCoordinates(position);
        return grid.ContainsKey(coords) ? grid[coords] : new List<T>();
    }

    public List<T> GetObjectsInRadius(Vector3 center, float radius)
    {
        List<T> results = new List<T>();
        float radiusSq = radius * radius;
        
        //foreach (var cellObjects in grid.Values)
        //{
        //    foreach (var obj in cellObjects)
        //    {
        //        if ((GetPositionForObject(obj) - center).sqrMagnitude <= radiusSq)
        //            results.Add(obj);
        //    }
        //}
        
        return results;
    }
}