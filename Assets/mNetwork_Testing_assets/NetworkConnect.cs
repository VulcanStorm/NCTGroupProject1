using UnityEngine;
using System.Collections;
using System;

public class NetworkConnect : MonoBehaviour {
	
	public string ipAddress = "127.0.0.1";
	public ushort port = 25001;
	// Use this for initialization
	void Start () {
	RPCStore.TryLoadRPCs_ND();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI () {
		
		if(GUI.Button (new Rect(0,250,100,25),"LOLRPC")){
			mNetwork.SendRPCMessage(mNetwork.internalNetID,"LolRPC", 50);
		}
		
		// check if we are disconnected
		if(mNetwork.networkState == mNetworkState.disconnected){
			
			// check if we are not a client or a server yet
			if(mNetwork.peerType == mNetworkPeerType.none){
				// client button
				if(GUI.Button(new Rect(10,10,150,25), "Start Client")){
					// start the client
					mNetwork.SetupAsClient();
				}
				
				// server button
				if(GUI.Button (new Rect(10,40,150,25),"Start Server")){
					// start the server, not dedicated, and 4 connections max
					mNetwork.SetupAsServer(false,4,port);
				}
				try{
					port = Convert.ToUInt16(GUI.TextField(new Rect(170,40,60,25),port.ToString(),5));
				}
				catch(Exception e){
					port = 25001;
				}
			}
			// now we have begun network activitiess
			else{
				
				// check if we are a client
				if(mNetwork.peerType == mNetworkPeerType.client){
					
					// ip address box
					ipAddress = GUI.TextField(new Rect(10,10,150,25),ipAddress,15);
					// port box
					try{
						port = Convert.ToUInt16(GUI.TextField(new Rect(170,10,60,25),port.ToString(),5));
					}
					catch(Exception e){
						port = 25001;
					}
					// connect button
					if(GUI.Button(new Rect(10,40,100,25),"Connect")){
						mNetwork.Connect(ipAddress,port);
					}
				}
				
			}
			
		}
	}
}
