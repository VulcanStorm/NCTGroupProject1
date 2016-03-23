using UnityEngine;
using System.Collections;

[System.Serializable]
public struct mNetworkPlayer {
	byte playerNo;
	bool isActive;
	public mNetworkPlayer(byte num, bool active){
		playerNo = num;
		isActive = active;
	}
}
