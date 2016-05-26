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
	int worldXSizeInt = 0;
	int worldYSizeInt = 0;
	public float tileWidth = 2f;
	public float wallHeight = 3.5f;
	public float floorThickness = 0.5f;

	MazeTile[,] mazeToGen;

	// Use this for initialization
	void Start () {
		singleton = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetWorldGenParams (float nTileWidth, float nWallHeight){
		tileWidth = nTileWidth;
		wallHeight = nWallHeight;
	}

	public void CreateMazeWorld (Vector3 center, MazeTile[,] rawData){

		mazeToGen = rawData;
			
		// create a new array

		worldSize = new Vector2(rawData.GetLength(0),rawData.GetLength(1));
		worldXSizeInt = (int)worldSize.x;
		worldYSizeInt = (int)worldSize.y;
		//WorldController.singleton.chunkSize = chunkSize;

		// create the world chunk array
		// get the sizes for the array
		int chunkXSize = Mathf.CeilToInt(worldSize.x/chunkSize);
		int chunkYSize = Mathf.CeilToInt(worldSize.y/chunkSize);
		// create a new array
		worldChunks = new WorldChunk[chunkXSize,chunkYSize];

		// determine the offset required so that the overall maze is centered at the center

		// we know the x and y size of the chunk, so get the position in the bottom left corner
		Vector3 leftCornerPos = center - new Vector3 (worldSize.x/2, 0, worldSize.y/2);
		leftCornerPos *= tileWidth;
		// each chunk can then be offset from this corner position


		// fill in the array
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				// instantiate a new chunk
				GameObject newChunk =  (GameObject)Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity);
				// set the position
				worldChunks[i,j] = newChunk.GetComponent<WorldChunk>();
				worldChunks[i,j].SetChunk(new Vector3(i*chunkSize*tileWidth,0,j*chunkSize*tileWidth)+leftCornerPos,i,j);
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
		float nextTileHeight = 0;

		
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
		if(endXPos > worldXSizeInt){
			endXChunk = worldXSizeInt-startXPos;
		}
		else{
			endXChunk = chunkSize;
		}
		
		if(endYPos > worldYSizeInt){
			endYChunk = worldYSizeInt-startYPos;
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
				// calculate the position of this tile,relative to the chunk
				// create the local position, so that the centre of the chunk is in the origin of the object
				Vector3 tilePos = new Vector3(i-(chunkSize/2),chunk.position.y,j-(chunkSize/2));
				tilePos *=tileWidth;


				// check if this is a wall tile, if so, add the wall height
				if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.wall){
					tilePos.y += wallHeight;
					// draw nothing
				}
				else if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.floor){
					
					// draw floor face
					Vector3[] newVerts = new Vector3[8];
					newVerts[0] = new Vector3(0,0,0)+tilePos;
					newVerts[1] = new Vector3(0,0,1*tileWidth)+tilePos;
					newVerts[2] = new Vector3(1*tileWidth,0,1*tileWidth)+tilePos;
					newVerts[3] = new Vector3(1*tileWidth,0,0)+tilePos;

					// draw ceiling face
					newVerts[4] = new Vector3(0,wallHeight,0)+tilePos;
					newVerts[5] = new Vector3(0,wallHeight,1*tileWidth)+tilePos;
					newVerts[6] = new Vector3(1*tileWidth,wallHeight,1*tileWidth)+tilePos;
					newVerts[7] = new Vector3(1*tileWidth,wallHeight,0)+tilePos;
					
					int[] newTris = new int[12];
					// create floor triangles
					newTris[0] = 0+verts.Count;
					newTris[1] = 1+verts.Count;
					newTris[2] = 2+verts.Count;
					newTris[3] = 0+verts.Count;
					newTris[4] = 2+verts.Count;
					newTris[5] = 3+verts.Count;

					// create ceiling triangles
					newTris[6] = 6+verts.Count;
					newTris[7] = 5+verts.Count;
					newTris[8] = 4+verts.Count;
					newTris[9] = 7+verts.Count;
					newTris[10] = 6+verts.Count;
					newTris[11] = 4+verts.Count;

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
				Vector3 tilePos = new Vector3(i-(chunkSize/2),chunk.position.y,j-(chunkSize/2));
				tilePos *=tileWidth;
				// 	1------2
				//	|	   |
				//	|	   |
				//	0------3
				// only place walls around our wall neighbours

				
				#region UPPER TILE
				// get the upper tile pos
				Vector2 upperTilePos = new Vector2(worldXCoord,worldYCoord+1);
				

				// check for out of bounds
				if(OutOfBounds(upperTilePos) == false){
					// check if this tile is a floor tile, and the upper tile isnt a floor tile
					if((mazeToGen[(int)upperTilePos.x,(int)upperTilePos.y].tileType == MazeTileType.floor) || (mazeToGen[worldXCoord,worldYCoord].tileType != MazeTileType.floor)){
						// hence no tile height
						nextTileHeight = 0;
					}
					else{
						// found a wall, so we want a wall here
						nextTileHeight = wallHeight;
					}
				}
				// we're on the edge, so check if we are a wall
				else if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.wall){
					// edge of the map, we don't want a wall here
					nextTileHeight = 0;
				}
				// always default to building a wall
				else{
					nextTileHeight = wallHeight;
				}
				
				// check if the tile height is above us, since we only build walls up
				if(nextTileHeight != 0){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(1*tileWidth,0,1*tileWidth)+tilePos;
					newVerts[1] = new Vector3(0,0,1*tileWidth)+tilePos;
					newVerts[2] = new Vector3(0,nextTileHeight,1*tileWidth)+tilePos;
					newVerts[3] = new Vector3(1*tileWidth,nextTileHeight,1*tileWidth)+tilePos;
					
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
					// 
					if((mazeToGen[(int)downTilePos.x,(int)downTilePos.y].tileType == MazeTileType.floor) || (mazeToGen[worldXCoord,worldYCoord].tileType != MazeTileType.floor)){
						// hence no tile height
						nextTileHeight = 0;
					}
					else{
						// found a wall, so we want a wall here
						nextTileHeight = wallHeight;
					}
				}
				// we're on the edge, so check if we are a wall
				else if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.wall){
					// edge of the map, we don't want a wall here
					nextTileHeight = 0;
				}
				// always default to building a wall
				else{
					nextTileHeight = wallHeight;
				}
				
				// check if the tile height is above us, since we only build walls up
				if(nextTileHeight !=0){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(0,0,0)+tilePos;
					newVerts[1] = new Vector3(1*tileWidth,0,0)+tilePos;
					newVerts[2] = new Vector3(1*tileWidth,nextTileHeight,0)+tilePos;
					newVerts[3] = new Vector3(0,nextTileHeight,0)+tilePos;
					
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
				// create the left tile pos
				Vector2 leftTilePos = new Vector2(worldXCoord-1,worldYCoord);
				
				// check for out of bounds
				if(OutOfBounds(leftTilePos) == false){
					// 
					if((mazeToGen[(int)leftTilePos.x,(int)leftTilePos.y].tileType == MazeTileType.floor) || (mazeToGen[worldXCoord,worldYCoord].tileType != MazeTileType.floor)){
						// hence no tile height
						nextTileHeight = 0;
					}
					else{
						// found a wall, so we want a wall here
						nextTileHeight = wallHeight;
					}
				}
				// we're on the edge, so check if we are a wall
				else if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.wall){
					// edge of the map, we don't want a wall here
					nextTileHeight = 0;
				}
				// always default to building a wall
				else{
					nextTileHeight = wallHeight;
				}
				
				// check if the tile height is above us, since we only build walls up
				if(nextTileHeight !=0){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(0,0,1*tileWidth)+tilePos;
					newVerts[1] = new Vector3(0,0,0)+tilePos;
					newVerts[2] = new Vector3(0,nextTileHeight,0)+tilePos;
					newVerts[3] = new Vector3(0,nextTileHeight,1*tileWidth)+tilePos;
					
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
				// create the right tile pos
				Vector2 rightTilePos = new Vector2(worldXCoord+1,worldYCoord);
				
				// check for out of bounds
				if(OutOfBounds(rightTilePos) == false){
					// 
					if((mazeToGen[(int)rightTilePos.x,(int)rightTilePos.y].tileType == MazeTileType.floor) || (mazeToGen[worldXCoord,worldYCoord].tileType != MazeTileType.floor)){
						// hence no tile height
						nextTileHeight = 0;
					}
					else{
						// found a wall, so we want a wall here
						nextTileHeight = wallHeight;
					}
				}
				// we're on the edge, so check if we are a wall
				else if(mazeToGen[worldXCoord,worldYCoord].tileType == MazeTileType.wall){
					// edge of the map, we don't want a wall here
					nextTileHeight = 0;
				}
				// always default to building a wall
				else{
					nextTileHeight = wallHeight;
				}
				
				// check if the tile height is above us, since we only build walls up
				if(nextTileHeight !=0){
					
					// we have found a tile that is lower than us...
					// create some walls down
					
					// create the vertices
					Vector3[] newVerts = new Vector3[4];
					newVerts[0] = new Vector3(1*tileWidth,0,0)+tilePos;
					newVerts[1] = new Vector3(1*tileWidth,0,1*tileWidth)+tilePos;
					newVerts[2] = new Vector3(1*tileWidth,nextTileHeight,1*tileWidth)+tilePos;
					newVerts[3] = new Vector3(1*tileWidth,nextTileHeight,0)+tilePos;
					
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

	void RemoveDuplicateVertices(ref List<Vector3> verts,ref List<int> tris){
		
		Debug.Log ("Verts before duplication removal " + verts.Count);
		Debug.Log ("Tris before duplication removal " + tris.Count);
		
		// iterate over all of the vertices
		for(int i=0;i<verts.Count;i++){
			// check if there are duplicate positions
			// iterate over all of the remaining, since any before will already have been checked
			for(int k=i;k<verts.Count;k++){
				// check if we have the same vertex as before...
				if(i == k){
					// do nothing, since we have the same vertex
				}
				else{
					// check for the same position
					if(verts[i] == verts[k]){
						// merge this vertex
						verts.RemoveAt(k);
						
						// now find all of the triangles that reference the removed vertex
						// assign these to the merged one
						for(int t=0;t<tris.Count;t++){
							if(tris[t] == k){
								tris[t] = i;
							}
							// if the vertex referenced is further on in the list
							// than the current one we are merging into, then we 
							// need to move the refernce down, since the list has just got shorter
							else if(tris[t] > k){
								tris[t] -=1;
							}
						}
						
					}
				}
			}
		}
		
		
		Debug.Log ("Verts after duplication removal " + verts.Count);
		Debug.Log ("Tris after duplication removal " + tris.Count);
		
	}

	void RemoveDuplicateVertices(ref List<Vector3> verts,ref List<int> tris, ref List<Vector2> uvs){
		
		Debug.Log ("Verts before duplication removal " + verts.Count);
		Debug.Log ("Tris before duplication removal " + tris.Count);
		Debug.Log ("UVs before duplication removal" + uvs.Count);
		
		// iterate over all of the vertices
		for(int i=0;i<verts.Count;i++){
			// check if there are duplicate positions
			// iterate over all of the remaining, since any before will already have been checked
			for(int k=i;k<verts.Count;k++){
				// check if we have the same vertex as before...
				if(i == k){
					// do nothing, since we have the same vertex
				}
				else{
					// check for the same position
					if(verts[i] == verts[k]){
						// merge this vertex
						verts.RemoveAt(k);
						// remove the associated uv
						uvs.RemoveAt(k);
						// now find all of the triangles that reference the removed vertex
						// assign these to the merged one
						for(int t=0;t<tris.Count;t++){
							if(tris[t] == k){
								tris[t] = i;
							}
							// if the vertex referenced is further on in the list
							// than the current one we are merging into, then we 
							// need to move the refernce down, since the list has just got shorter
							else if(tris[t] > k){
								tris[t] -=1;
							}
						}
						
					}
				}
			}
		}
		
		Debug.Log ("Verts after duplication removal " + verts.Count);
		Debug.Log ("Tris after duplication removal " + tris.Count);
		Debug.Log ("UVs after duplication removal" + uvs.Count);
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

	bool OutOfBounds (Vector2 pos) {
		if(pos.x < 0 || pos.x >= worldSize.x){
			return true;
		}
		else if(pos.y < 0 || pos.y >= worldSize.y){
			return true;
		}
		else{
			return false;
		}
	}
	
	bool OutOfBounds (int x, int y) {
		if(x < 0 || x >= worldSize.x){
			return true;
		}
		else if(y < 0 || y >= worldSize.y){
			return true;
		}
		else{
			return false;
		}
	}
}
