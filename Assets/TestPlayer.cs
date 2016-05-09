using UnityEngine;
using System.Collections;
using mNetworkLibrary;

public class TestPlayer : mNetworkBehaviour
{
	

	bool isLocal = false;

	Transform thisTransform;
	Rigidbody thisRigidbody;
	Vector3 movePos;
	Vector3 inputVect;
	Vector3 worldMoveVect;
	float moveSpeed = 3;
	// Use this for initialization
	void Start ()
	{
		thisTransform = this.transform;
		thisRigidbody = this.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isLocal == true) {
			inputVect.z = Input.GetAxis ("Vertical");
			inputVect.x = Input.GetAxis ("Horizontal");
		}
	}

	void FixedUpdate ()
	{
		if (isLocal == true) {
			worldMoveVect = thisTransform.TransformDirection (inputVect);
			worldMoveVect.Normalize ();
			movePos = thisRigidbody.position + worldMoveVect * moveSpeed * Time.deltaTime;
			thisRigidbody.MovePosition (movePos);
		}
	}

	void OnGUI ()
	{
	}

	[mNetworkRPC]
	public void UpdatePosition (SVector3 newPos)
	{
		thisRigidbody.MovePosition (newPos.Deserialise ());
	}

	public void OnMNetworkUpdate ()
	{
		if (isLocal == true) {
			thisNetworkID.SendRPC ("UpdatePosition", mNetworkRPCMode.Others, mNetwork.stateUpdateChannelId, movePos.GetSerialised ());
		}
		
	}

	void OnMNetworkInstantiate (mNetworkPlayer player)
	{
		if (player == mNetwork.player) {
			isLocal = true;
			Debug.Log ("This is MY INSTANTIATION");
			// get the camera
		} else {
			Debug.Log ("This is NOT MY INSTANTIATION");
			isLocal = false;
		}
		Debug.Log ("INSTANTIATED");
	}
}
