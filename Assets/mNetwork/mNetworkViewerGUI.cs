using UnityEngine;
using System.Collections;

namespace mNetworkLibrary{

public class mNetworkViewerGUI : MonoBehaviour {
	
	public int connectionId;
	public Vector2 scrollVect = Vector2.zero;
	Rect scrollContentRect = new Rect(0,100,155,0);

	// connection rects
	Rect scrollViewRect = new Rect(0,100,175,250);
	Rect ipRect = new Rect(0,120,110,25);
	Rect portRect = new Rect(0,120,40,25);
	Rect socketRect = new Rect(0,135,150,25);
	Rect connectionRect = new Rect(0,150,150,25);
	Rect activeRect = new Rect(0,165,140,25);
	Rect drawRect;

	// socket info rects
	Rect mySocketInfoRect = new Rect(0,0,100,100);
	Rect serverIdRect = new Rect (0, 20, 100, 100);
	Rect clientSocketIdRect = new Rect (0, 40, 100, 25);
	Rect clientConnectionIdRect = new Rect (0, 60, 100, 25);
	Rect peerTypeRect = new Rect (0, 80, 150, 25);

	void Start () {
		scrollContentRect.x = Screen.width - 165;
		// network connection draw rect
		scrollViewRect.x = Screen.width - 175;
		ipRect.x = Screen.width - 155;
		portRect.x = Screen.width - 50;
		socketRect.x = Screen.width - 155;
		connectionRect.x = Screen.width - 155;
		activeRect.x = Screen.width - 155;
		// network info draw rects
		mySocketInfoRect.x = Screen.width - 100;
		serverIdRect.x = Screen.width - 95;
		clientSocketIdRect.x = Screen.width - 95;
		clientConnectionIdRect.x = Screen.width - 95;
		peerTypeRect.x = Screen.width - 95;
	}

	void OnGUI () {
		// Draw the socket information
		GUI.Box(mySocketInfoRect, "Socket Info:");
		GUI.Label (serverIdRect,"sv socket:"+mNetwork.serverSocketId.ToString());
		GUI.Label (clientSocketIdRect,"cl socket:"+mNetwork.clientSocketId.ToString ());
		GUI.Label (clientConnectionIdRect,"cl conn:"+mNetwork.clientConnectionId.ToString ());
		GUI.Label (peerTypeRect,mNetwork.peerType.ToString ());
		// network connection list
		if(mNetwork.connections != null){
			int offset = 75;
			GUI.Box(scrollViewRect,"Connection Info:");
			// set the content height to be long enough to view all the connections
			scrollContentRect.height = 10 + (mNetwork.connections.Length*offset);
			// draw the scroll view
			scrollVect = GUI.BeginScrollView(scrollViewRect,scrollVect,scrollContentRect);
			// draw all the connection data we have within the scroll view
			for(int i=0;i<mNetwork.connections.Length;i++){
				
				connectionId = i;
				drawRect = ipRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,mNetwork.connections[i].ipAddress+"::");
				drawRect = portRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,mNetwork.connections[i].port.ToString());
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
			GUI.EndScrollView();
		}
		// TODO write a player list viewer
	}
}

}