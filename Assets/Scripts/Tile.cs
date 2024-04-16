using UnityEngine;

[CreateAssetMenu(fileName = "WFC Tile", menuName = "WaveFuncCollapse/Tile")]
public class Tile : ScriptableObject
{
    [Header("GameObj to hold sprite's / Map pieces etc.")]
    public GameObject gameObject;

    [Header("WaveFunc-Collapse - Algorithm tile data [Adjacent-compatabilty]")]
    public int left;
    public int right;
    public int top;
    public int bottom;
}
