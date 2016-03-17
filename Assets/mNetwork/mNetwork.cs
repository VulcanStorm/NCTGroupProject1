using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

public static class mNetwork {
	
	// Network configuration variables
	static ConnectionConfig config;
	
	// integers to hold the channel IDs
	static int reliableChannelId = -9;
	static int unreliableChannelId = -9;
	static int stateUpdateChannelId = -9;
	static int seqReliableChannelId = -9;
	
	// integers to hold the socket info
	static int serverSocketId = -9;
	static int clientSocketId = -9;
	static ushort socketPort = 25001;
	
	// how many connections are allowed
	public static int maxConnections = -9;
	// the id of the current client connection
	static int clientConnectionId = -9;
	
	// what kind of network peer are we
	public static mNetworkPeerType peerType = mNetworkPeerType.none;
	// what is the state of the network
	public static mNetworkState networkState = mNetworkState.disconnected;
	
	// an array to hold all of the connection info
	public static mNetworkConnection[] connections;
	
	// have we already setup the network
	static bool hasSetupNetworkTransport = false;
	
	// id for the network component
	// this is an internal ID used so that any network messages sent via this,
	// are sent straight to the network manager.
	public static readonly mNetworkID internalNetID = new mNetworkID(0,mNetworkIDType.Scene);
	
