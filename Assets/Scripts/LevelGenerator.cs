using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
/* Dalton Christopher
   04/2024
   TUA - AIP-201

   Procedual generation based on Bob Nystrom's, Implementation/Explanation of this method used in his, 
	 web-based roguelike written in Dart. Hauberk
   -- [2014] https://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/

   The Game his Prodcedual generation was developed for and that the article refers to [Very cool project]
   -- Hauberk: [2014 - 2024] https://github.com/munificent/hauberk

   There is already 26 methods and some are pretty chaotic. (Rip Edward) [My apologies]
*/
public class LevelGenerator : MonoBehaviour
{
  [SerializeField] int GridWidth; // Width of the grid to generate (To be set in the inspector) (Must be odd)
  [SerializeField] int GridHeight; // Width of the grid to generate (To be set in the inspector) (Must be odd)
  [SerializeField] int RoomGenAttempts = 300; // How many times we try to generate a room
  [SerializeField] int RoomSizeExtention = 6; // How many times we try to generate a room
  [SerializeField] int RoomExtentionRate = 8; // The maximum rooms size Length / Width
  [SerializeField] int StartRoomSize = 6; // The starting rooms size Length / Width
  [SerializeField] Tile[,] Tiles; // 2D Array of tile Obj's (Level's Grid)
  [SerializeField] GameObject TilePreFab; // Prefab for the tile obj
  [SerializeField] GameObject LadderPreFab; // Prefab for the tile obj
  [SerializeField] GameObject CoinPreFab; // Prefab for the coin obj
  [SerializeField] GameObject ChestPreFab; // Prefab for the chest obj
  [SerializeField] GameObject LightPreFab; // Prefab for the light obj
  [SerializeField] GameObject EnemyPreFab; // Prefab for the enemy
  [SerializeField] GameObject TreePreFab; // Prefab for A Tree
  [SerializeField] GameObject TreePreFab_2; // Prefab for A Tree
  [SerializeField] GameObject LargeTreePrefab; // Prefab for A Tree
  [SerializeField] List<GameObject> BushPrefabs; // Prefab for A Tree
  int[,] Regions; // An array of int's for each pos in the grid each int represents a different Region
  int CurrentRegion = -1; // Current region's ID (declared at -1, first region to be 0)
  readonly int ConnectorAttempts = 25; // How many times will we try to create region connectors
  readonly float LevelGenDelay = 0.5f; // The delay inbetween each generation action (To better visualise while developing)
  readonly List<Vector2Int> RoomList = new(); // A list of all grid position that contain a room
  List<Room> Rooms = new(); // A dictionary of all rooms in the level

  #region Wait for SpaceBar before generating
  void Update()
  {
    // If spacebar is pressed
    if (Input.GetKeyDown(KeyCode.Space)) InitGrid(GridWidth, GridHeight); // Initalise the grid (Start generating)
  }
  #endregion

  #region Generate Level Coroutine / Get Directions
  /// <summary>
  /// Coroutine to Go through the Level Generation Steps was small pauses between steps
  /// </summary>
  IEnumerator GenerateLevel()
  {
    yield return new WaitForSeconds(LevelGenDelay);
    PopulateGrid(TileType.Wall); // Populate grid with Tiles of wall type as default
    yield return new WaitForSeconds(LevelGenDelay);
    PopulateGrid_Rooms(TileType.Path); // Populate grid with roomsv
    yield return new WaitForSeconds(LevelGenDelay);
    AllocateStartArea(StartRoomSize); // Create the constant starting area
    yield return new WaitForSeconds(LevelGenDelay);
    PopulateGrid_Mazes(); // Populate the grid with mazes in free spaces
    yield return new WaitForSeconds(LevelGenDelay);
    ConnectRegions();// (Destroy a wall where the wall has a path to 1 region
                     // (up/down || left/right) & a (Different) Region Opposite to it
    yield return new WaitForSeconds(LevelGenDelay);
    CloseDeadEnds(); // remove dead ends "Paths Tiles with 3 walls"
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnLadders(); // Spawn in ladder objects at required positions (Must be called before setting grass tiles)
    yield return new WaitForSeconds(LevelGenDelay);
    SetGrassTiles(); // Update "Wall" Tiles to "Grass" tiles at required places
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnCoins(); // Spawn coins / Chests
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnLights(); // Spawn Lights in rooms randomly
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnEnemies(); // Spawn Enemies in rooms randomly
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnBackgroundItems(); // Spawn Enemies in rooms randomly
    yield return new WaitForSeconds(LevelGenDelay);
    SpawnBushes(); // Spawn Enemies in rooms randomly

  }

