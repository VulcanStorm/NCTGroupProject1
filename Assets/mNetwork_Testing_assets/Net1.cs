using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

/*
public enum mNetworkState :byte {
	disconnected,
	connected
}
*/

/*
public enum mNetworkPeerType :byte {
	client,
	dedicatedServer,
	server
}
*/

/*
[System.Serializable]
public struct mNetworkConnection{
	
	public bool isActive;
	public int socketID;
	public int connectionID;
	public string ipAddress;
	public int port;
	
}
*/

public class Net1 : MonoBehaviour {
	
	public static Net1 singleton;
	
	public InputField ipTextField;
	public InputField portTextField;
	public int serverMaxConnections = 10;
	
	public mNetworkState netState = mNetworkState.disconnected;
	public mNetworkPeerType netPeerType;
	
	public mNetworkConnection[] connections = new mNetworkConnection[0];
	
	// network transport variables
	ConnectionConfig config;
	int reliableChannelId = -9;
	int unreliableChannelId = -9;
	int stateUpdateChannelId = -9;
	int seqReliableChannelId = -9;
	int maxConnections = -9;
	
	int serverSocketId = -9;
	int clientSocketId = -9;
	int socketPort = 25001;
	
	// connection variables
	int clientConnectionId = -9;
	public string destinationIP = "127.0.0.1";
	public int destinationPort = 25001;
	
	// booleans to control execution
	bool hasSetupNetworkTransport = false;
	
	// Use this for initialization
	void Start () {
		// initialisation
		singleton = this;
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
		
	}
	
	public void StartServer (bool isDedicated) {
		if(isDedicated == true){
			netPeerType = mNetworkPeerType.dedicatedServer;
		}
		else{
			netPeerType = mNetworkPeerType.server;
		}
		maxConnections = serverMaxConnections;
		SetupNetworkTransport();
	}
	
	public void StartClient () {
		netPeerType = mNetworkPeerType.client;
		maxConnections = 1;
		SetupNetworkTransport();
	}
	
