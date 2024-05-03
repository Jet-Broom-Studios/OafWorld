using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimScript : MonoBehaviour
{
    private Animator anim;
    private UnitController uc;

    private float time = 0f;
    private float timeLim;

    // Start is called before the first frame update
    void Start()
    {
        uc = GetComponent<UnitController>();
        anim = GetComponent<Animator>();
        timeLim = Random.Range(4, 7);
    }

    // Update is called once per frame
    void Update()
    {
        if (uc.IsMoving())
        {
            anim.SetBool("isLooking", false);
            anim.SetBool("isWalking", true);
            anim.CrossFade("Walking", .1f);
            anim.SetBool("isWalking", false);
        }
        else
        {
            if (time > timeLim)
            {
                int rnd = Random.Range(0, 3);
                print("TIME! rnd = " + rnd);
                if (rnd == 1)
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
        /*else if (uc.IsIdle())
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isIdle", true);
            anim.CrossFade("Idle", .1f);
            if (Random.Range(1, 4) == 1)
            {
                anim.SetBool("isLooking", true);
                anim.CrossFade("LookAround", .1f);
            }
            else
            {
                anim.SetBool("isIdle", true);
                anim.CrossFade("Idle", .1f);
            }
        }*/
    }
}
