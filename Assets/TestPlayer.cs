using UnityEngine;
using System.Collections;

public class TestPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMNetworkInstantiate () {
		Debug.Log("INSTANTIATED");
	}
}
