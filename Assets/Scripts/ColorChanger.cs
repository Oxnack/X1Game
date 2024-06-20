
using UnityEngine;
using Mirror;

public class ColorChanger : NetworkBehaviour   //�������� �� ��������� � ����������� � �������� ����������� � ������ ����
{
    [SerializeField] private Renderer _rend;
    [SyncVar] [SerializeField] private Color _color;

    private void Start()
    {
        if (!isLocalPlayer) return;
        _color = Color.white;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        SetRender();
    }

    public void SetRender()
    {
        _rend.material.color = _color;
    }

    [ClientRpc]
    public void RpcSetRender()
    {
        SetRender();
    }

    public void SetColor(Color color)
    {
        _color = color;
        RpcSetRender();

        Debug.Log("ColorChanged");
    }
}
