using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.SceneManagement;

namespace lnxMazeGen {

public enum MazeTileType {
	open,
	floor,
	wall

}



public class MazeGen1 : MonoBehaviour{

	public static MazeGen1 singleton;

	public GameObject tilePrefab;

	public Transform posSprite;
	public Transform nextPosSprite;
	public Transform startPosSprite;

	public MazeTile[,] maze;
	public static Vector2 mazeSize;

	public List<MazeTileObj> mazeTileScripts = new List<MazeTileObj> ();
	LinkedList<Vector2> emptyTiles = new LinkedList<Vector2> ();

	public Vector2 pos;
	public Vector2 nextPos;
	public Vector2 startPos;
	public Vector3 endPos;

	static int mapNo = 0;

	Vector2[] lookupDir;

	Vector2[] allDir;

	public int minIslandSize = 2;
	public int worldSizeX = 32;
	public int worldSizeY = 32;

	void Awake () {
		singleton = this;
		// set the lookup directions
		lookupDir = new Vector2[4];
		lookupDir [0] = Vector2.up;
		lookupDir [1] = Vector2.right;
		lookupDir [2] = Vector2.down;
		lookupDir [3] = Vector2.left;

		allDir = new Vector2[8];
		allDir [0] = new Vector2 (-1, 0);//left
		allDir [1] = new Vector2 (-1, 1);// up left
		allDir [2] = new Vector2 (0, 1);// up
		allDir [3] = new Vector2 (1, 1);// up right
		allDir [4] = new Vector2 (1, 0);// right
		allDir [5] = new Vector2 (1, -1);// down right
		allDir [6] = new Vector2 (0, -1);// down
		allDir [7] = new Vector2 (-1, -1);// down left


	}
	void Start () {

		mazeSize = new Vector2 (worldSizeX, worldSizeY);
		maze = new MazeTile[worldSizeX, worldSizeY];
		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++) {
				maze [i, j].pos = new Vector2 (i, j);
				maze [i, j].isVisited = false;
			}
		}
		CreateMazeTileWorld ();

