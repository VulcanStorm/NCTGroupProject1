using UnityEngine;
using System.Collections;

[System.Serializable]
public class mNetworkRPCMessage_ND {

	public ushort targetNetId;
	public mNetworkPlayer networkPlayer
	public mNetworkIDType targetIdType;
	public ushort targetMethodId;
	public object[] data;
	
	public mNetworkRPCMessage_ND() {
		
	}
	
	public mNetworkRPCMessage_ND(mNetworkID netId, ushort methodId, object[] inData) {
		targetNetId = netId.idNum;
		targetIdType = netId.type;
		targetMethodId = methodId;
		data = inData;
	}
}
