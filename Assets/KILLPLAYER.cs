using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KILLPLAYER : MonoBehaviour
{
    //script damages the player and lets the Health script handle death/scene transition
    //OR can instantly load a different scene if loadSceneInstead is enabled
    
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 3f; 
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private bool instantKill = false; // Set to true to kill player instantly
    
    [Header("Optional: Scene Transition (bypasses Health system)")]
    [SerializeField] private bool loadSceneInstead = false; 
    [SerializeField] private string sceneToLoad = "Tent"; 

    private bool canDamage = true;

    //loads scene instead of damaging player if enables
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canDamage)
        {
            if (loadSceneInstead)
            {
                LoadScene();
            }
            else
            {
                // makes it so health script handles the damage and death bits
                var playerHealth = other.GetComponent<Health>();

                // checks where player health script is located
                if (playerHealth == null)
                {
                    playerHealth = other.GetComponentInParent<Health>();
                }
                
                if (playerHealth == null)
                {
                    playerHealth = other.GetComponentInChildren<Health>();
                }

                if (playerHealth != null)
                {
                    // Only damage if player is alive 
                    if (!playerHealth.Dead && playerHealth.currentHealth > 0)
                    {
                        float actualDamage = instantKill ? playerHealth.currentHealth : damageAmount;
                        playerHealth.TakeDamage(actualDamage);
                        Debug.Log($"Player took {actualDamage} damage from kill plane! Current HP: {playerHealth.currentHealth}");
                        
                        StartCoroutine(DamageCooldown());
                    }
                    else
                    {
                        Debug.Log("Player is already dead, not applying damage.");
                    }
                }
                else
                {
                    Debug.LogError($"Player does not have a Health component! GameObject: {other.gameObject.name}");
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // damages player over time if they stay in the kill plane 
        if (other.CompareTag("Player") && canDamage && !loadSceneInstead)
        {
            var playerHealth = other.GetComponent<Health>();
            
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<Health>();
            }
            
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInChildren<Health>();
            }

            if (playerHealth != null)
            {
                // Only damage if player is alive
                if (!playerHealth.Dead && playerHealth.currentHealth > 0)
                {
                    float actualDamage = instantKill ? playerHealth.currentHealth : damageAmount;
                    playerHealth.TakeDamage(actualDamage);
                    StartCoroutine(DamageCooldown());
                }
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Loading scene directly (bypassing Health system): {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Cannot load scene.");
        }
    }
}
