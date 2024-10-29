using UnityEngine;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    public string Name;

    private void Start()
    {
        if (!isServer)
        {
            Name = PlayerPrefs.GetString("name");
            CmdSetName(Name);
        }
    }

    [Command]
    private void CmdSetName(string name)
    {
        Debug.Log("New Robot Name: " + name);

        Name = name;
    }
}
