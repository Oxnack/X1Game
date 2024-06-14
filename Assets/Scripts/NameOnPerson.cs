using UnityEngine;
using Mirror;
using TMPro;

public class NameOnPerson : NetworkBehaviour        // синх и вывод имени на перса вешается на канвас имени на пересе
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
