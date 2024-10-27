
using UnityEngine;
using Mirror;

public class BallSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject _ballPrefab;

    [SerializeField] private Vector3 _position = new Vector3(0,0,0);

    public override void OnStartServer()
    {
        if (!isServer) return;

        GameObject ball = Instantiate(_ballPrefab, _position, new Quaternion(0,0,0,0));
        NetworkServer.Spawn(ball);
    }

}
