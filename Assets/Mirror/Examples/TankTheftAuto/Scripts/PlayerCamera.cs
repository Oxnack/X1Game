﻿using UnityEngine;
using UnityEngine.SceneManagement;

// This sets up the scene camera for the local player

namespace Mirror.Examples.TankTheftAuto
{
    public class PlayerCamera : NetworkBehaviour
    {
        Camera mainCam;

        void Awake()
        {
            mainCam = Camera.main;
        }

        public override void OnStartLocalPlayer()
        {
            if (mainCam != null)
            {
                // configure and make camera a child of player with 3rd person offset
                mainCam.orthographic = false;
                mainCam.transform.SetParent(transform);
                mainCam.transform.localPosition = new Vector3(0f, 6f, -11f);
                mainCam.transform.localEulerAngles = new Vector3(25f, 0f, 0f);
            }
            else
                Debug.LogWarning("PlayerCamera: Could not find a camera in scene with 'MainCamera' tag.");
        }

        public override void OnStopLocalPlayer()
        {
            if (mainCam != null)
            {
                mainCam.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(mainCam.gameObject, SceneManager.GetActiveScene());
                mainCam.orthographic = true;
                mainCam.orthographicSize = 15f;
                mainCam.transform.localPosition = new Vector3(0f, 70f, 0f);
                mainCam.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
        }
    }
}
