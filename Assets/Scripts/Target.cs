using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private Rigidbody targetRb;               // Rigidbody used for physics movement
    private GameManager gameManager;          // Reference to the GameManager

    private float minSpeed = 14;              // Minimum upward force
    private float maxSpeed = 15;              // Maximum upward force
    private float maxTorque = 10;             // Rotation randomness
    private float xRange = 4;                 // Horizontal spawn range
    private float ySpawnPos = -6;            // Spawn height below screen
    public ParticleSystem explosionParticle; // Particle effect played upon clicking
    public int pointValue;                   // Points awarded for hitting this target

    void Start()
    {
        // Get rigidbody and manager references
        targetRb = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        
        // Apply random upward force
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        
        // Apply random torque to spin target
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
        
        // Spawn at a random horizontal position at bottom of screen
        transform.position = RandomSpawnPos();
    }
    
    // Called when player clicks on the target.
    // Only works if game is active.
    private void OnMouseDown()
    {
        if (gameManager.IsGameActive)
        {
            Destroy(gameObject);                  // Destroy target immediately
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
            gameManager.UpdateScore(pointValue);  // Add points
        }
    }
    
    // Called when the target enters a trigger (usually the bottom boundary).
    // If the target is missed (and it isn't a bomb), player loses.
    private void OnTriggerEnter(Collider other)
    {
        // Always destroy target when it hits the trigger
        Destroy(gameObject);                      
        
        // If it's NOT a bomb AND game is active AND player has not already won → Game Over
        if (!gameObject.CompareTag("Bomb") && gameManager.IsGameActive && !gameManager.IsGameWon)
        {
            gameManager.GameOver();
        }
    }
    
    // Returns a random upward force.
    Vector3 RandomForce()
    {
        return Vector3.up * Random.Range(minSpeed, maxSpeed);
    }
    
    // Returns a random torque value for spinning.
    float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }
    
    // Returns a random spawn position along the X axis at Y = ySpawnPos.
    Vector3 RandomSpawnPos()
    {
        return new Vector3(Random.Range(-xRange, xRange), ySpawnPos);
    }
}