  /// <summary>
  /// A method used get a list of vectors representing the cardinal directions: up, down, left, and right.
  /// (Short hand vector directions for N, S, E, W)
  /// </summary>
  /// <returns>
  /// A list of <see cref="Vector2Int"/> containing the cardinal directions.
  /// </returns>
  List<Vector2Int> GetDirections()
  {
    return new List<Vector2Int>{
                        Vector2Int.up,
                        Vector2Int.down,
                        Vector2Int.left,
                        Vector2Int.right
                      };
  }
  #endregion

  #region Initalialise Grid / Default tiles / Rooms
  /// <summary>
  /// This method takes the passed width / Height ints's and will allocate grid based locations
  /// within the two dimenstional array, for the Tile obj's in Tiles[,] / And just int's for Regions, to initalise them.
  /// The method also checks if the width/height are even ands throws an argument if so. (must be odd)
  /// Finally it calls the GenerateLevel Coroutine. (this might just be a method later coroutine to delays steps, to debug easier.
  /// </summary>
  /// <param name="width"> the width of the grid to initalise </param>
  /// <param name="height"> the height of the grid to initalise </param>
  void InitGrid(int width, int height)
  {
    #region Ensure Grid size is Odd
    if (width % 2 == 0 || height % 2 == 0) // If width / height are divisable by 2 with no remainder they are even.
    {
      throw new ArgumentException("The width & height must be odd numbers"); // Throw exception
    }
    #endregion

    #region Initalise Arrays
    Tiles = new Tile[width, height]; // initalise Tiles array with width/height.
    Regions = new int[width, height]; // initalise Regions array with width/height.
    #endregion

    #region Start generating
    StartCoroutine(GenerateLevel()); // Start Generate Level coroutine
    #endregion
  }

  /// <summary>
  /// A method to populate the grid with tiles
  /// </summary>
  /// <param name="tile"> The TileType (enum) to fill the grid with </param>
  void PopulateGrid(TileType tile)
  {
    // loop through all grid positions
    for (int y = 0; y < GridHeight; y++) // For each Position in the grids y axis
    {
      for (int x = 0; x < GridWidth; x++) // For each Position in the grids X axis
      {
        #region Instantiate Tiles in all Grid cells
        // Instantiate a tile Prefab at the current x,y pos in the grid
        GameObject tileObject = Instantiate(TilePreFab, new Vector3(x, y, 0), Quaternion.identity);
        Tile tilePlaced = tileObject.GetComponent<Tile>(); // Get the tileprefabs Tile script
        Tiles[x, y] = tilePlaced; // add the tile script the the Tiles array at the current pos
        tilePlaced.SetType(tile); // Set the TileType to the type passed to the method
        #endregion
      }
    }
  }

