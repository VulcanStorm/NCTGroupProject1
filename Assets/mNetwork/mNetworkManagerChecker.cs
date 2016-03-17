using UnityEngine;
using System.Collections;

public class mNetworkManagerChecker : MonoBehaviour {

	
	/*void Awake () {
		if(mNetworkManager.isCreated == false){
			mNetworkManager.iManagerDoesExist = true;
			mNetworkManager.CreateNetworkManager();
			mNetworkManager.GetSceneNetIDs();
			Debug.Log ("Created New Network Manager");
		}
	}*/
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	/*void Update () {
		if(mNetworkManager.isCreated == false){
			mNetworkManager.iManagerDoesExist = true;
			mNetworkManager.CreateNetworkManager();
			mNetworkManager.GetSceneNetIDs();
			Debug.Log ("Created New Network Manager");
		}
	}*/
	
	void OnGUI () {
		if(GUI.Button(new Rect(10,10,120,25),"LoadGameScene")){
			Application.LoadLevel("net_game_scene_test");
		}
		
		if(GUI.Button(new Rect(140,10,100,25),"RunGC")){
			System.GC.Collect();
		}
	}
}
