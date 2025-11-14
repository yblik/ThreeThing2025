using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineVirtualCamera cutsceneCam;
    public CinemachineFreeLook freeLookCam;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerCutscene();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            EndCutscene();
        }
        //maybe disable movement in cutscenes
    }
    void TriggerCutscene()
    {
        cutsceneCam.Priority = 20;
        freeLookCam.Priority = 10;
    }

    void EndCutscene()
    {
        cutsceneCam.Priority = 10;
        freeLookCam.Priority = 20;
    }
}