  /// <summary>
  /// A method to populate the grid with various randomly sized rooms at random positions
  /// </summary>
  /// <param name="roomTile"> The tileType to set the rooms tiles to </param>
  void PopulateGrid_Rooms(TileType roomTile)
  {
    for (int i = 0; i < RoomGenAttempts; i++)
    {
      #region Randomise Room Size / Position
      int size = Random.Range(3, 5) * 2 + 1; // Random range between 3,5 Multiplied by 2 + 1 to keep rooms odd
      int width = size; // declare width int set to size
      int height = size / 2; // declare height int set to half size

      // Randomly int the width 33~% chance
      if (Random.Range(0, 3) == 0) width *= 3;// If is 0, tripple width
      if (Random.Range(0, RoomExtentionRate) == 0) width += RoomSizeExtention; height++; // Roll extra expansion rate chance set in insepcector
      if (height % 2 == 0) height++; // If height isn't odd Increment height  [Ensure they are odd]
      if (width % 2 == 0) width--; // If width isn't odd Increment height  [Ensure they are odd]

      // Randomise x,y position in grid for room to be placed
      // Random number between (1 and the width of the Levels grid - RoomWidth / 2 ) * 2 + 1 | to always bee and odd number for the postions - Do same for height
      int x = Random.Range(1, (GridWidth - width) / 2) * 2 + 1;
      int y = Random.Range(1, (GridHeight - height) / 2) * 2 + 1;
      #endregion

      #region Check if this room would overlap existing rooms
      bool overlaps = false; // flag for a check if the rooms overlap

      // For each position from the randomised start-postion - 1 (extra 1 tile check for a boundry) through to start-position + height/width + 1 (extra tile row/col)
      for (int yy = y - 1; yy < y + height + 1 && !overlaps; yy++) // Y axis loop (Height)
      {
        for (int xx = x - 1; xx < x + width + 1 && !overlaps; xx++) // X axis loop (Width)
        {
          Vector2Int roomPos = new Vector2Int(xx, yy); // A vector2 of the current x,y position in the grid of (Room position)
          foreach (var room in RoomList) // For each position of current rooms in room list
          {
            // Check if the absolute differences between the current roomPos and the stored room pos is <=1 for each x/y
            if (Mathf.Abs(roomPos.x - room.x) <= 1 && Mathf.Abs(roomPos.y - room.y) <= 1)
            {
              // If it is the rooms overlap
              overlaps = true; // Set flag to true
              break; // exit the foreach
            }
          }
        }
      }
      if (overlaps) continue; // If overlaps dont add this room and attempt another room
      #endregion

      //Debug log for each room positon and width/height
      Debug.Log($"Room being added @ - X: {x} Y: {y} Size: Width: {width} Height: {height}");

      #region Add the room to the grid

      StartRegion(); // Start a new region (Increment current region)



      Vector2Int startPos = new Vector2Int(x, y);
      Rooms.Add(new Room(startPos, width, height)); // Add the room to the RoomList


      // loop through grid positions (starting at the randomise x,y position)
      for (int yy = y; yy < y + height; yy++) // For each Position in the grids y axis from start pos -> to start pos + height (room height)
      {
        for (int xx = x; xx < x + width; xx++) // For each Position in the grids x axis from start pos -> to start pos + width (room width)
        {
          // Set the type of the tile at the current x,y pos (xx,yy) to the passed roomTile var
          Tiles[xx, yy].SetType(roomTile);
          // Update the Regions array with the new region information the room tile
          Regions[xx, yy] = CurrentRegion;
          // Add the room position to the RoomList
          RoomList.Add(new Vector2Int(xx, yy));
        }
      }
      #endregion
    }
  }

  /// <summary>
  /// A Method to Create a constantly placed starting room to simplify a player spawn point, after the room is allocated,
  /// A Path 2x the room size is created horizontialy on the x axis (ensure region connection / multiple exits etc.)
  /// </summary>
  /// <param name="startRoomSize"> The size both Height & Width for the room </param>
  void AllocateStartArea(int startRoomSize) // Mayb--DONT add to room list (easy way to avoid spawnpoint item/enemy spawns)
  {
    StartRegion(); // Start a new region (Increment current region)
    for (int y = 1; y < startRoomSize; y++) // For each Position in the grid starting at 1 on the y axis ending at size of the start room
    {
      for (int x = 1; x < startRoomSize; x++) // For each Position in the grid starting at 1 on the x axis ending at size of the start room
      {
        Tiles[x, y].SetType(TileType.Path); // Set type
        Regions[x, y] = CurrentRegion;// Update the Regions array with the new region information the room tile
                                      // Add the room position to the RoomList
        RoomList.Add(new Vector2Int(x, y));
      }
    }
    for (int x = startRoomSize; x < startRoomSize * 2; x++) // Create a constant path out of the room 
    {
      int y = startRoomSize / 2;
      Tiles[x, y].SetType(TileType.Path); // Set type
      Regions[x, y] = CurrentRegion;// Update the Regions array with the new region information the room tile                     
      RoomList.Add(new Vector2Int(x, y)); // Add the room position to the RoomList
    }
  }
  #endregion

  #region Maze Generation Methods
  /// <summary>
  /// Fill the Rest of the Grid with mazes starting from odd Positions in the LevelGrid
  /// </summary>
  void PopulateGrid_Mazes()
  {
    // Loop through the grid's odd positions
    for (int y = 1; y < GridHeight; y += 2) // Loop through the grid's odd y positions
    {
      for (int x = 1; x < GridWidth; x += 2) // Loop through the grid's odd x positions
      {
        Vector2Int mazeStartPos = new(x, y); // Current position - Possible Maze start position
                                             // Check if the tile at the current position is a wall
        if (Tiles[x, y].Type != TileType.Wall) continue; // Tile isn't a wall, go to the next odd position
        GenerateMaze(mazeStartPos); // Start generating the maze from the current position
      }
    }
  }

