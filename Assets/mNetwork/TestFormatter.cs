using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class TestFormatter : MonoBehaviour {

	byte[] data;
	public string receivedData;
	// Use this for initialization
	void Start () {
		SerialiseData ();
		DeserialiseData ();
	}


	void SerialiseData () {
		using (Stream stream = new MemoryStream()) {
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,"MEGA TEST STRING RAAAAWRRRR");
			data = new byte[stream.Length];
			Debug.Log("MEGA TEST "+stream.Length);
			stream.Seek(0,SeekOrigin.Begin);
			stream.Read(data,0,(int)stream.Length);
		}
	}

	void DeserialiseData () {
		using (Stream stream = new MemoryStream(data,0,data.Length)) {
			BinaryFormatter formatter = new BinaryFormatter();
			//stream.Seek(0,SeekOrigin.Begin);
			receivedData = (string)formatter.Deserialize(stream);
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
