using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellCave
{
    public bool Collapsed;
    public List<Possibility> Possibilities; 

    public CellCave(List<Possibility> possibilities)
    {
        Collapsed = false;
        Possibilities = possibilities;
    }
}
