using UnityEngine;
using System.Collections;

public class mNetworkViewerGUI : MonoBehaviour {
	
	Rect conRect = new Rect(0,0,150,25);
	
	public int connectionId;

	// connection rects
	Rect ipRect = new Rect(0,125,150,25);
	Rect portRect = new Rect(0,125,50,25);
	Rect socketRect = new Rect(0,145,150,25);
	Rect connectionRect = new Rect(0,165,150,25);
	Rect activeRect = new Rect(0,185,150,25);
	Rect drawRect;

	// socket info rects
	Rect mySocketInfoRect = new Rect(0,0,100,100);
	Rect serverIdRect = new Rect (0, 20, 100, 100);
	Rect clientSocketIdRect = new Rect (0, 40, 100, 25);
	Rect clientConnectionIdRect = new Rect (0, 60, 100, 25);
	Rect peerTypeRect = new Rect (0, 80, 150, 25);

	void Start () {
		// network connection draw rect
		ipRect.x = Screen.width - 200;
		portRect.x = Screen.width - 50;
		socketRect.x = Screen.width - 150;
		connectionRect.x = Screen.width - 150;
		activeRect.x = Screen.width - 150;
		// network info draw rects
		mySocketInfoRect.x = Screen.width - 100;
		serverIdRect.x = Screen.width - 95;
		clientSocketIdRect.x = Screen.width - 95;
		clientConnectionIdRect.x = Screen.width - 95;
		peerTypeRect.x = Screen.width - 95;
	}

	void OnGUI () {
		GUI.Box(mySocketInfoRect, "Socket Info:");
		GUI.Label (serverIdRect,"sv socket:"+mNetwork.serverSocketId.ToString());
		GUI.Label (clientSocketIdRect,"cl socket:"+mNetwork.clientSocketId.ToString ());
		GUI.Label (clientConnectionIdRect,"cl conn:"+mNetwork.clientConnectionId.ToString ());
		GUI.Label (peerTypeRect,mNetwork.peerType.ToString ());

		if(mNetwork.connections != null){
			//if(mNetwork.connections.Length != 0){
			for(int i=0;i<mNetwork.connections.Length;i++){
				int offset = 95;
				connectionId = i;
				//connectionId = Mathf.Clamp(connectionId,0,mNetwork.connections.Length-1);
				drawRect = ipRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,mNetwork.connections[connectionId].ipAddress);
				drawRect = portRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,mNetwork.connections[connectionId].port.ToString());
				drawRect = socketRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,"Socket:"+mNetwork.connections[connectionId].socketID);
				drawRect = connectionRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect, "Con ID:"+mNetwork.connections[connectionId].connectionID);
				drawRect = activeRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect, "Active?:"+mNetwork.connections[connectionId].isActive);

				}
			//}
		}
	}
}
