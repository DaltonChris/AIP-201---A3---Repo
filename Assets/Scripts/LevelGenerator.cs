using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int GridWidth; // Width of the grid to generate (To be set in the inspector) (Must be odd)
    [SerializeField] int GridHeight; // Width of the grid to generate (To be set in the inspector) (Must be odd)
    [SerializeField] int RoomGenAttempts = 300; // How many times we try to generate a room
    [SerializeField] GameObject TilePreFab; // Prefab for the tile obj
    [SerializeField] Tile[,] Tiles;

    int[,] Regions; // An array of int's for each pos in the grid each int represents a different Region 
    int CurrentRegion = -1; // Current region (declared at -1, first region to be 0)
    readonly float LevelGenDelay = 1f; // The delay inbetween each generation action (To better visualise while developing)
    List<Vector2Int> RoomList = new(); // A list of all grid position that contain a room


    void Update()
    {
        #region Wait for SpaceBar before generating
        if (Input.GetKeyDown(KeyCode.Space)) // If spacebar is pressed
        {
            StopAllCoroutines(); // Stop all coroutines
            InitGrid(GridWidth, GridHeight); // Initalise the grid (Start generating)
        }
        #endregion
    }

    // Coroutine to Go through the Level Generation Steps was small pauses between steps
    IEnumerator GenerateLevel()
    {
        yield return new WaitForSeconds(LevelGenDelay);
        PopulateGrid(TileType.Wall); // Populate grid with Tiles of wall type as default
        yield return new WaitForSeconds(LevelGenDelay);
        PopulateGrid_WithRooms(TileType.Path); // Populate grid with rooms
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
        if (width % 2 == 0 || height % 2 == 0) // If width / height are divisable by 2 with no remainder they are even.
        {
            throw new ArgumentException("The width & height must be odd numbers"); // Throw exception 
        }
        Tiles = new Tile[width, height]; // initalise Tiles array with width/height.
        Regions = new int[width, height]; // initalise Regions array with width/height.

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
                // Instantiate a tile Prefab at the current x,y pos in the grid
                GameObject tileObject = Instantiate(TilePreFab, new Vector3(x, y, 0), Quaternion.identity);
                Tile tilePlaced = tileObject.GetComponent<Tile>(); // Get the tileprefabs Tile script
                Tiles[x, y] = tilePlaced; // add the tile script the the Tiles array at the current pos
                tilePlaced.SetType(tile); // Set the TileType to the type passed to the method
            }
        }
    }

    /// <summary>
    /// A method to populate the grid with various randomly sized rooms at random positions
    /// </summary>
    /// <param name="roomTile"> The tileType to set the rooms tiles to </param>
    void PopulateGrid_WithRooms(TileType roomTile)
    {
        for (int i = 0; i < RoomGenAttempts; i++)
        {
            #region Randomise Room Size / Position
            // min 7 max 9
            int size = Random.Range(3, 5) * 2 + 1; // Random range between 3,5 Multiplied by 2 + 1 to keep rooms odd
            int width = size; // declare width int set to size
            int height = size / 2; // declare height int set to half size

            // Randomly int the width 50% chance
            if (Random.Range(0, 2) == 0) width *= 2; // If is 0, double width

            // Randomise x,y position in grid for room to be placed
            // Random number between 2 and the width of the Levels grid - width (room width) - Do same for height
            int x = Random.Range(2, GridWidth - width);
            int y = Random.Range(2, GridHeight - height);
            #endregion


            //Debug log for each room positon and width/height
            Debug.Log($"Room being added @ - X: {x} Y: {y} Size: Width: {width} Height: {height}");
            
            StartRegion(); // Start a new region (Increment current region)

            #region Add the room to the grid
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
    /// Increments the CurrentRegion int (Each int is a marker of a region)
    /// </summary>
    private void StartRegion()
    {
        CurrentRegion++; // Increment Current region
    }
}