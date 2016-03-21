using UnityEngine;
using System.Collections;

public class mNetworkUpdater : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		mNetwork.PollNetworkEvents ();
	}


}
