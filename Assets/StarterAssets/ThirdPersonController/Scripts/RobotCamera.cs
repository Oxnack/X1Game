using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCamera : MonoBehaviour //On Camera
{
    private string _name;
    private GameObject _robot = null;
    private void OnEnable()
    {
        _name = PlayerPrefs.GetString("name");

        PlayerName[] names = FindObjectsOfType<PlayerName>();

        _robot = null;

        foreach (PlayerName name in names)
        {
            if (name.Name == _name)
            {
                _robot = name.gameObject;
            }
        }
    }

    private void Update()
    {
        if (_robot != null)
        {
            gameObject.transform.position = _robot.transform.position + new Vector3(-3, 3, -3);
        }
    }
}