  /// <summary>
  /// A method that will Open the path of the maze up from the given startPosition while continuing to
  /// Check if the path is valid
  /// </summary>
  /// <param name="startPosition"> The Positon in the Grid to start teh maaze </param>
  void GenerateMaze(Vector2Int startPosition)
  {
    #region Process Start Position
    var gridCells = new List<Vector2Int>(); // List to keep track of cells
    StartRegion(); // Start a new region (increment)
    OpenPath(startPosition); // Open the starting cell
    gridCells.Add(startPosition); // Add the starting cell to the list
    #endregion

    while (gridCells.Count > 0) // While there is still atleast 1 position in the list of GridCells
    {
      Vector2Int cell = gridCells[gridCells.Count - 1]; // Get the last cell from the list
      var pathCells = new List<Vector2Int>(); // List to keep track of directions the path that are not open

      foreach (var direction in GetDirections()) // Check each direction (up, down, left, right)
      {
        // Check if we can open a path in the current direction
        if (IsValidPath(cell, direction) && IsValidPath(cell + direction * 2, direction)) // Check current cell and future cells
        {
          pathCells.Add(direction); // Add the direction to the list of unopened cells
        }
      }
      if (pathCells.Count > 0) // Theres atleast 1 path position that can be Opened
      {
        #region Open Path Cells & Add Next Cell to Cell list

        // Randomly select a direction from the list of Path Cells
        Vector2Int direction = pathCells[Random.Range(0, pathCells.Count)];
        OpenPath(cell + direction); // Open the cell in the selected direction
        OpenPath(cell + direction * 2); // Open the next cell in the same direction
        gridCells.Add(cell + direction * 2); // Add the next cell to the list

        #endregion
      }
      else gridCells.RemoveAt(gridCells.Count - 1); // Remove the last cell from the list
    }
  }

  /// <summary>
  /// Method to Check if we can open a path in a given direction from a position..
  /// Is the future path positon inside the Levels Grid/ Is the Future path in the direction a TileType.Wall?
  /// </summary>
  /// <param name="position">  </param>
  /// <param name="direction">  </param>
  /// <returns>  </returns>
  bool IsValidPath(Vector2Int position, Vector2Int direction)
  {
    // Calculate the resulting position after opening a path (current x,y position + direction * 2 [ie from where we are 2 steps in that direction])
    Vector2Int nextPosition = position + direction * 2;

    // Check if the resulting position after opening a path is out of bounds
    if (nextPosition.x < 0 || nextPosition.x >= GridWidth || nextPosition.y < 0 || nextPosition.y >= GridHeight)
      return false; // The position is outside of the grid. Path is not valid

    // Check if the tile two steps in the specified direction from the given position is a wall
    if (Tiles[nextPosition.x, nextPosition.y].Type == TileType.Wall)
      return true; // The path is Valid as tile is a wall (Path can be opened)

    else return false; // Else path is not valid
  }

  /// <summary>
  /// Open the path at the passed Vector2 position in the grid
  /// </summary>
  /// <param name="position"> The Position to Open (Vector2Int) </param>
  void OpenPath(Vector2Int position)
  {
    // At the position in the Tiles array - Set the tile type to path
    Tiles[position.x, position.y].SetType(TileType.Path);

    // At the position in the Tiles array -  Set the region for the path's position
    Regions[position.x, position.y] = CurrentRegion;
  }
  #endregion

  #region General Region Methods / Connecting Regions (Rooms/Mazes)
  /// <summary>
  /// Connects regions in the maze by finding connectors and creating connections between them.
  /// </summary>
  void ConnectRegions()
  {
    for (int i = 0; i < ConnectorAttempts; i++) // For each total attempt to connect regions
    {
      // This will become the main region (region to be fully connected)
      int mainRegion = 0; // I just set to the first region (-1 is initalised value)

      // Find all connectors (positions that could be valid to connect regions) in the maze
      var connectors = GetConnectorPositions();

      // Create connections using a minimum spanning tree algorithm 
      // (I dont think this is actually a min spanning tree) I tried to suss this out further, It boiled my brain
      // Bob Nystrom mentions that algorithm in his article but i kinda just brute forced this part hence the multiple attempts
      CreateConnections(mainRegion, connectors);
    }
  }

