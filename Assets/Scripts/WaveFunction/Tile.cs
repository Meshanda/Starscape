using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Tile") ]
public class Tile : ScriptableObject
{
    public TileBase tile;
    public int weight;
    public bool inDecor;
    public List<Tile> upNeighbours;
    public List<Tile> rightNeighbours;
    public List<Tile> downNeighbours;
    public List<Tile> leftNeighbours;
}
