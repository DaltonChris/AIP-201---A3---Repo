using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/*
    Procedual generation based on Bob Nystrom's, Implementation/Explanation of this method used in his, web-based roguelike written in Dart. Hauberk 
    --- [2014] https://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/

    The Game his Prodcedual generation was developed for and that the article refers to [Very cool project] 
    --- Hauberk: [2014 - 2024] https://github.com/munificent/hauberk
*/
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int GridWidth; // Width of the grid to generate (To be set in the inspector) (Must be odd)
    [SerializeField] int GridHeight; // Width of the grid to generate (To be set in the inspector) (Must be odd)
    [SerializeField] int RoomGenAttempts = 300; // How many times we try to generate a room
    [SerializeField] GameObject TilePreFab; // Prefab for the tile obj
    [SerializeField] Tile[,] Tiles;

    int[,] Regions; // An array of int's for each pos in the grid each int represents a different Region 
    int CurrentRegion = -1; // Current region (declared at -1, first region to be 0)
    
    readonly float LevelGenDelay = 1.5f; // The delay inbetween each generation action (To better visualise while developing)
    readonly List<Vector2Int> RoomList = new(); // A list of all grid position that contain a room


    void Update()
    {
        #region Wait for SpaceBar before generating
        // If spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space)) InitGrid(GridWidth, GridHeight); // Initalise the grid (Start generating)
        #endregion
    }

    /// <summary>
    /// Coroutine to Go through the Level Generation Steps was small pauses between steps
    /// </summary>
    IEnumerator GenerateLevel()
    {
        yield return new WaitForSeconds(LevelGenDelay/2);
        PopulateGrid(TileType.Wall); // Populate grid with Tiles of wall type as default
        yield return new WaitForSeconds(LevelGenDelay);
        PopulateGrid_Rooms(TileType.Path); // Populate grid with rooms
        yield return new WaitForSeconds(LevelGenDelay);
        // Populate the grid with mazes in free spaces - TO DO
        PopulateGrid_Mazes();
        yield return new WaitForSeconds(LevelGenDelay);
        // Connect Regions - Makes entries (Destroy a wall where the wall has a path to 1 region (up/down || left/right) & 1 To Opposite (Different)Region  - TO DO

        yield return new WaitForSeconds(LevelGenDelay);
        // remove dead ends "Paths Tiles with 3 walls"  - TO DO

    }

    /// <summary>
    /// Increments the CurrentRegion int (Each int is a marker of a region)
    /// </summary>
    void StartRegion()
    {
        CurrentRegion++; // Increment Current region
    }

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

        StartCoroutine(GenerateLevel()); // Start Generate Level coroutine
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

            // Randomly int the width 50% chance
            if (Random.Range(0, 2) == 0) width *= 2; // If is 0, double width
            if (height % 2 == 0) height++; // If height isn't odd Increment height  [Ensure they are odd]
            if (width % 2 == 0) width++; // If width isn't odd Increment height  [Ensure they are odd]

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

            Debug.Log($"Room being added @ - X: {x} Y: {y} Size: Width: {width} Height: {height}"); //Debug log for each room positon and width/height

            #region Add the room to the grid
            StartRegion(); // Start a new region (Increment current region)

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
    /// Fill the Rest of the Grid with mazes starting from odd Positions in the LevelGrid
    /// </summary>
    void PopulateGrid_Mazes()
    {
        // Loop through the grid's odd positions
        for (int y = 1; y < GridHeight; y += 2) // Loop through the grid's odd y positions
        {
            for (int x = 1; x < GridWidth; x += 2) // Loop through the grid's odd x positions
            {
                #region Check if current position is valid to start a maze
                Vector2Int mazeStartPos = new(x, y); // Current position - Possible Maze start position
                // Check if the tile at the current position is a wall
                if (Tiles[x, y].Type != TileType.Wall) continue; // Tile isn't a wall, go to the next odd position
                
                GenerateMaze(mazeStartPos); // Start generating the maze from the current position
                #endregion
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
            var pathCells = new List<Vector2Int>(); // List to keep track of directions int the path that are not open

            foreach (var direction in GetDirections()) // Check each direction (up, down, left, right)
            {
                #region Check if Cell is valid in path in the direcetion
                // Check if we can open a path in the current direction
                if (IsValidPath(cell, direction) && IsValidPath(cell + direction * 2, direction)) // Check current cell and future cells
                {
                    pathCells.Add(direction); // Add the direction to the list of unopened cells
                }
                #endregion
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
        if (nextPosition.x < 0 || nextPosition.x >= GridWidth ||nextPosition.y < 0 || nextPosition.y >= GridHeight)
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
    /// <summary>
    /// Connects regions in the maze by finding connectors and creating connections between them.
    /// </summary>

}