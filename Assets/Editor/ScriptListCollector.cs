using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using mNetworkLibrary;


public class ScriptListCollector {
	
	private static bool loadedRPCs = false;
	
	//[MenuItem("Networking/PrintALLScriptPaths")]
	public static void PrintScriptList () {
		string[] assetPaths = AssetDatabase.GetAllAssetPaths();
		foreach(string assetPath in assetPaths){
			if(assetPath.Contains (".cs"))
			{
				Debug.Log(assetPath);
			}
		}
	}
	
	
	
	
	//private static Dictionary<string,MonoScript> allScripts = new Dictionary<string, MonoScript>();
	
	// delegate function to store rpc methods
	
	/*
	public static void TryCallRPC(int rpcID, mNetworkBehaviour script, byte[] data){
		//Debug.Log ("Call RPC Called");
		//Debug.Log ("Number of stored RPCs "+storedRPCs.Length);
		storedRPCs[rpcID].Invoke(script,data);
	}
	
	public static void CallRPC(int rpcID, mNetworkBehaviour script, byte[] data){
		//Debug.Log ("Call RPC Called");
		//Debug.Log ("Number of stored RPCs "+storedRPCs.Length);
		storedRPCs[rpcID].Invoke(script,data);
	}
	
	public static void TryLoadRPCs () {
		// we can only load rpcs once
		if(loadedRPCs == false){
			loadedRPCs = true;
			// create a data store for the rpcs
			mNetworkRPCData dataStore = mNetworkRPCData.Load();
			// clone the data store and turn into an array
			storedRPCs = (RpcFunction[])dataStore.rpcs.Clone();
			Debug.Log ("Loaded RPCs");
			// create a dictionary to look up name of rpc against ID number
			lookupRPCName = new Dictionary<string, int>();
			// iterate over the array of rpcs and populate the dictionary
			for(int i=0;i<storedRPCs.Length;i++){
				lookupRPCName.Add(storedRPCs[i].Method.Name,i);
			}
			Debug.Log ("Created RPC Name Lookup Table");
		}
	}
	
	public static int GetIDForRPCName (string name){
		int foundID;
		if(lookupRPCName.TryGetValue(name,out foundID)){
			return foundID;
		}
		else{
			throw new KeyNotFoundException("The RPC "+name+" is not found in the Lookup Table");
		}
	}
	
	public static RpcFunction[] storedRPCs; 
	public static Dictionary<string,int> lookupRPCName;
	*/
	
	[MenuItem("Networking/Recalculate Network Scene IDs")]
	public static void ReScanSceneIDs(){
		mNetworkManager.ReallocateSceneIds();
	}
	
	//[MenuItem("Networking/Refresh RPC Array")]
	/*public static void CreateRPCList(){
		string path = Application.dataPath;
		Debug.Log (path);
		//allScripts.Clear ();
		RpcFunction[] storedRPCs = new RpcFunction[0];
		MonoScript[] foundStuff = MonoImporter.GetAllRuntimeMonoScripts();
		UnityEngine.Object[] scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
		
		Debug.Log ("number of scripts found: "+scripts.Length);
		Debug.Log ("number of found stuff found: "+foundStuff.Length);
		List<RpcFunction> rpcList = new List<RpcFunction>();
		
		foreach(MonoScript stuff in foundStuff){
			//if(script.GetType().Equals(typeof(MonoScript))){
			
			//	allScripts.Add (script.name,(MonoScript)script);
				///Debug.Log(stuff.name);
				Type stuffType = stuff.GetClass();
				//Debug.Log("Class: "+stuffType);
			//}
			if(stuffType != null){
				try{
					if(stuffType.BaseType == typeof(mNetworkBehaviour)){
						Debug.Log ("WOOOOOT");
						Debug.Log (stuff.name + " is derived from Network Behaviour");
						
						// get all the methods in the class
						MethodInfo[] methodsInStuff = stuffType.GetMethods();
						Debug.Log ("There are "+methodsInStuff.Length+" methods that are declared in the class");
						for(int i=0;i<methodsInStuff.Length;i++){
								
							//Debug.Log("loop "+i);
							//Debug.Log (methodsInStuff[i].DeclaringType);
							if(methodsInStuff[i].DeclaringType == stuffType){
								Debug.Log (methodsInStuff[i].Name+"()");
								object[] methodAttributes = methodsInStuff[i].GetCustomAttributes(typeof(mNetworkRPC),false);
								if(methodAttributes.Length !=0){
									// create delegate function
									Debug.Log ("RPC FUNCTION: "+methodsInStuff[i].Name+"()");
									RpcFunction methodDelegate = (RpcFunction)Delegate.CreateDelegate(typeof(RpcFunction),methodsInStuff[i]);
									rpcList.Add (methodDelegate);
									
									// add the function to the array
									Debug.Log ("RPC was added to list");
								}
							}
							//object[] methodAttributes = methodsInStuff[i].GetCustomAttributes(true);
						//for(int n=0;n<methodAttributes.Length;i++){
							//if(methodAttributes[i].Equals (typeof(mNetworkRPC))){
								//Debug.Log ("Found RPC Attribute");
								//Debug.Log (methodsInStuff[i].Name);
							}
						}
							//Debug.Log ("There are "+methodAttributes.Length+" attributes");
						}
					}
					//if(stuffClass 
				//Debug.Log (stuff.name + "Could be converted to a monobehaviour");
				}
				catch(Exception e){
						Debug.LogException(e);
				//Debug.Log (stuff.name + "Could not be converted to a NetworkBehaviour");
				}
			}
		}
		
		storedRPCs = rpcList.ToArray();
		// TODO serialise this array :D
		// then load it up at runtime, and call the functions!
		Debug.Log ("Total RPCs stored: "+storedRPCs.Length);
		//Debug.Log ("Total MonoScripts Found: "+allScripts.Count);
		try{
		Debug.Log ("Creating file");
		mNetworkRPCData dataToSave = new mNetworkRPCData();
		dataToSave.rpcs = new RpcFunction[storedRPCs.Length];
		Debug.Log("rpc array length = "+dataToSave.rpcs.Length);
		Debug.Log("rpc source array length = "+storedRPCs.Length);
		storedRPCs.CopyTo(dataToSave.rpcs,0);
		dataToSave.Save();
		}
		catch(Exception e){
			Debug.LogException(e);
		}
		
		
	}*/
	
