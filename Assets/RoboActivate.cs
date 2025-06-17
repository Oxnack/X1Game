using UnityEngine;
using Mirror;
using Newtonsoft.Json;

public class RoboActivate : NetworkBehaviour
{
    [SerializeField] private GameObject _robotPrefab;
    public string _name;

    public override void OnStartClient()
    {
        base.OnStartClient();
        string _name = PlayerPrefs.GetString("name");

        if (isLocalPlayer) 
        {
            CmdSpawnRobot(_name);
        }
    }


    [Command]
    private void CmdSpawnRobot(string name)
    {
        _name = name;
        GameObject robo = Instantiate(_robotPrefab); 
        NetworkServer.Spawn(robo); 

        robo.GetComponent<PlayerName>().enabled = true;

        robo.GetComponent<PlayerName>().Name = name;

        Debug.Log("Spawn New Robot, with name: " + name);
    }

    private void OnDestroy()
    {
        PlayerName[] RobotNames = FindObjectsOfType<PlayerName>();

        GameObject robo = null;

        foreach (PlayerName RobotName in RobotNames)
        {
            if (RobotName.Name == _name)
            {
                robo = RobotName.gameObject;
            }
        }

        if (robo != null)
        {
            Destroy(robo);
            Debug.Log("Robot Destroyed");
        }
        else
        {
            Debug.Log("No Robot to desroy");
        }

    }
}
