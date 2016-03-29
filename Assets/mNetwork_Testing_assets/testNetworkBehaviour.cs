using UnityEngine;
using System.Collections;

using mNetworkLibrary;

public class testNetworkBehaviour : mNetworkBehaviour {
	
	public Material red;

	// Use this for initialization
	void Start () {
		Debug.Log ("Default Start Called");
		byte[] bytes = new byte[5];
		// TODO!!!!! make this automatic NOW
		RPCStore.TryLoadRPCs_ND();
		// END TODO
		mNetwork.StartmNetwork();
		mNetwork.SetupAsServer(false,10,25001);
		mNetwork.Connect("127.0.0.1",25001);
		//ScriptListCollector.ListScriptObjects();
		//ScriptListCollector.TryCallRPC(0,this,bytes);
	}
	
	public int freddo;
	
	/*[mNetworkRPC]
	public static void SayJeff (mNetworkBehaviour netScript, byte[] data) {
		testNetworkBehaviour thisScript = netScript as testNetworkBehaviour;
		Debug.Log("Jeff and Freddo! "+thisScript.freddo);
		thisScript.GetComponent<MeshRenderer>().sharedMaterial = thisScript.red;
		
	}
	
	[mNetworkRPC]
	public static void SayBobby (mNetworkBehaviour thisScript, byte[] data) {
		Debug.Log("Bobby");
	}
	
	public void NotJeffOrBobby () {
		Debug.Log("Not Jeff, Or Bobby");
	}*/
	
	[mNetworkRPC]
	public void TestHi (string name){
		Debug.Log (name);
	}
	
	public void TryRPC () {
		thisNetworkID.SendRPC("TestHi",mNetworkRPCMode.All,mNetwork.reliableChannelId,"hi");
	}
	
	void OnGUI () {
		if(GUI.Button(new Rect(0,Screen.height-30,100,30),"TRY RPC")){
			TryRPC();
		}
	}
}
