using UnityEngine;
using System.Collections;

namespace mNetworkLibrary
{

	[System.Serializable]
	public struct mNetworkPlayer
	{
		byte _playerNo;

		public byte playerNo {
			get {
				return _playerNo;
			}
		}

		bool _isActive;

		public bool isActive {
			get {
				return _isActive;
			}
			internal set {
				_isActive = value;
			}
		}

		public mNetworkPlayer (byte num, bool active)
		{
			_playerNo = num;
			_isActive = active;
		}

		public static bool operator == (mNetworkPlayer p1, mNetworkPlayer p2)
		{
			if (p1.isActive == p2.isActive && p1.playerNo == p2.playerNo) {
				return true;
			} else {
				return false;
			}
		}

		public static bool operator != (mNetworkPlayer p1, mNetworkPlayer p2)
		{
			if (p1.isActive == p2.isActive && p1.playerNo == p2.playerNo) {
				return false;
			} else {
				return true;
			}
		}
	}

}