	public static void StartmNetwork () {
		if(NetworkTransport.IsStarted == true){
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
	
	public static void Connect (string destinationIP, int destinationPort) {
		if(networkState == mNetworkState.disconnected){
			// setup a variable to hold the error
			byte error;
			// set the network state
			networkState = mNetworkState.connecting;
			// attempt connection and get the ID of that connection
			clientConnectionId = NetworkTransport.Connect(clientSocketId, destinationIP, destinationPort,0,out error);

			if(CheckForNetworkError(error)){
				networkState = mNetworkState.disconnected;
			}
			else{
				Debug.Log("Connection Message Sent");
			}
		}
	}
	
	public static void Disconnect () {
		Debug.Log("Disconnecting...");
		if(networkState == mNetworkState.connected){
			byte error;
			NetworkTransport.Disconnect(serverSocketId,clientConnectionId,out error);
			CheckForNetworkError(error);
			networkState = mNetworkState.disconnected;
		}
	}
	
	// <----------------------------------------------------------------------------------------------------->
	
	// <----------------------------------------------------------------------------------------------------->
	
	static void NewNetworkConnection (int conId, int socketID){
		string cnAddress;
		int cnPort;
		byte cnError;
		
		UnityEngine.Networking.Types.NetworkID cnNetwork;
		UnityEngine.Networking.Types.NodeID cnDstNode;
		// try getting connection info
		NetworkTransport.GetConnectionInfo(socketID,conId,out cnAddress,out cnPort, out cnNetwork, out cnDstNode, out cnError);
		
		// check for an error
		if(!CheckForNetworkError(cnError)){
			Debug.LogError("Could not get connection info.");
		}
		else{
			Debug.Log ("For Connection ID: "+conId);
			Debug.Log ("Socket ID is: "+socketID);
			Debug.Log ("Address is: "+cnAddress);
			Debug.Log ("Port is: "+cnPort);
			
			// iterate over the connections array to find an empty slot
			for(int i=0;i<connections.Length;i++){
				// we have found an empty slot
				if(connections[i].isActive == false){
					connections[i].connectionID = conId;
					connections[i].socketID = socketID;
					connections[i].ipAddress = cnAddress;
					connections[i].port = cnPort;
					connections[i].isActive = true;
					// cut the loop here
					i=connections.Length;
				}
			}
			
		}
	}
	
	static void RemoveNetworkConnection (int conId) {
		for(int i=0;i<connections.Length;i++){
			if(connections[i].connectionID == conId){
				connections[i] = new mNetworkConnection();
			}
		}
	}
	
	[mNetworkRPC]
	public static void LolRPC (int conID){
		Debug.Log("OMFG LOL " + conID);
	}
	
	// <----------------------------------------------------------------------------------------------------->
	
	// <----------------------------------------------------------------------------------------------------->
	public static void SendRPCMessage (ref mNetworkRPCMessage dataToSend){
		// check if the network has been started
		if(!(networkState == mNetworkState.connected)){
			// TODO CHANGE THIS
			//Debug.LogError("No RPC could be sent, since we are not connected");
			Debug.LogWarning("Not connected, so the RPC will only be local");
			// send it locally instead
			byte[] localbuffer = new byte[1024];
			using(Stream stream = new MemoryStream(localbuffer)){
				
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream,dataToSend);
				mNetworkManager.ProcessNonDelegateRPC(ref localbuffer);
			}
			return;
		}
		
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,dataToSend);
			
			int bufferSize = 1024;
			
			NetworkTransport.Send(serverSocketId,clientConnectionId,reliableChannelId,buffer,bufferSize,out error);
			
		}
	}
	
	
	public static void SendRPCMessage (mNetworkID _netId, ushort _methodId, object[] args){
		
		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netId,_methodId,args);
		
		// check if the network has been started
		if(!(networkState == mNetworkState.connected)){
			// TODO CHANGE THIS
			//Debug.LogError("No RPC could be sent, since we are not connected");
			Debug.LogWarning("Not connected, so the RPC will only be local");
			// send it locally instead
			byte[] localbuffer = new byte[1024];
			using(Stream stream = new MemoryStream(localbuffer)){
				
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream,dataToSend);
				mNetworkManager.ProcessNonDelegateRPC(ref localbuffer);
			}
			return;
		}
		
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,dataToSend);
			
			int bufferSize = 1024;
			
			NetworkTransport.Send(serverSocketId,clientConnectionId,reliableChannelId,buffer,bufferSize,out error);
			
		}
	}
	
	public static void SendRPCMessage (mNetworkID _netId, string _methodName, params object[] args){
		
		// get the method ID for the name
		ushort _methodId = (ushort)RPCStore.GetIDForRPCName_ND(_methodName);
		
		// create the network message with the new formatted data
		mNetworkRPCMessage_ND dataToSend = new mNetworkRPCMessage_ND(_netId,_methodId,args);
		
		// check if the network has been started
		if(!(networkState == mNetworkState.connected)){
			// TODO CHANGE THIS
			//Debug.LogError("No RPC could be sent, since we are not connected");
			Debug.LogWarning("Not connected, so the RPC will only be local");
			// send it locally instead
			byte[] localbuffer = new byte[1024];
			using(Stream stream = new MemoryStream(localbuffer)){
				
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream,dataToSend);
				mNetworkManager.ProcessNonDelegateRPC(ref localbuffer);
			}
			return;
		}
		
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,dataToSend);
			
			int bufferSize = 1024;
			
			NetworkTransport.Send(serverSocketId,clientConnectionId,reliableChannelId,buffer,bufferSize,out error);
			
		}
	}
	
	
	public static void Update () {
	Debug.Log("update");
		PollNetworkEvents();
	}
	
	public static void PollNetworkEvents() {
		
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
			if(CheckForNetworkError(error) == false){
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
					
					// add this connection to the list
					NewNetworkConnection(recConnectionId, recSocketId);
					
					// this is a connection event on the server
					if(recSocketId == serverSocketId){
						Debug.Log ("Server: Player " + recConnectionId.ToString() + " connected!");
						networkState = mNetworkState.connected;
					}
					// this is our client who connected
					if(recSocketId == clientSocketId){
						Debug.Log ("Client: Client connected to " + recConnectionId.ToString () + "!" );
						networkState = mNetworkState.connected;
					}
					
					
					break;
					
				case NetworkEventType.DataEvent:
					// WOOT! WE GOT DATA!
					
					// send the network manager the data to process
					mNetworkManager.ProcessNonDelegateRPC(ref recBuffer);
					
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
			return true;
		}
		else{
			return false;
		}
			
	}
	
}
