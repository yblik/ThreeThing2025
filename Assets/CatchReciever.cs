using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchReciever : MonoBehaviour
{
    public PlayerCatch PC;
    public ThirdPersonMovement ThirdPersonMovement;

    public void StartMove()
    {
        ThirdPersonMovement.EnableMovement();
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
