using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Gates2 : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnVariableChanged))]
    public int score = 0;
    [SerializeField] private TextMeshProUGUI _TMP;

    private void OnVariableChanged(int oldValue, int newValue)
    {
        Debug.Log($"Variable changed from {oldValue} to {newValue}");
        _TMP.text = score.ToString();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _TMP.text = score.ToString();
    }



    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Ball"))
            {
                score += 1;
            }
        }
    }
}


