using UnityEngine;
using System.Collections;

public static class SerialisationMethods {
	
	// These methods add give serialisation functionality to the existing data types
	// because Vector3 and Quaternion are not serialisable
	// by using a custom structure containing the same data
	// These are used during serialisation

	/// <summary>
	///	Returns a serialisable version of this vector.
	/// </summary>
	/// <returns>The serialised vector</returns>
	/// <param name="vect">this vector to serialise</param>
	public static SVector3 GetSerialised (this Vector3 vect){
		return new SVector3(vect);
	}
	
	// quaternion is not serialisable
	// to extend the build in quaternion class, to give serialisation functionality
	// by using a custom structure containing the same data

	/// <summary>
	/// Returns a serialisable version of this quaternion
	/// </summary>
	/// <returns>The serialised quaternion</returns>
	/// <param name="vect">this quaternion to serialise</param>
	public static SQuaternion GetSerialised (this Quaternion quat){
		return new SQuaternion(quat);
	}
}