  /// <summary>
  /// Finds and returns all connector positions in the maze 
  /// </summary>
  /// <returns>List of connector positions </returns>
  List<Vector2Int> GetConnectorPositions()
  {
    List<Vector2Int> connectors = new();

    // Loop through the grid to search for potential connectors
    for (int y = 1; y < GridHeight - 1; y++) // For each Position in the grids y axis
    {
      for (int x = 1; x < GridWidth - 1; x++) // For each Position in the grids X axis
      {
        // Skip the iteration if the current tile is not a wall
        if (Tiles[x, y].Type != TileType.Wall) continue;

        int region1 = -1; // Initialize the first neighboring region ID to -1
        int region2 = -1; // Initialize the second neighboring region ID to -1

        // Check the left and right neighboring tiles
        if (Tiles[x - 1, y].Type == TileType.Path) region1 = Regions[x - 1, y];
        if (Tiles[x + 1, y].Type == TileType.Path) region2 = Regions[x + 1, y];

        // Check the above and below neighboring tiles
        if (Tiles[x, y - 1].Type == TileType.Path) // Below
        {
          // If region1 is still unassigned, assign it the region ID of the above neighbor
          if (region1 == -1) region1 = Regions[x, y - 1];

          // Otherwise, assign region2 the region ID of the above neighbor
          else if (region2 == -1) region2 = Regions[x, y - 1];
        }
        if (Tiles[x, y + 1].Type == TileType.Path) // Above
        {
          // If region1 is still unassigned, assign it the region ID of the lower neighbor
          if (region1 == -1) region1 = Regions[x, y + 1];

          // Otherwise, assign region2 the region ID of the lower neighbor
          else if (region2 == -1) region2 = Regions[x, y + 1];
        }
        // If there are exactly two distinct neighboring regions, the tile is a connector
        if (region1 != -1 && region2 != -1 && region1 != region2)
        {
          connectors.Add(new Vector2Int(x, y)); // Add the connector position to the list
        }
      }
    }
    return connectors; // Return the list of connector positions
  }

  /// <summary>
  /// Creates connections between regions
  /// </summary>
  /// <param name="mainRegion"> Main region to start the connection process </param>
  /// <param name="connectors"> List of connector positions </param>
  void CreateConnections(int mainRegion, List<Vector2Int> connectors)
  {
    // List of ints to reflect the Regions we have visted
    List<int> visitedRegions = new() { mainRegion };

    // List of Vect2 ints to store the connectors
    List<Vector2Int> openConnectors = new(connectors);

    // Continue looping until all regions are connected or no open connectors are left
    while (visitedRegions.Count < CurrentRegion + 1 && openConnectors.Count > 0)
    {
      #region Check Connectors adjacent Regions
      var c = openConnectors[Random.Range(0, openConnectors.Count)]; // Pick a random connector

      openConnectors.Remove(c); // Remove the current connect for the openConnectors list

      int region_1 = Regions[c.x, c.y]; // Region 1 = to Connectors x,y position

      // Check region 2 to see if the region next to the connector is the same region as Region 1
      int region_2 = Regions[c.x + 1, c.y] == region_1 ? Regions[c.x, c.y + 1] : Regions[c.x + 1, c.y];

      // Skip if the regions connected by the connector have both been visited
      if (visitedRegions.Contains(region_1) && visitedRegions.Contains(region_2)) continue;

      OpenConnection(c); // Open the connector to connect the regions (Replace wall with Path)

      MergeRegions(region_1, region_2); // Merge the two regions into one region

      visitedRegions.Add(region_1); // Update Visted Regions list
      visitedRegions.Add(region_2); // Update Visted Regions list
      #endregion
    }
  }

  /// <summary>
  /// Increments the CurrentRegion int (Each int is a marker of a region)
  /// </summary>
  void StartRegion()
  {
    CurrentRegion++; // Increment Current region
  }

