using UnityEngine;
using Mirror;
using Org.BouncyCastle.Crypto.Macs;

public class RoboActivate : NetworkBehaviour
{
    [SerializeField] private GameObject _robot;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameObject robo = Instantiate(_robot);
        CmdSpawnRobot(robo);
        robo.GetComponent<PlayerName>().enabled = true;
    }


    [Command]
    private void CmdSpawnRobot(GameObject robo)
    {
        robo.GetComponent<PlayerName>().enabled = true;
        NetworkServer.Spawn(robo);
    }
}
