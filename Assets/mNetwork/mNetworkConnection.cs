using UnityEngine;
using System.Collections;

namespace mNetworkLibrary{

[System.Serializable]
public struct mNetworkConnection{
	
	public bool isActive;
	public int socketID;
	public int connectionID;
	public string ipAddress;
	public int port;

	
}

}
