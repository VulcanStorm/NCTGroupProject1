using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable]
public struct SQuaternion : ISerializable{

	// components of the quaternion
	public float x;
	public float y;
	public float z;
	public float w;

	/// <summary>
	/// Initializes a new instance of the SQuaternion struct with the given values.
	/// </summary>
	/// <param name="nx">x component</param>
	/// <param name="ny">y component</param>
	/// <param name="nz">z component</param>
	/// <param name="nw">w component</param>
	public SQuaternion (float nx, float ny, float nz, float nw) {
		// assign each component its new value
		x = nx;
		y = ny;
		z = nz;
		w = nw;
	}

	/// <summary>
	/// Initializes a new instance of the SQuaternion struct from an existing quaternion.
	/// </summary>
	/// <param name="q">existing quaternion data to assign</param>
	public SQuaternion(Quaternion q){
		// assign each component its new values
		x = q.x;
		y = q.y;
		z = q.z;
		w = q.w;
	}

	/// <summary>
	/// Returns a Quaternion containing this objects data. Used when loading
	/// </summary>
	///<returns> A new quaternion containing this SQuaternion data</returns>
	public Quaternion Deserialise (){
		return new Quaternion(x,y,z,w);
	}


	
	#region ISerializable implementation
	void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
	{
		info.AddValue ("x", x, typeof(float));
		info.AddValue ("y", y, typeof(float));
		info.AddValue ("z", z, typeof(float));
		info.AddValue ("w", w, typeof(float));
	}

	public SQuaternion(SerializationInfo info, StreamingContext context){
		x = info.GetSingle ("x");
		y = info.GetSingle ("y");
		z = info.GetSingle ("z");
		w = info.GetSingle ("w");
	}
	#endregion
}
