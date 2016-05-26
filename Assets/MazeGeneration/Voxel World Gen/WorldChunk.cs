using UnityEngine;
using System.Collections;

public class WorldChunk : MonoBehaviour {
	
	public byte size = 0;
	public bool needRegen = false;
	public bool hasCollider;
	public Mesh chunkMesh;
	public MeshFilter meshFilter;
	public Vector3 position;
	public MeshCollider meshCollider;
	
	// function to set the local chunk variables
	public void SetChunk (Vector3 newPosition,int xPos, int yPos) {
		position = newPosition;
		transform.position = position;
		chunkMesh = new Mesh();
		chunkMesh.name = "Chunk_"+xPos+","+yPos;
	}
	
	public void UpdateMesh(){
		meshFilter.sharedMesh = chunkMesh;
		UpdateCollider();
	}
	
	public void AddCollider () {
		if(hasCollider == false){
			hasCollider = true;
			gameObject.AddComponent<MeshCollider>();
			meshCollider = this.GetComponent<MeshCollider>();
			UpdateCollider();
		}
	}
	
	public void UpdateCollider () {
		if(hasCollider == true){
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = meshFilter.sharedMesh;
		}
	}
	
	public void DestroyChunk () {
		Destroy (gameObject);
	}
	
}
