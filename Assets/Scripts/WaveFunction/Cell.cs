using System.Collections.Generic;
using UnityEngine;

public class Cell 
{
    public bool collapsed = false;
    public List<Tile> tileOptions;
    public Vector3Int position;
	public Vector3Int arrayPosition;

    public Cell upNeighbor;
    public Cell downNeighbor;
    public Cell rightNeighbor;
    public Cell leftNeighbor;

    public Cell(Vector3Int position, bool collapsed ,List<Tile> tileOtions = null)
    {
        this.position = position;
        this.collapsed = collapsed;
        this.tileOptions = tileOtions;
    }

    public void CreateCell(bool collapseState, List<Tile> tiles)
    {
        collapsed = collapseState;
        tileOptions = tiles;
    }

    public void RecreateCell(List<Tile> tiles)
    {
        tileOptions = tiles;
    }
}
