using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrottosChestFiller : MonoBehaviour
{
    [SerializeField] StrateGeneration _stratesGeneration;
    [SerializeField] private int _nbrePossibleStack;
    // Start is called before the first frame update
    public IEnumerator Fill(List<ChestAndPos> list ) 
    {
        if (list == null || list.Count == 0)
            yield break;
        yield return new WaitForSeconds(1f);
        var strates = _stratesGeneration.GetStrates();
        List<TileBase> tiles = new List<TileBase>();
        int range = 0; 
        Tilemap placeTilemap = World.Instance.GetTilemapsFromLayers(GameManager.Instance.database.GetItemByTile(list[0].chest).tileInfo.placingRules.placeLayer)[0];

        for (int i = 1; i < strates.Count; i++) 
        {
            foreach (var c in strates[i].Tiles) 
            {
                tiles.Add(c.Tile);
               
            }
            tiles = tiles.Distinct().ToList();
            for(int j = list.Count-1; j >= 0 ; j--) 
            {
                if (list[j].pos.y<-range && list[j].pos.y>-range - strates[i].SizeY) 
                {
                    Vector3 pos = placeTilemap.CellToWorld(list[j].pos);
                    pos -= Camera.main.transform.forward * 10 - Camera.main.transform.up * 0.16f;
                    Debug.DrawRay(pos, Camera.main.transform.forward * 11, Color.white, 1000);
                    RaycastHit2D hit = Physics2D.Raycast(pos, Camera.main.transform.forward, 11);
                    if(hit.collider != null)
                        hit.collider.GetComponent<Chest>().PlaceLoot(GenerateLoot(tiles));
                }
                //else 
                //{
                //    Debug.Log((-range)+" "+ list[j].pos.y + " "+(-range - strates[i].SizeY));
                //}
            }
            range += strates[i].SizeY;
        }
    }

    public List<ItemStack> GenerateLoot(List<TileBase> tiles)
    {
        List<ItemStack> list = new List<ItemStack>();
        for (int i = 0; i < InventorySystem.Instance._nbChestSlots; i++)
        {
            list.Add(null);
        }

        int nbrOfitem = Random.Range(1, InventorySystem.Instance._nbChestSlots);

        for (int i = 0; i < nbrOfitem; i++)
        {
            var itemStack = GameManager.Instance.database.GetLootByTile(tiles[Random.Range(0, tiles.Count)]);
            itemStack.number = Random.Range(1, _nbrePossibleStack);
            list[i] = itemStack;
        }

        return list;
    }
}
