using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public delegate void RpcFunction (mNetworkBehaviour TargetScript, byte[] NetData);

[System.Serializable]
public class mNetworkRPCData {
	public RpcFunction[] rpcs;
	
	public void Save()
	{
		string path = Application.dataPath;
		path += "/Resources/RPCs/RPCStore.bytes";
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(path, FileMode.Create))
		{
			binaryFormatter.Serialize(stream, this);
		}
	}
	
	public static mNetworkRPCData Load()
	{
		try{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			TextAsset file = Resources.Load("RPCs/RPCStore", typeof(TextAsset))as TextAsset;
			byte[] fileBytes = file.bytes;
			//Debug.Log ("got bytes");
			using(var stream = new MemoryStream(file.bytes))
			{
				
				return binaryFormatter.Deserialize(stream) as mNetworkRPCData;
			}
		}
		catch(Exception e){
			Debug.LogException(e);
		}
		return new mNetworkRPCData();
	}
}

