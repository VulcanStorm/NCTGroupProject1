using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

	public Transform placeholder;
	Transform thisTransform;
	// Use this for initialization
	void Start () {
		thisTransform = this.transform;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate () {
		if (placeholder != null) {
			thisTransform.position = placeholder.position;
			thisTransform.rotation = placeholder.rotation;
		}
	}
}
