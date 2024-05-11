using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public float gravity = -30f;
    private bool grounded;
    private bool isFrozen = true;
    private float verticalVelocity;
    private int jumpCount = 0; // Track the number of jumps
    private int coinCounter = 0; // Coin counter

    public int maxHealth = 3; // Maximum health
    private int currentHealth;
    private bool isImmune = false; // Immunity flag
    private bool isOnLadder = false; // Ladder flag

    public Text coinCounterText; // Reference to the UI text element
    public Text healthText; // Reference to the health UI text element

    public Camera PlayCamera;
    public Camera GridCamera;
    private Camera currentCamera;

    public LayerMask groundLayer;  // Define which layer is considered as ground or obstacle

    private PolygonCollider2D playerCollider;

    void Start()
    {
        currentCamera = PlayCamera;
        SetCameraActive(PlayCamera);
        playerCollider = GetComponent<PolygonCollider2D>();
        currentHealth = maxHealth; // Initialize health
        UpdateCoinCounterText(); // Initialize the coin counter text
        UpdateHealthText(); // Initialize the health text
    }

    void Update()
    {
        if (!isFrozen) // Check if the player is not frozen
        {
            HandleMovement();
        }
        SwitchCameraOnInput();
        HandleUnfreeze();
        ResetLadderFlag(); // Reset the ladder flag every frame
    }

    private void HandleMovement()
    {
        // Horizontal Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(horizontalInput, 0f, 0f);

        // Apply horizontal movement with collision detection and resolution
        if (horizontalInput != 0)
        {
            move = HorizontalCollisionCheck(move);
        }
        transform.Translate(move * speed * Time.deltaTime);

        if (isOnLadder)
        {
            HandleLadderMovement();
        }
        else
        {
            HandleGravityAndJumping();
        }
    }

    private void HandleLadderMovement()
    {
        // Vertical movement on ladder
        if (Input.GetKey(KeyCode.W))
        {
            verticalVelocity = speed;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime; // Apply normal gravity when not pressing W
        }
        Vector3 verticalMove = new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        transform.Translate(verticalMove);
    }

    private void HandleGravityAndJumping()
    {
        // Apply gravity and jumping
        verticalVelocity += gravity * Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded || jumpCount < 1) // Allow jumping if grounded or if player hasn't double jumped yet
            {
                verticalVelocity = jumpForce;
                grounded = false;
                jumpCount++;
            }
        }
        Vector3 verticalMove = new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        verticalMove = VerticalCollisionCheck(verticalMove);
        transform.Translate(verticalMove);

        UpdateGroundedStatus();
    }

    private Vector3 HorizontalCollisionCheck(Vector3 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, playerCollider.bounds.size, 0f, new Vector2(move.x, 0), Mathf.Abs(move.x * Time.deltaTime), groundLayer);
        if (hit.collider != null)
        {
            move.x = 0;  // Stop horizontal movement if collision is detected

            // Push player out of the wall smoothly
            float pushDistance = 0.05f; // Small increment to push the player out
            if (hit.normal.x > 0)
            {
                // Push player to the right
                transform.position += new Vector3(pushDistance, 0, 0);
            }
            else if (hit.normal.x < 0)
            {
                // Push player to the left
                transform.position += new Vector3(-pushDistance, 0, 0);
            }
        }
        return move;
    }

    private Vector3 VerticalCollisionCheck(Vector3 move)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, playerCollider.bounds.size, 0f, new Vector2(0, move.y), Mathf.Abs(move.y), groundLayer);
        if (hit.collider != null)
        {
            verticalVelocity = 0;  // Stop vertical movement if collision is detected
            move.y = 0;
            grounded = move.y > 0 ? false : true;
        }
        return move;
    }

    private void UpdateGroundedStatus()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, groundLayer);
        grounded = hit.collider != null;
        if (grounded)
        {
            jumpCount = 0; // Reset jump count when grounded
        }
    }

    private void HandleUnfreeze()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isFrozen)
        {
            StartCoroutine(UnfreezeAfterDelay(3));
        }
    }

    IEnumerator UnfreezeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFrozen = false;
    }

    private void SwitchCameraOnInput()
    {
        // Camera switch
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SwitchCamera();
        }
    }

    void SwitchCamera()
    {
        if (currentCamera == PlayCamera)
        {
            SetCameraActive(GridCamera);
            currentCamera = GridCamera;
        }
        else
        {
            SetCameraActive(PlayCamera);
            currentCamera = PlayCamera;
        }
    }

    void SetCameraActive(Camera camera)
    {
        PlayCamera.enabled = false;
        GridCamera.enabled = false;

        camera.enabled = true;
    }

    public void IncrementCoinCounter()
    {
        coinCounter++;
        UpdateCoinCounterText();
    }

    public void IncrementCoinCounterByAmount(int amount)
    {
        coinCounter += amount;
        UpdateCoinCounterText();
    }

    private void UpdateCoinCounterText()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = "Coins: " + coinCounter;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isImmune)
        {
            currentHealth -= damage;
            UpdateHealthText();

            if (currentHealth <= 0)
            {
                HandleDeath();
            }
            else
            {
                StartCoroutine(ImmunityFrame(2)); // 2 seconds immunity
            }
        }
    }

    private IEnumerator ImmunityFrame(float duration)
    {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        isImmune = false;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }

    private void HandleDeath() // Add player death animations or game over screen here
    {       
        Destroy(gameObject);
    }

    public void SetOnLadder(bool onLadder)
    {
        isOnLadder = onLadder;
    }

    private void ResetLadderFlag()
    {
        isOnLadder = false;
    }
}