using Mirror;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : NetworkBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        [SerializeField] private float cameraJoystickSensivity = 1f;

        public override void OnStartClient()       
        {
            base.OnStartClient();
            if (Application.isMobilePlatform == false) gameObject.SetActive(false);

            Debug.Log("GetLockal Person for Input");
        }

        private void Update()
        {
            if (isServer) return;
            if (starterAssetsInputs == null)
            {
                starterAssetsInputs = NetworkClient.localPlayer.gameObject.GetComponent<StarterAssetsInputs>();
            }
        }
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            virtualLookDirection *= cameraJoystickSensivity;
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }
        
    }

}
