using UnityEngine;
using Mirror;
using TMPro;

public class NameOnPerson : NetworkBehaviour        // ���� � ����� ����� �� ����� �������� �� ������ ����� �� ������
{
    [SyncVar] private string _name = "Player";
    [SerializeField] private TextMeshProUGUI _nameText;

    private void Start()
    {
        if (isLocalPlayer)
        {
            if (PlayerPrefs.GetString("name") != "")
            {
                _name = PlayerPrefs.GetString("name");
            }
        }

        _nameText.text = _name;
        if (!isServer) return;
        RpcSetName(_name);
    }

    [ClientRpc] 
    private void RpcSetName(string name) 
    {
        _nameText.text = name;
    }
}
