using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Must collapse waves and make good game things.
// It'll be easy surely
public class WavFuncCollapse : MonoBehaviour
{
    [SerializeField] Tile[] tiles;
    [SerializeField] int gridHeight;
    [SerializeField] int gridWidth;

    Dictionary<Vector2Int, WaveCell> grid = new(); // The grid as a dict
    
    // Start is called before the first frame update
    void Start()
    {
        // Initalise the Grid Dict.
        InitGrid();


        // Generate the grid 
        do {  }
        while (!IsGridFullyCollapsed());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool IsGridFullyCollapsed()
    {
        return grid.Values.All(WaveCell => WaveCell.hasCollapsed);
    }

    Vector2Int GetCellWithMinEntropy()
    {
        Vector2Int minEntropyCellVectorPos = Vector2Int.zero;


        return minEntropyCellVectorPos;
    }

    void InitGrid()
    {
        grid.Clear(); // make sure the grid dict is empty

        // 2 deep for loops for x/y of the Vector 2 int in the dict to be the width /height
        // in cells of the grid

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int X_Y_Tile_Pos = new(x, y); // pos to add to grid
                WaveCell waveCell = new(tiles); // Fresh WaveCell 
                grid.Add(X_Y_Tile_Pos, waveCell); // Add to Grid dict
            }
        }
    }
}
