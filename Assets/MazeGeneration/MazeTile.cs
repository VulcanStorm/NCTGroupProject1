using UnityEngine;
using System.Collections;

[System.Serializable]
public struct MazeTile {

	public MazeTileType tileType;
	public MazeTileFeature feature;
}

[System.Serializable]
public struct MazeTileGenData {
	
	public Vector2 pos;
	public Vector3 prevTilePos;
	public MazeTileType type;
	public MazeTileFeature feature;
	public bool isVisited;
	public bool isConnectedToStart;
	public bool isIsland;
	public bool isDeadEnd;
	
}
