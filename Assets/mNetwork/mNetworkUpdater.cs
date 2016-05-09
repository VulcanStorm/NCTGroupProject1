using UnityEngine;
using System.Collections;

namespace mNetworkLibrary
{

	public class mNetworkUpdater : MonoBehaviour
	{
		public int updateRate = 30;

		float updateTime;
		float timer;

		void Start ()
		{
			updateTime = 1 / updateRate;
		}

		// Update is called once per frame
		// TODO implement a send rate
		void Update ()
		{
			timer += Time.deltaTime;
			if (timer > updateTime) {
				timer = 0;
				UpdateMNetwork ();

			}
		}

		void UpdateMNetwork ()
		{
			mNetworkManager.UpdateAllBehaviours ();
			mNetwork.PollNetworkEvents ();
		}

		void OnApplicationQuit ()
		{
			Debug.Log ("quit");
			mNetwork.ShutDown ();
		}


	}

}