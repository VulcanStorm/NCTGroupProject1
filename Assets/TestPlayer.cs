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

	Vector3 desiredPos;
	float lastUpdateTime = 0;
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
			inputVect.z = Input.GetAxisRaw ("Vertical");
			inputVect.x = Input.GetAxisRaw ("Horizontal");
		}
	}

	void FixedUpdate ()
	{
		if (isLocal == true) {
			worldMoveVect = thisTransform.TransformDirection (inputVect);
			worldMoveVect.Normalize ();
			movePos = thisRigidbody.position + worldMoveVect * moveSpeed * Time.deltaTime;
			thisRigidbody.MovePosition (movePos);
		} else {
			float timeDelta = 5*(Time.time - lastUpdateTime);
			timeDelta = Mathf.Clamp01(timeDelta);
			thisRigidbody.position = Vector3.Lerp(thisRigidbody.position,desiredPos,timeDelta);
		}
	}

	void OnGUI ()
	{
	}

	[mNetworkRPC]
	public void UpdatePosition (SVector3 newPos, SQuaternion rotation)
	{	
		lastUpdateTime = Time.time;
		desiredPos = newPos.Deserialise ();
		thisRigidbody.rotation = rotation.Deserialise ();

	}

	public void OnMNetworkUpdate ()
	{
		if (isLocal == true) {
			thisNetworkID.SendRPC ("UpdatePosition", mNetworkRPCMode.Others, mNetwork.stateUpdateChannelId, movePos.GetSerialised (), thisRigidbody.rotation.GetSerialised());
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
