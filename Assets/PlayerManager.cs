using UnityEngine;
using System.Collections;
using mNetworkLibrary;

public class PlayerManager : mNetworkBehaviour {
	
	public GameObject playerPrefab;

	public PlayerData[] playerDataList;
	public int myPlayerIndex = -1;
	public int numDataToReceive = 1;
	public int amountOfDataReceived = 0;

	void SetupPlayerList () {
		playerDataList = new PlayerData[mNetwork.networkPlayers.Length];
		// fill the array
		for(int i=0;i <playerDataList.Length;i++){
			playerDataList[i] = new PlayerData();
		}
		// set my player index
		myPlayerIndex = mNetwork.player.playerNo;

	}

	public void OnConnectedToServer () {
		// setup the player list
		SetupPlayerList ();
		//request all player data
		StartCoroutine (GetAllPlayerData ());
	}

	IEnumerator GetAllPlayerData () {
		for (int i=0; i<playerDataList.Length; i++) {
			thisNetworkID.SendRPC("REQ_UpdatePlayerGameData",mNetworkRPCMode.Server,mNetwork.seqReliableChannelId,mNetwork.player,i);
			amountOfDataReceived = 0;
			while(amountOfDataReceived < numDataToReceive){
				yield return new WaitForEndOfFrame(); 
			}
		}
		SendMyPlayerData ();
	}


	[mNetworkRPC]
	public void REC_UpdatePlayerGameData (int index, PlayerGameData newData) {
		playerDataList[index].gameData = newData;
		amountOfDataReceived += 1;
	}

	[mNetworkRPC]
	public void REQ_UpdatePlayerGameData(mNetworkPlayer pl, int index){
		thisNetworkID.SendRPC ("REC_UpdatePlayerGameData", pl, mNetwork.seqReliableChannelId, index, playerDataList [index].gameData);
	}

	public void SendMyPlayerData () {
		SendPlayerGameData ();
	}

	private void SendPlayerGameData () {
		// update our player game data... and add a network object for us to spawn
		thisNetworkID.SendRPC ("REC_UpdatePlayerGameData", mNetworkRPCMode.Others, mNetwork.reliableChannelId, myPlayerIndex, playerDataList [myPlayerIndex].gameData);
		mNetwork.Instantiate(playerPrefab,Vector3.zero,Quaternion.identity);
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
