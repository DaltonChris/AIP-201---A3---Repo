using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linear_Path : MonoBehaviour
{
    public GameObject Start_Point;
    public GameObject End_Point;
    private Rigidbody2D rigid_body;
    private Animator Enemy_animation;
    private Transform current_Point;
    public float Bandit1_Speed;

    // Start is called before the first frame update
    void Start()
    {
        rigid_body = GetComponent<Rigidbody2D>();
        Enemy_animation = GetComponent<Animator>();
        current_Point = End_Point.transform;
        Enemy_animation.SetBool("IsRunning", true);
    }

    // Update is called once per frame
    void Update()
    {
        Enemy_Bound(); // Makes sure Enemy doesn't move pass the Start and End point
        Enemy_Movement();
    }

    //computes enemy movement
    void Enemy_Movement()
    {
        Vector2 moveDirection = current_Point.position - transform.position;
        rigid_body.velocity = new Vector2(moveDirection.x * Bandit1_Speed, rigid_body.velocity.y);
    }

    //This is to compute the enemy bounds
    void Enemy_Bound()
    {
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
    }

    //This so the can face the direction their going to
    private void Enemy_flip() 
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    //This is so we can see the starting points width
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Start_Point.transform.position, 0.5f);
        Gizmos.DrawWireSphere(End_Point.transform.position, 0.5f);
        Gizmos.DrawLine(Start_Point.transform.position, End_Point.transform.position);
    }
}
    
