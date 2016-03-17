using UnityEngine;
using System.Collections;

public class mNetworkViewerGUI : MonoBehaviour {
	
	Rect conRect = new Rect(0,0,150,25);
	
	public int connectionId;
	
	Rect ipRect = new Rect(0,0,150,25);
	Rect portRect = new Rect(0,0,50,25);
	Rect socketRect = new Rect(0,25,150,25);
	Rect connectionRect = new Rect(0,50,150,25);
	Rect activeRect = new Rect(0,75,150,25);
	
	void Start () {
		ipRect.x = Screen.width - 200;
		portRect.x = Screen.width - 50;
		socketRect.x = Screen.width - 150;
		connectionRect.x = Screen.width - 150;
		activeRect.x = Screen.width - 150;
	}
	
	void OnGUI () {
		if(mNetwork.connections.Length != 0 && mNetwork.connections != null){
			connectionId = Mathf.Clamp(connectionId,0,mNetwork.connections.Length-1);
			GUI.Label(ipRect,mNetwork.connections[connectionId].ipAddress);
			GUI.Label(portRect,mNetwork.connections[connectionId].port.ToString());
			GUI.Label(socketRect,"Socket:"+mNetwork.connections[connectionId].socketID);
			GUI.Label(connectionRect, "Con ID:"+mNetwork.connections[connectionId].connectionID);
			GUI.Label(activeRect, "Active?:"+mNetwork.connections[connectionId].isActive);
		}
	}
}
