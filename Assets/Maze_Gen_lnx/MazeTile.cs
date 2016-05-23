using UnityEngine;
using System.Collections;

namespace lnxMazeGen {

[System.Serializable]
public struct MazeTile {

	public Vector2 pos;
	public Vector3 prevTilePos;
	public MazeTileType type;
	public bool isVisited;
	public bool isConnectedToStart;
	public bool isIsland;
	public bool isDeadEnd;

}

}
