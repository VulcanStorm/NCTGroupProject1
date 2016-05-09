using UnityEngine;
using System.Collections;
using mNetworkLibrary;

public class TestPlayer : mNetworkBehaviour
{

	public Transform camPos = null;
	PlayerCamera camScript;

	bool isLocal = false;

	Transform thisTransform;
	Rigidbody thisRigidbody;
	Vector3 movePos;
	Vector3 inputVect;
	Vector3 worldMoveVect;
	public float moveSpeed = 3;

	Vector3 desiredPos;
	float lastUpdateTime = 0;

	float yawInput;
	float pitchInput;
	Quaternion moveRot;
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

			yawInput = Input.GetAxisRaw ("Mouse X");
			pitchInput = Input.GetAxis("Mouse Y");
		}
	}

	void FixedUpdate ()
	{
		if (isLocal == true) {
			// calculate the world move vector
			worldMoveVect = thisTransform.TransformDirection (inputVect);
			// normalise this (make a length of 1)
			worldMoveVect.Normalize ();
			// calculate the move position
			movePos = thisRigidbody.position + worldMoveVect * moveSpeed * Time.deltaTime;
			// move the rigidbody to that position
			thisRigidbody.MovePosition (movePos);
			// calculate our new rotation
			moveRot = Quaternion.AngleAxis(yawInput,Vector3.up)*thisRigidbody.rotation;
			// move the rigidboyd to that rotation
			thisRigidbody.MoveRotation(moveRot);
		} else {
			// calculate the time delta
			float timeDelta = 5*(Time.time - lastUpdateTime);
			// clamp the time delta between 0 and 1 for use with the lerp
			timeDelta = Mathf.Clamp01(timeDelta);
			// calculate the new move position
			movePos = Vector3.Lerp(thisRigidbody.position,desiredPos,timeDelta);
			// move the rigidbody to the new position
			thisRigidbody.MovePosition(movePos);
			// force zero velocities
			thisRigidbody.angularVelocity = Vector3.zero;
			thisRigidbody.velocity = Vector3.zero;
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
			camScript = Camera.main.GetComponent<PlayerCamera>();
			camScript.placeholder = camPos;
		} else {
			Debug.Log ("This is NOT MY INSTANTIATION");
			isLocal = false;
		}
		Debug.Log ("INSTANTIATED");
	}
}
