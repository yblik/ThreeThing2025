using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerAnimManager : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;

    private void Start()
    {
        //animator.SetBool("walk", true);
    }
    private void Update()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            animator.SetBool("w", true);
        }
        else
        {
            animator.SetBool("w", false);
        }
        //Debug.Log(isMoving);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("j", true);
        }
        else
        {
            animator.SetBool("j", false);
        }   
        if (Input.GetMouseButton(0))
        {
            animator.SetBool("s", true);
            //animator.SetLayerWeight(1, 0.7f); // Sets layer 2

        }
        else
        {
            animator.SetBool("s", false);
            //animator.SetLayerWeight(1, 0); // Sets layer 2 
        }

    }
}
