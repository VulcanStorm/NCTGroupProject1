using UnityEngine;
using System.Collections;

public class rpc_attribute_test_script : MonoBehaviour {
	
	
	public int freddo;
	
	[mNetworkRPC]
	public void SayJeff () {
		Debug.Log("Jeff");
	}
	
	[mNetworkRPC]
	public void SayBobby () {
		Debug.Log("Bobby");
	}
	
	public void NotJeffBobby () {
		Debug.Log("Not Jeff, Or Bobby");
	}
}