  /// <summary>
  /// A method that is used to merge 2 Regions together
  /// </summary>
  /// <param name="Region"> Region to merge with 'RegionToMerge' </param>
  /// <param name="RegionToMerge"> The region to merge </param>
  void MergeRegions(int Region, int RegionToMerge)
  {
    // loop through all grid positions
    for (int y = 0; y < GridHeight; y++) // For each Position in the grids y axis
    {
      for (int x = 0; x < GridWidth; x++) // For each Position in the grids X axis
      {
        if (Regions[x, y] == RegionToMerge) Regions[x, y] = Region; // Set RegionToMerge To Region
      }
    }
  }

  /// <summary>
  /// Opens a connection at the given tile used to connect two isolated regions
  /// </summary>
  /// <param name="position">Position of the connector tile</param>
  void OpenConnection(Vector2Int position)
  {
    Tiles[position.x, position.y].SetType(TileType.Path); // Set tile type to path
  }

  /// <summary>
  /// A method that will close the dead ends provided the regions are connected.
  /// Deadends are found by checking if 3 or more walls are found to be surrounding a floor tile.
  /// </summary>
  void CloseDeadEnds()
  {
    bool closed = false; // flag to continue the loop while the dead end isnt closed

    while (!closed) // while dead end path isnt close
    {
      closed = true; // Set to true so if a wall isnt closed this iteration the while loop will exit

      // loop through all grid positions
      for (int x = 0; x < GridWidth; x++) // For each Position in the grids y axis
      {
        for (int y = 0; y < GridHeight; y++) // For each Position in the grids X axis
        {
          if (Tiles[x, y].Type == TileType.Path)// Check if the tile is a path tile Type
          {
            int wallCount = 0; // To count surounding walls of a current cell
            foreach (var direction in GetDirections()) // Check each direction
            {
              // If the tile in the direction is of TileType Wall, Increment wallcount
              if (Tiles[x + direction.x, y + direction.y].Type == TileType.Wall) wallCount++;
            }
            if (wallCount >= 3) // If there are 3 walls around the tile, change its type to a wall
            {
              Tiles[x, y].SetType(TileType.Wall); // Set Tile Type to Wall at the position
              closed = false; // Set the flag to false as a Dead end path is stil being closed
            }
          }
        }
      }
    }
  }
  #endregion

  #region Tile Checks (For Spawnables/ alt tiles)
  /// <summary>
  /// Checks if a tile in the grid is a wall-type and has a path type above it (y+1)
  /// This is the check made to set grass tiles in the grid
  /// </summary>
  /// <param name="x"> x postion of tile </param>
  /// <param name="y"> y position of tile to check </param>
  /// <returns> True if tile is a wall with path above / else false </returns>
  bool IsWallWithPathAbove(int x, int y)
  {
    // Check if the current tile is a wall tile
    if (Tiles[x, y].Type != TileType.Wall) return false;

    // Check if there's a path tile above this tile
    if (y < GridHeight - 1 && Tiles[x, y + 1].Type == TileType.Path)
    {
      return true;
    }
    return false;
  }

  /// <summary>
  /// Checks if a tile in the grid is a wall-type and has a path type below it (y-1)
  /// </summary>
  /// <param name="x"> x postion of tile </param>
  /// <param name="y"> y position of tile to check </param>
  /// <returns> True if tile is a wall with path below / else false </returns>
  bool IsWallWithPathBelow(int x, int y)
  {
    // Check if the current tile is a wall tile
    if (Tiles[x, y].Type != TileType.Wall) return false;

    // Check if there's a path tile Below this tile
    if (y > 0 && Tiles[x, y - 1].Type == TileType.Path)
    {
      return true;
    }
    return false;
  }

  /// <summary>
  /// Checks if the tile at the position is surrounded by walls to the left and right
  /// and paths to the up and down.
  /// </summary>
  /// <param name="x"> x postion of tile </param>
  /// <param name="y"> x postion of tile </param>
  /// <returns>True if the tile matches the described pattern, otherwise false.</returns>
  bool IsLadderPosition(int x, int y)
  {
    // Check if the current tile is a path tile
    if (Tiles[x, y].Type != TileType.Path) return false;

    if (x > 0 && x < GridWidth - 1) // Check if either tiles to the left and right are not walls
    {
      if (Tiles[x - 1, y].Type != TileType.Wall || Tiles[x + 1, y].Type != TileType.Wall)
        return false;
    }
    if (y > 0 && y < GridHeight - 1) // Check if either tiles above and below are not paths
    {
      if (Tiles[x, y - 1].Type != TileType.Path || Tiles[x, y + 1].Type != TileType.Path)
        return false;
    }
    return true; // Both tiles up/down are paths, both tiles to the sides are walls
  }

