using UnityEngine;
using Mirror;

namespace Mirror.Examples.CharacterSelection
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransformUnreliable))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerControllerScript : NetworkBehaviour
    {
        public enum GroundState : byte { Jumping, Falling, Grounded }

        [Header("Avatar Components")]
        public CharacterController characterController;

        [Header("Movement")]
        [Range(1, 20)]
        public float moveSpeedMultiplier = 8f;

        [Header("Turning")]
        [Range(1f, 200f)]
        public float maxTurnSpeed = 100f;
        [Range(.5f, 5f)]
        public float turnDelta = 3f;

        [Header("Jumping")]
        [Range(0.1f, 1f)]
        public float initialJumpSpeed = 0.2f;
        [Range(1f, 10f)]
        public float maxJumpSpeed = 5f;
        [Range(0.1f, 1f)]
        public float jumpDelta = 0.2f;

        [Header("Diagnostics")]
        [ReadOnly, SerializeField] GroundState groundState = GroundState.Grounded;

        [ReadOnly, SerializeField, Range(-1f, 1f)]
        float horizontal;
        [ReadOnly, SerializeField, Range(-1f, 1f)]
        float vertical;

        [ReadOnly, SerializeField, Range(-200f, 200f)]
        float turnSpeed;

        [ReadOnly, SerializeField, Range(-10f, 10f)]
        float jumpSpeed;

        [ReadOnly, SerializeField, Range(-1.5f, 1.5f)]
        float animVelocity;

        [ReadOnly, SerializeField, Range(-1.5f, 1.5f)]
        float animRotation;

        [ReadOnly, SerializeField] Vector3Int velocity;
        [ReadOnly, SerializeField] Vector3 direction;

        private Transform cameraObj;
        public Transform cameraTarget;
        private SceneReferencer sceneReferencer;
        private CharacterSelection characterSelection;

        public void Awake()
        {

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (characterSelection == null)
                characterSelection = GetComponent<CharacterSelection>();

            // Override CharacterController default values
            characterController.enabled = false;
            characterController.skinWidth = 0.02f;
            characterController.minMoveDistance = 0f;

            GetComponent<Rigidbody>().isKinematic = true;

            this.enabled = false;
        }

        public override void OnStartAuthority()
        {
            characterController.enabled = true;
            this.enabled = true;

#if UNITY_2022_2_OR_NEWER
            sceneReferencer = GameObject.FindAnyObjectByType<SceneReferencer>();
#else
            // Deprecated in Unity 2023.1
            sceneReferencer = GameObject.FindObjectOfType<SceneReferencer>();
#endif
            cameraObj = sceneReferencer.cameraObject.transform;
        }

        public override void OnStopAuthority()
        {
            this.enabled = false;
            characterController.enabled = false;
        }

        void Update()
        {
            if (cameraObj && characterSelection)
            {
                characterSelection.floatingInfo.forward = cameraObj.transform.forward;
            }

            if (!Application.isFocused) return;
            if (!characterController.enabled)
                return;

            HandleTurning();
            HandleJumping();
            HandleMove();

            // Reset ground state
            if (characterController.isGrounded)
                groundState = GroundState.Grounded;
            else if (groundState != GroundState.Jumping)
                groundState = GroundState.Falling;

            // Diagnostic velocity...FloorToInt for display purposes
            velocity = Vector3Int.FloorToInt(characterController.velocity);

            if (cameraObj != null)
            {
                cameraObj.position = cameraTarget.position;
                cameraObj.rotation = cameraTarget.rotation;
            }
        }

        // TODO: Turning works while airborne...feature?
        void HandleTurning()
        {
            // Q and E cancel each other out, reducing the turn to zero.
            if (Input.GetKey(KeyCode.Q))
                turnSpeed = Mathf.MoveTowards(turnSpeed, -maxTurnSpeed, turnDelta);
            if (Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, maxTurnSpeed, turnDelta);

            // If both pressed, reduce turning speed toward zero.
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, 0, turnDelta);

            // If neither pressed, reduce turning speed toward zero.
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, 0, turnDelta);

            transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f);
        }

        void HandleJumping()
        {
            // Handle variable force jumping.
            // Jump starts with initial power on takeoff, and jumps higher / longer
            // as player holds spacebar. Jump power is increased by a diminishing amout
            // every frame until it reaches maxJumpSpeed, or player releases the spacebar,
            // and then changes to the falling state until it gets grounded.
            if (groundState != GroundState.Falling && Input.GetKey(KeyCode.Space))
            {
                if (groundState != GroundState.Jumping)
                {
                    // Start jump at initial power.
                    groundState = GroundState.Jumping;
                    jumpSpeed = initialJumpSpeed;
                }
                else
                    // Jumping has already started...increase power toward maxJumpSpeed over time.
                    jumpSpeed = Mathf.MoveTowards(jumpSpeed, maxJumpSpeed, jumpDelta);

                // If power has reached maxJumpSpeed, change to falling until grounded.
                // This prevents over-applying jump power while already in the air.
                if (jumpSpeed == maxJumpSpeed)
                    groundState = GroundState.Falling;
            }
            else if (groundState != GroundState.Grounded)
            {
                // handles running off a cliff and/or player released Spacebar.
                groundState = GroundState.Falling;
                jumpSpeed = Mathf.Min(jumpSpeed, maxJumpSpeed);
                jumpSpeed += Physics.gravity.y * Time.deltaTime;
            }
            else
                jumpSpeed = Physics.gravity.y * Time.deltaTime;
        }

        // TODO: Directional input works while airborne...feature?
        void HandleMove()
        {
            // Capture inputs
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Create initial direction vector without jumpSpeed (y-axis).
            direction = new Vector3(horizontal, 0f, vertical);

            // Clamp so diagonal strafing isn't a speed advantage.
            direction = Vector3.ClampMagnitude(direction, 1f);

            // Transforms direction from local space to world space.
            direction = transform.TransformDirection(direction);

            // Multiply for desired ground speed.
            direction *= moveSpeedMultiplier;

            // Add jumpSpeed to direction as last step.
            direction.y = jumpSpeed;

            // Finally move the character.
            characterController.Move(direction * Time.deltaTime);
        }
    }
}
