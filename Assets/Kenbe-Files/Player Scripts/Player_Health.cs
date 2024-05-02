using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int health;
    public int max_Health = 10;
    // Start is called before the first frame update
    void Start()
    {
        health = max_Health;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        //If player is taking damage I want it to sprite to blinking red

        if (health <= 0)
        {
            //Play death animation
            Debug.Log("Player has died");
        }
    }
}
