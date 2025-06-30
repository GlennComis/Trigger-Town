using UnityEngine;

public static class GridDirectionExtensions
{
    public static Vector2Int ToVector(this GridDirection dir) => dir switch
    {
        GridDirection.Up => Vector2Int.up,
        GridDirection.Down => Vector2Int.down,
        GridDirection.Left => Vector2Int.left,
        GridDirection.Right => Vector2Int.right,
        _ => Vector2Int.zero,
    };
}

public enum GridDirection
{
    Up, Down, Left, Right
}