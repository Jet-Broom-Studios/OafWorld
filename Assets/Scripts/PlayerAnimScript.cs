using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimScript : MonoBehaviour
{
    private Animator anim;
    private UnitController uc;

    private float time = 0f;
    private float timeLim;
    private Vector3 prevPos;
    private Quaternion prevRot;

    // Start is called before the first frame update
    void Start()
    {
        prevPos = transform.position;
        uc = GetComponent<UnitController>();
        anim = GetComponent<Animator>();
        timeLim = Random.Range(4, 7);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == prevPos)
        {
            anim.SetBool("isWalking", false);
        }
        // if in a different position than last frame, then they must be moving
        if (transform.position != prevPos)
        {
            // Rotate toward next node
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - prevPos), Time.deltaTime * 5);
            // Start Walk animation
            if (!anim.GetBool("isWalking"))
                anim.SetBool("isWalking", true);
        }
        else if (!anim.GetBool("isAttacking"))  // If not walking or attacking, must be idle (atks handled below)
        {
            // Chance of "look around" idle after a random amount of time
            if (time > timeLim)
            {
                int rnd = Random.Range(0, 3);
                //print("TIME! rnd = " + rnd);
                if (rnd == 1 && !anim.GetBool("action0") && !anim.GetBool("action1") && !anim.GetBool("action2") && !anim.GetBool("isWalking"))
                {
                    anim.SetBool("isLooking", true);
                    anim.CrossFade("LookAround", .1f);
                    anim.SetBool("isLooking", false);
                }
                timeLim = Random.Range(4, 7);
                time = 0;
            }
            time += Time.deltaTime;
        }
        prevPos = transform.position;
        prevRot = transform.rotation;
    }
    private UnitController enemyTarget;
    public void StartAttack(UnitController targetUnit, int action)
    {
        enemyTarget = targetUnit;   // Cannot pass much arguments in animation events unfortunately, so I set a private var in global
        StartCoroutine(RotateToTarget(enemyTarget, action));
    }
    IEnumerator RotateToTarget(UnitController targetUnit, int action)   // Allows smooth rotating first then attacking - Messes up if you atk then move during rotation? (remove input until complete?)
    {
        while (transform.rotation != Quaternion.LookRotation(targetUnit.transform.position - prevPos))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetUnit.transform.position - transform.position), Time.deltaTime * 500);
            yield return new WaitForEndOfFrame();
        }
        switch (action)
        {
            case 1:
                anim.SetBool("action1", true);
                break;
            case 2:
                anim.SetBool("action2", true);
                break;
            default:
                anim.SetBool("action0", true);
                break;
        }
        anim.SetBool("isAttacking", true);
    }
    private void SendAction0()
    {
        uc.commitAction(enemyTarget);   // Calls commitAttack from the UnitController which is the original logical side of the interaction
        anim.SetBool("action0", false);
    }
    private void SendAction1()
    {
        uc.commitAction(enemyTarget);   // Calls commitAttack from the UnitController which is the original logical side of the interaction
        anim.SetBool("action1", false);
    }
    private void SendAction2()
    {
        uc.commitAction(enemyTarget);   // Calls commitAttack from the UnitController which is the original logical side of the interaction
        anim.SetBool("action2", false);
    }
}
