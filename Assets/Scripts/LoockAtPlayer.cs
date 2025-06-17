using UnityEngine;
using Mirror;

public class LoockAtPlayer : NetworkBehaviour  //����� ��� �������� ���� ������� � ������ ��� �������   
{
    private GameObject _lockalPlayerCamera;             // �������� �� ������ ������� ����� ������������

    private void Start()
    {
        Debug.Log("Camera Inicialise");
        _lockalPlayerCamera = Camera.main.gameObject;         //������� ������ �� ������

    }
    private void Update()
    {
        transform.LookAt(_lockalPlayerCamera.transform);
    }
}
