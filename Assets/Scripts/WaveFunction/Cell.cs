using UnityEngine;

public class Cell 
{
    public bool collapsed = false;
    public Tile[] tileOptions;
    public Vector3Int position;

    public Cell(Vector3Int position, bool collapsed ,Tile[] tileOtions = null)
    {
        this.position = position;
        this.collapsed = collapsed;
        this.tileOptions = tileOtions;
    }

    public void CreateCell(bool collapseState, Tile[] tiles)
    {
        collapsed = collapseState;
        tileOptions = tiles;
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }
}
