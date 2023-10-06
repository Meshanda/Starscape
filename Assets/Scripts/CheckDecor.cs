using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CheckDecor : MonoBehaviour
{
    [SerializeField] private Tilemap _decor;
    [SerializeField] private List<Vector3Int> _listToCheckDecor;
    // Start is called before the first frame update
    public void SetList(List<Vector3Int> listToCheck)
    {
        _listToCheckDecor = listToCheck;
    }

    public void Start()
    {
        Mining.OnMineTile += DestroyedSquare;
    }
    private void DestroyedSquare(Item item,Vector3Int TilePos, Vector2 Worldpos) 
    {
        if( !item.canHaveDecorOnTop ) 
        {
            return;
        }

        for(int i = 0; i < _listToCheckDecor.Count; i++) 
        {
            if (_listToCheckDecor[i].x == TilePos.x && TilePos.y + 1 == _listToCheckDecor[i].y)
            {
                _decor.SetTile(_listToCheckDecor[i], null);
                _listToCheckDecor.RemoveAt(i);
                return;
            }
        }
    }
}
