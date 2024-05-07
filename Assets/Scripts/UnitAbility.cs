using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAbility : MonoBehaviour
{
    // The Human-friendly name of this ability
    public string abilityName;
    // Whether this ability targets allies or enemies
    public bool targetAllies;
    // How far this ability reaches
    public int range;
    // How much damage this ability does (set < 0 to heal instead)
    public int damage;
    // how much mana it costs to use the ability
    public int manaCost;
    // Whether this ability ignores cover (e.g. a heal spell shouldn't be blocked by cover)
    public bool ignoreCover;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
