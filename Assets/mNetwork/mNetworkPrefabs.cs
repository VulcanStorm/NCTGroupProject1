using UnityEngine;
using System.Collections;

namespace mNetworkLibrary
{

	[DisallowMultipleComponent]
	public class mNetworkPrefabs : MonoBehaviour
	{

		public static mNetworkPrefabs singleton;

		public GameObject[] prefabList;

		void Awake ()
		{
			if (singleton == null) {
				singleton = this;
				DontDestroyOnLoad (gameObject);
			}
		// only allow 1 to exist
		else {
				Destroy (gameObject);
			}

		}


		// Use this for initialization
		void Start ()
		{
			
		}

		internal static int GetIdForPrefab (GameObject _prefab)
		{
			int returnId = -1;
			for (int i = 0; i < singleton.prefabList.Length; i++) {
				if (singleton.prefabList [i] == _prefab) {
					returnId = i;
					i = singleton.prefabList.Length;
				}
			}
			return returnId;
		}

		internal static void CreatePrefabFromID (int _prefabID, mNetworkID _id, SVector3 _pos, SQuaternion _rot)
		{
			Debug.Log ("Prefab ID is " + _prefabID);
			// check for a valid prefab ID
			if (_prefabID < 0 || _prefabID >= singleton.prefabList.Length) {
			
				Debug.LogError ("Invalid prefab ID given, out of range. Object not spawned!.");
				return;
			}

			// deserialise the values
			Vector3 pos = _pos.Deserialise ();
			Quaternion rot = _rot.Deserialise ();

			// spawn the prefab
			GameObject newObj = Instantiate (singleton.prefabList [_prefabID], pos, rot) as GameObject;
			mNetworkObjectID objId = newObj.GetComponent<mNetworkObjectID> ();
			if (objId == null) {
				Debug.LogWarning ("No mNetworkObjectID attached to " + newObj.name + "... adding one");
				objId = newObj.AddComponent<mNetworkObjectID> ();
			}
			// set the in-game id
			objId.SetInGameID (_id);

			// set the new object ID
			mNetworkManager.SetExistingObjectID (objId, _id);
			// call OnNetworkInstantiate on the object if it has one
			newObj.SendMessage ("OnMNetworkInstantiate", SendMessageOptions.DontRequireReceiver);
		}
	}

}
