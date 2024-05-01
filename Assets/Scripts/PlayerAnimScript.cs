using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimScript : MonoBehaviour
{
    private Animator anim;
    private UnitController uc;

    // Start is called before the first frame update
    void Start()
    {
        uc = GetComponent<UnitController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (uc.IsMoving())
        {
            anim.SetBool("isWalking", true);
            anim.CrossFade("Walking", .1f);
        }
        if (uc.IsIdle())
        {
            anim.SetBool("isIdle", true);
            anim.CrossFade("Idle", .1f);
            /*if (Random.Range(1, 4) == 1)
            {
                anim.SetBool("isLooking", true);
                anim.CrossFade("LookAround", .1f);
            }
            else
            {
                anim.SetBool("isIdle", true);
                anim.CrossFade("Idle", .1f);
            }*/
        }
    }
}