  /// <summary>
  /// Checks if a tile at aposition is in the RoomList
  /// </summary>
  /// <param name="x"> x postion of tile </param>
  /// <param name="y"> y position of tile to check </param>
  /// <returns> True if the tile is in the RoomList. else false </returns>
  bool IsTileInRoomList(int x, int y)
  {
    // Create a Vector2Int from the given coordinates
    Vector2Int tilePosition = new Vector2Int(x, y);

    // Check if the tile position is in the RoomList
    if (RoomList.Contains(tilePosition))
    {
      return true;
    }
    return false;
  }

  /// <summary>
  /// Checks if the tile at the given position is a grass tile.
  /// </summary>
  /// <param name="x">The x-coordinate of the tile to check.</param>
  /// <param name="y">The y-coordinate of the tile to check.</param>
  /// <returns>True if the tile is a grass tile, otherwise false.</returns>
  bool IsGrassTile(int x, int y)
  {
    // Check if the tile at (x, y) is a grass tile
    if (Tiles[x, y].Type == TileType.Grass)
    {
      return true;
    }
    return false;
  }
  #endregion

  #region Spawn Item Methods / Set Alt Tiles
  /// <summary>
  /// Loops through the grid and checks each tile using the "IsWallWithPathAbove" method
  /// If that check returns true then the tile type of the current positon is set to Grass
  /// </summary>
  void SetGrassTiles()
  {
    for (int y = 0; y < GridHeight - 1; y++)
    {
      for (int x = 0; x < GridWidth - 1; x++)
      {
        if (IsWallWithPathAbove(x, y))
        {
          // Set the type of the tile at the current x,y pos
          Tiles[x, y].SetType(TileType.Grass);
        }
      }
    }
  }

  /// <summary>
  ///
  /// </summary>
  void SpawnLadders()
  {
    for (int y = 0; y < GridHeight - 1; y++)
    {
      for (int x = 0; x < GridWidth - 1; x++)
      {
        if (IsLadderPosition(x, y))
        {
          Instantiate(LadderPreFab, new Vector3(x, y, 0), Quaternion.identity);
        }
      }
    }
  }

  /// <summary>
  ///
  /// </summary>
  void SpawnLights()
  {
    for (int y = 0; y < GridHeight; y++)
    {
      for (int x = 0; x < GridWidth; x++)
      {
        if (IsWallWithPathBelow(x, y) && IsTileInRoomList(x, y - 1))
        {
          if (Random.Range(0, 4) == 0) Instantiate(LightPreFab, new Vector3(x, y, 0), Quaternion.identity);
        }
      }
    }
  }

