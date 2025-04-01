using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryDetector : MonoBehaviour
{
    public GameManager gameManager;
    public float checkDelay = 1.5f;
    public float topBoundaryY = 14f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if a fruit entered the top boundary zone
        FruitBehavior fruit = collision.GetComponent<FruitBehavior>();
        if (fruit != null)
        {
            // Start a coroutine to check if the fruit stays in the boundary
            StartCoroutine(CheckFruitPosition(fruit));
        }
    }

    private IEnumerator CheckFruitPosition(FruitBehavior fruit)
    {
        // Wait for the check delay
        yield return new WaitForSeconds(checkDelay);

        // Check if the fruit still exists and is still above the boundary
        if (fruit != null && fruit.transform.position.y > topBoundaryY)
        {
            // Notify the game manager
            if (gameManager != null)
            {
                gameManager.CheckGameOver(fruit.transform.position);
            }
        }
    }
}
