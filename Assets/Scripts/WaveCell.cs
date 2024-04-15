using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WaveCell
{
    public bool hasCollapsed;
    public List<Tile> SupPositonList;
    public Tile tile;

    public int Entropy => SupPositonList.Count;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
