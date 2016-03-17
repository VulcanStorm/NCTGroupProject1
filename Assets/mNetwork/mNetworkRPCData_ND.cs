using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

[System.Serializable]
public class mNetworkRPCData_ND {
	public MethodInfo[] rpcs;
	
	public void Save()
	{
		try{
			string path = Application.dataPath;
			path += "/Resources/RPCs/RPCStoreND.bytes";
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using(var stream = new FileStream(path, FileMode.Create))
			{
				binaryFormatter.Serialize(stream, this);
			}
		}
		catch(Exception e){
			Debug.LogException(e);
		}
	}
	
	public static mNetworkRPCData_ND Load()
	{
		try{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			TextAsset file = Resources.Load("RPCs/RPCStoreND", typeof(TextAsset))as TextAsset;
			byte[] fileBytes = file.bytes;
			//Debug.Log ("got bytes");
			using(var stream = new MemoryStream(file.bytes))
			{
				return binaryFormatter.Deserialize(stream) as mNetworkRPCData_ND;
			}
		}
		catch(Exception e){
			Debug.LogException(e);
		}
		return new mNetworkRPCData_ND();
	}
}


