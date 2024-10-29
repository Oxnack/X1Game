using UnityEngine;
using Mirror;

public class RoboActivate : NetworkBehaviour
{
    [SerializeField] private GameObject _robotPrefab;

    public override void OnStartClient()
    {
        base.OnStartClient();
        string name = PlayerPrefs.GetString("name");

        if (isLocalPlayer) 
        {
            CmdSpawnRobot(name);
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
}
