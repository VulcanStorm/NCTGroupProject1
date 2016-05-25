using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeBuilder : MonoBehaviour {

	public static MazeBuilder singleton;
	public Transform worldContainer;
	public GameObject chunkPrefab;

	public WorldChunk[,] worldChunks = new WorldChunk[0,0];
	public int chunkSize = 8;

	Vector2 worldSize;
	float tileWidth = 1.5f;
	float wallHeight = 2.5f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetWorldGenParams (float nTileWidth, float nWallHeight){
		tileWidth = nTileWidth;
		wallHeight = nWallHeight;
	}

	void CreateMazeWorld (Vector3 center, MazeTile[,] rawData){
			
			
		// create a new array

		worldSize = new Vector2(rawData.GetLength(0),rawData.GetLength(1));
		WorldController.singleton.chunkSize = chunkSize;

		// create the world chunk array
		// get the sizes for the array
		int chunkXSize = Mathf.CeilToInt(worldSize.x/chunkSize);
		int chunkYSize = Mathf.CeilToInt(worldSize.y/chunkSize);
		// create a new array
		worldChunks = new WorldChunk[chunkXSize,chunkYSize];

		// determine the offset required so that the overall maze is centered at the center

		// we know the x and y size of the chunk, so get the position in the bottom left corner
		Vector3 leftCornerPos = center - new Vector3 (worldSize.x/2, 0, worldSize.y/2);
		// each chunk can then be offset from this corner position


		// fill in the array
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				// instantiate a new chunk
				GameObject newChunk =  (GameObject)Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity);
				// set the position
				worldChunks[i,j] = newChunk.GetComponent<WorldChunk>();
				worldChunks[i,j].SetChunk(new Vector3(i*chunkSize,0,j*chunkSize)+leftCornerPos,i,j);
				if(worldContainer != null){
					newChunk.transform.parent = worldContainer;
				}
			}
		}
			
		CreateNewWorldMesh();
	}

	void CreateNewWorldMesh () {
		
		// now the world is made up of chunks, iterate over each chunk, and create the associated geometry
		
		for(int a=0;a<worldChunks.GetLength(0);a++){
			for(int b=0;b<worldChunks.GetLength(1);b++){
				CreateChunkMesh(a,b,ref worldChunks[a,b]);
			}
		}
		System.GC.Collect();
		
	}


	void CreateChunkMesh (int x, int y, ref WorldChunk chunk){
		// create a vertex list
		List<Vector3> verts = new List<Vector3>();
		// create a triangle list
		List<int> tris = new List<int>();
		// create a uv list
		List<Vector2> uvs = new List<Vector2>();
		// create a new mesh
		/*Mesh newMesh = new Mesh();
		newMesh.name = ("Chunk_"+x+","+y);*/
		
		// setup the tile height variables for the walls
		short nextTileHeight = 0;
		short currentTileHeight = 0;
		
		#region FLOOR
		
		// calculate the possible end points of the chunk
		// so we know what to fill in
		int startXPos = x*chunkSize;
		int startYPos = y*chunkSize;
		int endXPos = (x+1)*chunkSize;
		int endYPos = (y+1)*chunkSize;
		
		// these are the real ends of the chunk, so we dont overflow
		int endXChunk = -1;
		int endYChunk = -1;
		
		// set the real ends of the chunk
		if(endXPos > world.GetLength(0)){
			endXChunk = world.GetLength(0)-startXPos;
		}
		else{
			endXChunk = chunkSize;
		}
		
		if(endYPos > world.GetLength(1)){
			endYChunk = world.GetLength(1)-startYPos;
		}
		else{
			endYChunk = chunkSize;
		}
		
		// draw the floor
		for(int i=0;i<endXChunk;i++){
			for(int j=0;j<endYChunk;j++){
				
				// calculate the world array coordinates
				int worldXCoord = i+startXPos;
				int worldYCoord = j+startYPos;
				
				// check if the current tile height is zero, since this shouldnt be drawn
				if(world[worldXCoord,worldYCoord].height != 0){
					
					// calculate the position of this tile,relative to the chunk
					// create the local position, so that the centre of the chunk is in the origin of the object
					Vector3 tilePos = new Vector3(i-(chunkSize/2),world[worldXCoord,worldYCoord].height,j-(chunkSize/2));
					
					// draw a face here
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(0,0,0)+tilePos;
					newVerts[1] = new Vector3(0,0,1)+tilePos;
					newVerts[2] = new Vector3(1,0,1)+tilePos;
					newVerts[3] = new Vector3(1,0,0)+tilePos;
					
					int[] newTris = new int[6];
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;
					
					for(int n=0;n<newVerts.Length;n++){
						verts.Add(newVerts[n]);	
					}
					
					for(int n=0;n<newTris.Length;n++){
						tris.Add(newTris[n]);
					}
					
				}
			}
		}
		
		
		
		// now optimise this
		RemoveDuplicateVertices(ref verts, ref tris);
		// now calculate the uvs, after optimising the vertices
		for(int i=0;i<verts.Count;i++){
			uvs.Add (new Vector2(verts[i].x,verts[i].z));
		}
		
		#endregion
		
		#region WALLS
		
		// draw the walls down
		for(int i=0;i<endXChunk;i++){
			for(int j=0;j<endYChunk;j++){
				
				// calculate the world array coordinates
				int worldXCoord = i+startXPos;
				int worldYCoord = j+startYPos;
				
				// set our tile pos
				Vector3 tilePos = new Vector3(i-(chunkSize/2),world[worldXCoord,worldYCoord].height,j-(chunkSize/2));
				// 	1------2
				//	|	   |
				//	|	   |
				//	0------3
				// determine if the neighbours are below us
				// dont bother building edges up, since we can just build them down
				
				// get our current height
				currentTileHeight = world[worldXCoord,worldYCoord].height;
				
				#region UPPER TILE
				// create the upper tile pos
				Vector2 upperTilePos = new Vector2(worldXCoord,worldYCoord+1);
				
				//short nextTileHeight = 0;
				// check for out of bounds
				if(OutOfBounds(upperTilePos) == false){
					nextTileHeight = world[(int)upperTilePos.x,(int)upperTilePos.y].height;
				}
				else{
					nextTileHeight = 0;
				}
				
				// check if the tile is lower than us
				if(nextTileHeight < currentTileHeight){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(1,0,1)+tilePos;
					newVerts[1] = new Vector3(0,0,1)+tilePos;
					newVerts[2] = new Vector3(0,nextTileHeight-currentTileHeight,1)+tilePos;
					newVerts[3] = new Vector3(1,nextTileHeight-currentTileHeight,1)+tilePos;
					
					// create the triangles
					int[] newTris = new int[6];
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;
					
					// add these to the mesh
					for(int n=0;n<newVerts.Length;n++){
						verts.Add(newVerts[n]);
					}
					
					for(int n=0;n<newTris.Length;n++){
						tris.Add(newTris[n]);
					}
					
					// add appropriate uvs for the vertices
					for(int n=0;n<newVerts.Length;n++){
						uvs.Add (new Vector2((newVerts[n].y),(newVerts[n].x)));
					}
				}
				
				#endregion
				
				#region DOWN TILE
				// create the down tile pos
				Vector2 downTilePos = new Vector2(worldXCoord,worldYCoord-1);
				
				// check for out of bounds
				if(OutOfBounds(downTilePos) == false){
					nextTileHeight = world[(int)downTilePos.x,(int)downTilePos.y].height;
				}
				else{
					nextTileHeight = 0;
				}
				
				// check if the tile is lower than us
				if(nextTileHeight < currentTileHeight){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(0,0,0)+tilePos;
					newVerts[1] = new Vector3(1,0,0)+tilePos;
					newVerts[2] = new Vector3(1,nextTileHeight-currentTileHeight,0)+tilePos;
					newVerts[3] = new Vector3(0,nextTileHeight-currentTileHeight,0)+tilePos;
					
					// create the triangles
					int[] newTris = new int[6];
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;
					
					// add these to the mesh
					for(int n=0;n<newVerts.Length;n++){
						verts.Add(newVerts[n]);
					}
					
					for(int n=0;n<newTris.Length;n++){
						tris.Add(newTris[n]);
					}
					
					// add appropriate uvs for the vertices
					for(int n=0;n<newVerts.Length;n++){
						uvs.Add (new Vector2((newVerts[n].y),(newVerts[n].x)));
					}
				} 
				
				#endregion
				
				#region LEFT TILE
				// create the upper tile pos
				Vector2 leftTilePos = new Vector2(worldXCoord-1,worldYCoord);
				
				// check for out of bounds
				if(OutOfBounds(leftTilePos) == false){
					nextTileHeight = world[(int)leftTilePos.x,(int)leftTilePos.y].height;
				}
				else{
					nextTileHeight = 0;
				}
				
				// check if the tile is lower than us
				if(nextTileHeight < currentTileHeight){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(0,0,1)+tilePos;
					newVerts[1] = new Vector3(0,0,0)+tilePos;
					newVerts[2] = new Vector3(0,nextTileHeight-currentTileHeight,0)+tilePos;
					newVerts[3] = new Vector3(0,nextTileHeight-currentTileHeight,1)+tilePos;
					
					// create the triangles
					int[] newTris = new int[6];
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;
					
					// add these to the mesh
					for(int n=0;n<newVerts.Length;n++){
						verts.Add(newVerts[n]);
					}
					
					for(int n=0;n<newTris.Length;n++){
						tris.Add(newTris[n]);
					}
					
					// add appropriate uvs for the vertices
					for(int n=0;n<newVerts.Length;n++){
						uvs.Add (new Vector2((newVerts[n].y),(newVerts[n].z)));
					}
				} 
				
				#endregion
				
				#region RIGHT TILE
				// create the upper tile pos
				Vector2 rightTilePos = new Vector2(worldXCoord+1,worldYCoord);
				
				// check for out of bounds
				if(OutOfBounds(rightTilePos) == false){
					nextTileHeight = world[(int)rightTilePos.x,(int)rightTilePos.y].height;
				}
				else{
					nextTileHeight = 0;
				}
				
				// check if the tile is lower than us
				if(nextTileHeight < currentTileHeight){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(1,0,0)+tilePos;
					newVerts[1] = new Vector3(1,0,1)+tilePos;
					newVerts[2] = new Vector3(1,nextTileHeight-currentTileHeight,1)+tilePos;
					newVerts[3] = new Vector3(1,nextTileHeight-currentTileHeight,0)+tilePos;
					
					// create the triangles
					int[] newTris = new int[6];
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;
					
					
					// add these to the mesh
					for(int n=0;n<newVerts.Length;n++){
						verts.Add(newVerts[n]);
					}
					
					for(int n=0;n<newTris.Length;n++){
						tris.Add(newTris[n]);
					}
					
					// add appropriate uvs for the vertices
					for(int n=0;n<newVerts.Length;n++){
						uvs.Add (new Vector2((newVerts[n].y),(newVerts[n].z)));
					}
					
				} 
				
				#endregion
			}
		}
		
		#endregion
		
		
		// finally write this mesh data back to the chunk, so it can be rendered
		//RemoveDuplicateVertices(ref verts, ref tris, ref uvs);
		chunk.chunkMesh.Clear();
		chunk.chunkMesh.vertices = verts.ToArray();
		chunk.chunkMesh.triangles = tris.ToArray();
		chunk.chunkMesh.uv = uvs.ToArray();
		chunk.chunkMesh.RecalculateBounds();
		chunk.chunkMesh.RecalculateNormals();
		chunk.chunkMesh.Optimize();
		chunk.UpdateMesh();
		
		
		
		
	}

	// used to destroy the world
	public void DestroyWorld () {
		// clear the previous world if there was one
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				worldChunks[i,j].DestroyChunk();
				worldChunks[i,j] = null;
			}
		}
	}
}
