using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Followplayer_Path : MonoBehaviour
{
    public float enemy_Speed;
    public float line_of_site;
    private Transform playerObject;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(playerObject.position, transform.position);
        if (distanceFromPlayer < line_of_site)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, playerObject.position, enemy_Speed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, line_of_site);
    }
}
