using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public GameObject Target;
    public float speed;
    public Rigidbody2D bulletprefab;

    // Start is called before the first frame update
    void Start()
    {
        bulletprefab = GetComponent<Rigidbody2D>();
        Target = GameObject.FindGameObjectWithTag("Player");

        //computes bullet direction
        Vector2 bulletDirection = (Target.transform.position - transform.position).normalized * speed;
        bulletprefab.velocity = new Vector2(bulletDirection.x, bulletDirection.y);
        Destroy(this.gameObject, 2);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
