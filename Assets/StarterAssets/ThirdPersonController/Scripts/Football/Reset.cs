using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Reset : NetworkBehaviour
{
    [SerializeField] private Gates2 _gates;
    [SerializeField] private Gates2 _gates1;

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _gates.score = 0;
                _gates1.score = 0;
            }
        }
    }
}
