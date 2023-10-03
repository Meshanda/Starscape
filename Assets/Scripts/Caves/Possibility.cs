using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Cave/Rules/Possibility")]
public class Possibility : ScriptableObject
{
    public TileBase Tile;

    public List<Possibility> UpRules;
    public List<Possibility> RightRules;
    public List<Possibility> DownRules;
    public List<Possibility> LeftRules;
}
