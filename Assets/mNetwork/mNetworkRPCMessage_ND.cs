using UnityEngine;
using System.Collections;

namespace mNetworkLibrary
{

	[System.Serializable]
	public struct mNetworkRPCMessage_ND
	{

		public ushort targetNetId;
		public mNetworkPlayer networkPlayer;
		public mNetworkRPCMode rpcMode;
		public mNetworkIDType targetIdType;
		public ushort targetMethodId;
		public object[] data;

		/// <summary>
		/// Initializes a new instance of the <see cref="mNetworkRPCMessage_ND"/> class.
		/// To be used to with the RPC Mode.
		/// </summary>
		/// <param name="netId">Net identifier.</param>
		/// <param name="methodId">Method identifier.</param>
		/// <param name="inData">In data.</param>
		/// <param name="_rpcMode">Rpc mode.</param>
		public mNetworkRPCMessage_ND (mNetworkID netId, ushort methodId, mNetworkRPCMode _rpcMode, object[] inData)
		{
			targetNetId = netId.idNum;
			targetIdType = netId.type;
			rpcMode = _rpcMode;
			networkPlayer = new mNetworkPlayer (0, false);
			targetMethodId = methodId;
			data = inData;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="mNetworkRPCMessage_ND"/> class.
		/// TO be used with a target Network Player.
		/// </summary>
		/// <param name="netId">Net identifier.</param>
		/// <param name="methodId">Method identifier.</param>
		/// <param name="_netPlayer">Net player.</param>
		/// <param name="inData">In data.</param>
		public mNetworkRPCMessage_ND (mNetworkID netId, ushort methodId, mNetworkPlayer _netPlayer, object[] inData)
		{
			targetNetId = netId.idNum;
			targetIdType = netId.type;
			rpcMode = mNetworkRPCMode.None;
			networkPlayer = _netPlayer;
			targetMethodId = methodId;
			data = inData;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="mNetworkRPCMessage_ND"/> class.
		/// Will only send to the server.
		/// </summary>
		/// <param name="netId">Net identifier.</param>
		/// <param name="methodId">Method identifier.</param>
		/// <param name="inData">In data.</param>
		public mNetworkRPCMessage_ND (mNetworkID netId, ushort methodId, object[] inData)
		{
			targetNetId = netId.idNum;
			targetIdType = netId.type;
			rpcMode = mNetworkRPCMode.Server;
			networkPlayer = new mNetworkPlayer (0, false);
			targetMethodId = methodId;
			data = inData;
		}
	}

}
