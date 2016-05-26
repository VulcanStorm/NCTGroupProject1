using UnityEngine;
using System.Collections;

[System.Serializable]
public struct MapTile {
	public byte height;
	public byte waterLevel;
	
	public MapTile(byte h){
		height = h;
		waterLevel = 0;
	}
}
