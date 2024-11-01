using UnityEngine;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    [SyncVar] public string Name;
}