		StartCoroutine(RunRecurseBackTrack3 ());
	}
	
	// Update is called once per frame
	void Update () {
		//UpdateMazeTileWorld ();
	}

	void AddEmptyNeighbours (Vector2 _pos) {
		Vector2 neighbourTilePos;
		for (int i = 0; i < 4; i++) {
			// get the next tile position
			neighbourTilePos = _pos + lookupDir [i];
			// check if its in bounds
			if(CheckInBounds(neighbourTilePos)){
				// check if its open
				if (GetTile (neighbourTilePos).type == MazeTileType.open) {
					// check if its not already on the list
					if (emptyTiles.Contains (neighbourTilePos) == false) {
						// add the tile
						emptyTiles.AddLast (neighbourTilePos);
					}
				}
			}
		}
	}

	/// <summary>
	/// Makes a really cool city layout :D
	/// NOT USED IN ACTUAL MAZE GEN
	/// </summary>
	/// <returns>The recurse back track2.</returns>
	IEnumerator RunRecurseBackTrack2 () {
		// generate random starting coords
		pos = Vector2.zero;
		Vector2 lastPos;
		pos.x = (int)Random.Range (0, mazeSize.x);
		pos.y = (int)Random.Range (0, mazeSize.y);
		// set this to be a floor tile
		maze [(int)pos.x, (int)pos.y].type= MazeTileType.floor;
		// get the neighbours and set them all to walls to carve
		// SetNeighboursToWalls(pos);
		// add all the neighbours to the list of tiles to carve
		AddNeighboursToCarve(pos);
		yield return new WaitForEndOfFrame ();
		// begin the loop
		// keep choosing tiles from the list
		while (emptyTiles.Count != 0) {
			//Debug.Log ("---start move---");
			yield return new WaitForEndOfFrame ();
			// set the last tile position
			lastPos = pos;

			// get the next tile position
			List<int> directions = new List<int> ();
			for (int i = 0; i < 4; i++) {
				directions.Add (i);
			}
			bool hasDirection = false;
			bool doneLoop = false;
			int dir = -1;
			nextPos = Vector2.zero;
			// generate a valid random direction
			while (doneLoop == false) {
				// now pick a random direction to walk in
				dir = directions[Random.Range (0, directions.Count)];
				//Debug.Log("direction is:"+dir.ToString());
				//Debug.Log ("number of directions left:" + directions.Count);
				// get the position of the next tile in this direction
				nextPos = pos + lookupDir[dir];
				// remove that direction from the list
				directions.Remove(dir);
				// check if this tile is in bounds
				if (CheckInBounds (nextPos)) {
					// now we have a valid position, check if its open, hence possibly carvable
					MazeTile nextTile = GetTile (nextPos);
					if (nextTile.type == MazeTileType.open) {
						// now determine if this should be a permanent wall...
						if (CheckForPermanentWall (nextPos) == true) {
							//Debug.Log ("creating permanent wall in direction " + dir);
							// set this tile to be a wall
							nextTile.type = MazeTileType.wall;
							// set the tile back to the array
							SetTile (nextTile);
							// remove this tile from the empty tile list
							RemovePosFromEmptyList (nextPos);
							// add all the neighbours of this tile to the array
							//AddNeighboursToCarve(nextPos);
							UpdateMazeTileWorld ();
						} else {
							// this tile shouldnt be a permanent wall...
							//Debug.Log ("found valid tile in direction " + dir);
							// so we can carve here :D
							hasDirection = true;
							doneLoop = true;


						}
					} else {
						//Debug.Log ("found " + nextTile.type + " in direction " + dir);
					}
				} else {
					//Debug.Log ("out of bounds tile in direction " + dir);
				}

				if (directions.Count == 0) {
					doneLoop = true;
				}

				UpdateMazeTileWorld ();
				yield return new WaitForEndOfFrame ();
					
			}

			UpdateMazeTileWorld ();
			yield return new WaitForEndOfFrame ();
			// check if we found a direction to move in
			if (hasDirection == true) {
				
				// set this new tile to be a floor
				maze [(int)nextPos.x, (int)nextPos.y].type = MazeTileType.floor;
				// add the empty neighbours
				AddNeighboursToCarve (nextPos);
				// remove this tile from the search list
				RemovePosFromEmptyList(nextPos);
				// set the position of the current tile to be this tile position
				pos = nextPos;

				UpdateMazeTileWorld ();
				//Debug.Log ("moving in direction " + dir);

			} else {
				// we didnt find a tile to go to... so backtrack!
				//yield return new WaitForSeconds(0.5f);
				//Debug.Log("Dead end");
				// remove the tile from the empty list
				Debug.Log("backtracking...");
				if (emptyTiles.Count != 0) {
					// get the last tile we added, and restart the search from there
					pos = emptyTiles.Last.Value;
					// add the neighbours of this tile
					AddNeighboursToCarve (pos);
					// remove this tile from the list
					emptyTiles.RemoveLast ();


					bool needNewTile = CheckForPermanentWall (pos);
					// keep checking for a valid tile, since if this should be a wall, we can't go here
					while (needNewTile == true) {
					
						//Debug.Log ("creating permanent wall in backtracked tile as position " + pos);
						// set this tile to be a wall
						maze [(int)pos.x, (int)pos.y].type = MazeTileType.wall;
						// add all the neighbours to the search list
						AddNeighboursToCarve (pos);
						// get the last tile we added, and restart the search from there
						if (emptyTiles.Count != 0) {
							pos = emptyTiles.Last.Value;
							// remove this tile from the list
							emptyTiles.RemoveLast ();
							// check for a permanent wall at the new position
							needNewTile = CheckForPermanentWall (pos);
						} else {
							needNewTile = false;
						}

						UpdateMazeTileWorld ();
						yield return new WaitForEndOfFrame ();
					}
					// set this tile to be a floor
					maze [(int)pos.x, (int)pos.y].type = MazeTileType.floor;
					// add all the neighbours to the search list
					AddNeighboursToCarve (pos);
					UpdateMazeTileWorld ();
					yield return new WaitForEndOfFrame ();
				}
				//yield return new WaitForSeconds(0.2f);
			}
			UpdateMazeTileWorld ();
			//Debug.Log ("---done move---");


		}
		UpdateMazeTileWorld ();
		Debug.Log ("Done");
		ValidateMaze ();
		yield return new WaitForEndOfFrame ();

	}

	/// <summary>
	/// Runs the recurse back track3.
	/// </summary>
	/// <returns>The recurse back track3.</returns>
	IEnumerator RunRecurseBackTrack3 () {
		
		// generate random starting coords
		pos = Vector2.zero;
		Vector2 lastPos;
		pos.x = (int)Random.Range (0, mazeSize.x);
		pos.y = (int)Random.Range (0, mazeSize.y);
		startPos = pos;
		// set this to be a floor tile
		maze [(int)pos.x, (int)pos.y].type= MazeTileType.floor;
		// get the neighbours and set them all to walls to carve
		// SetNeighboursToWalls(pos);
		// add all the neighbours to the list of tiles to carve
		AddNeighboursToCarve(pos);
		yield return new WaitForEndOfFrame ();
		// begin the loop
		// keep choosing tiles from the list
		while (emptyTiles.Count != 0) {
			//Debug.Log ("---start move---");
			yield return new WaitForEndOfFrame ();
			// set the last tile position
			lastPos = pos;

			// get the next tile position
			List<int> directions = new List<int> ();
			for (int i = 0; i < 4; i++) {
				directions.Add (i);
			}
			bool hasDirection = false;
			bool doneLoop = false;
			int dir = -1;
			nextPos = Vector2.zero;
			// generate a valid random direction
			while (doneLoop == false) {
				// now pick a random direction to walk in
				dir = directions[Random.Range (0, directions.Count)];
				//Debug.Log("direction is:"+dir.ToString());
				//Debug.Log ("number of directions left:" + directions.Count);
				// get the position of the next tile in this direction
				nextPos = pos + lookupDir[dir];
				// remove that direction from the list
				directions.Remove(dir);
				// check if this tile is in bounds
				if (CheckInBounds (nextPos)) {
					// now we have a valid position, check if its open, hence possibly carvable
					MazeTile nextTile = GetTile (nextPos);
					if (nextTile.type == MazeTileType.open) {
						// now determine if this should be a permanent wall...
						if (CheckForPermanentWall (nextPos) == true || CheckForTTile(nextPos) == true || CheckForCornerTileAround(nextPos)) {
							//Debug.Log ("creating permanent wall in direction " + dir);
							// set this tile to be a wall
							nextTile.type = MazeTileType.wall;
							// set the tile back to the array
							SetTile (nextTile);
							// remove this tile from the empty tile list
							RemovePosFromEmptyList (nextPos);
							// add all the neighbours of this tile to the array
							//AddNeighboursToCarve(nextPos);
							UpdateMazeTileWorld ();
						} else {
							// this tile shouldnt be a permanent wall...
							//Debug.Log ("found valid tile in direction " + dir);
							// so we can carve here :D
							hasDirection = true;
							doneLoop = true;
							//Debug.Log ("found " + nextTile.type + " in direction " + dir);

						}
					}
				} else {
					//Debug.Log ("out of bounds tile in direction " + dir);
				}

				if (directions.Count == 0) {
					doneLoop = true;
				}
				//UpdateMazeTileWorld ();
				//yield return new WaitForEndOfFrame ();

			}

			//UpdateMazeTileWorld ();
			//yield return new WaitForEndOfFrame ();
			// check if we found a direction to move in
			if (hasDirection == true) {

				// set this new tile to be a floor
				maze [(int)nextPos.x, (int)nextPos.y].type = MazeTileType.floor;
				// add the empty neighbours
				AddNeighboursToCarve (nextPos);
				// remove this tile from the search list
				RemovePosFromEmptyList(nextPos);
				// set the position of the current tile to be this tile position
				pos = nextPos;

				UpdateMazeTileWorld ();
				//Debug.Log ("moving in direction " + dir);

			} else {
				// we didnt find a tile to go to... so backtrack!
				//yield return new WaitForSeconds(0.5f);
				Debug.Log("Dead end");
				// remove the tile from the empty list
				//Debug.Log("backtracking...");
				if (emptyTiles.Count != 0) {
					// get the last tile we added, and restart the search from there
					pos = emptyTiles.Last.Value;

					// remove this tile from the list
					emptyTiles.RemoveLast ();

					// keep checking for a valid tile, since if this should be a wall, we can't go here
					while ((CheckForPermanentWall(pos) == true || CheckForHTile(pos) == true || CheckForLineTile(pos) || CheckForCornerTileAround(pos))&& GetTile(pos).type == MazeTileType.open) {
						// set this tile to be a wall
						maze [(int)pos.x, (int)pos.y].type = MazeTileType.wall;
						// add all the neighbours to the search list
						//AddNeighboursToCarve (pos);
						// get the last tile we added, and restart the search from there
						if (emptyTiles.Count != 0) {
							pos = emptyTiles.Last.Value;
							// remove this tile from the list
							emptyTiles.RemoveLast ();
						} else {
							break;
						}

						/*

					// check for a permanent wall corner here
					bool needNewTile = CheckForPermanentWall (pos);
					if (needNewTile == false) {
							// check for a permanent line here
							needNewTile = CheckForLineTile (pos);
					}
					// keep checking for a valid tile, since if this should be a wall, we can't go here
					while (CheckForPermanentWall(pos) == true || CheckForLineTile(pos) == true) {

						//Debug.Log ("creating permanent wall in backtracked tile as position " + pos);
						// set this tile to be a wall
						maze [(int)pos.x, (int)pos.y].type = MazeTileType.wall;
						// add all the neighbours to the search list
						AddNeighboursToCarve (pos);
						// get the last tile we added, and restart the search from there
						if (emptyTiles.Count != 0) {
							pos = emptyTiles.Last.Value;
							// remove this tile from the list
							emptyTiles.RemoveLast ();
							// check for a permanent wall at the new position
							needNewTile = CheckForPermanentWall (pos);
							// now check for a H tile, since we don't want many bridges between the corridoors
							if (needNewTile == false) {
								needNewTile = CheckForLineTile (pos);
							}

						} else {
							needNewTile = false;
						}
						*/

						//UpdateMazeTileWorld ();
						//yield return new WaitForEndOfFrame ();
					}
						if (CheckForPermanentWall(pos) == false) {
						// set this tile to be a floor
						maze [(int)pos.x, (int)pos.y].type = MazeTileType.floor;
						// add the neighbours of this tile
						AddNeighboursToCarve (pos);
					}
					UpdateMazeTileWorld ();
					yield return new WaitForEndOfFrame ();
				}
				//yield return new WaitForSeconds(0.2f);
			}
			UpdateMazeTileWorld ();
			//Debug.Log ("---done move---");


		}
		UpdateMazeTileWorld ();
		Debug.Log ("Done");
		yield return new WaitForEndOfFrame ();
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_before_postproc_"+mapNo+".png");
		ValidateMaze ();
		UpdateMazeTileWorld ();
		yield return new WaitForEndOfFrame ();
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_after_postproc_"+mapNo+".png");
		// place the start and exit tiles
		StartCoroutine(PlaceStartAndExit());

		yield return new WaitForEndOfFrame ();


	}

	IEnumerator PlaceStartAndExit () {
		UpdateMazeTileWorld ();
		yield return new WaitForEndOfFrame ();
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_islands_before_"+mapNo+".png");
		
		posSprite.gameObject.SetActive (false);
		// find the end position
		List<Vector2> tilesToCheck = new List<Vector2>();
		tilesToCheck.Add (startPos);
		maze[(int)startPos.x,(int)startPos.y].prevTilePos = startPos;
		List<Vector2> nextTiles = new List<Vector2> ();

		while (tilesToCheck.Count != 0) {
			yield return new WaitForEndOfFrame ();
			// flood fill
			while(tilesToCheck.Count != 0) {
				// get the neighbours, add all the un-visited ones to the list
				Vector2[] neighbours = GetValidNeighbours (tilesToCheck [0]);

				// check each of the neighbours to see if it has been visited
				for (int n = 0; n < neighbours.Length; n++) {
						if (GetTile (neighbours [n]).isConnectedToStart == false && GetTile (neighbours [n]).type != MazeTileType.wall) {
						// set the last position found
						endPos = neighbours [n];
						// set the visited flag on the maze tile
						maze[(int)neighbours[n].x,(int)neighbours[n].y].isConnectedToStart = true;
						maze[(int)neighbours[n].x,(int)neighbours[n].y].type = MazeTileType.floor;
						maze [(int)neighbours[n].x, (int)neighbours[n].y].prevTilePos = tilesToCheck[0];
						// add this neighbour to the list to check next
						nextTiles.Add (neighbours [n]);

					}
				}

				// remove this tile just checked from the list
				tilesToCheck.RemoveAt (0);

			}
			yield return new WaitForEndOfFrame ();
			UpdateMazeTileWorld ();
			for (int i = 0; i < nextTiles.Count; i++) {
				tilesToCheck.Add (nextTiles [i]);
			}

			nextTiles.Clear ();
				
		}
		Debug.Log("Merging Islands...");
		//Debug.Break ();
		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++){
					if(maze [i, j].isConnectedToStart == false && maze[i,j].isIsland ==  false&& maze[i,j].type != MazeTileType.wall){
					Debug.Log("found island tile");
					maze[i,j].isIsland = true;
					maze [i, j].type = MazeTileType.floor;

					List<Vector2> islandTiles = new List<Vector2>();
					islandTiles.Add(new Vector2(i,j));
					// we've found an island, so fill it, and determine the size
					tilesToCheck.Clear();
					nextTiles.Clear();
					tilesToCheck.Add(maze[i,j].pos);
					while (tilesToCheck.Count != 0) {
						yield return new WaitForEndOfFrame ();
						// flood fill
						while(tilesToCheck.Count != 0) {
							// get the neighbours, add all the un-visited ones to the list
							Vector2[] neighbours = GetValidNeighbours (tilesToCheck [0]);
			
							// check each of the neighbours to see if it has been visited
							for (int n = 0; n < neighbours.Length; n++) {
									if (GetTile (neighbours [n]).isIsland == false && GetTile(neighbours[n]).isConnectedToStart == false && GetTile (neighbours [n]).type != MazeTileType.wall) {
									// set the last position found
									endPos = neighbours [n];
									// set the visited flag on the maze tile
									maze[(int)neighbours[n].x,(int)neighbours[n].y].isIsland = true;
									// this is also a floor, since we can have islands of unvisited tiles
									maze[(int)neighbours[n].x,(int)neighbours[n].y].type = MazeTileType.floor;
									// add this neighbour to the list to check next
									nextTiles.Add (neighbours [n]);
									islandTiles.Add(neighbours[n]);
								}
							}
			
							// remove this tile just checked from the list
							tilesToCheck.RemoveAt (0);
			
						}
							UpdateMazeTileWorld ();
						yield return new WaitForEndOfFrame ();
						
						for (int n = 0; n < nextTiles.Count; n++) {
							tilesToCheck.Add (nextTiles [n]);
						}
			
						nextTiles.Clear ();
							
					}
					
					Debug.Log ("found island of "+islandTiles.Count+" tiles");
					// check for large islands
					if(islandTiles.Count > minIslandSize){
						Debug.Log ("checking for way back in...");
						// now check all the neighbours of these island tiles
						bool foundConnection = false;
						// and check for a common neighbour with part of the maze
						for(int n=0;n<islandTiles.Count;n++){
							// get the neighbours for this tile
							Vector2[] currentTileNeighbours = GetValidNeighbours(islandTiles[n]);
							for(int k=0;k<currentTileNeighbours.Length;k++){
								// check if this tile is not a permanent wall
								if(CheckForPermanentWall(currentTileNeighbours[k]) == false){
									Vector2[] neighbours = GetValidNeighbours(currentTileNeighbours[k]);

									for(int m = 0;m<neighbours.Length;m++){
										if(GetTile(neighbours[m]).isConnectedToStart == true){
											// we found a tile to use to join the sections
											maze[(int)currentTileNeighbours[k].x,(int)currentTileNeighbours[k].y].type = MazeTileType.floor;
											maze [(int)currentTileNeighbours [k].x, (int)currentTileNeighbours [k].y].isConnectedToStart = true;
											Debug.Log("Found way back in!");	
											// stop the loops
											k = currentTileNeighbours.Length;
											n = islandTiles.Count;
											m = neighbours.Length;
											foundConnection = true;

											// turn these island tiles back to part of the maze
											for (int a = 0; a < islandTiles.Count; a++) {
												maze[(int)islandTiles[a].x,(int)islandTiles[a].y].isIsland = false;
												maze[(int)islandTiles[a].x,(int)islandTiles[a].y].isConnectedToStart = true;
											}
											UpdateMazeTileWorld ();
											yield return new WaitForEndOfFrame();
										}
										
									}
	
								}
							}
						}

						if (foundConnection == false) {
							Debug.Log ("Didn't find a way back in, looking for a new island to merge with");
								for(int n=0;n<islandTiles.Count;n++){
									// get the neighbours for this tile
									Vector2[] currentTileNeighbours = GetValidNeighbours(islandTiles[n]);
									for(int k=0;k<currentTileNeighbours.Length;k++){
										// check if this tile is not a permanent wall
										if(CheckForPermanentWall(currentTileNeighbours[k]) == false){
											Vector2[] neighbours = GetValidNeighbours(currentTileNeighbours[k]);

											for(int m = 0;m<neighbours.Length;m++){
													if(GetTile(neighbours[m]).type == MazeTileType.floor && GetTile(neighbours[m]).isIsland == false && GetTile(neighbours[m]).isConnectedToStart == false){
													Debug.Log ("Merging 2 islands...");
												Debug.Break ();
													// we found a tile to use to join the sections
													maze[(int)currentTileNeighbours[k].x,(int)currentTileNeighbours[k].y].type = MazeTileType.floor;
													//maze[(int)currentTileNeighbours[k].x,(int)currentTileNeighbours[k].y].isIsland = true;
													// stop the loop!
													m=neighbours.Length;
													n = islandTiles.Count;
													k=currentTileNeighbours.Length;
													// turn these old island tiles back to empty
														for (int a = 0; a < islandTiles.Count; a++) {
															maze[(int)islandTiles[a].x,(int)islandTiles[a].y].isIsland = false;
					
														}
														// go back 1 space, and retry
														j--;
												UpdateMazeTileWorld ();
													yield return new WaitForEndOfFrame();
												}

											}

										}
									}
								}
						}
						
						
					}
				}
			}
		}
			
		UpdateMazeTileWorld ();
		yield return new WaitForEndOfFrame ();
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_islands_after_"+mapNo+".png");
		

		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++){
			maze [i, j].isConnectedToStart = false;
			}
		}
		UpdateMazeTileWorld();
		yield return new WaitForEndOfFrame();

		// reset for another search from the start
		tilesToCheck.Clear();
		nextTiles.Clear();
		tilesToCheck.Add (startPos);
		// rescan the maze for the next furthest tile, since this might have changed when we merged the islands
			while (tilesToCheck.Count != 0) {
			yield return new WaitForEndOfFrame ();
			// flood fill
			while(tilesToCheck.Count != 0) {
				// get the neighbours, add all the un-visited ones to the list
				Vector2[] neighbours = GetValidNeighbours (tilesToCheck [0]);

				// check each of the neighbours to see if it has been visited
				for (int n = 0; n < neighbours.Length; n++) {
					if (GetTile (neighbours [n]).isConnectedToStart == false && GetTile (neighbours [n]).type == MazeTileType.floor) {
						// set the last position found
						endPos = neighbours [n];
						// set the visited flag on the maze tile
						maze[(int)neighbours[n].x,(int)neighbours[n].y].isConnectedToStart = true;
						maze[(int)neighbours[n].x,(int)neighbours[n].y].isIsland = false;
						maze [(int)neighbours[n].x, (int)neighbours[n].y].prevTilePos = tilesToCheck[0];
						// add this neighbour to the list to check next
						nextTiles.Add (neighbours [n]);

					}
				}

				// remove this tile just checked from the list
				tilesToCheck.RemoveAt (0);

			}
			yield return new WaitForEndOfFrame ();
			UpdateMazeTileWorld ();
			for (int i = 0; i < nextTiles.Count; i++) {
				tilesToCheck.Add (nextTiles [i]);
			}

			nextTiles.Clear ();
				
		}

		nextPos = endPos;
		Debug.Log ("placed start and end positions");

		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++){
				maze [i, j].isConnectedToStart = false;
			}
		}
		UpdateMazeTileWorld();
		yield return new WaitForEndOfFrame();
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_"+mapNo+".png");
		// run backwards and highlight from start to finish
		Vector2 prevTilePos = GetTile(endPos).prevTilePos;
		while (prevTilePos != startPos) {
			// set this as connected to start
			maze[(int)prevTilePos.x,(int)prevTilePos.y].isConnectedToStart = true;
			// set the new previous tile position
			prevTilePos = GetTile (prevTilePos).prevTilePos;
			UpdateMazeTileWorld();
			yield return new WaitForEndOfFrame ();
		}
		Debug.Log("Found Quickest Route");
		UpdateMazeTileWorld ();
		yield return new WaitForEndOfFrame ();
		// take a picture of the map
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_solved_"+mapNo+".png");
			for (int i = 0; i < maze.GetLength (0); i++) {
				for (int j = 0; j < maze.GetLength (1); j++){
				maze [i, j].isConnectedToStart = false;
				}
			}
		StartCoroutine (HighlightDeadEnds ());

	}
	
	IEnumerator RestartMazeGen(){
		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++){
				maze [i, j].isConnectedToStart = false;
				maze [i, j].isDeadEnd = false;
			}
		}
		UpdateMazeTileWorld ();
		System.GC.Collect ();
		Screen.sleepTimeout = 180;
		mapNo+=1;
		yield return new WaitForEndOfFrame();
		Application.LoadLevel (Application.loadedLevelName);
		//SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}
	
	void ValidateMaze () {
		// post process the maze
		// find any open tiles that are left, and highlight them
		// find any walls that haven't been added, and add those
		//Debug.Log("validating maze");
		MazeTile tile;
		for (int i = 0; i < worldSizeX; i++) {
			for (int j = 0; j < worldSizeY; j++) {
				tile = GetTile(new Vector2(i,j));
				// check for permanent walls
				if(tile.pos != startPos){
					if ((CheckForPermanentWall (tile.pos) == true) || CheckForHTile (tile.pos) == true) {
						tile.type = MazeTileType.wall;
						SetTile (tile);
					} else if (CheckForTTile (tile.pos) == true && CheckForLineTile (tile.pos) == false) {
						tile.type = MazeTileType.floor;
						SetTile (tile);
					}
				}
			}
		}
		//posSprite.gameObject.SetActive (false);
		//nextPosSprite.gameObject.SetActive (false);
		// final gfx update
		UpdateMazeTileWorld ();
		Debug.Log ("maze validated");
	}
	
	IEnumerator HighlightDeadEnds () {
			for (int i = 0; i < maze.GetLength(0); i++) {
				for (int j = 0; j < maze.GetLength(1); j++) {
				// check if this tile is connected to the start
					if (maze [i, j].isIsland == false && maze[i,j].type == MazeTileType.floor) {

					// get all the floor neighbours
					Vector2[] floorNeighbours = GetAllFloorNeighbours(maze[i,j].pos);
					// set us to be a dead end
					if (floorNeighbours.Length == 1) {
						maze [i, j].isDeadEnd = true;
					
						UpdateMazeTileWorld ();
						yield return new WaitForEndOfFrame ();
						Vector3 lastPos;
						int numOfDeadEnds = 0;
						while (floorNeighbours.Length == 1 && numOfDeadEnds < 2) {
							maze [(int)floorNeighbours [0].x, (int)floorNeighbours [0].y].isDeadEnd = true;
							lastPos = floorNeighbours [0];
							floorNeighbours = GetAllNonDeadEndNeighbours (lastPos, out numOfDeadEnds);
							UpdateMazeTileWorld ();
							yield return new WaitForEndOfFrame ();
						
						}
						maze [(int)lastPos.x, (int)lastPos.y].isDeadEnd = false;
					}
					
				}
					//UpdateMazeTileWorld ();
					//yield return new WaitForEndOfFrame ();

			}
		}
			UpdateMazeTileWorld ();
		yield return new WaitForEndOfFrame ();
		
		Application.CaptureScreenshot(Application.dataPath+"Maze_Screenshot_DeadEnds_"+mapNo+".png");
		StartCoroutine (RestartMazeGen ());
	}
	

	void SetNeighboursToWalls(Vector2 _pos){
		Vector2 neighbourTilePos;
		for (int i = 0; i < 4; i++) {
			// get the next tile position
			neighbourTilePos = _pos + lookupDir [i];
			// check if its in bounds
			if(CheckInBounds(neighbourTilePos)){
				// check if its open
				if (GetTile (neighbourTilePos).type == MazeTileType.open) {
					maze [(int)_pos.x, (int)_pos.y].type = MazeTileType.wall;
				}
			}
		}
	}
	
	void AddNeighboursToCarve(Vector2 _pos){
		Vector2 neighbourTilePos;
		for (int i = 0; i < 4; i++) {
			// get the next tile position
			neighbourTilePos = _pos + lookupDir [i];
			// check if its in bounds
			if(CheckInBounds(neighbourTilePos)){
				// check if its not a floor
				if (GetTile (neighbourTilePos).type == MazeTileType.open && emptyTiles.Contains(neighbourTilePos) == false) {
					maze [(int)neighbourTilePos.x, (int)neighbourTilePos.y].isVisited = true;

					emptyTiles.AddLast (neighbourTilePos);
				}
			}
		}
	}

	public Vector2[] GetValidNeighbours(Vector2 _pos){
		List<Vector2> neighbours = new List<Vector2> ();
		for (int i = 0; i < 4; i++) {
			if (CheckInBounds (_pos + lookupDir [i])) {
				neighbours.Add (_pos + lookupDir [i]);
			}
		}
		return neighbours.ToArray ();
	}
	
	public Vector2[] GetAllNonDeadEndNeighbours(Vector2 _pos, out int deadEndCount){
		List<Vector2> neighbours = new List<Vector2> ();
		deadEndCount = 0;
		for (int i = 0; i < 4; i++) {
			if (CheckInBounds (_pos + lookupDir [i])) {
				if (GetTile (_pos + lookupDir [i]).type == MazeTileType.floor) {
					if (GetTile (_pos + lookupDir [i]).isDeadEnd == false) {
						neighbours.Add (_pos + lookupDir [i]);
					} else {
						deadEndCount++;
					}
				}
				
			}
		}
		return neighbours.ToArray ();
	}

	public Vector2[] GetAllFloorNeighbours(Vector2 _pos){
			List<Vector2> neighbours = new List<Vector2> ();
			for (int i = 0; i < 4; i++) {
				if (CheckInBounds (_pos + lookupDir [i])) {
					if (GetTile (_pos + lookupDir [i]).type == MazeTileType.floor) {
						neighbours.Add (_pos + lookupDir [i]);
					}

				}
			}
			return neighbours.ToArray ();
		}

	void RemovePosFromEmptyList (Vector2 _pos){
		emptyTiles.Remove (_pos);
	}

	bool CheckForPermanentWall(Vector2 _pos){
		Vector2 t1Pos, t2Pos, t3Pos;
		int t1Index, t2Index, t3Index;

		for(int i=0;i<4;i++){
			t1Index = (0+(i*2))%8;
			t2Index = (1+(i*2))%8;
			t3Index = (2+(i*2))%8;
			t1Pos = _pos + allDir [t1Index];
			t2Pos = _pos + allDir [t2Index];
			t3Pos = _pos + allDir [t3Index];
			if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
				if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
					return true;
				}
			}

			}
		return false;
	}

	
	bool CheckForCornerTileAround(Vector2 _pos){
		Vector2 t1Pos, t2Pos;
		int t1Index, t2Index;
		
		for (int i = 0; i < 4; i++) {
			t1Index = (0 + (2 * i))%8;
			t2Index = (2 + (2 * i))%8;
			t1Pos = _pos + allDir [t1Index];
			t2Pos = _pos + allDir [t2Index];
			if (CheckInBounds (t1Pos) && CheckInBounds (t2Pos)) {
				if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor) {
					return true;
				}
				
			}
		}

		return false;
	}
	
	bool CheckForHTile (Vector2 _pos){
		// check the upper line
		Vector2 t1Pos, t2Pos, t3Pos;
		t1Pos = _pos + allDir [1];
		t2Pos = _pos + allDir [2];
		t3Pos = _pos + allDir [3];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				// check the lower line
				t1Pos = _pos + allDir [5];
				t2Pos = _pos + allDir [6];
				t3Pos = _pos + allDir [7];
				if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
					if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
						return true;
					}
				}
			}
		}

		// check the left line
		t1Pos = _pos + allDir [0];
		t2Pos = _pos + allDir [1];
		t3Pos = _pos + allDir [7];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				// check the right line
				t1Pos = _pos + allDir [3];
				t2Pos = _pos + allDir [4];
				t3Pos = _pos + allDir [5];
				if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
					if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
						return true;
					}
				}
			}
		}

		return false;


	}

	bool CheckForTTile (Vector2 _pos){
		
	Vector2 t1Pos, t2Pos, t3Pos;
		// check the upper line
		t1Pos = _pos + allDir [1];
		t2Pos = _pos + allDir [2];
		t3Pos = _pos + allDir [3];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				return true;
			}
		}

		// check the down line
		t1Pos = _pos + allDir [5];
		t2Pos = _pos + allDir [6];
		t3Pos = _pos + allDir [7];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				return true;
			}
		}

		// check the left line
		t1Pos = _pos + allDir [0];
		t2Pos = _pos + allDir [1];
		t3Pos = _pos + allDir [7];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				return true;
			}
		}

		// check the right line
		t1Pos = _pos + allDir [3];
		t2Pos = _pos + allDir [4];
		t3Pos = _pos + allDir [5];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos) && CheckInBounds(t3Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor && GetTile (t3Pos).type == MazeTileType.floor) {
				return true;
			}
		}

		return false;


	}

	bool CheckForLineTile (Vector2 _pos){
		
		Vector2 t1Pos, t2Pos;
		// check the horizontal line
		t1Pos = _pos + allDir [0];
		t2Pos = _pos + allDir [4];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor) {
				return true;
			}
		}
		// check the vertical line
		t1Pos = _pos + allDir [2];
		t2Pos = _pos + allDir [6];
		if(CheckInBounds(t1Pos) && CheckInBounds(t2Pos)){
			if (GetTile (t1Pos).type == MazeTileType.floor && GetTile (t2Pos).type == MazeTileType.floor) {
				return true;
			}
		}
		return false;


	}

	/*IEnumerator RunRecurseBackTrack (){
		// generate random starting coords
		Vector2 pos = Vector2.zero;
		pos.x = Random.Range (0, mazeSize.x);
		pos.y = Random.Range (0, mazeSize.y);
		// set this to be a floor tile
		maze [(int)pos.x, (int)pos.y].type= MazeTileType.floor;
		// add this tile to the empty tile list
		//emptyTiles.AddLast(pos);
		AddEmptyNeighbours (pos);
			
		while (emptyTiles.Count != 0) {
			//Debug.Log ("processing tile at:" + pos.ToString());
			// remove the tile from the list
			emptyTiles.RemoveLast();
			yield return new WaitForEndOfFrame ();
			// generate a new list of valid directions
			List<int> directions = new Linextnextst<int> ();
			for (int i = 0; i < 4; i++) {
				directions.Add (i);
			}
			bool hasDirection = false;
			int dir = -1;
			// keep finding a direction until we have none left, or we find a valid one
			while (hasDirection == false) {
				yield return new WaitForEndOfFrame ();
				// now pick a random direction to walk in
				dir = Random.Range (0, directions.Count);
				//Debug.Log("direction is:"+dir.ToString());
				//Debug.Log ("number of directions left:" + directions.Count);
				// remove that direction from the list
				directions.RemoveAt (dir);

				// get the next tile position
				Vector2 nextPos;
				nextPos = pos + lookupDir [dir];
				// check if the next tile is free
				if (CheckInBounds (nextPos)) {
					
					if (GetTile (nextPos).type == MazeTileType.open) {
						// get the next tnextile to check
						MazeTile nextTile = GetTile(nextPos);
						int neighbourCount = 0;
						// check all its neighbours, see if it has more than 1 occupied neighbours
						for (int n = 0; n < 4; n++) {
							// get the neighbour tile postition
							Vector2 neighbourTilePos = nextPos + lookupDir [n];
							// check if this neighbour is in bounds and isnt equal to the original tile
							if (CheckInBounds (neighbourTilePos) && neighbourTilePos != pos) {
								MazeTile neighbourTile = GetTile (neighbourTilePos);
								// check if	this neighbour tile is a floor tile
								if (neighbourTile.type==MazeTileType.floor) {
									// increase the neighbour count
									neighbourCount++;
								}
								if (neighbourCount == 2) {
									// mark this tile as a permanent wall
									nextTile.type = MazeTileType.wall;
									// set this tile back in the array
									SetTile (nextTile);
									// stop the loop
									n = 4;
								}
							}
						}
						// check if we haven't moved into a permanent wall
						if (neighbourCount < 2 && directions.Count != 0) {
							hasDirection = true;
							// TODO change this
						} else {
							hasDirection = false;
						}
					}
				}

				// check if we have no more directions... and we still haven't found one... end the loop anyway
				if (directions.Count == 0) {
					hasDirection = true;
				}
			}

			//Debug.Log ("got direction, direction is: "+dir);
			// now we have a direction
			// check if we have a direction...
			if (directions.Count != 0) {
				//Debug.Log ("found new tile");
				// set the position of the next tile
				pos += lookupDir [dir];
				// set the tile to be a floor tile
				MazeTile tile = GetTile(pos);
				tile.type = MazeTileType.floor;
				SetTile (tile);


				AddEmptyNeighbours (pos);
			}
			// we have hit a dead end... shit!
			else{
				//Debug.Log ("dead end");
				//Debug.Log ("there are " + emptyTiles.Count + " empty tiles left");
				if (emptyTiles.Count != 0) {
					// move back to the last tile in the list
					pos = emptyTiles.Last.Value;
				}

			}
			UpdateMazeTileWorld ();
		}
		UpdateMazeTileWorld ();

	}*/

	void CreateMazeTileWorld () {
		for (int i = 0; i < maze.GetLength (0); i++) {
			for (int j = 0; j < maze.GetLength (1); j++) {
				Vector3 tileBuildPos = new Vector3 (i, j, -1);
				GameObject newTile = (GameObject)Instantiate (tilePrefab, tileBuildPos, Quaternion.identity);
				MazeTileObj tileScript = newTile.GetComponent<MazeTileObj> ();
				tileScript.tileInfo = maze [i, j];
				tileScript.SetTileGraphic ();
				mazeTileScripts.Add (tileScript);
			}
		}

	}

	void UpdateMazeTileWorld(){
		for (int i = 0; i < mazeTileScripts.Count; i++) {
			MazeTile tl = GetTile (mazeTileScripts [i].tileInfo.pos);
			mazeTileScripts [i].tileInfo = tl;
			mazeTileScripts [i].SetTileGraphic ();
		}
		posSprite.position = pos;
		nextPosSprite.position = nextPos;
		startPosSprite.position = startPos;
	}


	public MazeTile GetTile (Vector2 _pos){
		return maze[(int)_pos.x,(int)_pos.y];
	}

	void SetTile (MazeTile _tile){
		maze[(int)_tile.pos.x,(int)_tile.pos.y] = _tile;
	}

	bool CheckInBounds(Vector2 p){
		if (p.x < 0 || p.x >= mazeSize.x || p.y < 0 || p.y >= mazeSize.y) {
			return false;
		} else {
			return true;
		}
	}
}

}
