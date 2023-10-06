using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles", fileName = "CraftingTableTile")]
public class CraftingTableTile : UnityEngine.Tilemaps.Tile
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
		base.GetTileData(position, tilemap, ref tileData); // fills tileData with inspectorData
		if (tilemap.GetComponent<Tilemap>() == World.Instance.VisualizationTilemap)
		{
			tileData.gameObject = null; // do not instantiate the custom gameObject if we are currently visualizing the tile
		}
	}
}
