using Mirror;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerSetupScript : NetworkBehaviour
{
    // remember to set these on player prefab
    public PlayerInput playerInput;
    public ThirdPersonController thirdPersonController;

    public override void OnStartLocalPlayer()
    {
        //disable by default on player prefab
        playerInput.enabled = true;
        thirdPersonController.enabled = true;
    }
}