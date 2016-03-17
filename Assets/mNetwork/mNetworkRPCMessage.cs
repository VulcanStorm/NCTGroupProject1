using UnityEngine;
using System.Collections;

[System.Serializable]
public class mNetworkRPCMessage {

	public ushort targetNetId;
	public mNetworkIDType targetIdType;
	public ushort targetMethodId;
	public byte[] data;
	
	public mNetworkRPCMessage() {
	
	}
	
	public mNetworkRPCMessage(mNetworkID netId, ushort methodId, byte[] inData) {
		targetNetId = netId.idNum;
		targetIdType = netId.type;
		targetMethodId = methodId;
		data = inData;
	}
}


