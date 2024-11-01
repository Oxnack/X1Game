using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoboName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private PlayerName _name;

    private void Update()
    {
        _nameText.text = _name.Name;
    }
}
