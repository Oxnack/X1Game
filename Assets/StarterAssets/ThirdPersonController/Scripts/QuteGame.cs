
using UnityEngine;
using Mirror;

public class QuteGame : MonoBehaviour
{
    public void StopGame()
    {
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
