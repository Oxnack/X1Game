using UnityEngine;
using Mirror;

public class CommandColor : NetworkBehaviour   // �������� �� ������ ����������� ������� (�������� ������ �� �������)
{
    [SerializeField] private Color _color; // ���� �� ������� ����� ���������� ����

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.GetComponent<ColorChanger>().SetColor(_color);
            }
        }
    }
}

