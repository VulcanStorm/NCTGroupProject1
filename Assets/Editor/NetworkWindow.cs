using UnityEngine;
using UnityEditor;
using System.Collections;

namespace mNetworkLibrary{

[ExecuteInEditMode]
public class NetworkWindow : EditorWindow {
	static string currentScene;
	[MenuItem("Networking/NetworkWindow")]
	static void ShowNetworkWindow () {
		EditorWindow.GetWindow(typeof(NetworkWindow));
		currentScene = EditorApplication.currentScene;
	}
	
	void OnEnable () {
		Debug.Log ("ENABLED Networking Window");
		#if UNITY_EDITOR
			EditorApplication.playmodeStateChanged += PlayModeStateChanged;
			EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
		#endif
	}
	
	
	void HierarchyWindowChanged () {
		Debug.Log ("Heirarchy Window Changed");
		if(currentScene != EditorApplication.currentScene){
			Debug.Log ("Scene changed");
			currentScene = EditorApplication.currentScene;
			mNetworkManager.GetSceneNetIDs();
			Repaint();
			
		}
	}
	
	void PlayModeStateChanged () {
		//Debug.Log("Play Mode changed");
		#if UNITY_EDITOR
			Debug.Log("Play Mode changed in editor");
			// TODO hook this function up to rescan for rpc functions
			mNetworkManager.GetSceneNetIDs();
			Repaint();
		#endif
	}
	
	/*static bool isCompiling = false;
	
	void Update () {
		if(EditorApplication.isCompiling != isCompiling){
			
			isCompiling = EditorApplication.isCompiling;
		}
	}*/
	
	[UnityEditor.Callbacks.DidReloadScripts]
	static void RefreshNetworkManager () {
		mNetworkManager.CreateNetworkManager();
		mNetworkManager.GetSceneNetIDs();
		NetworkWindow.GetWindow<NetworkWindow>().Repaint();
	}
	void OnGUI () {
		
		EditorGUI.LabelField(new Rect(0,0,200,25),"TEST NETWORK WINDOW");
		if(mNetworkManager.isCreated == false){
			EditorGUI.LabelField(new Rect(5,30,200,20),"NO NETWORK MANAGER!");
		}
		else{
		EditorGUI.LabelField(new Rect(5,30,50,16),"ID No.|");
		EditorGUI.LabelField(new Rect(50,30,100,16),"Object");
		EditorGUI.LabelField(new Rect(0,50,300,16),"ID 0 is reserved for internal network messages.");
		for(int i=0;i<mNetworkManager.nextSceneNetID+1;i++){
			EditorGUI.LabelField(new Rect(15,70+i*18,25,16),i.ToString());
			mNetworkObjectID newObjectID = (mNetworkObjectID)EditorGUI.ObjectField(new Rect(50,70+i*18,200,16),mNetworkManager.sceneNetworkIDs[i].targetObject,typeof(mNetworkObjectID),true);
			// check if the value has changed, since we need to assign it now...
			//if(mNetworkManager.sceneNetworkIDs[i].targetObject != newObjectID){
				// we need to actually change these references
				//mNetworkManager.SceneIDSetFromEditor((ushort)i, newObjectID);
			//}
		}
		}
	}
}

}