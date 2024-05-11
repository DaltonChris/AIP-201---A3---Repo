using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float detectionRadius = 0.5f; // Radius for detecting the player
    public LayerMask playerLayer; // Layer for the player

    private void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        foreach (var hit in hits)
        {
            StefanPlayer player = hit.GetComponent<StefanPlayer>();
            if (player != null)
            {
                // If player detected, deal damage to the player
                player.TakeDamage(1);
                break; // Exit loop after finding the player
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection radius in the editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
