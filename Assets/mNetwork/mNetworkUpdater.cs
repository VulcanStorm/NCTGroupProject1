using UnityEngine;
using System.Collections;

namespace mNetworkLibrary{

public class mNetworkUpdater : MonoBehaviour {
	
	// Update is called once per frame
	// TODO implement a send rate
	void Update () {
		mNetwork.PollNetworkEvents ();
	}

	void OnApplicationQuit(){
		Debug.Log("quit");
		mNetwork.ShutDown();
	}


}

}