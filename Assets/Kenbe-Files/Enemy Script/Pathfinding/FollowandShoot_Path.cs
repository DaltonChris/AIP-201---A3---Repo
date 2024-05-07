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

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(playerObject.position, transform.position);
        if (distanceFromPlayer < line_of_site && distanceFromPlayer > bullet_range)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, playerObject.position, enemy_Speed * Time.deltaTime);
        }
        else if (distanceFromPlayer <= bullet_range && fireInBetween < Time.time)
        {
            Instantiate(bullet,bullet_Parent.transform.position, Quaternion.identity);
            fireInBetween = Time.time + numberOfBullets;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, line_of_site);
        Gizmos.DrawWireSphere(transform.position, bullet_range);
    }
}
