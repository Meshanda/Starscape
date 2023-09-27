using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles", fileName = "CraftingTableTile")]
public class CraftingTableTile : Tile
{
	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
	{
		return base.StartUp(position, tilemap, go);
	}
	
	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		base.RefreshTile(position, tilemap);
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData(position, tilemap, ref tileData);
	}
}
