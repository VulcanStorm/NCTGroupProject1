using UnityEngine;
using System.Collections;

public class mNetworkViewerGUI : MonoBehaviour {
	
	public int connectionId;
	public Vector2 scrollVect = Vector2.zero;
	Rect scrollContentRect = new Rect(0,100,155,0);

	// connection rects
	Rect myConnectionInfoRect = new Rect(0,100,175,250);
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
		myConnectionInfoRect.x = Screen.width - 175;
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
		GUI.Box(mySocketInfoRect, "Socket Info:");
		GUI.Label (serverIdRect,"sv socket:"+mNetwork.serverSocketId.ToString());
		GUI.Label (clientSocketIdRect,"cl socket:"+mNetwork.clientSocketId.ToString ());
		GUI.Label (clientConnectionIdRect,"cl conn:"+mNetwork.clientConnectionId.ToString ());
		GUI.Label (peerTypeRect,mNetwork.peerType.ToString ());
		int offset = 75;
		if(mNetwork.connections != null){

			GUI.Box(myConnectionInfoRect,"Connection Info:");
			scrollContentRect.height = 10 + (mNetwork.connections.Length*offset);
			scrollVect = GUI.BeginScrollView(myConnectionInfoRect,scrollVect,scrollContentRect);
			for(int i=0;i<mNetwork.connections.Length;i++){
				
				connectionId = i;
				//connectionId = Mathf.Clamp(connectionId,0,mNetwork.connections.Length-1);
				drawRect = ipRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,"127.127.127.127"+"::");
				drawRect = portRect;
				drawRect.y += i*offset;
				GUI.Label(drawRect,"66666");
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
	}
}
