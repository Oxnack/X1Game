using UnityEngine;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    public string Name;

    private void Start()
    {
        if (isLocalPlayer)
        {
            Name = PlayerPrefs.GetString("name");
            CmdSetName(Name);
        }
    }

    [Command]
    private void CmdSetName(string name)
    {
        Name = name;
    }
}
