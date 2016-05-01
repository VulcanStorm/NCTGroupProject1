using UnityEngine;
using System.Collections;


public enum mNetworkIDType :byte
{
	Scene,
	Game
}

namespace mNetworkLibrary
{

	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class mNetworkObjectID: MonoBehaviour
	{
	
		// for use later, to determine which objects are in the scene
		//public mNetworkIDType idType = mNetworkIDType.Game;
		[HideInInspector]
		public bool hasAllocatedID = false;
		public bool autoAllocateID = true;
		static bool hasResetIds = false;
		//[SerializeField]
		//public ushort id = 0;
		public mNetworkID id;
	
		// TODO may need to be removed at a later date
		void Awake ()
		{
			//hasAllocatedID = false;
			//ResetAllSceneIDs();
			Debug.Log ("awake called");
			mNetworkManager.CreateNetworkManager ();
		
		}

		/*
	public void MakePersistentID(){
		mNetworkManager.RemoveID(id, idType);
		id = 0;
		idType = mNetworkIDType.Game;
		mNetworkManager.RegisterNewID(this);
	}
	*/
		public static void ResetAllSceneIDs ()
		{
			// check if we already have reset the IDs
			//if(hasResetIds == false){
			Debug.Log ("Resetting scene Ids");
			hasResetIds = true;
			// get all the network Ids that exist
			mNetworkObjectID[] netIds = Component.FindObjectsOfType<mNetworkObjectID> () as mNetworkObjectID[];
			// filter them for scene IDs
			for (int i = 0; i < netIds.Length; i++) {
				// Check if the Id is a scene Id
				if (netIds [i].id.type == mNetworkIDType.Scene) {
					// reset the allocated flag
					netIds [i].hasAllocatedID = false;
				}
			}
			//}
		}

		void Start ()
		{
			if (hasAllocatedID == false) {
				RegisterNetID ();
			}
		}

		void RegisterNetID ()
		{	
			if (Application.isEditor == true && Application.isPlaying == false) {
				id.type = mNetworkIDType.Scene;
			} else {
				id.type = mNetworkIDType.Game;
			}
			id = mNetworkManager.RegisterNewID (this);
			hasAllocatedID = true;
		}

		public void SetInGameID (mNetworkID _newId)
		{
			id = _newId;
			hasAllocatedID = true;
			autoAllocateID = false;
		}

		void OnDestroy ()
		{
			if (mNetworkManager.isCreated == true) {
				mNetworkManager.RemoveID (id);
			}
		}
	
		/*public void SendRPC (string methodName, byte[] data){
		Debug.Log ("delegate");
		ushort methodId = (ushort)RPCStore.GetIDForRPCName(methodName);
		mNetworkRPCMessage rpcData = new mNetworkRPCMessage(id,methodId,data);
		Debug.LogError ("Delegate RPCs have been removed, this message will not send");
		//mNetwork.SendRPCMessage(ref rpcData);
	}*/

		public void SendRPC (string methodName, mNetworkPlayer targetPlayer, int channel, params object[] arguments)
		{
			//Debug.Log ("sending RPC:" + methodName);
			mNetwork.SendRPCMessage (methodName, id, targetPlayer, channel, arguments);
		}

		public void SendRPC (string methodName, mNetworkRPCMode rpcMode, int channel, params object[] arguments)
		{
			//Debug.Log ("sending RPC:" + methodName);
			mNetwork.SendRPCMessage (methodName, id, rpcMode, channel, arguments);
		}
	}

}