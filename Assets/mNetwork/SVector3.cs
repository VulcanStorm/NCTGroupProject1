using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

[System.Serializable]
public struct SVector3 : ISerializable {

	// components of the vector
	public float x;
	public float y;
	public float z;

	/// <summary>
	/// Initializes a new instance of the SVector3 struct with given values
	/// </summary>
	/// <param name="nx">x component</param>
	/// <param name="ny">y component</param>
	/// <param name="nz">z component</param>
	public SVector3 (float nx, float ny, float nz){
		x = nx;
		y = ny;
		z = nz;
	}

	/// <summary>
	/// Initializes a new instance of the SVector3 struct from an existing Vector3
	/// </summary>
	/// <param name="v">existing vector3 data to assign</param>
	public SVector3 (Vector3 v){
		x = v.x;
		y = v.y;
		z = v.z;
	}

	/// <summary>
	/// Returns a Vector3 based on this SVector3. Used when loading
	/// </summary>
	/// <returns> A new vector3 containing this SVector3 data</returns>
	public Vector3 Deserialise () {
		return new Vector3(x,y,z);
	}

	#region ISerializable implementation

	void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
	{
		info.AddValue ("x", x, typeof(float));
		info.AddValue ("y", y, typeof(float));
		info.AddValue ("z", z, typeof(float));
	}
	// required constructor to get data back
	public SVector3(SerializationInfo info, StreamingContext context){
		x = info.GetSingle ("x");
		y = info.GetSingle ("y");
		z = info.GetSingle ("z");
	}

	#endregion
}
