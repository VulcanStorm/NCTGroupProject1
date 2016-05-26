using UnityEngine;
using System.Collections;

namespace lnxMazeGen{

public class MazeGenSpriteStore : MonoBehaviour {

	public static MazeGenSpriteStore singleton;

	public Sprite[] mazeSprites = new Sprite[16];
	public Sprite openSprite;
	public Sprite visitedSprite;
	public Sprite connectedToStartSprite;
	public Sprite islandSprite;
	public Sprite deadEndSprite;

	Vector2[] lookupDir;

	void Awake () {
		singleton = this;
		// set the lookup directions
		lookupDir = new Vector2[4];
		lookupDir [0] = Vector2.up;
		lookupDir [1] = Vector2.right;
		lookupDir [2] = Vector2.down;
		lookupDir [3] = Vector2.left;
	}

	void OnDestroy () {
		singleton = null;
	}

	// Use this for initialization
	public Sprite LookupSprite(MazeTile _tileInfo){
		// count the filled tiles around the tile

		// check all directions
		int total = 0;
		if (_tileInfo.isConnectedToStart == true) {
			return connectedToStartSprite;
		} else if (_tileInfo.isIsland == true) {
			return islandSprite;
		} else if (_tileInfo.isDeadEnd == true) {
			return deadEndSprite;
		}
		// check if this tile is a floor tile
		if (_tileInfo.type == MazeTileType.floor) {	
			for (int i = 0; i < 4; i++) {
				Vector2 newTilePos = _tileInfo.pos + lookupDir [i];

				// check if this position is out of bounds
				if (CheckInBounds (newTilePos)) {
					// get the tile at this position
					MazeTile checkTile = MazeGen1.singleton.GetTile (newTilePos);
					if (checkTile.type == MazeTileType.floor) {
						total += (int)Mathf.Pow (2, i);
					}
				}
			}
		} else if (_tileInfo.type == MazeTileType.open) {
			if (_tileInfo.isVisited == true) {
				return visitedSprite;
			} else {
				return openSprite;
			}
		}else if(_tileInfo.type == MazeTileType.wall){
			return mazeSprites[0];
		} 
		// if we're a floor and have no nearby floors, then we should display the floor texture
		if (_tileInfo.type == MazeTileType.floor && total == 0) {
			total = 15;
		}
		// return the sprite from the array
		return mazeSprites[total];

	}

	bool CheckInBounds(Vector2 p){
		if (p.x < 0 || p.x >= MazeGen1.mazeSize.x || p.y < 0 || p.y >= MazeGen1.mazeSize.y) {
			return false;
		} else {
			return true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

}