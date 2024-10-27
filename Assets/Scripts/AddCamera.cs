using UnityEngine;
using Mirror;
using Cinemachine;

public class AddCamera : NetworkBehaviour   // длядобавления в синемашину персонажа игрока
{
    private GameObject _PlayerFollowCamera;
    private CinemachineVirtualCamera _cinemachine;

    private void Start()
    {
        if (!isLocalPlayer) return;
        if (_PlayerFollowCamera == null)         //         обьект с машиной
        {
            _PlayerFollowCamera = GameObject.FindGameObjectWithTag("PlayerFollowCamera");
        }

        _cinemachine = _PlayerFollowCamera.GetComponent<CinemachineVirtualCamera>();
        _cinemachine.Follow = gameObject.transform;             // указываем наш трансформ
    }
}
