using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class MapData {

	public int mapSizeX;
	public int mapSizeY;
	
	public int waterStartPosX;
	public int waterStartPosY;
	public byte waterStartAmount;
	
	public MapTile[,] map;
	
	public void Save(string path)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(path, FileMode.Create))
		{
			binaryFormatter.Serialize(stream, this);
		}
	}
	
	public static MapData Load(string path)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(path, FileMode.Open))
		{
			return binaryFormatter.Deserialize(stream) as MapData;
		}
	}
	
}
