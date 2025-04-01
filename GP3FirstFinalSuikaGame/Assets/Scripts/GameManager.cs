using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float dropZoneWidth = 10f;
    public float dropZoneHeight = 15f;
    public float dropHeight = 14f;
    public int scorePerMerge = 100;
    public int maxFruitLevel = 11; // Watermelon is level 11

    [Header("References")]
    public Transform dropZone;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI gameOverText;
    public GameObject nextFruitPreview;
    public GameObject gameOverPanel;
    public TrajectoryPredictor trajectoryPredictor; // Added trajectory predictor reference

    [Header("Fruit Prefabs")]
    public GameObject[] fruitPrefabs; // Array of fruit prefabs ordered by size

    private GameObject currentFruit;
    private GameObject nextFruit;
    private int nextFruitLevel;
    private int score = 0;
    private int highScore = 0;
    private bool isGameOver = false;
    private Vector3 dropPosition;
    private bool isDragging = false; // Track if player is dragging the fruit
    private float currentFruitRadius; // Track current fruit radius for trajectory prediction

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
        gameOverPanel.SetActive(false);

        // Initialize trajectory predictor if not set
        if (trajectoryPredictor == null)
        {
            trajectoryPredictor = GetComponent<TrajectoryPredictor>();
            if (trajectoryPredictor == null)
            {
                trajectoryPredictor = gameObject.AddComponent<TrajectoryPredictor>();
                trajectoryPredictor.gameManager = this;
            }
        }

        ResetGame();
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
            }
            return;
        }

        // Update drop position based on mouse position
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

        // Clamp to drop zone boundaries
        float halfWidth = dropZoneWidth / 2f;
        worldPos.x = Mathf.Clamp(worldPos.x, -halfWidth + 1f, halfWidth - 1f);
        worldPos.y = dropHeight;
        worldPos.z = 0f;

        dropPosition = worldPos;

        if (currentFruit != null)
        {
            currentFruit.transform.position = dropPosition;

            // Track if we're starting to drag the fruit
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;

                // Create ghost fruit for trajectory prediction
                if (trajectoryPredictor != null)
                {
                    trajectoryPredictor.CreateGhostFruit(fruitPrefabs[nextFruitLevel], dropPosition);
                }
            }

            // Update trajectory while dragging
            if (isDragging)
            {
                if (trajectoryPredictor != null)
                {
                    trajectoryPredictor.UpdateTrajectory(dropPosition, currentFruitRadius);
                }

                // Drop fruit on mouse button release
                if (Input.GetMouseButtonUp(0))
                {
                    DropFruit();
                    isDragging = false;

                    // Hide trajectory on drop
                    if (trajectoryPredictor != null)
                    {
                        trajectoryPredictor.HideTrajectory();
                    }
                }
            }
        }
    }

    private void ResetGame()
    {
        // Clear existing fruits
        FruitBehavior[] existingFruits = FindObjectsOfType<FruitBehavior>();
        foreach (FruitBehavior fruit in existingFruits)
        {
            Destroy(fruit.gameObject);
        }

        score = 0;
        UpdateScoreText();
        isGameOver = false;
        gameOverPanel.SetActive(false);
        isDragging = false;

        // Hide trajectory
        if (trajectoryPredictor != null)
        {
            trajectoryPredictor.HideTrajectory();
        }

        // Spawn initial fruit
        SpawnNextFruit();
        CreateNewFruit();
    }

    private void SpawnNextFruit()
    {
        // Determine next fruit level (with weighted probability)
        float randomValue = Random.value;

        if (randomValue < 0.5f)
            nextFruitLevel = 0; // Cherry (50% chance)
        else if (randomValue < 0.8f)
            nextFruitLevel = 1; // Grape (30% chance)
        else if (randomValue < 0.95f)
            nextFruitLevel = 2; // Orange (15% chance)
        else
            nextFruitLevel = 3; // Lemon (5% chance)

        // Update preview
        if (nextFruit != null)
            Destroy(nextFruit);

        nextFruit = Instantiate(fruitPrefabs[nextFruitLevel], nextFruitPreview.transform);
        nextFruit.transform.localPosition = Vector3.zero;

        // Disable physics on preview
        Rigidbody2D rb = nextFruit.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.isKinematic = true;

        // Disable colliders on preview
        Collider2D[] colliders = nextFruit.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
            collider.enabled = false;

        // Set preview scale
        nextFruit.transform.localScale = Vector3.one * 0.5f;
    }

    private void CreateNewFruit()
    {
        currentFruit = Instantiate(fruitPrefabs[nextFruitLevel], dropPosition, Quaternion.identity);

        // Make it kinematic until dropped
        Rigidbody2D rb = currentFruit.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        // Disable collision detection until dropped
        Collider2D collider = currentFruit.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;

            // Get fruit radius for trajectory prediction
            if (collider is CircleCollider2D)
            {
                currentFruitRadius = ((CircleCollider2D)collider).radius * currentFruit.transform.localScale.x;
            }
            else
            {
                // Approximate for other collider types
                currentFruitRadius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y);
            }
        }

        // Get next fruit ready
        SpawnNextFruit();
    }

    private void DropFruit()
    {
        if (currentFruit == null) return;

        // Enable physics
        Rigidbody2D rb = currentFruit.GetComponent<Rigidbody2D>();
        rb.isKinematic = false;

        // Enable collision
        Collider2D collider = currentFruit.GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = false;

        // Set fruit variables
        FruitBehavior fruitBehavior = currentFruit.GetComponent<FruitBehavior>();
        fruitBehavior.SetFruitLevel(nextFruitLevel);
        fruitBehavior.SetGameManager(this);

        // Play drop sound if available
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayDropSound();
        }

        // Wait a bit before spawning next fruit
        StartCoroutine(WaitAndSpawnNext());

        // Release control of current fruit
        currentFruit = null;
    }

    private IEnumerator WaitAndSpawnNext()
    {
        yield return new WaitForSeconds(0.5f);
        CreateNewFruit();
    }

    public void MergeFruits(FruitBehavior fruit1, FruitBehavior fruit2)
    {
        if (fruit1.fruitLevel != fruit2.fruitLevel) return;
        if (fruit1.fruitLevel >= maxFruitLevel) return;

        // Calculate midpoint between the two fruits
        Vector3 mergePosition = (fruit1.transform.position + fruit2.transform.position) / 2f;

        // Create new fruit at next level
        GameObject newFruit = Instantiate(fruitPrefabs[fruit1.fruitLevel + 1], mergePosition, Quaternion.identity);
        FruitBehavior newFruitBehavior = newFruit.GetComponent<FruitBehavior>();
        newFruitBehavior.SetFruitLevel(fruit1.fruitLevel + 1);
        newFruitBehavior.SetGameManager(this);

        // Destroy old fruits
        Destroy(fruit1.gameObject);
        Destroy(fruit2.gameObject);

        // Add score
        AddScore(scorePerMerge * (fruit1.fruitLevel + 1));

        // Play merge sound if available
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayMergeSound();
        }

        // Play merge effect if available
        EffectsManager effectsManager = FindObjectOfType<EffectsManager>();
        if (effectsManager != null)
        {
            effectsManager.PlayMergeEffect(mergePosition, fruit1.fruitLevel);
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void CheckGameOver(Vector3 fruitPosition)
    {
        // Check if any fruit is above the top boundary
        if (fruitPosition.y > dropZoneHeight - 1f && !isGameOver)
        {
            // Give a grace period to see if it falls back down
            StartCoroutine(GameOverCheck());
        }
    }

    private IEnumerator GameOverCheck()
    {
        // Wait to see if the fruit falls back down
        yield return new WaitForSeconds(1.5f);

        // Check if any fruit is still above the boundary
        FruitBehavior[] fruits = FindObjectsOfType<FruitBehavior>();
        foreach (FruitBehavior fruit in fruits)
        {
            if (fruit.transform.position.y > dropZoneHeight - 1f)
            {
                GameOver();
                break;
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        gameOverText.text = "Game Over!\nScore: " + score + "\nPress R to Restart";

        // Play game over sound if available
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayGameOverSound();
        }

        // Play game over effect if available
        EffectsManager effectsManager = FindObjectOfType<EffectsManager>();
        if (effectsManager != null)
        {
            effectsManager.PlayGameOverEffect();
        }

        if (currentFruit != null)
        {
            Destroy(currentFruit);
            currentFruit = null;
        }

        // Hide trajectory
        if (trajectoryPredictor != null)
        {
            trajectoryPredictor.HideTrajectory();
        }
    }

    // Add method to get fruit radius for specific fruit level (used by trajectory predictor)
    public float GetFruitRadius(int fruitLevel)
    {
        if (fruitLevel < 0 || fruitLevel >= fruitPrefabs.Length)
            return 0.5f;

        GameObject fruitPrefab = fruitPrefabs[fruitLevel];
        Collider2D collider = fruitPrefab.GetComponent<Collider2D>();

        if (collider is CircleCollider2D)
        {
            return ((CircleCollider2D)collider).radius * fruitPrefab.transform.localScale.x;
        }
        else
        {
            // Approximate for other collider types
            return Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y);
        }
    }
}
