using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int GridWidth;
    [SerializeField] int GridHeight;
    [SerializeField] int RoomGenAttempts = 300; // How many times we try to generate a room
    [SerializeField] GameObject TilePreFab; // Prefab for the tile obj
    [SerializeField] Tile[,] Tiles;

    int[,] Regions;
    readonly float LevelGenDelay = 1f;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // If spacebar is pressed
        {
            StopAllCoroutines(); // Stop all coroutines
            InitGrid(GridWidth, GridHeight); // Initalise the grid (Start generating)
        }
    }

    // Coroutine to Go through the Level Generation Steps was small pauses between steps
    IEnumerator GenerateLevel()
    {
        yield return new WaitForSeconds(LevelGenDelay);
        PopulateGrid(TileType.Wall); // Populate grid with Tiles of wall type as default
        yield return new WaitForSeconds(LevelGenDelay);
        PopulateGrid_WithRooms(); // Populate grid with rooms
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
    void PopulateGrid_WithRooms()
    {

    }
}