  /// <summary>
  ///
  /// </summary>
  void SpawnCoins()
  {
    for (int y = 0; y < GridHeight - 1; y++)
    {
      for (int x = 0; x < GridWidth - 1; x++)
      {
        if (IsGrassTile(x, y))
        {
          if (Random.Range(0, 10) == 0) Instantiate(CoinPreFab, new Vector3(x, y + 1, 0), Quaternion.identity); // spawn a coin

          if (IsTileInRoomList(x, y + 1)) // Spawn a chest
          {
            if (Random.Range(0, 13) == 0) Instantiate(ChestPreFab, new Vector3(x, y + 1, 0), Quaternion.identity);
          }
        }
      }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  void SpawnEnemies()
  {
    // Do enemy spawning bruh
    for (int y = 0; y < GridHeight - 1; y++)
    {
      for (int x = 0; x < GridWidth - 1; x++)
      {
        if (IsGrassTile(x, y) && IsTileInRoomList(x, y + 1)) // can spawn only on grass tiles that are the floors of rooms
        {
          if (Tiles[x - 1, y].Type == TileType.Grass || Tiles[x + 1, y].Type == TileType.Grass) // cant spawn on isolated grass tiles must have a grass neighbour
          {
            if (Random.Range(0, 6) == 0) // 20% chance of spawning at valid pos
            {
              Instantiate(EnemyPreFab, new Vector3(x, y + 1, 0), Quaternion.identity); // Instantiate an enemy
            }
          }
        }
      }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  void SpawnBackgroundItems()
  {
    foreach (Room room in Rooms)
    {
      List<Vector2Int> possibleItemPositions = GetPossibleItemPositions(room);
      List<Vector2Int> currentItemPositions = possibleItemPositions;

      currentItemPositions.Remove(new Vector2Int(room.EntryPos.x + 1, room.EntryPos.y));
      //currentItemPositions.Remove(new Vector2Int(room.EntryPos.x - 1, room.EntryPos.y));
      currentItemPositions.Remove(new Vector2Int(room.StartPos.x, room.StartPos.y));
      currentItemPositions.Remove(new Vector2Int(room.StartPos.x + room.Width - 1, room.StartPos.y));
      int LargeTreeAttempts = 3;
      int SmallTreeAttempts = 20;

      while (currentItemPositions.Count > 0)
      {

        Vector2Int randomSpawn;
        while (LargeTreeAttempts > 0 && room.Height >= 5)
        {
          if (currentItemPositions.Count > 0)
          {
            randomSpawn = currentItemPositions[Random.Range(0, currentItemPositions.Count)];
            if (randomSpawn.x > room.StartPos.x + 2 && randomSpawn.x < room.StartPos.x + room.Width - 2) // Big trees can spawn
            {
              Instantiate(LargeTreePrefab, new Vector3(randomSpawn.x, randomSpawn.y, 0), Quaternion.identity);
            }
            currentItemPositions.Remove(randomSpawn);
            randomSpawn.x++; currentItemPositions.Remove(randomSpawn);
            randomSpawn.x++; currentItemPositions.Remove(randomSpawn);
            randomSpawn.x -= 3; currentItemPositions.Remove(randomSpawn);
            randomSpawn.x--; currentItemPositions.Remove(randomSpawn);
          }
          LargeTreeAttempts--;
        }
        for (int i = 0; i < SmallTreeAttempts; i++) // normal trees can spawn
        {
          if (currentItemPositions.Count > 0 && room.Height >= 3)
          {
            randomSpawn = currentItemPositions[Random.Range(0, currentItemPositions.Count)];
            if (Random.Range(0, 2) == 0)
            {
              Instantiate(TreePreFab, new Vector3(randomSpawn.x, randomSpawn.y, 0), Quaternion.identity);
            }
            else Instantiate(TreePreFab_2, new Vector3(randomSpawn.x, randomSpawn.y, 0), Quaternion.identity);
            currentItemPositions.Remove(randomSpawn);
            randomSpawn.x++; currentItemPositions.Remove(randomSpawn);
            randomSpawn.x -= 2; currentItemPositions.Remove(randomSpawn);
          }
          SmallTreeAttempts--;
        }
      }
      currentItemPositions = possibleItemPositions; // Reset current Item

      // Spawn Other room items....

    }
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="room"></param>
  /// <returns></returns>
  List<Vector2Int> GetPossibleItemPositions(Room room)
  {
    List<Vector2Int> possibleItemPositions = new List<Vector2Int>();
    int x = room.StartPos.x;
    int y = room.StartPos.y;

    for (int xx = x; xx < x + room.Width; xx++)
    {
      if (Tiles[xx, y - 1].Type != TileType.Path)
      {
        possibleItemPositions.Add(new Vector2Int(xx, y));
      }
      if (Tiles[xx, y - 1].Type == TileType.Path)
      {
        room.EntryPos = new Vector2Int(xx, y);
      }
    }
    possibleItemPositions.Remove(room.EntryPos);
    return possibleItemPositions;
  }

  void SpawnBushes()
  {
    List<Vector2Int> possibleBushPositions = GetPossibleBushPositions();
    foreach (Vector2Int pos in possibleBushPositions)
    {
      int randomBush = Random.Range(0, BushPrefabs.Count);
      if (Tiles[pos.x + 1, pos.y].Type != TileType.Grass) continue;
      Instantiate(BushPrefabs[randomBush], new Vector3(pos.x, pos.y, 0), Quaternion.identity);
    }
  }
  List<Vector2Int> GetPossibleBushPositions()
  {
    List<Vector2Int> possibleBushPositions = new List<Vector2Int>();
    for (int x = 0; x < GridWidth; x++)
    {
      for (int y = 0; y < GridHeight; y++)
      {
        if (IsGrassTile(x, y))
        {
          possibleBushPositions.Add(new Vector2Int(x, y));
        }
      }
    }
    return possibleBushPositions;
  }


  #endregion




} // End of Class (stop dyslexia please)

