using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace mNetworkLibrary{

public static class mNetwork {
	
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
	public static bool isStarted{
		get{
			return hasSetupNetworkTransport;
		}
	}

	private static bool hasSetupNetworkTransport = false;
	
	// id for the network component
	// this is an internal ID used so that any network messages sent via this,
	// are sent straight to the network manager.
	public static readonly mNetworkID internalNetID = new mNetworkID(0,mNetworkIDType.Scene);

	#region NETWORK SETUP

	public static void StartmNetwork () {
		if(isStarted == true){
			return;
		}
		

		
		// initialise network transport
		NetworkTransport.Init();
		// create a network configuration
		config = new ConnectionConfig();
		
		// add an unreliable channel
		// for misc events, that dont matter if they dont happen
		unreliableChannelId = config.AddChannel(QosType.Unreliable);
		
		// add a state update channel
		// for sending position update data
		stateUpdateChannelId = config.AddChannel(QosType.StateUpdate);
		
		// add a reliable channel
		// for events that must get through at some point
		reliableChannelId = config.AddChannel(QosType.Reliable);
		
		// add a sequenced reliable channel
		// for events that must be recieved in order
		seqReliableChannelId = config.AddChannel(QosType.ReliableSequenced);
		
		// load the rpcs
		try{
		RPCStore.TryLoadRPCs_ND();
		}
		catch(Exception e){
			Debug.Log("RPC Loading Failed");
			Debug.LogException(e);
		}
	}
	
	// fucntion called to setup as server
	public static void SetupAsServer (bool isDedicated, int serverMaxConnections, ushort port) {
		
		// check that the network transport layer has started
		if(NetworkTransport.IsStarted == false){
			StartmNetwork();
		}
		
		// we have setup network transport already, it can only be done once
		if(hasSetupNetworkTransport == true){
			return;
		}
		
		socketPort = port;
		
		hasSetupNetworkTransport = true;
		
		// check for a dedicated server
		if(isDedicated == true){
			peerType = mNetworkPeerType.dedicatedServer;
		}
		else{
			peerType = mNetworkPeerType.server;
		}
		
		// set the maximum connections
		maxConnections = serverMaxConnections;
		
		// setup the transport layer
		SetupNetworkTransport();
		// setup the network player array
		networkPlayers = new mNetworkPlayer[maxConnections];
			// check if we are a non-dedicated server
			if(peerType == mNetworkPeerType.server){
			// connect the client if we are a non-dedicated server
			Connect("127.0.0.1",socketPort);
			}
	}
	
	public static void SetupAsClient () {
		// check that the network transport layer has started
		if(NetworkTransport.IsStarted == false){
			StartmNetwork();
		}
		if(hasSetupNetworkTransport == true){
			return;
		}
		// we have setup network transport already, it can only be done once
		hasSetupNetworkTransport = true;
		
		// we are a client
		peerType = mNetworkPeerType.client;
		
		// we only want 1 connection
		maxConnections = 1;
		
		// setup the transport layer
		SetupNetworkTransport();
		
	}
	
	// only called from setup client or setup server
	private static void SetupNetworkTransport () {
			
			// set the topology
			HostTopology svTopology = new HostTopology(config,maxConnections);
			HostTopology clTopology = new HostTopology(config,1);
			
			// get the socket ID
			// try to open the sockets
			try{
				
				if(peerType != mNetworkPeerType.client){
					serverSocketId = NetworkTransport.AddHost(svTopology,socketPort);
					Debug.Log("Server Socket Open. SocketId is: "+serverSocketId);
				}
				if(peerType != mNetworkPeerType.dedicatedServer){
					clientSocketId = NetworkTransport.AddHost(clTopology);
					Debug.Log ("Client Socket Open. SocketId is: "+clientSocketId);
				}
				
			}
			catch(Exception e){
				Debug.LogException(e);
			}
			
			// now create the network connections so we can view them
			connections = new mNetworkConnection[maxConnections];


			
	}

	public static void ShutDown () {
		// clear the data
		NetworkTransport.Shutdown();
	}

	#endregion

	#region NETWORK PLAYER ARRAY SYNCHRONISING
		
	[mNetworkRPC]
	private static void SetFullNetworkPlayerArray (mNetworkPlayer[] ary){
		networkPlayers = (mNetworkPlayer[])ary.Clone ();
	}

	[mNetworkRPC]
	private static void UpdateSingleNetworkPlayerInArray (int index, mNetworkPlayer _pl){
		networkPlayers [index] = _pl;
	}

	[mNetworkRPC]
	private static void SetCurrentNetworkPlayer (mNetworkPlayer _pl){
		player = _pl;
	}

	private static void AddNewNetworkPlayer(int playerIndex){
		// TODO fix this
		throw new NotImplementedException();
	}
		
	#endregion

	static internal int GetConnectionIDForPlayer(mNetworkPlayer pl){
		if(pl.isActive == true){
		return connections[pl.playerNo].connectionID;
		}
		else{
			return -1;
		}
	}

	static internal int GetConnectionIDForPlayer (int playerNo){
			return connections[playerNo].connectionID;
	}

	#region CONNECTION/DISCONNECTION

	public static void Connect (string destinationIP, int destinationPort) {
		if(networkState == mNetworkState.disconnected){
			// setup a variable to hold the error
			byte error;
			// set the network state
			networkState = mNetworkState.connecting;
			Debug.Log ("client id:"+clientConnectionId);
			// attempt connection and get the ID of that connection
			clientConnectionId = NetworkTransport.Connect(clientSocketId, destinationIP, destinationPort,0,out error);

			if(CheckForNetworkError(error)){
				networkState = mNetworkState.disconnected;
			}
			else{
				Debug.Log("Connection Message Sent");
				Debug.Log ("client connection id:"+clientConnectionId);
			}
		}
	}
	
	public static void Disconnect () {
		
		if(networkState == mNetworkState.connected){
			Debug.Log("Disconnecting...");
			byte error;
			NetworkTransport.Disconnect(serverSocketId,clientConnectionId,out error);
			CheckForNetworkError(error);
			networkState = mNetworkState.disconnected;
		}
		else{
			Debug.LogError("Cannot disconnect since no connection was started");
		}
	}

	/// <summary>
	/// Creates a new Network Connection
	/// </summary>
	/// <returns>The index in the connection array that the data was stored in. Effectively the "player number", when on the server.</returns>
	/// <param name="conId">Con identifier.</param>
	/// <param name="socketID">Socket I.</param>
	static int NewNetworkConnection (int conId, int socketID){
		string cnAddress;
		int cnPort;
		byte cnError;
		
		UnityEngine.Networking.Types.NetworkID cnNetwork;
		UnityEngine.Networking.Types.NodeID cnDstNode;
		// try getting connection info
		NetworkTransport.GetConnectionInfo(socketID,conId,out cnAddress,out cnPort, out cnNetwork, out cnDstNode, out cnError);
		
		// check for an error
		if(CheckForNetworkError(cnError)){
			Debug.LogError("Could not get connection info.");
			return -1;
		}
		else{
			Debug.Log ("For Connection ID: "+conId);
			Debug.Log ("Socket ID is: "+socketID);
			Debug.Log ("Address is: "+cnAddress);
			Debug.Log ("Port is: "+cnPort);
			int playerIndex = -1;
			// iterate over the connections array to find an empty slot
			for(int i=0;i<connections.Length;i++){
				// we have found an empty slot
				if(connections[i].isActive == false){
					connections[i].connectionID = conId;
					connections[i].socketID = socketID;
					connections[i].ipAddress = cnAddress;
					connections[i].port = cnPort;
					connections[i].isActive = true;
					// get the player index
					playerIndex = i;
					// cut the loop here
					i=connections.Length;
				}
			}
			if(playerIndex == -1){
				Debug.LogError("No connection array space for the new entry, no player index was obtained");
			}
			return playerIndex;
		}
	}
	/// <summary>
	/// Removes a network connection from the array.
	/// </summary>
	/// <returns>The index that the connection was removed from, or -1 if the connection was not removed.</returns>
	/// <param name="conId">Connection identifier.</param>
	static int RemoveNetworkConnection (int conId) {
		int foundConNr = -1;
		for(int i=0;i<connections.Length;i++){
			if(connections[i].connectionID == conId){
				connections[i] = new mNetworkConnection();
				foundConNr = i;
			}
		}
		return foundConNr;
	}

	#endregion
	// <----------------------------------------------------------------------------------------------------->
	
	// <----------------------------------------------------------------------------------------------------->

	
	[mNetworkRPC]
	private static void LolRPC (int conID){
		Debug.Log("OMFG LOL " + conID);
	}
	
	#region RPC SENDING
	/// <summary>
	/// Sends an RPC Now. Sends this like a clientNote: use the server RPC call if this should be a server message.
	/// </summary>
	/// <param name="_dataToSend">Data to send.</param>
	/// <param name="sendChannelID">Send channel I.</param>
	private static void RPCNow (ref mNetworkRPCMessage_ND _dataToSend, int sendChannelID){
			// TODO optimise this
			// bypass the serialisation and just re-route it via network manager if we are a dedi server.

		// check if the network has been started
		if(!(networkState == mNetworkState.connected)){
			
			Debug.LogWarning("Not connected, so the RPC will only be local");
			// send it locally instead
			byte[] localbuffer = new byte[1024];
			using(Stream stream = new MemoryStream(localbuffer)){
				
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream,_dataToSend);
				// always process this like a client
				mNetworkManager.ProcessNonDelegateRPC(ref localbuffer, clientSocketId, -1, sendChannelID);
			}
			return;
		}
		
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,_dataToSend);
			
		}

		int bufferSize = 1024;
		if (peerType == mNetworkPeerType.client || peerType == mNetworkPeerType.server) {
			NetworkTransport.Send (clientSocketId, clientConnectionId, sendChannelID, buffer, bufferSize, out error);
		}
		// we're a dedicated server... shit...
		// what to do...
		else if(peerType == mNetworkPeerType.dedicatedServer){
			// we're a dedicated server... so relay this to the correct client since we can't send this to ourselves
			mNetworkManager.ProcessNonDelegateRPC(ref buffer,serverSocketId,-1,sendChannelID);
		}
	}

	/// <summary>
	/// Sends an RPC Message to the server only.
	/// </summary>
	/// <param name="_methodName">Method name.</param>
	/// <param name="_netId">Net identifier.</param>
	/// <param name="channelID">Channel identifier.</param>
	/// <param name="args">Arguments.</param>
	public static void SendRPCMessage (string _methodName, mNetworkID _netID, int channelID, params object[] args){
		
		// get the method ID for the name
		ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND(_methodName);
		
		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netID,_methodId,args);
		RPCNow (ref dataToSend,channelID);
	}

	/// <summary>
	/// Sends an RPC message to a specific group.
	/// </summary>
	/// <param name="_methodName">Method name.</param>
	/// <param name="_netID">Net ID.</param>
	/// <param name="_mode">Mode.</param>
	/// <param name="channelID">Channel identifier.</param>
	/// <param name="args">Arguments.</param>
	public static void SendRPCMessage(string _methodName, mNetworkID _netID, mNetworkRPCMode _mode, int channelID, params object[] args){
		Debug.Log("sending RPC mNetwork");
		// get the method ID for the name
		ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND(_methodName);
		
		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netID,_methodId,_mode, args);

		RPCNow(ref dataToSend, channelID);
	}

	/// <summary>
	/// Sends an RPC message to a specific player.
	/// </summary>
	/// <param name="_methodName">Method name.</param>
	/// <param name="_netID">Net I.</param>
	/// <param name="_targetPlayer">Target player.</param>
	/// <param name="channelID">Channel identifier.</param>
	/// <param name="args">Arguments.</param>
	public static void SendRPCMessage(string _methodName, mNetworkID _netID, mNetworkPlayer _targetPlayer, int channelID, params object[] args){
		// get the method ID for the name
		ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND(_methodName);
		
		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netID,_methodId,_targetPlayer, args);

		RPCNow(ref dataToSend, channelID);
	}

	/// <summary>
	/// SERVER ONLY. Sends an RPC to the specified connection.
	/// </summary>
	/// <param name="_methodName">Method name.</param>
	/// <param name="_netID">Net ID.</param>
	/// <param name="_connectionID">Connection ID.</param>
	/// <param name="_channelID">Channel ID.</param> 
	/// <param name="args">Arguments.</param>
	private static void sv_SendRPC(string _methodName, mNetworkID _netID, int _connectionID, int _channelID, params object[] args){
		// get the method ID for the name
		ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND(_methodName);

		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netID,_methodId,mNetworkRPCMode.None, args);
		// check if we are connected
		if(!(networkState == mNetworkState.connected)){
			Debug.LogError("Not connected, no RPC sent");
			return;
		}
		// check if we are a server
		if(!(peerType == mNetworkPeerType.dedicatedServer || peerType == mNetworkPeerType.server)){
			Debug.LogError("Not a server, cannot use this method");
			return;
		}
		
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, dataToSend);
			
		}

		int bufferSize = 1024;
			NetworkTransport.Send (serverSocketId, _connectionID, _channelID, buffer, bufferSize, out error);
			CheckForNetworkError(error);
	}

	public static void sv_RelayRPCToConnection(ref byte[] rawRPCData, int _connectionID, int _channelID){
		// check if we are connected
		if(!(networkState == mNetworkState.connected)){
			Debug.LogError("Not connected, no RPC sent");
			return;
		}
		// check if we are a server
		if(!(peerType == mNetworkPeerType.dedicatedServer || peerType == mNetworkPeerType.server)){
			Debug.LogError("Not a server, cannot use this method");
			return;
		}
		byte error;
		int bufferSize = rawRPCData.Length;
		NetworkTransport.Send (serverSocketId, _connectionID, _channelID, rawRPCData, bufferSize, out error);
		CheckForNetworkError(error);
	}

	#endregion
	
	public static void PollNetworkEvents() {
		if (isStarted == false) {
			return;
		}
		//Debug.Log ("PollNetworkEvents called");
		bool hasNetworkEvent = true;
		
		while(hasNetworkEvent == true){
			// create variables for recieving data
			int recSocketId;
			int recConnectionId;
			int recChannelId;
			byte[] recBuffer = new byte[1024];
			int bufferSize = 1024;
			int dataSize;
			byte error;
			
			// recieve the event
			NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recSocketId, out recConnectionId, 
			                                                            out recChannelId, recBuffer, bufferSize,out dataSize, out error);
			// check for an error
			if(CheckForNetworkError(error)){
				NetworkError err = (NetworkError)error;
				Debug.Log ("<<<Error Details>>>");
				Debug.Log ("socketID:"+recSocketId);
				Debug.Log ("connectionId:"+recConnectionId);
				Debug.Log ("channelId:"+recChannelId);
				Debug.Log ("<<<End Of Error Details>>>");
				switch(err){
				case NetworkError.Timeout:
					// remove the connection
					int removedConnectionID = RemoveNetworkConnection(recConnectionId);
					// check if we're the server, because then we need to change the player array
					if(recSocketId == serverSocketId){
						// TODO send an RPC to everyone to clear the player from the array
					}
					break;
				}
			}
			else{
				// determine what happened
				switch(recNetworkEvent){
				case NetworkEventType.Nothing:
					// NOTHING HAPPENED
					hasNetworkEvent = false;
					//Debug.Log("No Network Event");
					break;
					
				case NetworkEventType.ConnectEvent:
					// SOMEONE CONNECTED!
					Debug.Log ("<------------------------------------------>");
					Debug.Log ("Connection Event Recieved");
					Debug.Log ("Connected in socket: "+recSocketId);
					Debug.Log ("Connection ID: "+recConnectionId);
					Debug.Log ("Recieved on channel: "+recChannelId);
					

					
					// this is a connection event on the server
					if(recSocketId == serverSocketId){
						Debug.Log ("Server: Player " + recConnectionId.ToString() + " connected!");
						networkState = mNetworkState.connected;
						// add this connection to the list
						int newPlayerNum = NewNetworkConnection(recConnectionId, recSocketId);
						// add the new network player
						networkPlayers[newPlayerNum] = new mNetworkPlayer((byte)newPlayerNum,true);
						// send the network player array to the new client
						sv_SendRPC("SetFullNetworkPlayerArray",internalNetID,recConnectionId,seqReliableChannelId,networkPlayers);
						// update all clients with the new player
					}
					// this is our client who connected
					// FORCE CLIENT ONLY, as a server also has a client socket ID:)
					if(recSocketId == clientSocketId){
						Debug.Log ("Client: Client connected to " + recConnectionId.ToString () + "!" );
						if(peerType == mNetworkPeerType.client){
						// add this connection to the list
						NewNetworkConnection(recConnectionId, recSocketId);
						networkState = mNetworkState.connected;
						}
						else{
							Debug.Log("not adding new connection since we already have one");
						}
					}
					
					
					break;
					
				case NetworkEventType.DataEvent:
					Debug.Log ("WOOT! WE GOT DATA!");
					
					// send the network manager the data to process
					mNetworkManager.ProcessNonDelegateRPC(ref recBuffer, recSocketId, recConnectionId, recChannelId);
					
					break;
					
				case NetworkEventType.DisconnectEvent:
					// OH NOES, THEY LEFT... :(
					
					Debug.Log ("Disconnection Event Recieved");
					Debug.Log ("Disconnected in socket: "+recSocketId);
					Debug.Log ("Connection ID: "+recConnectionId);
					Debug.Log ("Recieved on channel: "+recChannelId);
					// remove the stored connection info
					RemoveNetworkConnection(recConnectionId);
					
					// we just disconnected
					if(recSocketId == clientSocketId && recConnectionId == clientConnectionId){
						Debug.Log ("Client: Disconnected from server!");
						networkState = mNetworkState.disconnected;
					}
					
					// server recieved disconnect message
					if(recSocketId == serverSocketId){
						Debug.Log ("Server: Received disconnect from " + recConnectionId.ToString () );
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
			
	public static bool CheckForNetworkError(byte _error){
		NetworkError err = (NetworkError)_error;
	
		if(err != NetworkError.Ok){
			Debug.LogError("Network Error Detected: "+err);

			if(networkState == mNetworkState.connecting){
				networkState = mNetworkState.disconnected;
				Debug.Log ("client id:"+clientConnectionId);

			}

			else if (networkState == mNetworkState.connected && err == NetworkError.Timeout){
				networkState = mNetworkState.disconnected;
			}
			return true;
		}
		else{
			return false;
		}
			
	}
	
}

}