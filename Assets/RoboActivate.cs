using UnityEngine;
using Mirror;
using Newtonsoft.Json;

public class RoboActivate : NetworkBehaviour
{
    [SerializeField] private GameObject _robotPrefab;
    private string _name;

    public override void OnStartClient()
    {
        base.OnStartClient();
        string _name = PlayerPrefs.GetString("name");

        if (isLocalPlayer) 
        {
            CmdSpawnRobot(_name);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (isLocalPlayer)
        {
            CmdDestroyRobot(_name);
        }
    }

    [Command]
    private void CmdSpawnRobot(string name)
    {
        GameObject robo = Instantiate(_robotPrefab); 
        NetworkServer.Spawn(robo); 

        robo.GetComponent<PlayerName>().enabled = true;

        robo.GetComponent<PlayerName>().Name = name;

        Debug.Log("Spawn New Robot, with name: " + name);
    }

    [Command]
    private void CmdDestroyRobot(string name)
    {
        PlayerName[] RobotNames = FindObjectsOfType<PlayerName>();

        GameObject robo = null;

        foreach (PlayerName RobotName in RobotNames)
        {
            if (RobotName.Name == name)
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