	void SetupNetworkTransport () {
		if(hasSetupNetworkTransport == false){
		// we have called this function already, it can only be called once
		hasSetupNetworkTransport = true;
		
		// set the topology
		HostTopology svTopology = new HostTopology(config,maxConnections);
		HostTopology clTopology = new HostTopology(config,1);
		
		// get the socket ID
		// and open the socket
		try{
		
			if(netPeerType != mNetworkPeerType.client){
				serverSocketId = NetworkTransport.AddHost(svTopology,socketPort);
				Debug.Log("Server Socket Open. SocketId is: "+serverSocketId);
			}
			if(netPeerType != mNetworkPeerType.dedicatedServer){
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
	}
	
	
	
	public void Connect () {
		if(netState == mNetworkState.disconnected){
			// setup a variable to hold the error
			byte error;
			// attempt connection and get the ID of that connection
			clientConnectionId = NetworkTransport.Connect(clientSocketId, destinationIP, destinationPort,0,out error);
			NetworkError conError = (NetworkError)error;
			
			if(conError != NetworkError.Ok){
				Debug.LogError("Could not connect to server for reason: "+conError.ToString());
			}
			else{
				Debug.Log("Connected to server. ConnectionId: " + clientConnectionId);
				// add this connection to the connection list
			}
		}
	}
	
	public void ConnectWDomain (string domainName) {
		if(netState == mNetworkState.disconnected){
			// setup a variable to hold the error
			byte error;
			
			IPAddress[] ips= Dns.GetHostAddresses(domainName);
			for(int i=0;i<ips.Length;i++){
				Debug.Log (ips[i]);
			}
			destinationIP = ips[0].ToString();
			Debug.Log ("Destination IP from "+domainName+" is: "+destinationIP);
			//destinationPort = 37771;
			/*
			// attempt connection and get the ID of that connection
			clientConnectionId = NetworkTransport.Connect(clientSocketId, destinationIP, destinationPort,0,out error);
			NetworkError conError = (NetworkError)error;
			
			if(conError != NetworkError.Ok){
				Debug.LogError("Could not connect to server for reason: "+conError.ToString());
			}
			else{
				Debug.Log("Connected message sent");
				// add this connection to the connection list
			}*/
		}
	}
	
	void NewNetworkConnection (int conId, int socketID){
		string cnAddress;
		int cnPort;
		byte cnError;
		
		UnityEngine.Networking.Types.NetworkID cnNetwork;
		UnityEngine.Networking.Types.NodeID cnDstNode;
		NetworkTransport.GetConnectionInfo(socketID,conId,out cnAddress,out cnPort, out cnNetwork, out cnDstNode, out cnError);
		
		NetworkError conError = (NetworkError)cnError;
		
		if(conError != NetworkError.Ok){
			Debug.LogError("Could not get connection info for reason: "+conError.ToString());
		}
		else{
			Debug.Log ("For Connection ID: "+conId);
			Debug.Log ("Socket ID is: "+socketID);
			Debug.Log ("Address is: "+cnAddress);
			Debug.Log ("Port is: "+cnPort);
			
			// iterate over the connections array to find an empty slot
			
			for(int i=0;i<connections.Length;i++){
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
	
	void RemoveNetworkConnection (int conId) {
		for(int i=0;i<connections.Length;i++){
			if(connections[i].connectionID == conId){
				connections[i] = new mNetworkConnection();
			}
		}
	}
	
	// disconnect from the network
	public void Disconnect () {
		Debug.Log("Disconnecting");
		if(netState == mNetworkState.connected){
			byte error;
			NetworkTransport.Disconnect(serverSocketId,clientConnectionId,out error);
			CheckForNetworkError(error);
			netState = mNetworkState.disconnected;
		}
	}
	
	public void SendSocketMessage () {
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,"Hello Server");
			
			int bufferSize = 1024;
			
			NetworkTransport.Send(serverSocketId,clientConnectionId,reliableChannelId,buffer,bufferSize,out error);
			
		}
		
		
	}
	
	public void SendChatMessage (string msg) {
		byte error;
		byte[] buffer = new byte[1024];
		// using so the stream will be disposed of afterwards
		using(Stream stream = new MemoryStream(buffer)){
			
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,msg);
			
			int bufferSize = 1024;
			
			NetworkTransport.Send(serverSocketId,clientConnectionId,reliableChannelId,buffer,bufferSize,out error);
			CheckForNetworkError(error);
		}
		
		
	}
	
	
	
	// Update is called once per frame
	void Update () {
		PollNetworkEvents();
	}
	
	void PollNetworkEvents() {
		
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
						NewNetworkConnection(recConnectionId, recSocketId);
						netState = mNetworkState.connected;
								
					break;
			
					case NetworkEventType.DataEvent:
						// WOOT! WE GOT DATA!
						// deserialise the data
						using(Stream stream = new MemoryStream(recBuffer)){
							BinaryFormatter formatter = new BinaryFormatter();
							string msg = (string)formatter.Deserialize(stream);
							// print the data
							Debug.Log ("Data Event Recieved");
							Debug.Log("New Data: "+msg);
							mNetworkChat.NewLocalMsg(msg);
						}
					break;
			
					case NetworkEventType.DisconnectEvent:
					// OH NOES, THEY LEFT... :(
					RemoveNetworkConnection(recConnectionId);
					Debug.Log ("Disconnection Event Recieved");
					Debug.Log ("Disconnected in socket: "+recSocketId);
					Debug.Log ("Connection ID: "+recConnectionId);
					Debug.Log ("Recieved on channel: "+recChannelId);
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
	
	public bool CheckForNetworkError(byte _error){
		NetworkError err = (NetworkError)_error;
		
		if(err != NetworkError.Ok){
			Debug.LogError("Network Error Detected: "+err);
			return true;
		}
		else{
			return false;
		}
		
	}
	
	public void GetDestinationIP () {
		
		// get the value from the text field
		string foundIP = ipTextField.text;
		
		// TODO
		// validate IP here
		
		// assign the new value
		destinationIP = foundIP;
	}
	
	public void GetDestinationPort () {
		
		// get the value from the text field
		string foundPort = portTextField.text;
		
		// convert from string to integer
		try{
			destinationPort = Convert.ToInt32(foundPort);
		}
		catch(Exception e){
			Debug.LogError("Destination port cannot be converted, please check the input");
		}
		// TODO
		// validate port here
		
	}
	
}
