using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverController : MonoBehaviour, IDamageable
{
    // How much damage this cover object can absorb before breaking
    public int maxHealth;
    private int currentHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Damage or heal this cover
    // If the new HP total is 0 or less, destroy this cover instead.
    // If the new HP total is greater than maxHealth, set it to maxHealth instead.
    public void ChangeHealth(int deltaHP)
    {
        currentHealth += deltaHP;
        if (deltaHP > 0)
        {
            print(name + " healed " + deltaHP + " HP!");
        }
        else
        {
            print(name + " lost " + -deltaHP + " HP!");
        }

        if (currentHealth <= 0)
        {
            Kill();
        }
        else if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // Destroy this cover
    public void Kill()
    {
        print(name + " was destroyed!");
        Destroy(gameObject);

        // Since cover can alter unit pathing, the grid needs to be updated when cover is destroyed.
        MapManager.instance.QueueUpdate();
    }
}
