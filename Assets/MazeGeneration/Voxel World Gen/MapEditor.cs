using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MapEditor : MonoBehaviour {
	
	public static MapEditor singleton;
	
	public Camera mainCam;
	
	public Mesh editMesh;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	
	MapTile[,] world;
	GUIObject[] guiRects;
	
	Vector3 hitPos;
	Ray ray;
	RaycastHit rayHit;
	int chunkSize = 8;
	Vector2 chunkOffset = Vector2.zero;
	
	
	
	// gui variables
	Vector2 guiStart;
	Vector2 guiEnd;
	bool editMenuOpen = true;
	bool saveMenuOpen = false;
	bool loadMenuOpen = false;
	
	string mapFolderPath;
	
	// loading map variables
	GUIObject[] mapFileGUIObjs;
	string[] mapFiles, mapFilePaths;
	bool hasMapFiles = true;
	Rect mapFileNameRect, loadMapRect;
	Rect scrollRect, scrollViewRect;
	Vector2 scrollPos;
	
	// map editing variables
	public string editMapName = "Name Here";
	
	public byte paintHeight = 2;
	const byte minPaintHeight = 0;
	const byte maxPaintHeight = 20;
	string tempHeightStr = "1";
	float tempHeight=5;
	
	public int currentBrushSize = 5;
	const int minBrushSize = 1;
	const int maxBrushSize = 18;
	float tempBrushSize=5;
	
	void Awake () {
		singleton = this;
	}
	
	void Start () {
		mainCam = Camera.main;
		// setup the brush sizes
		SetupMesh();
		SetupGUIRects();
		SetupFilePaths();
		WorldController.singleton.CreateWorld(new Vector2(64,64));
		world = WorldController.singleton.world;
	}
	
	void SetupFilePaths () {
		mapFolderPath = Application.dataPath+"/Maps";
	}
	
	void SetupGUIRects () {
	
		// set the start and end position of the gui
		guiStart = new Vector2(Screen.width-220,-170);
		guiEnd = new Vector2(Screen.width-20,Screen.width-20);
		
		guiRects = new GUIObject[16];
		
		// border box for editor
		guiRects[0].rect = new Rect(Screen.width-220,20,200,250);
		guiRects[0].text = "Map Editor";
		
		// brush size label
		guiRects[1].rect = new Rect(Screen.width-210,50,90,20);
		guiRects[1].text = "Brush Size:";
		
		// brush size slider
		guiRects[2].rect = new Rect(Screen.width-125,55,100,20);
		guiRects[2].text = string.Empty;
		
		// brush size value label
		guiRects[3].rect = new Rect(Screen.width-140,50,10,20);
		guiRects[3].text = string.Empty;
		
		// height label
		guiRects[4].rect = new Rect(Screen.width-210,80,50,30);
		guiRects[4].text = "Height:";
		
		// height box
		guiRects[5].rect = new Rect(Screen.width-165,80,30,20);
		guiRects[5].text = string.Empty;
		
		// height slider
		guiRects[6].rect = new Rect(Screen.width-125,80,100,20);
		guiRects[6].text = string.Empty;
		
		// save button
		guiRects[7].rect = new Rect(Screen.width-210,140,80,20);
		guiRects[7].text = string.Empty;
		
		// back button
		guiRects[8].rect = new Rect(Screen.width-215,25,60,20);
		guiRects[8].text = "Cancel";
		
		// name label
		guiRects[9].rect = new Rect(Screen.width-210,80,90,20);
		guiRects[9].text = "Map Name:";
		
		// name input box
		guiRects[10].rect = new Rect(Screen.width-135,80,110,20);
		guiRects[10].text = string.Empty;
		
		// map name info box
		guiRects[11].rect = new Rect(Screen.width-210,95,190,20);
		guiRects[11].text = "(Must contain letters only)";		
		
		// load map button
		// save button
		guiRects[12].rect = new Rect(Screen.width-120,140,80,20);
		guiRects[12].text = string.Empty;
		
		// refresh map button 
		guiRects[13].rect = new Rect(Screen.width-215,50,60,20);
		guiRects[13].text = "Refresh";
		
		// load map box
		guiRects[14].rect = new Rect(Screen.width-215,75,190,190);
		guiRects[14].text = "Map Loader";
		
		
		
		// map file stuff
		
		mapFileGUIObjs = new GUIObject[2];
		scrollRect = new Rect(Screen.width-210,115,185,100);
		scrollViewRect = new Rect(Screen.width-210, 115, 160, 300);
		scrollPos = new Vector2();
		
		// map name rect;
		mapFileGUIObjs[0].rect = new Rect(Screen.width-210,90,110,25);
		mapFileGUIObjs[0].text = "Map Name:";
		
		mapFileGUIObjs[1].rect = new Rect(Screen.width-95,90,50,25);
		mapFileGUIObjs[1].text = "Load?";
	}
	
	void SetupMesh () {
		// setup the world
		
		
		meshFilter =this.GetComponent<MeshFilter>();
		editMesh = new Mesh();
		editMesh.name = "EditWorldMesh";
		meshRenderer = this.GetComponent<MeshRenderer>();
	}

	// creates colliders for all of the world chunks
	public static void CreateColliders () {
		MapGenerator.singleton.AddCollidersToChunks();
	}
	
	void Update () {
		
	}
	
	void CreateNewEditMesh (int x, int y) {
		MapGenerator.singleton.CreateEditMesh(paintHeight,x,y,currentBrushSize);
		meshFilter.sharedMesh = editMesh;
	}
	
	
	void PaintHeight (int xPos, int yPos) {
		
		chunkSize = WorldController.singleton.chunkSize;
		chunkOffset = Vector2.one * chunkSize * 0.5f;
		
		int startXPos = xPos-((currentBrushSize-1)/2);
		int startYPos = yPos-((currentBrushSize-1)/2);
		int endXPos = xPos+((currentBrushSize-1)/2)+1;
		int endYPos = yPos+((currentBrushSize-1)/2)+1;
		
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
		
		// paint all of the blocks at the specified height
		
		for(int i=startXPos;i<endXPos;i++){
			for(int j=startYPos;j<endYPos;j++){
			
				// paint the desired height
				world[i,j].height = paintHeight;
			}
		}
		
		// and refresh all of the chunks including the border ones
		// increase the draw area by 1 block in all directions
		
		// check for edge of world
		if(startXPos !=0){
			startXPos -=1;
		}
		else if(endXPos == world.GetLength(0)){
			endXPos +=1;
		}
		
		if(startYPos != 0){
			startYPos -=1;
		}
		else if(endYPos == world.GetLength(1)){
			endYPos +=1;
		}
		// refresh all of the chunks
		for(int i=startXPos;i<endXPos;i++){
			for(int j=startYPos;j<endYPos;j++){
			
				// find which chunk the blocks belong to
				int chunkX = Mathf.RoundToInt(i/chunkSize);
				int chunkZ = Mathf.RoundToInt(j/chunkSize);
				// this chunk needs to be regenerated
				MapGenerator.singleton.worldChunks[chunkX,chunkZ].needRegen = true;
			}
		}
		
		
		
		MapGenerator.singleton.RefreshChunks();
	}
	
	
	void FixedUpdate () { 
		
		// check if the mouse is in the gui
		// since if it is, then we dont want to create the edit mesh, or paint it
		if(CheckMouseInGUI() == false){
		ray = mainCam.ScreenPointToRay(Input.mousePosition);
		
		if(Physics.Raycast(ray,out rayHit,1000)){
			// get the nearest world coordinate to this point
			int hitX = Mathf.FloorToInt(rayHit.point.x);
			int hitZ = Mathf.FloorToInt(rayHit.point.z);
			// scale this point to the nearest chunk size
			
			// show the new selection
			// generate a new mesh from the map gen.
			CreateNewEditMesh (hitX, hitZ);
			
			
			if(Input.GetMouseButton(0)){
				// paint the new height
				PaintHeight(hitX, hitZ);
			}
		}
		}
	
	}
	
	void OnGUI () {
		// map editor box
		GUI.Box (guiRects[0].rect,guiRects[0].text);
		
		if(editMenuOpen == true){
		// brush size stuff
		GUI.Label(guiRects[1].rect,guiRects[1].text);
		
		tempBrushSize = GUI.HorizontalSlider(guiRects[2].rect,tempBrushSize,minBrushSize, maxBrushSize);
		
		currentBrushSize = (int)(tempBrushSize+1);
		currentBrushSize /=2;
		if(currentBrushSize%2 == 0){
			currentBrushSize+=1;
		}
		
		GUI.Label(guiRects[3].rect,currentBrushSize.ToString());
		
		// height stuff
		GUI.Label(guiRects[4].rect,guiRects[4].text);
		
		tempHeightStr = GUI.TextField(guiRects[5].rect,tempHeightStr,3);
		
		// convert the value to a reasonable byte
		try{
			paintHeight = Convert.ToByte(tempHeightStr);
			paintHeight = (byte)Mathf.Clamp(paintHeight,minPaintHeight,maxPaintHeight);
		}
		catch(Exception e){
			print(e.Message);
			paintHeight = 1;
			
		}
		
		tempHeight = GUI.HorizontalSlider(guiRects[6].rect,tempHeight,minPaintHeight,maxPaintHeight);
		paintHeight = (byte)tempHeight;
		
		tempHeightStr = paintHeight.ToString();
		
		// draw the save menu button
		if(GUI.Button (guiRects[7].rect, "Save")){
			editMenuOpen = false;
			saveMenuOpen = true;
			editMapName = "";
		}
		
			// draw the save menu button
		if(GUI.Button (guiRects[12].rect, "Load")){
			editMenuOpen = false;
			loadMenuOpen = true;
			RefreshMapFiles();
		}
		
		
		}
		else if(saveMenuOpen == true){
			// cancel button
			if(GUI.Button(guiRects[8].rect,guiRects[8].text)){
				saveMenuOpen = false;
				editMenuOpen = true;
			}
			
			// map name label
			GUI.Label(guiRects[9].rect,guiRects[9].text);
			
			// map name input box
			editMapName = GUI.TextField(guiRects[10].rect,editMapName,15);
			// must not contain spaces label
			GUI.Label (guiRects[11].rect, guiRects[11].text);
			
			// check for no map name
			if(editMapName != ""){
				// check for spaces
				if(DoesContainSpaces(editMapName) == false){
					// save button
					if(GUI.Button(guiRects[7].rect, "Save Map")){
						SaveMap();
					}
				}
			
			}
		}
		else if(loadMenuOpen == true){
			
			// map loader box
			
			// cancel button
			if(GUI.Button(guiRects[8].rect,guiRects[8].text)){
				loadMenuOpen = false;
				editMenuOpen = true;
				hasMapFiles = false;
			}
			
			// refresh map button
			if(GUI.Button (guiRects[13].rect, guiRects[13].text)){
				RefreshMapFiles();
			}
			
			GUI.Box (guiRects[14].rect,guiRects[14].text);
			
			// show map file browser
			if(hasMapFiles == true){
				// the top bar
				GUI.Label (mapFileGUIObjs[0].rect, mapFileGUIObjs[0].text);
				GUI.Label (mapFileGUIObjs[1].rect, mapFileGUIObjs[1].text);
				
				// the scroll view
				scrollRect.height = (mapFiles.Length)*30;
				scrollPos = GUI.BeginScrollView (scrollRect, scrollPos, scrollViewRect);
			
				for (int i=0; i<mapFiles.Length; i++) {
					int rectYPos = (30 * i)+115;
					
					mapFileNameRect = mapFileGUIObjs[0].rect;
					mapFileNameRect.y = rectYPos;
					
					loadMapRect = mapFileGUIObjs[1].rect;
					loadMapRect.y = rectYPos;
					
					GUI.Box(mapFileNameRect,mapFiles[i]);
					
					if(GUI.Button (loadMapRect,"Load")){
						
						loadMenuOpen = false;
						editMenuOpen = true;
						// load the map here
						LoadMap(i);
					}
				}	
			GUI.EndScrollView ();
			}
		}
		
	}
	
	
	void RefreshMapFiles () {
		mapFilePaths = Directory.GetFiles(mapFolderPath, "*.map");
		mapFiles = new string[mapFilePaths.Length];
		for(int i=0;i<mapFilePaths.Length;i++){
			mapFiles[i] = Path.GetFileNameWithoutExtension(mapFilePaths[i]);
		}
		hasMapFiles = true;
	}
	
	bool CheckMouseInGUI () {
		// check x
		if(Input.mousePosition.x < guiStart.x || Input.mousePosition.x > guiEnd.x){
			return false;
		}
		else if(Input.mousePosition.y > guiStart.y || Input.mousePosition.y < guiEnd.y){
			return false;
		}
		else{
			return true;
		}
	}
	
	bool invalidChar = false;
	bool DoesContainSpaces (string str){
		
		invalidChar = false;
		
		for(int i=0;i<str.Length;i++){
			if(char.IsLetter(str[i]) == false){
				invalidChar = true;
				i=str.Length; 
			}
		}
		
		return invalidChar;
	}
	
	void LoadMap (int filePathIndex) {
		MapGenerator.singleton.DestroyWorld();
		WorldController.singleton.LoadMap(mapFilePaths[filePathIndex]);
		CreateColliders();
		
	}
	
	void SaveMap () {
		string saveMapName = "mp_"+editMapName;
		
		MapData mapToSave = new MapData();
		mapToSave.map = world;
		mapToSave.mapSizeX = world.GetLength(0);
		mapToSave.mapSizeY = world.GetLength(1);
				
		VerifyFilePath(mapFolderPath);
		string filePath = mapFolderPath+"/"+saveMapName+".map";
		
		mapToSave.Save(filePath);
	}
	
	void VerifyFilePath (string path) {
		// first check for the data files folder
		if(Directory.Exists(path) == false){
			print ("creating maps folder");
			DirectoryInfo newFolder = Directory.CreateDirectory(path);
		}
		else{
			print ("found maps folder");
		}
		
	}
}
