using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct WaveCell
{
    // Each title could be many types of tiles depending on what will randomly collapse around them altering the types of
    // Tiles that could be valid, entropy will slowly lower during the further collapse of surrounding tiles. I think reee
    public int Entropy => SupPositonList.Count; // The total possible states A-TILE within the grid? i think maybe

    // List of all the tile's possible states that the tile could or could not be 
    public List<Tile> SupPositonList;

    // A tile (Atleast this is striaght forward lmao)
    public Tile tile;

    // Boolean flag to be flagged if the tile (Cell) has been collapsed. ie. the type state has be decided.
    public bool hasCollapsed;

    public WaveCell(Tile[] DEFAULT_supPostion)
    {
        tile = null;
        hasCollapsed = false;
        SupPositonList = DEFAULT_supPostion.ToList();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
