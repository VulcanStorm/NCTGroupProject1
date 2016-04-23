using UnityEngine;
//using UnityEditor;
using System.Collections;

using mNetworkLibrary;

[ExecuteInEditMode]
public class mNetworkIManager : MonoBehaviour {
		[SerializeField]
		bool wasInLastMode = false;
		
	void Awake () {
		if(wasInLastMode == true){
			Debug.Log ("Destroying Old INetworkManager");
			DestroyImmediate(this.gameObject);	
		}
		else{
		wasInLastMode = true;
		//mNetworkID.ResetAllSceneIDs();
		mNetworkManager.GetSceneNetIDs();
		print ("getting Net Ids from inetworkmanager");
		}
	}
	
	void Update () {
		if(mNetworkManager.isCreated == false && Application.isEditor == true){
			mNetworkManager.iManagerDoesExist = true;
			mNetworkManager.CreateNetworkManager();
			mNetworkManager.GetSceneNetIDs();
			Debug.Log ("Created New Network Manager in editor");
		}
	}
	
	void OnDestroy() {
		mNetworkManager.iManagerDoesExist = false;
		//if(mNetworkManager.singleton != null){
			//mNetworkManager.singleton.iNetworkManager = null;
		//}
		print ("destroying InetworkManager");
	}
}
