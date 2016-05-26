using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace mNetworkLibrary
{

	public static class mNetwork
	{
	
		// Network configuration variables
		static ConnectionConfig config;

		// TODO convert these to properties
		// integers to hold the channel IDs
		public static int reliableChannelId = -9;
		public static int unreliableChannelId = -9;
		public static int stateUpdateChannelId = -9;
		public static int seqReliableChannelId = -9;
	
		// integers to hold the socket info
		public static int serverSocketId = -9;
		public static int clientSocketId = -9;
		public static ushort socketPort = 25001;
	
		// how many connections are allowed
		public static int maxConnections = -9;
		// the id of the current client connection
		public static int clientConnectionId = -9;
	
		// what kind of network peer are we
		public static mNetworkPeerType peerType = mNetworkPeerType.none;
		// what is the state of the network
		public static mNetworkState networkState = mNetworkState.disconnected;
	
		// an array to hold all of the connection info
		public static mNetworkConnection[] connections;
		// an array of netowrk players to send RPCs to
		public static mNetworkPlayer[] networkPlayers;
		// the network player that we belong to
		public static mNetworkPlayer player;
		// have we already setup the network
		public static bool isStarted {
			get {
				return hasSetupNetworkTransport;
			}
		}

		private static bool hasSetupNetworkTransport = false;
	
		// id for the network component
		// this is an internal ID used so that any network messages sent via this,
		// are sent straight to the network manager.
		public static readonly mNetworkID internalNetID = new mNetworkID (0, mNetworkIDType.Scene);

		public static string ipString {
			get {
				return _ipStr;
			}
			private set {
				_ipStr = value;
			}
		}

		public static ushort clientPort {
			get {
				return _clPort;
			}
			private set {
				_clPort = value;
			}
		}

		public static ushort serverPort {
			get {
				return _svPort;
			}
			private set {
				_svPort = value;
			}
		}

		private static string _ipStr;
		private static ushort _clPort;
		private static ushort _svPort;
	
		// list of rpcs in the buffer, will get sent before all other network messages
		public static List<mNetworkRPCMessage_ND> bufferedRPCs = new List<mNetworkRPCMessage_ND> ();

		#region NETWORK SETUP

		public static void StartmNetwork ()
		{
			if (isStarted == true) {
				return;
			}
		
			
		
			// initialise network transport
			NetworkTransport.Init ();
			// create a network configuration
			config = new ConnectionConfig ();

			// add an unreliable channel
			// for misc events, that dont matter if they dont happen
			unreliableChannelId = config.AddChannel (QosType.Unreliable);
		
			// add a state update channel
			// for sending position update data
			stateUpdateChannelId = config.AddChannel (QosType.StateUpdate);
		
			// add a reliable channel
			// for events that must get through at some point
			reliableChannelId = config.AddChannel (QosType.Reliable);
		
			// add a sequenced reliable channel
			// for events that must be recieved in order
			seqReliableChannelId = config.AddChannel (QosType.ReliableSequenced);
		
			// load the rpcs
			try {
				RPCStore.TryLoadRPCs_ND ();
			} catch (Exception e) {
				Debug.Log ("RPC Loading Failed");
				Debug.LogException (e);
			}
		}
	
		// fucntion called to setup as server
		public static void SetupAsServer (bool isDedicated, int serverMaxConnections, ushort port)
		{
		
			// check that the network transport layer has started
			if (NetworkTransport.IsStarted == false) {
				StartmNetwork ();
			}
		
			// we have setup network transport already, it can only be done once
			if (hasSetupNetworkTransport == true) {
				return;
			}
			// set the socket port
			socketPort = port;
			// since we're a server, set the server port
			serverPort = port;
		
			hasSetupNetworkTransport = true;
		
			// check for a dedicated server
			if (isDedicated == true) {
				peerType = mNetworkPeerType.dedicatedServer;
			} else {
				peerType = mNetworkPeerType.server;
			}
		
			// set the maximum connections
			maxConnections = serverMaxConnections;
		
			// setup the transport layer
			SetupNetworkTransport ();
			// setup the network player array
			networkPlayers = new mNetworkPlayer[maxConnections];
			// check if we are a non-dedicated server
			if (peerType == mNetworkPeerType.server) {
				// connect the client if we are a non-dedicated server
				Connect ("127.0.0.1", serverPort);
			}
			// set the IP and port info
			string hostName = Dns.GetHostName ();
			Debug.Log ("Host name is: " + hostName);
			IPHostEntry hostEntry = Dns.GetHostEntry (hostName);
			IPAddress[] ipList = hostEntry.AddressList;
			for (int i = 0; i < ipList.Length; i++) {
				Debug.Log ("ip " + i + ": " + ipList [i].ToString ());
			}
			// set the local ip address
			ipString = ipList [0].ToString ();

		}

		public static void SetupAsClient ()
		{
			// check that the network transport layer has started
			if (NetworkTransport.IsStarted == false) {
				StartmNetwork ();
			}
			if (hasSetupNetworkTransport == true) {
				return;
			}
			// we have setup network transport already, it can only be done once
			hasSetupNetworkTransport = true;
		
			// we are a client
			peerType = mNetworkPeerType.client;
		
			// we only want 1 connection
			maxConnections = 1;
		
			// setup the transport layer
			SetupNetworkTransport ();
		
		}
	
		// only called from setup client or setup server
		private static void SetupNetworkTransport ()
		{
			
			// set the topology
			HostTopology svTopology = new HostTopology (config, maxConnections);
			HostTopology clTopology = new HostTopology (config, 1);


			// get the socket ID
			// try to open the sockets
			try {
				
				if (peerType != mNetworkPeerType.client) {
					serverSocketId = NetworkTransport.AddHost (svTopology, serverPort);
					Debug.Log ("Server Socket Open. SocketId is: " + serverSocketId);
				}
				if (peerType != mNetworkPeerType.dedicatedServer) {
					clientSocketId = NetworkTransport.AddHost (clTopology);
					Debug.Log ("Client Socket Open. SocketId is: " + clientSocketId);
				}
				
			} catch (Exception e) {
				Debug.LogException (e);
			}
			
			// now create the network connections so we can view them
			connections = new mNetworkConnection[maxConnections];


			
		}

		public static void ShutDown ()
		{
			// clear the data
			NetworkTransport.Shutdown ();
		}

		#endregion

		#region NETWORK PLAYER ARRAY SYNCHRONISING

		[mNetworkRPC]
		private static void SetFullNetworkPlayerArray (mNetworkPlayer[] ary)
		{
			networkPlayers = (mNetworkPlayer[])ary.Clone ();
		}

		[mNetworkRPC]
		private static void UpdateSingleNetworkPlayerInArray (int index, mNetworkPlayer _pl)
		{
			networkPlayers [index] = _pl;
		}

		[mNetworkRPC]
		private static void SetCurrentNetworkPlayer (mNetworkPlayer _pl)
		{
			Debug.Log ("Setting My Player... Player Number is: " + _pl.playerNo);
			player = _pl;
		}

		private static void AddNewNetworkPlayer (int playerIndex)
		{
			// TODO fix this
			throw new NotImplementedException ("Write the damn code here you idiot");
		}

		#endregion

		static internal int GetConnectionIDForPlayer (mNetworkPlayer pl)
		{
			if (pl.playerNo < connections.Length) {
				return connections [pl.playerNo].connectionID;
			} else {
				return -1;
			}
		}

		static internal int GetConnectionIDForPlayer (int playerNo)
		{
			if (playerNo < connections.Length) {
				return connections [playerNo].connectionID;
			} else {
				return -1;
			}
		}

		#region CONNECTION/DISCONNECTION

		public static void Connect (string destinationIP, int destinationPort)
		{
			if (networkState == mNetworkState.disconnected) {
				// setup a variable to hold the error
				byte error;
				// set the network state
				networkState = mNetworkState.connecting;
				Debug.Log ("client id:" + clientConnectionId);
				// attempt connection and get the ID of that connection
				clientConnectionId = NetworkTransport.Connect (clientSocketId, destinationIP, destinationPort, 0, out error);

				if (CheckForNetworkError (error)) {
					networkState = mNetworkState.disconnected;
				} else {
					Debug.Log ("Connection Message Sent");
					Debug.Log ("client connection id:" + clientConnectionId);
				}
			}
		}

		public static void Disconnect ()
		{
		
			if (networkState == mNetworkState.connected) {
				Debug.Log ("Disconnecting...");
				byte error;
				NetworkTransport.Disconnect (serverSocketId, clientConnectionId, out error);
				CheckForNetworkError (error);
				networkState = mNetworkState.disconnected;
				// clear the player lists and the connection array
				CleanupNetworkDataArrays ();
			} else {
				Debug.LogError ("Cannot disconnect since no connection was started");
			}
		}

		private static void CleanupNetworkDataArrays ()
		{
			// empty the arrays
			networkPlayers = new mNetworkPlayer[0];
			connections = new mNetworkConnection[0];
		}

		/// <summary>
		/// Creates a new Network Connection
		/// </summary>
		/// <returns>The index in the connection array that the data was stored in. Effectively the "player number", when on the server.</returns>
		/// <param name="conId">Con identifier.</param>
		/// <param name="socketID">Socket I.</param>
		static int NewNetworkConnection (int conId, int socketID)
		{
			string cnAddress;
			int cnPort;
			byte cnError;
		
			UnityEngine.Networking.Types.NetworkID cnNetwork;
			UnityEngine.Networking.Types.NodeID cnDstNode;
			// try getting connection info
			NetworkTransport.GetConnectionInfo (socketID, conId, out cnAddress, out cnPort, out cnNetwork, out cnDstNode, out cnError);
		
			// check for an error
			if (CheckForNetworkError (cnError)) {
				Debug.LogError ("Could not get connection info.");
				return -1;
			} else {
				Debug.Log ("For Connection ID: " + conId);
				Debug.Log ("Socket ID is: " + socketID);
				Debug.Log ("Address is: " + cnAddress);
				Debug.Log ("Port is: " + cnPort);
				int playerIndex = -1;
				// iterate over the connections array to find an empty slot
				for (int i = 0; i < connections.Length; i++) {
					// we have found an empty slot
					if (connections [i].isActive == false) {
						connections [i].connectionID = conId;
						connections [i].socketID = socketID;
						connections [i].ipAddress = cnAddress;
						connections [i].port = cnPort;
						connections [i].isActive = true;
						// get the player index
						playerIndex = i;
						// cut the loop here
						i = connections.Length;
					}
				}
				if (playerIndex == -1) {
					Debug.LogError ("No connection array space for the new entry, no player index was obtained");
				}
				return playerIndex;
			}
		}

		/// <summary>
		/// Removes a network connection from the array.
		/// </summary>
		/// <returns>The index that the connection was removed from, this is the player entry in the networkPlayers array. Or -1 if the connection was not removed.</returns>
		/// <param name="conId">Connection identifier.</param>
		static int RemoveNetworkConnection (int conId)
		{
			int foundConNr = -1;
			for (int i = 0; i < connections.Length; i++) {
				if (connections [i].connectionID == conId) {
					connections [i] = new mNetworkConnection ();
					foundConNr = i;
				}
			}
			return foundConNr;
		}

		#endregion

		#region NETWORK INSTANTIATION

		/// <summary>
		/// Sends an instantiation call over the network
		/// </summary>
		/// <param name="_object">Object.</param>
		/// <param name="_position">Position.</param>
		/// <param name="_rotation">Rotation.</param>
		public static void Instantiate (GameObject _object, Vector3 _position, Quaternion _rotation)
		{
			SVector3 spos = _position.GetSerialised ();
			SQuaternion srot = _rotation.GetSerialised ();
			// get the object prefab id
			int prefabId = mNetworkPrefabs.GetIdForPrefab (_object);
			// send a message to the server to build this object
			SendRPCMessage ("REQ_NetworkInstantiate", internalNetID, seqReliableChannelId, prefabId, spos, srot, player);

		}

		/// <summary>
		/// Called on the SERVER ONLY, via an RPC from Instantiate
		/// </summary>
		/// <param name="prefabId">Prefab identifier.</param>
		/// <param name="netId">Net identifier.</param>
		/// <param name="pos">Position.</param>
		/// <param name="rot">Rot.</param>
		[mNetworkRPC]
		private static void REQ_NetworkInstantiate (int prefabId, SVector3 pos, SQuaternion rot, mNetworkPlayer recPlayer)
		{
			Debug.Log ("Recieved Network Instantiation Request");
			// get a new network id for this object
			mNetworkID newNetId = mNetworkManager.GetNextUnusedGameID ();
			Debug.Log ("Game ID for instantiation is: " + newNetId.idNum);
			// send this message to all players to spawn this object
			mNetworkRPCMessage_ND msg = SendRPCMessage ("REC_NetworkInstantiate", internalNetID, mNetworkRPCMode.All, seqReliableChannelId, prefabId, newNetId, pos, rot, recPlayer);
			// add this to the buffer
			AddRPCToBuffer (msg);
		}

		[mNetworkRPC]
		private static void REC_NetworkInstantiate (int prefabId, mNetworkID netId, SVector3 pos, SQuaternion rot, mNetworkPlayer recPlayer)
		{
			Debug.Log ("Recieved Network Instantiation. Creating Object...");
			// create the prefab
			mNetworkPrefabs.CreatePrefabFromID (prefabId, netId, pos, rot, recPlayer);

		}

		#endregion

		/// <summary>
		/// Adds the RPC to the buffer. Can only be used on server, since the client doesnt use the buffer.
		/// </summary>
		/// <param name="_msg">Message.</param>
		public static void AddRPCToBuffer (mNetworkRPCMessage_ND _msg)
		{
			if (peerType == mNetworkPeerType.server || peerType == mNetworkPeerType.dedicatedServer) {
				bufferedRPCs.Add (_msg);
			} else {
				Debug.LogError ("Cannot call this from a client. Since only the server uses the RPC buffer.");
			}
		}
	
		// <----------------------------------------------------------------------------------------------------->
	
		// <----------------------------------------------------------------------------------------------------->
		/// <summary>
		/// called from server, once all data has been sent
		/// </summary>
		[mNetworkRPC]
		static void AllServerDataSent ()
		{
			// notify the server that we have recieved all the data
			SendRPCMessage ("AllServerDataReceived", internalNetID, seqReliableChannelId, player.playerNo);
			// also we have just connected, so notify all the objects that we are ready
			GameObject[] gos = GameObject.FindObjectsOfType<GameObject> ();
			foreach (GameObject go in gos) {
				go.SendMessage ("OnConnectedToServer", SendMessageOptions.DontRequireReceiver);
			}
		}

		/// <summary>
		/// sent from client, to acknowledge that all data has been recieved. called on server only
		/// </summary>
		[mNetworkRPC]
		static void AllServerDataReceived (int playerNum)
		{
			// this connection is no longer loading
			networkPlayers [playerNum].isActive = true;
			// update all clients with the now active player
			SendRPCMessage ("UpdateSingleNetworkPlayerInArray", internalNetID, mNetworkRPCMode.All, seqReliableChannelId, playerNum, networkPlayers [playerNum]);
		}

		/// <summary>
		/// Sent via RPC from the server. Used to set the client port on a remote machine.
		/// </summary>
		/// <param name="port">Port.</param>
		[mNetworkRPC]
		static void SetClientPort (int port)
		{
			clientPort = (ushort)port;
		}

		[mNetworkRPC]
		private static void LolRPC (int conID)
		{
			Debug.Log ("OMFG LOL " + conID);
		}

		#region RPC SENDING

		/// <summary>
		/// Sends an RPC Now. Sends this like a clientNote: use the server RPC call if this should be a server message.
		/// </summary>
		/// <param name="_dataToSend">Data to send.</param>
		/// <param name="sendChannelID">Send channel I.</param>
		private static void RPCNow (ref mNetworkRPCMessage_ND _dataToSend, int sendChannelID)
		{
			// TODO optimise this
			// bypass the serialisation and just re-route it via network manager if we are a dedi server.

			// check if the network has been started
			if (!(networkState == mNetworkState.connected)) {
			
				Debug.LogWarning ("Not connected, so the RPC will only be local");
				// send it locally instead
				byte[] localbuffer = new byte[1024];
				using (Stream stream = new MemoryStream (localbuffer)) {
				
					BinaryFormatter formatter = new BinaryFormatter ();
					formatter.Serialize (stream, _dataToSend);
					// always process this like a client
					mNetworkManager.ProcessNonDelegateRPC (ref localbuffer, clientSocketId, -1, sendChannelID);
				}
				return;
			}
		
			byte error;
			byte[] buffer = new byte[1024];

			// using so the stream will be disposed of afterwards
			using (Stream stream = new MemoryStream (buffer)) {
		
				BinaryFormatter formatter = new BinaryFormatter ();

				formatter.Serialize (stream, _dataToSend);
				//Debug.Log ("stream length " + stream.Length);
				//buffer = new byte[stream.Length];
				//stream.Read (buffer, 0, (int)stream.Length);



			}
			int bufferSize = buffer.Length;
			//Debug.Log (bufferSize);
			if (peerType != mNetworkPeerType.dedicatedServer) {
				NetworkTransport.Send (clientSocketId, clientConnectionId, sendChannelID, buffer, bufferSize, out error);
			}
		// we're a dedicated server... shit...
		// what to do...
		else {
				// we're a dedicated server... so relay this to the correct client since we can't send this to ourselves
				mNetworkManager.ProcessNonDelegateRPC (ref buffer, serverSocketId, -1, sendChannelID);
			}
		}

		/// <summary>
		/// Sends an RPC Message to the server only.
		/// </summary>
		/// <param name="_methodName">Method name.</param>
		/// <param name="_netId">Net identifier.</param>
		/// <param name="channelID">Channel identifier.</param>
		/// <param name="args">Arguments.</param>
		public static mNetworkRPCMessage_ND SendRPCMessage (string _methodName, mNetworkID _netID, int channelID, params object[] args)
		{
		
			// get the method ID for the name
			ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND (_methodName);
		
			// create the network message with the new formatted data
			mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND (_netID, _methodId, args);
			// send that rpc now.
			RPCNow (ref dataToSend, channelID);
			// return the data... incase we want to add this to the buffer
			return dataToSend;
		}

		/// <summary>
		/// Sends an RPC message to a specific group of players.
		/// </summary>
		/// <param name="_methodName">Method name.</param>
		/// <param name="_netID">Net ID.</param>
		/// <param name="_mode">Mode.</param>
		/// <param name="channelID">Channel identifier.</param>
		/// <param name="args">Arguments.</param>
		/// <returns> The RPC message data</returns>
		public static mNetworkRPCMessage_ND SendRPCMessage (string _methodName, mNetworkID _netID, mNetworkRPCMode _mode, int channelID, params object[] args)
		{
			//Debug.Log ("sending RPC mNetwork");
			// get the method ID for the name
			ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND (_methodName);
		
			// create the network message with the new formatted data
			mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND (_netID, _methodId, _mode, args);
			// send that rpc now
			RPCNow (ref dataToSend, channelID);
			// return the data... incase we want to add this to the buffer
			return dataToSend;
		}

		/// <summary>
		/// Sends an RPC message to a specific player.
		/// </summary>
		/// <param name="_methodName">Method name.</param>
		/// <param name="_netID">Net I.</param>
		/// <param name="_targetPlayer">Target player.</param>
		/// <param name="channelID">Channel identifier.</param>
		/// <param name="args">Arguments.</param>
		public static mNetworkRPCMessage_ND SendRPCMessage (string _methodName, mNetworkID _netID, mNetworkPlayer _targetPlayer, int channelID, params object[] args)
		{
			// get the method ID for the name
			ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND (_methodName);
		
			// create the network message with the new formatted data
			mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND (_netID, _methodId, _targetPlayer, args);
			// send that rpc now
			RPCNow (ref dataToSend, channelID);
			// return the data... incase we want to add this to the buffer
			return dataToSend;
		}

		/// <summary>
		/// SERVER ONLY. Sends an RPC to the specified connection.
		/// </summary>
		/// <param name="_methodName">Method name.</param>
		/// <param name="_netID">Net ID.</param>
		/// <param name="_connectionID">Connection ID.</param>
		/// <param name="_channelID">Channel ID.</param> 
		/// <param name="args">Arguments.</param>
		private static void sv_SendRPC (string _methodName, mNetworkID _netID, int _connectionID, int _channelID, params object[] args)
		{
			// get the method ID for the name
			ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND (_methodName);

			// create the network message with the new formatted data
			mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND (_netID, _methodId, mNetworkRPCMode.None, args);
			// check if we are connected
			if (!(networkState == mNetworkState.connected)) {
				Debug.LogError ("Not connected, no RPC sent");
				return;
			}
			// check if we are a server
			if (!(peerType == mNetworkPeerType.dedicatedServer || peerType == mNetworkPeerType.server)) {
				Debug.LogError ("Not a server, cannot use this method");
				return;
			}
		
			byte error;
			byte[] buffer = new byte[1024];
			// using so the stream will be disposed of afterwards
			using (Stream stream = new MemoryStream (buffer)) {
			
				BinaryFormatter formatter = new BinaryFormatter ();
				formatter.Serialize (stream, dataToSend);
			
			}

			int bufferSize = 1024;
			NetworkTransport.Send (serverSocketId, _connectionID, _channelID, buffer, bufferSize, out error);
			CheckForNetworkError (error);
		}

		/// <summary>
		/// Send raw RPC data.
		/// </summary>
		/// <param name="_msg">_msg.</param>
		/// <param name="_sendChannelID">_send channel I.</param>
		private static void sv_SendRawRPC (mNetworkRPCMessage_ND _msg, int _sendChannelID)
		{
			// check if the network has been started
			if (networkState == mNetworkState.connected) {
				

				// send it locally instead
				byte error;
				byte[] localbuffer = new byte[1024];
				using (Stream stream = new MemoryStream (localbuffer)) {
					
					BinaryFormatter formatter = new BinaryFormatter ();
					formatter.Serialize (stream, _msg);
				
				}
				// relay this to the correct destination
				mNetworkManager.ProcessNonDelegateRPC (ref localbuffer, serverSocketId, -1, _sendChannelID);
			
			} else {
				Debug.LogWarning ("Not connected, RPC not sent");
			}
		}

		/// <summary>
		/// Sends raw rpc data to a specfic connection. Used when executing the RPC buffer.
		/// </summary>
		/// <param name="_msg">_msg.</param>
		/// <param name="_connectionID">_connection I.</param>
		/// <param name="_sendChannelID">_send channel I.</param>
		private static void sv_SendRawRPCToConnection (mNetworkRPCMessage_ND _msg, int _connectionID, int _sendChannelID)
		{
			// check if the network has been started
			if (networkState == mNetworkState.connected) {
			
			
				// send it locally instead
				byte error;
				byte[] localbuffer = new byte[1024];
				using (Stream stream = new MemoryStream (localbuffer)) {
				
					BinaryFormatter formatter = new BinaryFormatter ();
					formatter.Serialize (stream, _msg);
				
				}
				// relay this to the correct destination
				NetworkTransport.Send (serverSocketId, _connectionID, _sendChannelID, localbuffer, localbuffer.Length, out error);
				CheckForNetworkError (error);
			
			} else {
				Debug.LogWarning ("Not connected, RPC not sent");
			}
		}

		public static void sv_RelayRPCToConnection (ref byte[] rawRPCData, int _connectionID, int _channelID)
		{
			// check if we are connected
			if (!(networkState == mNetworkState.connected)) {
				Debug.LogError ("Not connected, no RPC sent");
				return;
			}
			// check if we are a server
			if (!(peerType == mNetworkPeerType.dedicatedServer || peerType == mNetworkPeerType.server)) {
				Debug.LogError ("Not a server, cannot use this method");
				return;
			}
			byte error;
			int bufferSize = rawRPCData.Length;
			NetworkTransport.Send (serverSocketId, _connectionID, _channelID, rawRPCData, bufferSize, out error);
			CheckForNetworkError (error);
		}

		#endregion

		public static void PollNetworkEvents ()
		{
			if (isStarted == false) {
				return;
			}
			//Debug.Log ("PollNetworkEvents called");
			bool hasNetworkEvent = true;
		
			while (hasNetworkEvent == true) {
				// create variables for recieving data
				int recSocketId;
				int recConnectionId;
				int recChannelId;
				byte[] recBuffer = new byte[1024];
				int bufferSize = 1024;
				int dataSize;
				byte error;
			
				// recieve the event
				NetworkEventType recNetworkEvent = NetworkTransport.Receive (out recSocketId, out recConnectionId, 
					                                   out recChannelId, recBuffer, bufferSize, out dataSize, out error);
				// check for an error
				if (CheckForNetworkError (error)) {
					NetworkError err = (NetworkError)error;
					Debug.Log ("<<<Error Details>>>");
					Debug.Log ("socketID:" + recSocketId);
					Debug.Log ("connectionId:" + recConnectionId);
					Debug.Log ("channelId:" + recChannelId);
					Debug.Log ("<<<End Of Error Details>>>");
					switch (err) {
					case NetworkError.Timeout:
					// check if we are connected
						if (networkState == mNetworkState.connected) {
							// remove the connection
							int removedPlayerID = RemoveNetworkConnection (recConnectionId);
							// check if we're the server, because then we need to change the player array
							if (recSocketId == serverSocketId) {
								// this was effectively a disconnect event
								if (removedPlayerID != -1) {
									// send an RPC to everyone to clear the player from the array
									SendRPCMessage ("UpdateSingleNetworkPlayerInArray", internalNetID, mNetworkRPCMode.All, seqReliableChannelId, removedPlayerID, new mNetworkPlayer ());
								} else {
									Debug.LogError ("Failed to Remove Network Connection");
								}
							}
						// check if this was recieved in our client socket
						// therfore we must have timed out
						else if (recSocketId == clientSocketId) {
								// we are now disconnected
								networkState = mNetworkState.disconnected;
								// clear the network data arrays
								CleanupNetworkDataArrays ();
							} else {
								// unknown socket ID, error.
								Debug.LogError ("disconnect recieved in unknown socket");
							}
						}
					// check if we were connecting at the time, hence the connection failed
						if (networkState == mNetworkState.connecting) {
							networkState = mNetworkState.disconnected;
							Debug.Log ("client id:" + clientConnectionId);
						}
						break;
					}
				} else {
					// determine what happened
					switch (recNetworkEvent) {
					case NetworkEventType.Nothing:
					// NOTHING HAPPENED
						hasNetworkEvent = false;
					//Debug.Log("No Network Event");
						break;
					
					case NetworkEventType.ConnectEvent:
					// SOMEONE CONNECTED!
						Debug.Log ("<------------------------------------------>");
						Debug.Log ("Connection Event Recieved");
						Debug.Log ("Connected in socket: " + recSocketId);
						Debug.Log ("Connection ID: " + recConnectionId);
						Debug.Log ("Recieved on channel: " + recChannelId);
					

					
					// this is a connection event on the server
						if (recSocketId == serverSocketId) {
							Debug.Log ("Server: Player " + recConnectionId.ToString () + " connected!");
							networkState = mNetworkState.connected;
							// add this connection to the list
							int newPlayerNum = NewNetworkConnection (recConnectionId, recSocketId);
							// add the new network player
							// set this network player as loading, so active = false
							networkPlayers [newPlayerNum] = new mNetworkPlayer ((byte)newPlayerNum, false);
						
							// send the network player array to the new client
							sv_SendRPC ("SetFullNetworkPlayerArray", internalNetID, recConnectionId, seqReliableChannelId, networkPlayers);
							// notify the client of its player data
							sv_SendRPC ("SetCurrentNetworkPlayer", internalNetID, recConnectionId, seqReliableChannelId, networkPlayers [newPlayerNum]);
							// notify the client of the port that it is communicating on
							// TODO, possibly remove this if its a security risk. But there is no other way to know what port you are using...
							sv_SendRPC ("SetClientPort", internalNetID, recConnectionId, seqReliableChannelId, connections [newPlayerNum].port);
							// update all clients with the new player
							SendRPCMessage ("UpdateSingleNetworkPlayerInArray", internalNetID, mNetworkRPCMode.All, seqReliableChannelId, newPlayerNum, networkPlayers [newPlayerNum]);
						
							// execute the buffer of RPCs
							for (int i = 0; i < bufferedRPCs.Count; i++) {
								sv_SendRawRPCToConnection (bufferedRPCs [i], recConnectionId, seqReliableChannelId);
							}
							// TODO does the client know how many functions its recieving?
							sv_SendRPC ("AllServerDataSent", internalNetID, recConnectionId, seqReliableChannelId);

						}
					// this is our client who connected
					// FORCE CLIENT ONLY, as a server also has a client socket ID:)
						if (recSocketId == clientSocketId) {
							Debug.Log ("Client: Client connected to " + recConnectionId.ToString () + "!");
							if (peerType == mNetworkPeerType.client) {
								// add this connection to the list
								NewNetworkConnection (recConnectionId, recSocketId);
						
								networkState = mNetworkState.connected;
							} else {
								Debug.Log ("not adding new connection since we already have one");
							}
						}
					
					
						break;
					
					case NetworkEventType.DataEvent:
						//Debug.Log ("WOOT! WE GOT DATA!");
					
					// send the network manager the data to process
						mNetworkManager.ProcessNonDelegateRPC (ref recBuffer, recSocketId, recConnectionId, recChannelId);
					
						break;
					
					case NetworkEventType.DisconnectEvent:
					// OH NOES, THEY LEFT... :(
					
						Debug.Log ("Disconnection Event Recieved");
						Debug.Log ("Disconnected in socket: " + recSocketId);
						Debug.Log ("Connection ID: " + recConnectionId);
						Debug.Log ("Recieved on channel: " + recChannelId);
					// remove the stored connection info
						RemoveNetworkConnection (recConnectionId);
					
					// we just disconnected
						if (recSocketId == clientSocketId && recConnectionId == clientConnectionId) {
							Debug.Log ("Client: Disconnected from server!");
							networkState = mNetworkState.disconnected;
						}
					
					// server recieved disconnect message
						if (recSocketId == serverSocketId) {
							Debug.Log ("Server: Received disconnect from " + recConnectionId.ToString ());
							// TODO send an RPC to everyone to clear the player from the array
						}
					
						break;
					
					case NetworkEventType.BroadcastEvent:
					// SOMEONE IS BROADCASTING A SERVER!
						Debug.Log ("Broadcast Event Recieved");
						break;
					}
				}
			}
		
		}
	
	
		// <----------------------------------------------------------------------------------------------------->
	
		// <----------------------------------------------------------------------------------------------------->
	
		// Helper Functions
			
		public static bool CheckForNetworkError (byte _error)
		{
			NetworkError err = (NetworkError)_error;
	
			if (err != NetworkError.Ok) {
				Debug.LogError ("Network Error Detected: " + err);
				return true;
			} else {
				return false;
			}
			
		}
	
	}

}