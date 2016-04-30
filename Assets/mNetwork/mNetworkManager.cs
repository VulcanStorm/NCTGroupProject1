using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace mNetworkLibrary{

public static class mNetworkManager{
	
	
	public static bool isCreated = false;
	public static bool iManagerDoesExist = false;
	//[SerializeField]
	//public mNetworkIManager iNetworkManager;
	// linked list to hold all the ids to re-use
	static LinkedList<ushort> reUsedGameIDs = new LinkedList<ushort>();
	static LinkedList<ushort> reUsedSceneIDs = new LinkedList<ushort>();
	
	// fixed array to hold the network objects currently in use
	// with the indices corresponding to the network ID of the object
	static public mNetworkIDData[] gameNetworkIDs;
	static public mNetworkIDData[] sceneNetworkIDs;
	
	
	// integer to hold the position in the array to be used for the next id
	static int nextGameNetID = -1;
	static public int nextSceneNetID = 0;
	
	
	
	// TODO: write a network id synchroniser...
	// for in game
	
	public static void CreateNetworkManager () {
		if(isCreated == false){
			isCreated = true;
			
			gameNetworkIDs = new mNetworkIDData[256];
			sceneNetworkIDs = new mNetworkIDData[64];
			GetSceneNetIDs();
			Debug.Log("NETWORK MANAGER CREATED!");
			System.GC.Collect();
			/*GameObject go = new GameObject();
			go.name = "INetworkManager";
			go.AddComponent<mNetworkIManager>();*/
			//mNetworkIManager.doesExist = true;
		}
		//Debug.Log ("Create Net manager Called");
		//Debug.Log (mNetworkIManager.doesExist);
		
		/*if(iManagerDoesExist == false){
			Debug.Log ("CREATING NETWORK iMANAGER");
			iManagerDoesExist = true;
			GameObject go = new GameObject();
			go.name = "INetworkManager";
			go.AddComponent<mNetworkIManager>();
		}*/
		
	}
	
	public static void GetSceneNetIDs () {
		Debug.Log ("Getting Scene Network IDs");
		// create a variable to hold the largest ID detected
		int largestId = 0;
		// get all the network Ids that exist
		mNetworkObjectID[] netIds = Component.FindObjectsOfType<mNetworkObjectID>() as mNetworkObjectID[];
		Debug.Log ("Network objects Ids total "+netIds.Length);
		// filter them for scene IDs
		for(int i=0;i<netIds.Length;i++){
			// Check if the Id is a scene Id
			if(netIds[i].id.type == mNetworkIDType.Scene){
				// add the ID to the list
				sceneNetworkIDs[netIds[i].id.idNum].mNetID = netIds[i].id;
				sceneNetworkIDs[netIds[i].id.idNum].targetObject = netIds[i];
				// notify the object that its id has been allocated
				netIds[i].hasAllocatedID = true;
				// check if this ID is greater than the last
				if(netIds[i].id.idNum > largestId){
					// set the new largets ID
					largestId = netIds[i].id.idNum;
				}
			}
		}
		
		// first assign the next scene Id to be allocated
			nextSceneNetID = largestId;
			Debug.Log ("Largest Scene ID is: "+nextSceneNetID);
			
		
		// check for any gaps in the list
		// those IDs that dont have an object
		for(int i=1;i<(largestId+1);i++){
			if(sceneNetworkIDs[i].targetObject == null){
				// fill the unused Ids onto this list
				reUsedSceneIDs.AddLast((ushort)i);
				Debug.Log ("ID is Unused: "+i);
			}
			else{
				Debug.Log ("ID is Used: "+i);
			}
		}
	}

	
	public static void ReallocateSceneIds () {
		CreateNetworkManager();
		// create a variable to hold the last ID allocated
		ushort lastIDNumAllocated = 0;
		// get all the network Ids that exist
		mNetworkObjectID[] netObjectIds = Component.FindObjectsOfType<mNetworkObjectID>() as mNetworkObjectID[];
		// filter them for scene IDs
		for(int i=0;i<netObjectIds.Length;i++){
			// Check if the Id is a scene Id
			if(netObjectIds[i].id.type == mNetworkIDType.Scene){
				
				// increase the last IDNumAllocated
				// this is the same position in the network scene ID list
				lastIDNumAllocated ++;
				// set the ID number to the script
				netObjectIds[i].id.idNum = lastIDNumAllocated;
				// assign the new ID in the array
				sceneNetworkIDs[lastIDNumAllocated].mNetID = netObjectIds[i].id;
				sceneNetworkIDs[lastIDNumAllocated].targetObject = netObjectIds[i];
				// nofity the object that it has an ID
				netObjectIds[i].hasAllocatedID = true;
				
			}
		}
		// set the next ID to allocate
		nextSceneNetID = lastIDNumAllocated;
		Debug.Log ("Allocated "+netObjectIds.Length+" IDs");
		Debug.Log ("NextSceneNetID is "+nextSceneNetID);
		// and remove all the IDs to re-use, since no more exist
		reUsedSceneIDs.Clear();
	}
	
	public static mNetworkID RegisterNewID(mNetworkObjectID obj){
	// returns new ID given
	
		// variable to hold new ID
		mNetworkID newID;
		// set the ID type
		newID.type = obj.id.type;
		
		// check if this is a scene ID or a game ID
		if(newID.type == mNetworkIDType.Game){
			// THIS IS A GAME ID
			// get the next id available
			
			// check the list of re-used IDs
			if(reUsedGameIDs.Count != 0){
				newID.idNum = reUsedGameIDs.First.Value;
				reUsedGameIDs.RemoveFirst();
			}
			// no IDs in the list, so get the next ID.
			else{
				nextGameNetID ++;
				// check for overflow
				if(nextGameNetID == gameNetworkIDs.Length){
					Debug.LogError("The requested new Game ID cannot be allocated, all IDs have been used up");
					throw new IndexOutOfRangeException();
				}
				Debug.Log ("Assigning Game ID number:"+nextGameNetID);
				newID.idNum = (ushort)nextGameNetID;
			}
			
			newID.type = mNetworkIDType.Game;
			gameNetworkIDs[newID.idNum].mNetID = newID;
			gameNetworkIDs[newID.idNum].targetObject = obj;
		}
		else {
			// THIS IS A SCENE ID
			// get the next id available
			
			// check the list of re-used IDs
			if(reUsedSceneIDs.Count != 0){
				newID.idNum = reUsedSceneIDs.First.Value;
				reUsedSceneIDs.RemoveFirst();
			}
			// no IDs in the list, so get the next ID.
			else{
				nextSceneNetID ++;
				// check for overflow
				if(nextSceneNetID == sceneNetworkIDs.Length){
					Debug.LogError("The requested new Scene ID cannot be allocated, all IDs have been used up");
					throw new IndexOutOfRangeException();
				}
				Debug.Log ("Assigning Scene ID number:"+nextSceneNetID);
				newID.idNum = (ushort)nextSceneNetID;
			}
			newID.type = mNetworkIDType.Scene;
			sceneNetworkIDs[newID.idNum].mNetID = newID;
			sceneNetworkIDs[newID.idNum].targetObject = obj;
		}
		
		return newID;
		
	}

	/// <summary>
	/// Sets an existing object Id. Used when network instantiating.
	/// </summary>
	/// <param name="_objId">Object identifier.</param>
	/// <param name="_netId">Net identifier.</param>
	internal static void SetExistingObjectID (mNetworkObjectID _objId, mNetworkID _netId) {
		gameNetworkIDs[_netId.idNum].mNetID = _objId.id;
		gameNetworkIDs[_netId.idNum].targetObject = _objId;
		// this object has already allocated an ID, and don't auto allocate one.
		_objId.hasAllocatedID = true;
		_objId.autoAllocateID = false;

	}


	public static mNetworkID GetNextUnusedGameID () {
		mNetworkID newID;
			if(reUsedGameIDs.Count != 0){
				newID.idNum = reUsedGameIDs.First.Value;
				reUsedGameIDs.RemoveFirst();
			}
			// no IDs in the list, so get the next ID.
			else{
				nextGameNetID ++;
				// check for overflow
				if(nextGameNetID == gameNetworkIDs.Length){
					Debug.LogError("The requested new Game ID cannot be allocated, all IDs have been used up");
					throw new IndexOutOfRangeException();
				}
				Debug.Log ("Assigning Game ID number:"+nextGameNetID);
				newID.idNum = (ushort)nextGameNetID;
			}
			// set the ID type to be a game id
			newID.type = mNetworkIDType.Game;
			return newID;
	}
	
	
	public static void SceneIDSetFromEditor(ushort index, mNetworkObjectID obj){
		// check if this ID was null before
		if(sceneNetworkIDs[index].targetObject == null){
			// remove this value from the re-used IDs
			reUsedSceneIDs.Remove(index);
			sceneNetworkIDs[index].targetObject = obj;
			sceneNetworkIDs[index].mNetID = new mNetworkID(index,mNetworkIDType.Scene);
			
		}
	}
	
	public static void RemoveID(mNetworkID id){
		if(id.type == mNetworkIDType.Game){
			// nullify the reference to the component
			gameNetworkIDs[id.idNum].targetObject = null;
			// add the id to the re-used list
			reUsedGameIDs.AddLast(id.idNum);
		}
		else{
			// nullify the reference to the component
			sceneNetworkIDs[id.idNum].targetObject = null;
			// add the id to the re-used list
			reUsedSceneIDs.AddLast(id.idNum);
		}
	}
	
	/*public static void ProcessDelegateRPC(ref byte[] rawData){
		
		try{
		
		using(Stream stream = new MemoryStream(rawData)){
			//deserialise the data
			BinaryFormatter formatter = new BinaryFormatter();
			mNetworkRPCMessage msg = (mNetworkRPCMessage)formatter.Deserialize(stream);
			// find the type of script that this method belongs to
			System.Type classType = RPCStore.storedRPCs[msg.targetMethodId].Method.DeclaringType;
			Debug.Log ("Class type is "+classType);
			// create a variable to hold the script reference
			mNetworkBehaviour netScript;
			// get the script on the object
			if(msg.targetIdType == mNetworkIDType.Game){
				netScript = gameNetworkIDs[msg.targetNetId].targetObject.GetComponent(classType) as mNetworkBehaviour;
			}
			else{
				netScript = sceneNetworkIDs[msg.targetNetId].targetObject.GetComponent(classType) as mNetworkBehaviour;
			}
			// execute the method
			Debug.Log ("Calling method");
			RPCStore.storedRPCs[msg.targetMethodId].Invoke(netScript,msg.data);	
		}
		
		}
		catch(Exception e){
			Debug.LogException(e);
		}
	}*/

	#region RPC PROCESSING
	/// <summary>
	/// Processes a non-delegate RPC.
	/// </summary>
	/// <param name="rawData">The Raw byte array data.</param>
	/// <param name="socketID">The socket that this RPC was recieved in. -1 if local.</param>
	/// <param name="connectionID">The connection ID that this message came in on.</param> 
	public static void ProcessNonDelegateRPC(ref byte[] rawData, int socketID, int connectionID, int channelID){
		
		try{
			
			using(Stream stream = new MemoryStream(rawData)){
				//deserialise the data
				BinaryFormatter formatter = new BinaryFormatter();

				mNetworkRPCMessage_ND msg = (mNetworkRPCMessage_ND)formatter.Deserialize(stream);
				// read the message and determine the required action

				// check if it was recieved in the client socket
				if(socketID == mNetwork.clientSocketId){
					// process this like a client
					local_ProcessRPC_ND(msg);
				}
				// check if it was recieved in the server socket
				else if(socketID == mNetwork.serverSocketId){
					// this needs to be re-directed unless it is for the server
					switch(msg.rpcMode){
					// SEND TO ALL
					case mNetworkRPCMode.All:
						// redistribute this message to everyone
						Debug.Log("Redistributing message to all...");

						int i=0;
						// check if we are a server, since we also posess a client
						if(mNetwork.peerType == mNetworkPeerType.server){
						// skip the first player, since this will always be our client, and we're calling the function locally anyway
							i = 1;
						}

						for(;i<mNetwork.networkPlayers.Length;i++){
							// check if the player is active
							if(mNetwork.networkPlayers[i].isActive == true){
							// get the connection ID
							int relayConID = mNetwork.GetConnectionIDForPlayer(i);
							// send to the player
							mNetwork.sv_RelayRPCToConnection(ref rawData,relayConID,channelID);
							}
						}

						// if we're a dedicated server, we need this message too... since it won't be relayed to our client
						local_ProcessRPC_ND(msg);
					break;
					// SEND TO SERVER ONLY
					case mNetworkRPCMode.Server:
						Debug.Log("Handling the message on server...");
						// just process this here locally
						local_ProcessRPC_ND(msg);
					break;

					case mNetworkRPCMode.None:
						Debug.Log("Forwarding to correct client...");
						// get the connection ID
						int targetConID = mNetwork.GetConnectionIDForPlayer(msg.networkPlayer.playerNo);
						// check if this exists
						if(targetConID != -1){
							// forward the message
							mNetwork.sv_RelayRPCToConnection(ref rawData,targetConID,channelID);
						}
						else{
								throw new ArgumentNullException("NO RPC Target given. RPCMode is none, and player is invalid (out of range or not active).");
						}

					break;
					case mNetworkRPCMode.Others:
						Debug.Log("Redistributing message to others...");

						int a=0;
						// check if we are a server, since we also posess a client
						if(mNetwork.peerType == mNetworkPeerType.server){
						// skip the first player, since this will always be our client, and we're calling the function locally anyway
							a = 1;
						}

						for(;a<mNetwork.networkPlayers.Length;a++){
							// check if the player is active
							if(mNetwork.networkPlayers[a].isActive == true && mNetwork.connections[a].connectionID != connectionID){
							// get the connection ID for this player
							int relayConID = mNetwork.GetConnectionIDForPlayer(a);
							// send to the player
							mNetwork.sv_RelayRPCToConnection(ref rawData,relayConID,channelID);
							}
						}

						// if we're a dedicated server, we need this message too... since it won't be relayed to our client
						local_ProcessRPC_ND(msg);

						//throw new NotImplementedException("FINISH THIS CODE HERE");
					break;
					}
				}
			}
		}
		catch(Exception e){
			Debug.LogException(e);
		}
	}

	/// <summary>
	/// CLIENT. Processes an RPC message.
	/// </summary>
	/// <param name="_msg">Message.</param>
	private static void local_ProcessRPC_ND(mNetworkRPCMessage_ND _msg){
		// find the type of script that this method belongs to
		System.Type classType = RPCStore.storedRPCs_ND[_msg.targetMethodId].DeclaringType;
		Debug.Log ("Class of RPC being processed is "+classType);
		// create a variable to hold the script reference
		mNetworkBehaviour netScript = null;
		// create a variable to hold whether this is an internal RPC
		bool isInternalRPC = false;
		// get the script on the object
		if(_msg.targetIdType == mNetworkIDType.Game){
			netScript = gameNetworkIDs[_msg.targetNetId].targetObject.GetComponent(classType) as mNetworkBehaviour;
		}
		else{
		// check id this is for the network manager
			if(_msg.targetNetId == 0){
				isInternalRPC = true;
			}
			else{
				netScript = sceneNetworkIDs[_msg.targetNetId].targetObject.GetComponent(classType) as mNetworkBehaviour;
			}
		}


		// execute the method
		Debug.Log ("Calling method:"+RPCStore.storedRPCs_ND[_msg.targetMethodId].Name);
		MethodInfo methodInfo = RPCStore.storedRPCs_ND[_msg.targetMethodId];
		ParameterInfo[] parameters = methodInfo.GetParameters();
		object[] newParameterData = new object[_msg.data.Length];
		Debug.Log("There are "+parameters.Length+" parameters");
		Debug.Log("There are "+_msg.data.Length+" values");
		for(int i=0;i<_msg.data.Length;i++){
			try{
				newParameterData[i] = Convert.ChangeType(_msg.data[i],parameters[i].ParameterType);
			}
			catch(Exception e){
				Debug.LogException(e);
			}
		}
		// check if this RPC is internal
		if(isInternalRPC == false){
			RPCStore.storedRPCs_ND[_msg.targetMethodId].Invoke(netScript,newParameterData);	
		}
		else{
			RPCStore.storedRPCs_ND[_msg.targetMethodId].Invoke (null,newParameterData);
		}
	}

#endregion
	
}

}