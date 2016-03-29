using UnityEngine;
using System.Collections;

[System.Serializable]
public struct mNetworkID {
	public ushort idNum;
	public mNetworkIDType type;
	
	public mNetworkID (ushort newIDNum, mNetworkIDType newIDType){
		idNum = newIDNum;
		type = newIDType;
	}
}
