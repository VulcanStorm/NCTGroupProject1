using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace mNetworkLibrary
{

	[System.Serializable]
	public struct mNetworkPlayer : ISerializable
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

		#region ISerializable implementation

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("p", _playerNo, typeof(byte));
			info.AddValue("a",_isActive, typeof(bool));
		}

		public mNetworkPlayer(SerializationInfo info, StreamingContext context){
			_playerNo = info.GetByte ("p");
			_isActive = info.GetBoolean ("a");
		}

		#endregion
	}

}
