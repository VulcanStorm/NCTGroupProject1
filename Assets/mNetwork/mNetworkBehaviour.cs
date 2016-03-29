using UnityEngine;
using System.Collections;
using mNetworkLibrary;

[DisallowMultipleComponent]
[RequireComponent(typeof(mNetworkObjectID))]
public class mNetworkBehaviour : MonoBehaviour {
	
	public mNetworkObjectID thisNetworkID;
	
	void Awake() {
	thisNetworkID = this.GetComponent<mNetworkObjectID>();
	if(thisNetworkID == null){
		thisNetworkID = this.gameObject.AddComponent<mNetworkObjectID>();
		
	}
	mNetworkAwake();
	}
	
	public virtual void mNetworkAwake () {
		
	}
}
