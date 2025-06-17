using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayer : MonoBehaviour
{
    [SerializeField] private NetworkManager manager;
   
    public void StartSinglePlayer()
    {
        NetworkServer.dontListen = true;
        manager.StartHost();
    }
}
