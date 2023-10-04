using System.Collections;
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

    private void DestroyedSquare(Vector3Int pos) 
    {
        foreach(var decorPos in _listToCheckDecor)
        {
            if(decorPos.x == pos.x && decorPos.y == pos.y) 
            {
                _decor.SetTile(decorPos, null);
            }
        }
    }
}
