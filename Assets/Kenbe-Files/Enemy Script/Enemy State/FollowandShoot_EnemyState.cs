using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowandShoot_Path : MonoBehaviour
{
    public float enemy_Speed;
    public float line_of_site;
    private Transform playerObject;

    public float bullet_range;
    public GameObject bullet;
    public GameObject bullet_Parent;
    public float numberOfBullets = 1f;
    private float fireInBetween = 1f;

    private SpriteRenderer spriteRenderer;

    private EnemyState currentState;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the attached sprite renderer
    }

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
    }

    private void Update()
    {
        UpdateState();
        // Update behavior based on the current state
        switch (currentState)
        {
            case EnemyState.Idle:
                // Implement idle behavior
                break;
            case EnemyState.Chase:
                // Implement chase behavior
                Chase();
                break;

            case EnemyState.Attack:
                // Implement attack behavior
                Attack();
                break;
        }
    }

    void UpdateState()
    {

        playerObject = GameObject.FindGameObjectWithTag("Player").transform;

        //update player distance
        float distanceFromPlayer = Vector2.Distance(playerObject.position, transform.position);

        if (distanceFromPlayer < line_of_site && distanceFromPlayer > bullet_range) // Patrol
        {
            currentState = EnemyState.Idle;
        }
        else if (distanceFromPlayer <= bullet_range && fireInBetween < Time.time) //shoot
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceFromPlayer < line_of_site)
        {
            currentState = EnemyState.Chase;
        }
    }
    void Chase()
    {

        // X axis positions player/enemy
        float playerX = playerObject.position.x;
        float currentPositionX = transform.position.x;

        //check if player is to the right
        if (playerX > currentPositionX)
        {
            // we are moving the the right
            spriteRenderer.flipX = false;// Flip the sprite renderer to the left
        }
        //check if player is to the left
        else if (playerX < currentPositionX)
        {
            // we are moving to the left
            spriteRenderer.flipX = true;// Flip the sprite renderer to the right
        }

        // Move towards the player
        transform.position = Vector2.MoveTowards(this.transform.position, playerObject.position, enemy_Speed * Time.deltaTime);
    }

    void Attack()
    {
        Instantiate(bullet, bullet_Parent.transform.position, Quaternion.identity);
        fireInBetween = Time.time + numberOfBullets;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, line_of_site);
        Gizmos.DrawWireSphere(transform.position, bullet_range);
    }
}