	[MenuItem("Networking/Create RPC List Non-Delegate")]
	public static void CreateRPCListNoDelegate(){
		string path = Application.dataPath;
		Debug.Log (path);
		//allScripts.Clear ();
		MethodInfo[] storedRPCs = new MethodInfo[0];
		MonoScript[] foundStuff = MonoImporter.GetAllRuntimeMonoScripts();

		//Debug.Log ("number of scripts found: "+foundStuff.Length);
		List<MethodInfo> rpcList = new List<MethodInfo>();
		
		foreach(MonoScript stuff in foundStuff){
			//if(script.GetType().Equals(typeof(MonoScript))){
			
			//	allScripts.Add (script.name,(MonoScript)script);
			//Debug.Log("Reading:" +stuff.name);
			Type stuffType = stuff.GetClass();
			//Debug.Log("Found Class: "+stuffType);
			//}
			if(stuffType != null){
				try{
					//if(stuffType.BaseType == typeof(mNetworkBehaviour)){
						//Debug.Log ("WOOOOOT");
						//Debug.Log (stuff.name + " is derived from Network Behaviour");
						//Debug.Log (stuff.name + " is being checked");
						// get all the methods in the class
					MethodInfo[] methodsInStuff = stuffType.GetMethods(BindingFlags.Default|BindingFlags.DeclaredOnly|BindingFlags.ExactBinding|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public);
						//Debug.Log ("There are "+methodsInStuff.Length+" methods that exist in the class");
					//
					//MethodInfo[] methods2 = stuffType.GetMethods(BindingFlags.Default|BindingFlags.DeclaredOnly|BindingFlags.ExactBinding|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public);
						//Debug.Log("There are "+methods2.Length+" methods that are explicitly declared");
						//for(int i=0;i<methods2.Length;i++){
						//Debug.Log(methods2[i].Name+"()");
						//}
						//Debug.Log("<<--- END METHODS 2 --->>");
						for(int i=0;i<methodsInStuff.Length;i++){
							
							//Debug.Log("loop "+i);
							if(methodsInStuff[i].DeclaringType == stuffType){
								//Debug.Log (methodsInStuff[i].Name+"()");
								object[] methodAttributes = methodsInStuff[i].GetCustomAttributes(typeof(mNetworkRPC),false);
								if(methodAttributes.Length !=0){
									// create delegate function
									Debug.Log ("RPC FUNCTION: "+methodsInStuff[i].Name+"()");
									Debug.Log ("DELCARED IN TYPE: "+methodsInStuff[i].DeclaringType);
									rpcList.Add (methodsInStuff[i]);
									
									// add the function to the array
									//Debug.Log ("RPC was added to list");
								}
							}
							/*object[] methodAttributes = methodsInStuff[i].GetCustomAttributes(true);
						for(int n=0;n<methodAttributes.Length;i++){
							if(methodAttributes[i].Equals (typeof(mNetworkRPC))){
								Debug.Log ("Found RPC Attribute");
								Debug.Log (methodsInStuff[i].Name);
							}
						}*/
							//Debug.Log ("There are "+methodAttributes.Length+" attributes");
						}
					//}
					//if(stuffClass 
					//Debug.Log (stuff.name + "Could be converted to a monobehaviour");
				}
				catch(Exception e){
					Debug.LogException(e);
					//Debug.Log (stuff.name + "Could not be converted to a NetworkBehaviour");
				}
			}
		}
		
		storedRPCs = rpcList.ToArray();
		// TODO serialise this array :D
		// then load it up at runtime, and call the functions!
		Debug.Log ("Total RPCs non delegate stored: "+storedRPCs.Length);
		//Debug.Log ("Total MonoScripts Found: "+allScripts.Count);
		try{
			Debug.Log ("Creating file");
			mNetworkRPCData_ND dataToSave = new mNetworkRPCData_ND();
			dataToSave.rpcs = new MethodInfo[storedRPCs.Length];
			Debug.Log("rpc array length = "+dataToSave.rpcs.Length);
			Debug.Log("rpc source array length = "+storedRPCs.Length);
			storedRPCs.CopyTo(dataToSave.rpcs,0);
			dataToSave.Save();
		}
		catch(Exception e){
			Debug.LogException(e);
		}
		
		
	}
	
}
