/*
 * HealthScript
 * Creator:êºâYçWëæ Update:2024/08/23
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Heart's GameObject
    [SerializeField] GameObject heart;
    // Enemy's Health Value
    int enemHealth;
    // Player's Health Value
    int playerHealth;

    /// <summary>
    /// EnemyHealth's Property
    /// </summary>
    public int EnemHealth
    {
        get { return enemHealth; }
    }

    /// <summary>
    /// PlayerHealth's Property
    /// </summary>
    public int PlayerHealth
    {
        get { return playerHealth; }
    }

    void Awake()
    {
        enemHealth = 3*2;
        playerHealth = 5;
        EnemyLife();
        PlayerLife();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Set Enemy's Health
    /// </summary>
    public void EnemyLife()
    {
        Vector2 pos = this.transform.position;
        for (int i = 0; i < enemHealth; i++) // Loop for Enemy's Life Value
        {
            // Add Tag for Items
            heart.tag = "EnemyHP";
            // Create Instance from Heart Prefabs
            Instantiate(heart, new Vector2(pos.x + (0.8f * i), pos.y), Quaternion.identity);
        }
    }

    /// <summary>
    /// Set Player's Health
    /// </summary>
    public void PlayerLife()
    {
        for (int i = 0; i < playerHealth; i++) // Loop for Enemy's Life Value
        {
            // Add Tag for Items
            heart.tag = "PlayerHP";
            // Create Instance from Heart Prefabs
            Instantiate(heart, new Vector2(-8.5f + (0.8f * i), 0.7f), Quaternion.identity);
        }
    }
}
