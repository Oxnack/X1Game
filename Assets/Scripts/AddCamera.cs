using UnityEngine;
using Mirror;
using Cinemachine;

public class AddCamera : NetworkBehaviour   // для добавления в синемашину поицию персонажа игрока
{
    private CinemachineVirtualCamera _cinemachine;

    private Transform _playerArmTransform;
    private Transform _robotTransform;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _playerArmTransform = NetworkClient.localPlayer.gameObject.GetComponent<Transform>().Find("PlayerArmature");
        _robotTransform = NetworkClient.localPlayer.gameObject.GetComponent<Transform>().Find("Robot");

        _cinemachine = GetComponent<CinemachineVirtualCamera>();

        _cinemachine.Follow = _playerArmTransform;
    }

    public void PlayerLook()
    {
        _cinemachine.Follow = _playerArmTransform;
    }

    public void RobotLook()
    {
        _cinemachine.Follow = _robotTransform;
    }
}
