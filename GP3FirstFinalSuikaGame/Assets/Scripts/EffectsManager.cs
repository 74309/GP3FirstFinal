using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [System.Serializable]
    public class EffectGroup
    {
        public string name;
        public GameObject effectPrefab;
        public int poolSize = 5;
        [HideInInspector]
        public List<GameObject> pool;
    }

    public EffectGroup[] effects;
    public Transform effectsParent;

    private void Awake()
    {
        // Initialize effects pools
        foreach (EffectGroup group in effects)
        {
            group.pool = new List<GameObject>();

            for (int i = 0; i < group.poolSize; i++)
            {
                GameObject effect = Instantiate(group.effectPrefab, effectsParent);
                effect.SetActive(false);
                group.pool.Add(effect);
            }
        }
    }

    public void PlayEffect(string effectName, Vector3 position, float duration = 1f)
    {
        // Find the effect group by name
        EffectGroup group = System.Array.Find(effects, e => e.name == effectName);

        if (group == null)
        {
            Debug.LogWarning("Effect " + effectName + " not found!");
            return;
        }

        // Find an inactive effect in the pool
        GameObject effect = group.pool.Find(e => !e.activeSelf);

        if (effect == null)
        {
            // If all effects are active, create a new one
            effect = Instantiate(group.effectPrefab, effectsParent);
            group.pool.Add(effect);
        }

        // Set effect position and activate it
        effect.transform.position = position;
        effect.SetActive(true);

        // Deactivate after duration
        StartCoroutine(DeactivateAfterDuration(effect, duration));
    }

    private IEnumerator DeactivateAfterDuration(GameObject effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    public void PlayMergeEffect(Vector3 position, int fruitLevel)
    {
        // Play merge effect based on fruit level
        string effectName = "MergeEffect";

        // Different effects for different fruit levels
        if (fruitLevel >= 8) // Larger fruits
            effectName = "BigMergeEffect";
        else if (fruitLevel >= 4) // Medium fruits
            effectName = "MediumMergeEffect";

        PlayEffect(effectName, position);

        // Add score popup
        PlayScorePopup(position, fruitLevel);
    }

    public void PlayScorePopup(Vector3 position, int fruitLevel)
    {
        // Calculate score based on fruit level
        int score = 100 * (fruitLevel + 1);

        // Find a score popup effect
        GameObject scorePopup = GetEffectFromPool("ScorePopup");

        if (scorePopup != null)
        {
            // Set popup position
            scorePopup.transform.position = position + new Vector3(0, 0.5f, 0);

            // Set text
            TextMesh textMesh = scorePopup.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
                textMesh.text = "+" + score.ToString();

            // Activate popup
            scorePopup.SetActive(true);

            // Deactivate after duration
            StartCoroutine(DeactivateAfterDuration(scorePopup, 1f));
        }
    }

    private GameObject GetEffectFromPool(string effectName)
    {
        // Find the effect group by name
        EffectGroup group = System.Array.Find(effects, e => e.name == effectName);

        if (group == null)
            return null;

        // Find an inactive effect in the pool
        GameObject effect = group.pool.Find(e => !e.activeSelf);

        if (effect == null)
        {
            // If all effects are active, create a new one
            effect = Instantiate(group.effectPrefab, effectsParent);
            group.pool.Add(effect);
        }

        return effect;
    }

    public void PlayGameOverEffect()
    {
        // Play game over effect at the center of the screen
        PlayEffect("GameOverEffect", new Vector3(0, 0, 0), 2f);
    }

    public void PlayDropLineEffect(float xPosition, float duration = 0.5f)
    {
        // Play drop line effect to show where fruit will be dropped
        GameObject lineEffect = GetEffectFromPool("DropLineEffect");

        if (lineEffect != null)
        {
            // Set line position
            lineEffect.transform.position = new Vector3(xPosition, 0, 0);

            // Activate line
            lineEffect.SetActive(true);

            // Deactivate after duration
            StartCoroutine(DeactivateAfterDuration(lineEffect, duration));
        }
    }
}


