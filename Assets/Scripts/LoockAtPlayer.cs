using UnityEngine;
using Mirror;

public class LoockAtPlayer : NetworkBehaviour  //нужно для поворота имен игроков к камере или клиенту   
{
    private GameObject _lockalPlayerCamera;             // вешается на обьект который нужно поворачивать

    private void Start()
    {
        Debug.Log("Camera Inicialise");
        _lockalPlayerCamera = Camera.main.gameObject;         //получем камеру на старте

    }
    private void Update()
    {
        transform.LookAt(_lockalPlayerCamera.transform);
    }
}
