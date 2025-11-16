using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchReciever : MonoBehaviour
{
    public PlayerCatch PC;
    public ThirdPersonMovement ThirdPersonMovement;
    public Health hp;
    public Animator TransitionOut;

    public void StartMove()
    {
        if (!hp.Dead)
        {
            ThirdPersonMovement.EnableMovement();
            hp.backinaction();
            SpawnManager.Instance.SetRespawn(false);
        }
    }
    public void DeathDone()
    {
        if (hp.Dead)
        {
            TransitionOut.Play("SwitchRoom"); //get things into motion
        }

    }
    public void StartCatch()
    {
        PC.StartCatching ();
    }
    public void EndCatch()
    {
        PC.StopCatching();
    }
}
