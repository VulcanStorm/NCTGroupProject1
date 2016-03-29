using UnityEngine;
using System.Collections;

[System.Serializable]
public struct mNetworkPlayer {
	byte _playerNo;
	public byte playerNo{
		get{
		return _playerNo;
		}
	}
	bool _isActive;
	public bool isActive{
		get{
		return _isActive;
		}
	}
	public mNetworkPlayer(byte num, bool active){
		_playerNo = num;
		_isActive = active;
	}
}
