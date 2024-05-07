using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SquarePath : MonoBehaviour
{
    public float enemy_speed;
    public GameObject[] pathPoints;

    int next_pathpoint = 1;
    float distanceToNextPoint;
    float minDistance = 0.2f;

    void Update()
    {
        Enemy_Movement();
    }

    public void Enemy_Movement()
    {
        distanceToNextPoint = Vector2.Distance(transform.position, pathPoints[next_pathpoint].transform.position);

        if (distanceToNextPoint > minDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, pathPoints[next_pathpoint].transform.position, enemy_speed * Time.deltaTime);
        }
        else
        {
            TurnAround();
        }
    }

    public void TurnAround()
    {
        transform.position = pathPoints[next_pathpoint].transform.position; // Snap to the next point
        transform.rotation = Quaternion.LookRotation(Vector3.forward, pathPoints[next_pathpoint].transform.position - transform.position);
        chooseNextpoint();
    }

    public void chooseNextpoint()
    {
        next_pathpoint++;
        if (next_pathpoint == pathPoints.Length)
        {
            next_pathpoint = 0;
        }

        Debug.Log("Next Path Point: " + next_pathpoint);
    }
}
