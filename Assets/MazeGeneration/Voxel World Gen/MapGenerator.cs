using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
	
	public static MapGenerator singleton;
	public Transform worldContainer;
	public MeshFilter floorMeshFilter;
	public MeshFilter wallMeshFilter;
	public GameObject chunkPrefab;
	
	public WorldChunk[,] worldChunks = new WorldChunk[0,0];
	public int chunkSize = 16;
	
	
	MapTile[,] world;
	Vector2 worldSize;
	int[] ripple;
	Mesh floorMesh;
	Mesh wallMesh;

	void Awake () {
		singleton = this;
	}
	
	
	// Use this for initialization
	void Start () {		
	}
	
	public void AddCollidersToChunks () {
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				worldChunks[i,j].AddCollider();
			}
		}
	}
	
	
	public void RefreshChunks(){
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				if(worldChunks[i,j].needRegen == true){
					worldChunks[i,j].needRegen = false;
					RefreshChunkAtPosition(i,j);
				}
			}
		}
	}

	/// <summary>
	/// Updates a chunk at a specific position index.
	/// </summary>
	/// <param name="xPos">X position.</param>
	/// <param name="yPos">Y position.</param>
	void RefreshChunkAtPosition(int xPos, int yPos){
		CreateChunkMesh(xPos,yPos,ref worldChunks[xPos,yPos]);
	}
	
	
	// ONLY USED BY THE MAP EDITOR
	public void CreateEmptyWorld(Vector2 size){
		
		// create a new array
		world = WorldController.singleton.world;
		worldSize = size;
		WorldController.singleton.chunkSize = chunkSize;
		
		// create the world chunk array
		// get the sizes for the array
		int chunkXSize = Mathf.CeilToInt(size.x/chunkSize);
		int chunkYSize = Mathf.CeilToInt(size.y/chunkSize);
		// create the array
		worldChunks = new WorldChunk[chunkXSize,chunkYSize];
		// determine the offset required so that the world starts at 0,0
		Vector3 extraPos = new Vector3(chunkSize/2,0,chunkSize/2);
		
		// fill in the array
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				// instantiate a new chunk
				GameObject newChunk =  (GameObject)Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity);
				// set the position
				worldChunks[i,j] = newChunk.GetComponent<WorldChunk>();
				worldChunks[i,j].SetChunk(new Vector3(i*chunkSize,0,j*chunkSize)+extraPos,i,j);
				if(worldContainer != null){
					newChunk.transform.parent = worldContainer;
				}
			}
		}
		
		// random thing here
		/*for(int n=0;n<ripple.Length;n++){
		if(n < (ripple.Length-1)){
			ripple[n] = ripple[n+1];
		}
		else{
			ripple[n] = ripple[0];
		}
		}*/
		
		
		// populate the array
		for(int i=0;i<world.GetLength(0);i++){
			for(int j=0;j<world.GetLength(1);j++){
				
				// for minecraft like world, do this
				//world[i,j] = new MapTile((byte)(j+i));
				
				
				// draw some borders
				if((i==0 || i==(size.x-1)) || (j==0 || j==(size.y-1))){
					world[i,j] = new MapTile(10);
				}
				else{
					world[i,j] = new MapTile(1);
				}
				
			}
			
			
		}
		// create the world mesh
		
		CreateNewWorldMesh();
		// TODO remove this
		MapEditor.CreateColliders();
		/*CreateWorldMesh(size);
		floorMeshFilter.mesh = floorMesh;
		wallMeshFilter.mesh = wallMesh;
		print ("Floor:");
		print ("Total Vertices: "+floorMesh.vertexCount);
		print ("Total Triangles: "+(floorMesh.triangles.Length/3));
		
		print ("Walls:");
		print ("Total Vertices: "+wallMesh.vertexCount);
		print ("Total Triangles: "+(wallMesh.triangles.Length/3));*/
		
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
	
	// TO BE USED IN GAME
	// also used to load a new level
	public void CreateLoadedWorldMesh (Vector2 size) {
		
		
		// create a new array
		world = WorldController.singleton.world;
		worldSize = size;
		WorldController.singleton.chunkSize = chunkSize;
		
		// create the world chunk array
		// get the sizes for the array
		int chunkXSize = Mathf.CeilToInt(size.x/chunkSize);
		int chunkYSize = Mathf.CeilToInt(size.y/chunkSize);
		// create a new array
		worldChunks = new WorldChunk[chunkXSize,chunkYSize];
		// determine the offset required so that the world starts at 0,0
		Vector3 extraPos = new Vector3(chunkSize/2,0,chunkSize/2);
		
		// fill in the array
		for(int i=0;i<worldChunks.GetLength(0);i++){
			for(int j=0;j<worldChunks.GetLength(1);j++){
				// instantiate a new chunk
				GameObject newChunk =  (GameObject)Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity);
				// set the position
				worldChunks[i,j] = newChunk.GetComponent<WorldChunk>();
				worldChunks[i,j].SetChunk(new Vector3(i*chunkSize,0,j*chunkSize)+extraPos,i,j);
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
	
	/// <summary>
	/// Creates a world mesh. Deprecated.
	/// </summary>
	/// <param name="wrldSize">Wrld size.</param>
	/*void CreateWorldMesh(Vector2 wrldSize) {
		
		// create a vertex list
		List<Vector3> verts = new List<Vector3>();
		// create a triangle list
		List<int> tris = new List<int>();
		// create a uv list
		List<Vector2> uvs = new List<Vector2>();
		// create a new mesh
		floorMesh = new Mesh();
		floorMesh.name = "Floor";
		
		wallMesh = new Mesh();
		wallMesh.name = "Walls";
		
		#region FLOOR
		
		// draw the floor
		for(int i=0;i<world.GetLength(0);i++){
			for(int j=0;j<world.GetLength(1);j++){
				
				// check if the height is zero, since this shouldnt be drawn
				if(world[i,j].height != 0){
					
					Vector3 tilePos = new Vector3(i-(wrldSize.x/2),world[i,j].height,j-(wrldSize.y/2));
				
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
		
		#endregion 
		
		
		
		// now write to the floor
		RemoveDuplicateVertices(ref verts, ref tris);
		// now calculate the uvs, after optimising the vertices
		for(int i=0;i<verts.Count;i++){
			uvs.Add (new Vector2(verts[i].x,verts[i].z));
		}
		
		floorMesh.vertices = verts.ToArray();
		floorMesh.triangles = tris.ToArray();
		floorMesh.uv = uvs.ToArray();
		floorMesh.RecalculateBounds();
		floorMesh.RecalculateNormals();
		floorMesh.Optimize();
		
		// clear the lists ready for the walls
		verts.Clear();
		tris.Clear();
		uvs.Clear();
		
		
		#region WALLS
		
		// draw the walls down
		for(int i=0;i<world.GetLength(0);i++){
			for(int j=0;j<world.GetLength(1);j++){
				
				// set our tile pos
				Vector3 tilePos = new Vector3(i-(wrldSize.x/2),world[i,j].height,j-(wrldSize.y/2));
				// 	1------2
				//	|	   |
				//	|	   |
				//	0------3
				// determine if the neighbours are below us
				// dont bother building edges up, since we can just build them down
				
				#region UPPER TILE
				// create the upper tile pos
				Vector2 upperTilePos = new Vector2(i,j+1);
				
				// check for out of bounds
				if(OutOfBounds(upperTilePos) == false){
					short nextTileHeight = world[(int)upperTilePos.x,(int)upperTilePos.y].height;
					short currentTileHeight = world[i,j].height;
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
				}
				
				#endregion
				
				#region DOWN TILE
				// create the down tile pos
				Vector2 downTilePos = new Vector2(i,j-1);
				
				// check for out of bounds
				if(OutOfBounds(downTilePos) == false){
					
					short nextTileHeight = world[(int)downTilePos.x,(int)downTilePos.y].height;
					short currentTileHeight = world[i,j].height;
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
				}
				
				#endregion
				
				#region LEFT TILE
				// create the upper tile pos
				Vector2 leftTilePos = new Vector2(i-1,j);
				
				// check for out of bounds
				if(OutOfBounds(leftTilePos) == false){
					
					short nextTileHeight = world[(int)leftTilePos.x,(int)leftTilePos.y].height;
					short currentTileHeight = world[i,j].height;
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
				}
				
				#endregion
				
				#region RIGHT TILE
				// create the upper tile pos
				Vector2 rightTilePos = new Vector2(i+1,j);
				
				// check for out of bounds
				if(OutOfBounds(rightTilePos) == false){
					
					short nextTileHeight = world[(int)rightTilePos.x,(int)rightTilePos.y].height;
					short currentTileHeight = world[i,j].height;
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
				}
				
				#endregion
			}
		}
		
		#endregion
		
		// now write to the walls
		//RemoveDuplicateVertices(ref verts, ref tris, ref uvs);
		wallMesh.vertices = verts.ToArray();
		wallMesh.triangles = tris.ToArray();
		wallMesh.uv = uvs.ToArray();
		wallMesh.RecalculateBounds();
		wallMesh.RecalculateNormals();
		wallMesh.Optimize();
		
		
		
	}
	*/
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
	
	List<Vector3> editVerts = new List<Vector3>();
	List<int> editTris = new List<int>();
	List<Vector2> editUvs = new List<Vector2>();
	
	public void CreateEditMesh (int editHeight, int xPos, int yPos, int brushSize) {
		//print ("creating edit mesh");
		
		// clear any stuff remaining from the last generation
		editVerts.Clear ();
		editTris.Clear ();
		editUvs.Clear();
		
		// create arrays to hold the new vertices and triangles
		Vector3[] newEditVerts = new Vector3[4];
		int[] newEditTris = new int[6];
		
		
		short editNextTileHeight;
		
		int startXPos = xPos-((brushSize-1)/2);
		int startYPos = yPos-((brushSize-1)/2);
		int endXPos = xPos+((brushSize-1)/2)+1;
		int endYPos = yPos+((brushSize-1)/2)+1;
		
		
		// set the real ends and starts of the block
		if(startXPos < 0){
			startXPos = 0;
		}
		
		if(startYPos < 0){
			startYPos = 0;
		}
		
		if(endXPos > world.GetLength(0)){
			endXPos = world.GetLength(0);
		}
		
		if(endYPos > world.GetLength(1)){
			endYPos = world.GetLength(1);
		}
		
		
		for(int i=startXPos;i<endXPos;i++){
			for(int j=startYPos;j<endYPos;j++){
				
				// determine the world position of this tile based on the center
				//int worldXPos = i+(xPos-((brushSize)/2));
				//int worldYPos = i+(yPos-((brushSize)/2));
				//print ("world pos is "+worldXPos+","+worldYPos);
				// check if the height is zero, since this shouldnt be drawn
				// check to see if the tile lies out of bounds
				//if(OutOfBounds(worldXPos,worldYPos) == false){			
					Vector3 tilePos = new Vector3(i,editHeight,j);
					
					// draw a face here
				
					newEditVerts[0] = new Vector3(0,0,0)+tilePos;
					newEditVerts[1] = new Vector3(0,0,1)+tilePos;
					newEditVerts[2] = new Vector3(1,0,1)+tilePos;
					newEditVerts[3] = new Vector3(1,0,0)+tilePos;
					
				
					newEditTris[0] = 0+editVerts.Count;
					newEditTris[1] = 1+editVerts.Count;
					newEditTris[2] = 2+editVerts.Count;
					newEditTris[3] = 0+editVerts.Count;
					newEditTris[4] = 2+editVerts.Count;
					newEditTris[5] = 3+editVerts.Count;
					
					for(int n=0;n<newEditVerts.Length;n++){
						editVerts.Add(newEditVerts[n]);	
					}
					
					for(int n=0;n<newEditTris.Length;n++){
						editTris.Add(newEditTris[n]);
					}
				//}
			}
		}
		
		for(int i=0;i<editVerts.Count;i++){
			editUvs.Add (new Vector2(editVerts[i].x,editVerts[i].z));
		}
		
		#region WALLS
		
		// draw the walls down
		for(int i=startXPos;i<endXPos;i++){
			for(int j=startYPos;j<endYPos;j++){
				// only draw walls for the end tiles
				if((i==startXPos || i==(endXPos-1)) || (j==startYPos || j==(endYPos-1))){
				// calculate the world array coordinates
				int worldXCoord = i;
				int worldYCoord = j;
				
				// set our tile pos
				Vector3 tilePos = new Vector3(i,editHeight,j);
				// 	1------2
				//	|	   |
				//	|	   |
				//	0------3
				// determine if the neighbours are below us
				// dont bother building edges up, since we can just build them down
				
				#region UPPER TILE
				// create the upper tile pos
				Vector2 upperTilePos = new Vector2(worldXCoord,worldYCoord+1);
				// check to see if this tile is part of the edited set
				if(!(upperTilePos.x >= startXPos && upperTilePos.x <endXPos)||!(upperTilePos.y >=startYPos && upperTilePos.y <endYPos)){
				// check for out of bounds
				// determine that the tile is on the edge
				if(OutOfBounds(upperTilePos) == false){
					editNextTileHeight = world[(int)upperTilePos.x,(int)upperTilePos.y].height;
				}
				else{
					editNextTileHeight = world[worldXCoord,worldYCoord].height;
				}
					// check if the tile is less than us
					if(editNextTileHeight != editHeight){
					
						// we have found a tile that is lower than us...
						// create some walls down
						
						// create the vertices
						newEditVerts[0] = new Vector3(1,0,1)+tilePos;
						newEditVerts[1] = new Vector3(0,0,1)+tilePos;
						newEditVerts[2] = new Vector3(0,editNextTileHeight-editHeight,1)+tilePos;
						newEditVerts[3] = new Vector3(1,editNextTileHeight-editHeight,1)+tilePos;
						
						// create the triangles
						newEditTris[0] = 0+editVerts.Count;
						newEditTris[1] = 1+editVerts.Count;
						newEditTris[2] = 2+editVerts.Count;
						newEditTris[3] = 0+editVerts.Count;
						newEditTris[4] = 2+editVerts.Count;
						newEditTris[5] = 3+editVerts.Count;
						
						// add these to the mesh
						for(int n=0;n<newEditVerts.Length;n++){
							editVerts.Add(newEditVerts[n]);
						}
						
						for(int n=0;n<newEditTris.Length;n++){
							editTris.Add(newEditTris[n]);
						}
						
						// add appropriate uvs for the vertices
						for(int n=0;n<newEditVerts.Length;n++){
							editUvs.Add (new Vector2((newEditVerts[n].y),(newEditVerts[n].x)));
						}
					}
				}
				#endregion
				
				#region DOWN TILE
				// create the down tile pos
				Vector2 downTilePos = new Vector2(worldXCoord,worldYCoord-1);
				// check to see if this tile is part of the edited set
				if(!(downTilePos.x >= startXPos && downTilePos.x <endXPos)||!(downTilePos.y >=startYPos && downTilePos.y <endYPos)){
				// check for out of bounds
				if(OutOfBounds(downTilePos) == false){
					editNextTileHeight = world[(int)downTilePos.x,(int)downTilePos.y].height;
				}
				else{
					editNextTileHeight = world[worldXCoord,worldYCoord].height;
				}
					// check if the tile is lower than us
					if(editNextTileHeight != editHeight){
						
						// we have found a tile that is lower than us...
						// create some walls down
						
						// create the vertices
						newEditVerts[0] = new Vector3(0,0,0)+tilePos;
						newEditVerts[1] = new Vector3(1,0,0)+tilePos;
						newEditVerts[2] = new Vector3(1,editNextTileHeight-editHeight,0)+tilePos;
						newEditVerts[3] = new Vector3(0,editNextTileHeight-editHeight,0)+tilePos;
						
						// create the triangles
						newEditTris[0] = 0+editVerts.Count;
						newEditTris[1] = 1+editVerts.Count;
						newEditTris[2] = 2+editVerts.Count;
						newEditTris[3] = 0+editVerts.Count;
						newEditTris[4] = 2+editVerts.Count;
						newEditTris[5] = 3+editVerts.Count;
						
						// add these to the mesh
						for(int n=0;n<newEditVerts.Length;n++){
							editVerts.Add(newEditVerts[n]);
						}
						
						for(int n=0;n<newEditTris.Length;n++){
							editTris.Add(newEditTris[n]);
						}
						
						// add appropriate uvs for the vertices
						for(int n=0;n<newEditVerts.Length;n++){
							editUvs.Add (new Vector2((newEditVerts[n].y),(newEditVerts[n].x)));
						}
					} 
				}
				#endregion
				
				#region LEFT TILE
				// create the upper tile pos
				Vector2 leftTilePos = new Vector2(worldXCoord-1,worldYCoord);
				// check to see if this tile is part of the edited set
				if(!(leftTilePos.x >= startXPos && leftTilePos.x <endXPos)||!(leftTilePos.y >=startYPos && leftTilePos.y <endYPos)){
				// check for out of bounds
				if(OutOfBounds(leftTilePos) == false){
					editNextTileHeight = world[(int)leftTilePos.x,(int)leftTilePos.y].height;
				}
				else{
					editNextTileHeight = world[worldXCoord,worldYCoord].height;
				}
					// check if the tile is lower than us
					if(editNextTileHeight != editHeight){
						
						// we have found a tile that is lower than us...
						// create some walls down
						
						// create the vertices
						newEditVerts[0] = new Vector3(0,0,1)+tilePos;
						newEditVerts[1] = new Vector3(0,0,0)+tilePos;
						newEditVerts[2] = new Vector3(0,editNextTileHeight-editHeight,0)+tilePos;
						newEditVerts[3] = new Vector3(0,editNextTileHeight-editHeight,1)+tilePos;
						
						// create the triangles
						newEditTris[0] = 0+editVerts.Count;
						newEditTris[1] = 1+editVerts.Count;
						newEditTris[2] = 2+editVerts.Count;
						newEditTris[3] = 0+editVerts.Count;
						newEditTris[4] = 2+editVerts.Count;
						newEditTris[5] = 3+editVerts.Count;
						
						// add these to the mesh
						for(int n=0;n<newEditVerts.Length;n++){
							editVerts.Add(newEditVerts[n]);
						}
						
						for(int n=0;n<newEditTris.Length;n++){
							editTris.Add(newEditTris[n]);
						}
						
						// add appropriate uvs for the vertices
						for(int n=0;n<newEditVerts.Length;n++){
							editUvs.Add (new Vector2((newEditVerts[n].y),(newEditVerts[n].z)));
						}
					} 
				}
				#endregion
				
				#region RIGHT TILE
				// create the upper tile pos
				Vector2 rightTilePos = new Vector2(worldXCoord+1,worldYCoord);
				// check to see if this tile is part of the edited set
				if(!(rightTilePos.x >= startXPos && rightTilePos.x <endXPos)||!(rightTilePos.y >=startYPos && rightTilePos.y <endYPos)){
				// check for out of bounds
				if(OutOfBounds(rightTilePos) == false){
					editNextTileHeight = world[(int)rightTilePos.x,(int)rightTilePos.y].height;
				}
				else{
					editNextTileHeight = world[worldXCoord,worldYCoord].height;
				}
					// check if the tile is lower than us
					if(editNextTileHeight != editHeight){
						
						// we have found a tile that is lower than us...
						// create some walls down
						
						// create the vertices
						newEditVerts[0] = new Vector3(1,0,0)+tilePos;
						newEditVerts[1] = new Vector3(1,0,1)+tilePos;
						newEditVerts[2] = new Vector3(1,editNextTileHeight-editHeight,1)+tilePos;
						newEditVerts[3] = new Vector3(1,editNextTileHeight-editHeight,0)+tilePos;
						
						// create the triangles
						newEditTris[0] = 0+editVerts.Count;
						newEditTris[1] = 1+editVerts.Count;
						newEditTris[2] = 2+editVerts.Count;
						newEditTris[3] = 0+editVerts.Count;
						newEditTris[4] = 2+editVerts.Count;
						newEditTris[5] = 3+editVerts.Count;
						
						
						// add these to the mesh
						for(int n=0;n<newEditVerts.Length;n++){
							editVerts.Add(newEditVerts[n]);
						}
						
						for(int n=0;n<newEditTris.Length;n++){
							editTris.Add(newEditTris[n]);
						}
						
						// add appropriate uvs for the vertices
						for(int n=0;n<newEditVerts.Length;n++){
							editUvs.Add (new Vector2((newEditVerts[n].y),(newEditVerts[n].z)));
						}
						
					} 
				}
				#endregion
			}
			}
		}
		
		#endregion
		
		
		
		// now write this to the mesh
		//print ("verts "+editVerts.Count);
		//print ("tris "+(editTris.Count/3));
		MapEditor.singleton.editMesh.Clear();
		MapEditor.singleton.editMesh.vertices = editVerts.ToArray();
		MapEditor.singleton.editMesh.triangles = editTris.ToArray();
		MapEditor.singleton.editMesh.uv = editUvs.ToArray();
		MapEditor.singleton.editMesh.RecalculateNormals();
		MapEditor.singleton.editMesh.RecalculateBounds();
		
		//print ("verts "+editVerts.Count);
		//print ("tris "+(editTris.Count/3));
	}
}
