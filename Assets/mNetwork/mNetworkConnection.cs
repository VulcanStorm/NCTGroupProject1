using UnityEngine;
using System.Collections;

[System.Serializable]
public struct mNetworkConnection{
	
	public bool isActive;
	public int socketID;
	public int connectionID;
	public string ipAddress;
	public int port;
	
}
