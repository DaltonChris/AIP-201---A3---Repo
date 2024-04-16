using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // Generate the grid 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitGrid()
    {
        grid.Clear(); // make sure the grid dict is empty
    }
}
