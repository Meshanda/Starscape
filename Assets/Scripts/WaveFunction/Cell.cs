using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell 
{
    public bool collapsed = false;
    public Tile[] tileOptions;
    public Vector3Int position;

    public Cell(Vector3Int position, Tile[] tileOtions)
    {
        this.position = position;
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
