using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitBehavior : MonoBehaviour
{
    [Header("Fruit Properties")]
    public int fruitLevel;
    public float mergeCheckDelay = 0.1f;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Collider2D fruitCollider;
    public Rigidbody2D rb;

    private GameManager gameManager;
    private bool isCheckingMerge = false;
    private bool canMerge = true;
    private float mergeRadius;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (fruitCollider == null)
            fruitCollider = GetComponent<Collider2D>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Calculate merge radius based on collider size
        if (fruitCollider is CircleCollider2D)
        {
            mergeRadius = ((CircleCollider2D)fruitCollider).radius * transform.localScale.x;
        }
        else
        {
            // Approximate for other collider types
            mergeRadius = Mathf.Max(fruitCollider.bounds.extents.x, fruitCollider.bounds.extents.y);
        }
    }

    private void Update()
    {
        // Check for game over condition
        if (gameManager != null && !rb.isKinematic)
        {
            gameManager.CheckGameOver(transform.position);

            // Check for merge opportunity if not already checking and fruit is relatively stable
            if (!isCheckingMerge && rb.velocity.magnitude < 0.5f)
            {
                StartCoroutine(CheckForMerge());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we collided with another fruit
        FruitBehavior otherFruit = collision.gameObject.GetComponent<FruitBehavior>();

        if (otherFruit != null && canMerge && otherFruit.canMerge)
        {
            // If same fruit level, merge them
            if (otherFruit.fruitLevel == fruitLevel && gameManager != null)
            {
                // Prevent multiple merges
                canMerge = false;
                otherFruit.canMerge = false;

                // Merge the fruits
                gameManager.MergeFruits(this, otherFruit);
            }
        }
    }

    private IEnumerator CheckForMerge()
    {
        isCheckingMerge = true;

        // Wait for a short delay to ensure fruit has settled
        yield return new WaitForSeconds(mergeCheckDelay);

        // Only check if we're still able to merge
        if (canMerge)
        {
            // Find all fruits of the same level nearby
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, mergeRadius * 2.1f);

            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    FruitBehavior otherFruit = collider.GetComponent<FruitBehavior>();

                    if (otherFruit != null && otherFruit.fruitLevel == fruitLevel && otherFruit.canMerge)
                    {
                        // Calculate distance between fruits
                        float distance = Vector2.Distance(transform.position, otherFruit.transform.position);

                        // If fruits are close enough and same level, merge them
                        if (distance < mergeRadius * 1.8f && gameManager != null)
                        {
                            // Prevent multiple merges
                            canMerge = false;
                            otherFruit.canMerge = false;

                            // Merge the fruits
                            gameManager.MergeFruits(this, otherFruit);
                            break;
                        }
                    }
                }
            }
        }

        isCheckingMerge = false;
    }

    public void SetFruitLevel(int level)
    {
        fruitLevel = level;
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }
}
