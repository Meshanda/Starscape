using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Tile") ]
public class Tile : ScriptableObject
{
    public TileBase tile;
    public int weight;
    public bool inDecor;
    public Tile[] upNeighbours;
    public Tile[] rightNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;
}
