using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public LineRenderer trajectoryLine;
    public GameObject landingIndicator;

    [Header("Prediction Settings")]
    public int simulationSteps = 60;
    public float timeStep = 0.05f;
    public float gravity = 9.8f;
    public LayerMask collisionMask;

    private Vector3[] trajectoryPoints;
    private GameObject ghostFruit;
    private Vector2 predictedLandingPosition;
    private bool isSimulating = false;

    private void Start()
    {
        // Initialize trajectory line
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
            trajectoryLine.startWidth = 0.1f;
            trajectoryLine.endWidth = 0.1f;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = new Color(1f, 1f, 1f, 0.3f);
            trajectoryLine.endColor = new Color(1f, 1f, 1f, 0f);
        }

        // Initialize trajectory points array
        trajectoryPoints = new Vector3[simulationSteps];

        // Create landing indicator if not provided
        if (landingIndicator == null)
        {
            landingIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landingIndicator.transform.localScale = new Vector3(1f, 0.1f, 1f);
            landingIndicator.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.5f);
            landingIndicator.GetComponent<Collider>().enabled = false;
        }

        // Initially hide trajectory and landing indicator
        trajectoryLine.enabled = false;
        landingIndicator.SetActive(false);
    }

    public void CreateGhostFruit(GameObject fruitPrefab, Vector3 position)
    {
        // Create ghost fruit for prediction if not already created
        if (ghostFruit == null)
        {
            ghostFruit = Instantiate(fruitPrefab, position, Quaternion.identity);
            ghostFruit.name = "GhostFruit";

            // Make ghost fruit transparent
            Renderer[] renderers = ghostFruit.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material material = renderer.material;
                Color color = material.color;
                color.a = 0.3f;
                material.color = color;
            }

            // Disable all colliders
            Collider2D[] colliders = ghostFruit.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                collider.enabled = false;
            }

            // Disable any scripts that might affect simulation
            MonoBehaviour[] scripts = ghostFruit.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
        }
        else
        {
            // Update ghost fruit position
            ghostFruit.transform.position = position;
        }

        // Make ghost fruit visible
        ghostFruit.SetActive(true);
    }

    public void UpdateTrajectory(Vector3 startPosition, float fruitRadius)
    {
        // Don't simulate if already simulating
        if (isSimulating)
            return;

        StartCoroutine(SimulateTrajectory(startPosition, fruitRadius));
    }

    private IEnumerator SimulateTrajectory(Vector3 startPosition, float fruitRadius)
    {
        isSimulating = true;

        // Create physics scene for simulation
        Physics2D.simulationMode = SimulationMode2D.Script;

        // Save current state of ghost fruit
        Vector3 originalPosition = ghostFruit.transform.position;

        // Calculate trajectory points
        Vector2 velocity = Vector2.zero; // Initial velocity
        Vector2 position = startPosition;

        bool foundLanding = false;
        int landingIndex = simulationSteps - 1;

        for (int i = 0; i < simulationSteps; i++)
        {
            // Move ghost fruit to current position in simulation
            ghostFruit.transform.position = position;

            // Store point in trajectory
            trajectoryPoints[i] = position;

            // Apply gravity
            velocity += Vector2.down * gravity * timeStep;
            position += velocity * timeStep;

            // Check for collisions
            Collider2D[] collisions = Physics2D.OverlapCircleAll(position, fruitRadius, collisionMask);

            bool hitSomething = false;
            foreach (Collider2D collision in collisions)
            {
                // Ignore ghost fruit collider
                if (collision.transform.IsChildOf(ghostFruit.transform))
                    continue;

                // Found a collision
                hitSomething = true;
                break;
            }

            if (hitSomething && !foundLanding)
            {
                foundLanding = true;
                landingIndex = i;
                predictedLandingPosition = position;
                break;
            }

            // Wait for physics simulation
            Physics2D.Simulate(timeStep);

            // Yield to avoid freezing the game
            if (i % 10 == 0)
                yield return null;
        }

        // Reset ghost fruit position
        ghostFruit.transform.position = originalPosition;

        // Update trajectory line
        trajectoryLine.positionCount = landingIndex + 1;
        for (int i = 0; i <= landingIndex; i++)
        {
            trajectoryLine.SetPosition(i, trajectoryPoints[i]);
        }

        // Show trajectory
        trajectoryLine.enabled = true;

        // Update landing indicator
        landingIndicator.transform.position = new Vector3(predictedLandingPosition.x, predictedLandingPosition.y, 0f);
        landingIndicator.transform.localScale = new Vector3(fruitRadius * 2f, 0.1f, fruitRadius * 2f);
        landingIndicator.SetActive(true);

        // Reset physics simulation mode
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        isSimulating = false;
    }

    public void HideTrajectory()
    {
        // Hide trajectory line
        trajectoryLine.enabled = false;

        // Hide landing indicator
        landingIndicator.SetActive(false);

        // Hide ghost fruit
        if (ghostFruit != null)
            ghostFruit.SetActive(false);

        // Stop any running simulations
        StopAllCoroutines();
        isSimulating = false;

        // Reset physics simulation mode
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    private void OnDestroy()
    {
        // Ensure physics simulation mode is reset
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        // Clean up ghost fruit
        if (ghostFruit != null)
            Destroy(ghostFruit);
    }
}
