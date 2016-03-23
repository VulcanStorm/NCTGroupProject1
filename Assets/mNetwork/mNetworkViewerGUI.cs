using UnityEngine;
using System.Collections;

public class mNetworkViewerGUI : MonoBehaviour {
	
	Rect conRect = new Rect(0,0,150,25);
	
	public int connectionId;
	
	Rect ipRect = new Rect(0,0,150,25);
	Rect portRect = new Rect(0,0,50,25);
	Rect socketRect = new Rect(0,20,150,25);
	Rect connectionRect = new Rect(0,40,150,25);
	Rect activeRect = new Rect(0,60,150,25);
	Rect serverIdRect = new Rect (0, 0, 100, 25);
	Rect clientSocketIdRect = new Rect (0, 25, 100, 25);
	Rect clientConnectionIdRect = new Rect (0, 50, 100, 25);
	Rect peerTypeRect = new Rect (0, 75, 150, 25);
	Rect drawRect;
	void Start () {
		// network connection draw rect
		ipRect.x = Screen.width - 200;
		portRect.x = Screen.width - 50;
		socketRect.x = Screen.width - 150;
		connectionRect.x = Screen.width - 150;
		activeRect.x = Screen.width - 150;
		// network info draw rects
		serverIdRect.x = Screen.width - 300;
		clientSocketIdRect.x = Screen.width - 300;
		clientConnectionIdRect.x = Screen.width - 300;
		peerTypeRect.x = Screen.width - 300;
	}
	
	void OnGUI () {

		GUI.Box (serverIdRect,"sv socket:"+mNetwork.serverSocketId.ToString());
		GUI.Box (clientSocketIdRect,"cl socket:"+mNetwork.clientSocketId.ToString ());
		GUI.Box (clientConnectionIdRect,"cl conn:"+mNetwork.clientConnectionId.ToString ());
		GUI.Box (peerTypeRect,mNetwork.peerType.ToString ());

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
