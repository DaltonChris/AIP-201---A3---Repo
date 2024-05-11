using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
public class FindTarget_EnemyState : MonoBehaviour
{
    public float enemy_Speed;
    public float line_of_site;
    private Transform playerObject;

    public GameObject Start_Point;
    public GameObject End_Point;
    private Rigidbody2D rigid_body;
    private Animator Enemy_animation;
    private Transform current_Point;

    private SpriteRenderer spriteRenderer;

    private EnemyState currentState;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the attached sprite renderer

        rigid_body = GetComponent<Rigidbody2D>();
        Enemy_animation = GetComponent<Animator>();
        current_Point = End_Point.transform;
        Enemy_animation.SetBool("IsRunning", true);
    }

    public enum EnemyState
    {
        Patrol,
        Chase
    }

    private void Update()
    {
        UpdateState();
        // Update behavior based on the current state
        switch (currentState)
        {
            case EnemyState.Patrol:
                // Implement patrol behavior
                Patrol();
                break;
            case EnemyState.Chase:
                // Implement chase behavior
                Chase();
                break;

        }
    }

    void UpdateState()
    {

        playerObject = GameObject.FindGameObjectWithTag("Player").transform;

        //update player distance
        float distanceFromPlayer = Vector2.Distance(playerObject.position, transform.position);

        if (distanceFromPlayer > line_of_site) // Patrol
        {
            currentState = EnemyState.Patrol;
        }
        
        else if (distanceFromPlayer < line_of_site)
        {
            currentState = EnemyState.Chase;
        }
    }

    //This is for when the enemy haven't seen the player yet
    void Patrol()
    {
        Vector2 moveDirection = current_Point.position - transform.position;
        rigid_body.velocity = new Vector2(moveDirection.x * enemy_Speed, rigid_body.velocity.y);

        if (Vector2.Distance(transform.position, current_Point.position) < 0.5f && current_Point == End_Point.transform)
        {
            current_Point = Start_Point.transform;
            Enemy_flip();
        }

        if (Vector2.Distance(transform.position, current_Point.position) < 0.5f && current_Point == Start_Point.transform)
        {
            current_Point = End_Point.transform;
            Enemy_flip();
        }

        Debug.Log("Enemy is patroling");
    }

    //makes the enemy face the player
    private void Enemy_flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
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

        Debug.Log("Enemy is chasing the player");

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, line_of_site);
    }
}

