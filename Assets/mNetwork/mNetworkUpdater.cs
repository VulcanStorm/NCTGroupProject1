using UnityEngine;
using System.Collections;

namespace mNetworkLibrary{

public class mNetworkUpdater : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		mNetwork.PollNetworkEvents ();
	}

	void OnApplicationQuit(){
		Debug.Log("quit");
		mNetwork.ShutDown();
	}


}

}