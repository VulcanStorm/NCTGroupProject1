using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace mNetworkLibrary
{

	public static class RPCStore
	{
	
		private static bool loadedRPCs = false;
		private static bool loadedRPCs_ND = false;

		public static void TryCallRPC (int rpcID, mNetworkBehaviour script, byte[] data)
		{
			//Debug.Log ("Call RPC Called");
			//Debug.Log ("Number of stored RPCs "+storedRPCs.Length);
			storedRPCs [rpcID].Invoke (script, data);
		}

		public static void CallRPC (int rpcID, mNetworkBehaviour script, byte[] data)
		{
			//Debug.Log ("Call RPC Called");
			//Debug.Log ("Number of stored RPCs "+storedRPCs.Length);
			storedRPCs [rpcID].Invoke (script, data);
		}

		static void TryLoadRPCs ()
		{
			// OLD DELEGATE RPC CODE
			// we can only load rpcs once
			if (loadedRPCs == false) {
				loadedRPCs = true;
				// create a data store for the rpcs
				mNetworkRPCData dataStore = mNetworkRPCData.Load ();
				// clone the data store and turn into an array
				storedRPCs = (RpcFunction[])dataStore.rpcs.Clone ();
				Debug.Log ("Loaded RPCs");
				// create a dictionary to look up name of rpc against ID number
				lookupRPCName = new Dictionary<string, int> ();
				// iterate over the array of rpcs and populate the dictionary
				for (int i = 0; i < storedRPCs.Length; i++) {
					lookupRPCName.Add (storedRPCs [i].Method.Name, i);
				}
				Debug.Log ("Created delegate RPC Name Lookup Table");
			}
			TryLoadRPCs_ND ();
		}

		public static void TryLoadRPCs_ND ()
		{
			if (loadedRPCs_ND == false) {
				loadedRPCs_ND = true;
				// create a data store for the rpcs
				mNetworkRPCData_ND dataStore = mNetworkRPCData_ND.Load ();
				// clone the data store and turn into an array
				storedRPCs_ND = (MethodInfo[])dataStore.rpcs.Clone ();
				Debug.Log ("Loaded RPCs");
				// create a dictionary to look up name of rpc against ID number
				lookupRPCNames_ND = new Dictionary<string, int> ();
				// iterate over the array of rpcs and populate the dictionary
				for (int i = 0; i < storedRPCs_ND.Length; i++) {
					lookupRPCNames_ND.Add (storedRPCs_ND [i].Name, i);
					//Debug.Log ("Added RPC:"+storedRPCs_ND[i].Name);
				}
				//Debug.Log ("Created non-delegate RPC Name Lookup Table");
			}
		}

		public static int GetIDForRPCName (string name)
		{
			int foundID;
			if (lookupRPCName.TryGetValue (name, out foundID)) {
				return foundID;
			} else {
				throw new KeyNotFoundException ("The RPC " + name + " is not found in the Lookup Table");
			}
		}

		public static int GetIDForRPCName_ND (string name)
		{
			int foundID;
			if (lookupRPCNames_ND.TryGetValue (name, out foundID)) {
				return foundID;
			} else {
				throw new KeyNotFoundException ("The RPC " + name + " is not found in the Lookup Table");
			}
		}

		public static RpcFunction[] storedRPCs;
		public static Dictionary<string,int> lookupRPCName;
	
		public static MethodInfo[] storedRPCs_ND;
		public static Dictionary<string,int> lookupRPCNames_ND;
	}

}