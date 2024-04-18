using UnityEngine;

// Enum for the Tile Type
public enum TileType
{
    Wall,
    Path
}

public class Tile : MonoBehaviour
{
    [SerializeField] Sprite WallSprite;
    [SerializeField] Sprite PathSprite;
    SpriteRenderer SpriteRenderer;
    public TileType Type { get; internal set; }

    private void Awake() // On awake get the SpriteRenderer
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Method to change the type of the tile and update sprite
    public void SetType(TileType newType)
    {
        Type = newType; // Set the Type to the passed sprite
        UpdateSprite(); // call Update Sprite
    }

    // Method to update the sprite based on type
    private void UpdateSprite()
    {
        switch (Type) // Switch statement for the Tile Type
        {
            case TileType.Wall: // Case wall Type, Set Wall Sprite
                SpriteRenderer.sprite = WallSprite;
                break;
            case TileType.Path: // Case Path Type, Set Wall Sprite
                SpriteRenderer.sprite = PathSprite;
                break;
            default:
                break;
        }
    }
}
