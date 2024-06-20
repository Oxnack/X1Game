using UnityEngine;
using Mirror;
using TMPro;

public class NameOnPerson : NetworkBehaviour        // синх и вывод имени на перса вешается на канвас имени на пересе
{

    [SyncVar(hook = nameof(OnNameChanged))] private string _name = "";

    [SerializeField] private TextMeshProUGUI _nameText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetName(_name);
    }

    public void SetName(string newName)
    {
        _name = newName;
        _nameText.text = _name;
    }

    [Command]
    public void CmdSetName(string newName)
    {
        _name = newName;
    }

    void OnNameChanged(string oldName, string newName)
    {
        _nameText.text = newName;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            string playerName = PlayerPrefs.GetString("name");
            CmdSetName(playerName);
        }
    }

    /*
    [SyncVar] private string _name = "Player";
    [SerializeField] private TextMeshProUGUI _nameText;

    private void Start()
    {
        if (!isLocalPlayer) return;
        
        _name = PlayerPrefs.GetString("name");
        _nameText.text = _name;
        
        CmdSetName(_name);
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer) return;
        base.OnStartClient();
        SetName();
    }
    private void SetName()
    {
        _nameText.text = _name;
    }

    [Command] 
    private void CmdSetName(string name) 
    {
        _name = name;
        RpcSetName();
    }

    [ClientRpc]
    private void RpcSetName()
    {
        SetName();
    }*/
}


