using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour // On Camera
{
    private RobotCamera _robotCamera;
    private CinemachineBrain _cinemachineBrain;

    private void Start()
    {
        _robotCamera = GetComponent<RobotCamera>();
        _cinemachineBrain = GetComponent<CinemachineBrain>();
    }

    public void ChangeView()
    {
        if (_robotCamera.enabled == false)
        {
            _cinemachineBrain.enabled = false;
            _robotCamera.enabled = true;
        }
        else
        {
            _robotCamera.enabled = false;
            _cinemachineBrain.enabled = true;
        }
    }
}
