using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror
{
    /// <summary>
    /// A component to synchronize Mecanim animation states for networked objects.
    /// </summary>
    /// <remarks>
    /// <para>The animation of game objects can be networked by this component. There are two models of authority for networked movement:</para>
    /// <para>If the object has authority on the client, then it should be animated locally on the owning client. The animation state information will be sent from the owning client to the server, then broadcast to all of the other clients. This is common for player objects.</para>
    /// <para>If the object has authority on the server, then it should be animated on the server and state information will be sent to all clients. This is common for objects not related to a specific client, such as an enemy unit.</para>
    /// <para>The NetworkAnimator synchronizes all animation parameters of the selected Animator. It does not automatically synchronize triggers. The function SetTrigger can by used by an object with authority to fire an animation trigger on other clients.</para>
    /// </remarks>
    // [RequireComponent(typeof(NetworkIdentity))] disabled to allow child NetworkBehaviours
    [AddComponentMenu("Network/Network Animator")]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-animator")]
    public class NetworkAnimator : NetworkBehaviour
    {
        [Header("Authority")]
        [Tooltip("Set to true if animations come from owner client,  set to false if animations always come from server")]
        public bool clientAuthority;

        /// <summary>
        /// The animator component to synchronize.
        /// </summary>
        [FormerlySerializedAs("m_Animator")]
        [Header("Animator")]
        [Tooltip("Animator that will have parameters synchronized")]
        public Animator animator;

        /// <summary>
        /// Syncs animator.speed
        /// </summary>
        [SyncVar(hook = nameof(OnAnimatorSpeedChanged))]
        float animatorSpeed;
        float previousSpeed;

        // Note: not an object[] array because otherwise initialization is real annoying
        int[] lastIntParameters;
        float[] lastFloatParameters;
        bool[] lastBoolParameters;
        AnimatorControllerParameter[] parameters;

        // multiple layers
        int[] animationHash;
        int[] transitionHash;
        float[] layerWeight;
        double nextSendTime;

        bool SendMessagesAllowed
        {
            get
            {
                if (isServer)
                {
                    if (!clientAuthority)
                        return true;

                    // This is a special case where we have client authority but we have not assigned the client who has
                    // authority over it, no animator data will be sent over the network by the server.
                    //
                    // So we check here for a connectionToClient and if it is null we will
                    // let the server send animation data until we receive an owner.
                    if (netIdentity != null && netIdentity.connectionToClient == null)
                        return true;
                }

                return (isOwned && clientAuthority);
            }
        }

        void Initialize()
        {
            // store the animator parameters in a variable - the "Animator.parameters" getter allocates
            // a new parameter array every time it is accessed so we should avoid doing it in a loop
            parameters = animator.parameters
                .Where(par => !animator.IsParameterControlledByCurve(par.nameHash))
                .ToArray();
            lastIntParameters = new int[parameters.Length];
            lastFloatParameters = new float[parameters.Length];
            lastBoolParameters = new bool[parameters.Length];

            animationHash = new int[animator.layerCount];
            transitionHash = new int[animator.layerCount];
            layerWeight = new float[animator.layerCount];
        }

        // fix https://github.com/MirrorNetworking/Mirror/issues/2810
        // both Awake and Enable need to initialize arrays.
        // in case users call SetActive(false) -> SetActive(true).
        void Awake() => Initialize();
        void OnEnable() => Initialize();

        public virtual void Reset()
        {
            syncDirection = SyncDirection.ClientToServer;
        }

        void FixedUpdate()
        {
            if (!SendMessagesAllowed)
                return;

            if (!animator.enabled)
                return;

            CheckSendRate();

            for (int i = 0; i < animator.layerCount; i++)
            {
                int stateHash;
                float normalizedTime;
                if (!CheckAnimStateChanged(out stateHash, out normalizedTime, i))
                {
                    continue;
                }

                using (NetworkWriterPooled writer = NetworkWriterPool.Get())
                {
                    WriteParameters(writer);
                    SendAnimationMessage(stateHash, normalizedTime, i, layerWeight[i], writer.ToArray());
                }
            }

            CheckSpeed();
        }

        void CheckSpeed()
        {
            float newSpeed = animator.speed;
            if (Mathf.Abs(previousSpeed - newSpeed) > 0.001f)
            {
                previousSpeed = newSpeed;
                if (isServer)
                {
                    animatorSpeed = newSpeed;
                }
                else if (isClient)
                {
                    CmdSetAnimatorSpeed(newSpeed);
                }
            }
        }

        void OnAnimatorSpeedChanged(float _, float value)
        {
            // skip if host or client with authority
            // they will have already set the speed so don't set again
            if (isServer || (isOwned && clientAuthority))
                return;

            animator.speed = value;
        }

        bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layerId)
        {
            bool change = false;
            stateHash = 0;
            normalizedTime = 0;

            float lw = animator.GetLayerWeight(layerId);
            if (Mathf.Abs(lw - layerWeight[layerId]) > 0.001f)
            {
                layerWeight[layerId] = lw;
                change = true;
            }

            if (animator.IsInTransition(layerId))
            {
                AnimatorTransitionInfo tt = animator.GetAnimatorTransitionInfo(layerId);
                if (tt.fullPathHash != transitionHash[layerId])
                {
                    // first time in this transition
                    transitionHash[layerId] = tt.fullPathHash;
                    animationHash[layerId] = 0;
                    return true;
                }
                return change;
            }

            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(layerId);
            if (st.fullPathHash != animationHash[layerId])
            {
                // first time in this animation state
                if (animationHash[layerId] != 0)
                {
                    // came from another animation directly - from Play()
                    stateHash = st.fullPathHash;
                    normalizedTime = st.normalizedTime;
                }
                transitionHash[layerId] = 0;
                animationHash[layerId] = st.fullPathHash;
                return true;
            }
            return change;
        }

        void CheckSendRate()
        {
            double now = NetworkTime.localTime;
            if (SendMessagesAllowed && syncInterval >= 0 && now > nextSendTime)
            {
                nextSendTime = now + syncInterval;

                using (NetworkWriterPooled writer = NetworkWriterPool.Get())
                {
                    if (WriteParameters(writer))
                        SendAnimationParametersMessage(writer.ToArray());
                }
            }
        }

        void SendAnimationMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
        {
            if (isServer)
            {
                RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
            else if (isClient)
            {
                CmdOnAnimationServerMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
        }

        void SendAnimationParametersMessage(byte[] parameters)
        {
            if (isServer)
            {
                RpcOnAnimationParametersClientMessage(parameters);
            }
            else if (isClient)
            {
                CmdOnAnimationParametersServerMessage(parameters);
            }
        }

        void HandleAnimMsg(int stateHash, float normalizedTime, int layerId, float weight, NetworkReader reader)
        {
            if (isOwned && clientAuthority)
                return;

            // usually transitions will be triggered by parameters, if not, play anims directly.
            // NOTE: this plays "animations", not transitions, so any transitions will be skipped.
            // NOTE: there is no API to play a transition(?)
            if (stateHash != 0 && animator.enabled)
            {
                animator.Play(stateHash, layerId, normalizedTime);
            }

            animator.SetLayerWeight(layerId, weight);

            ReadParameters(reader);
        }

        void HandleAnimParamsMsg(NetworkReader reader)
        {
            if (isOwned && clientAuthority)
                return;

            ReadParameters(reader);
        }

        void HandleAnimTriggerMsg(int hash)
        {
            if (animator.enabled)
                animator.SetTrigger(hash);
        }

        void HandleAnimResetTriggerMsg(int hash)
        {
            if (animator.enabled)
                animator.ResetTrigger(hash);
        }

        ulong NextDirtyBits()
        {
            ulong dirtyBits = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter par = parameters[i];
                bool changed = false;
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = animator.GetInteger(par.nameHash);
                    changed = newIntValue != lastIntParameters[i];
                    if (changed)
                        lastIntParameters[i] = newIntValue;
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = animator.GetFloat(par.nameHash);
                    changed = Mathf.Abs(newFloatValue - lastFloatParameters[i]) > 0.001f;
                    // only set lastValue if it was changed, otherwise value could slowly drift within the 0.001f limit each frame
                    if (changed)
                        lastFloatParameters[i] = newFloatValue;
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = animator.GetBool(par.nameHash);
                    changed = newBoolValue != lastBoolParameters[i];
                    if (changed)
                        lastBoolParameters[i] = newBoolValue;
                }
                if (changed)
                {
                    dirtyBits |= 1ul << i;
                }
            }
            return dirtyBits;
        }

        bool WriteParameters(NetworkWriter writer, bool forceAll = false)
        {
            // fix: https://github.com/MirrorNetworking/Mirror/issues/2852
            // serialize parameterCount to be 100% sure we deserialize correct amount of bytes.
            // (255 parameters should be enough for everyone, write it as byte)
            byte parameterCount = (byte)parameters.Length;
            writer.WriteByte(parameterCount);

            ulong dirtyBits = forceAll ? (~0ul) : NextDirtyBits();
            writer.WriteULong(dirtyBits);

            // iterate on byte count. if it's >256, it won't break
            // serialization - just not serialize excess layers.
            for (int i = 0; i < parameterCount; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                AnimatorControllerParameter par = parameters[i];
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = animator.GetInteger(par.nameHash);
                    writer.WriteInt(newIntValue);
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = animator.GetFloat(par.nameHash);
                    writer.WriteFloat(newFloatValue);
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = animator.GetBool(par.nameHash);
                    writer.WriteBool(newBoolValue);
                }
            }
            return dirtyBits != 0;
        }

        void ReadParameters(NetworkReader reader)
        {
            // fix: https://github.com/MirrorNetworking/Mirror/issues/2852
            // serialize parameterCount to be 100% sure we deserialize correct amount of bytes.
            // mismatch shows error to make this super easy to debug.
            byte parameterCount = reader.ReadByte();
            if (parameterCount != parameters.Length)
            {
                Debug.LogError($"NetworkAnimator: serialized parameter count={parameterCount} does not match expected parameter count={parameters.Length}. Are you changing animators at runtime?", gameObject);
                return;
            }

            bool animatorEnabled = animator.enabled;
            // need to read values from NetworkReader even if animator is disabled
            ulong dirtyBits = reader.ReadULong();
            for (int i = 0; i < parameterCount; i++)
            {
                if ((dirtyBits & (1ul << i)) == 0)
                    continue;

                AnimatorControllerParameter par = parameters[i];
                if (par.type == AnimatorControllerParameterType.Int)
                {
                    int newIntValue = reader.ReadInt();
                    if (animatorEnabled)
                        animator.SetInteger(par.nameHash, newIntValue);
                }
                else if (par.type == AnimatorControllerParameterType.Float)
                {
                    float newFloatValue = reader.ReadFloat();
                    if (animatorEnabled)
                        animator.SetFloat(par.nameHash, newFloatValue);
                }
                else if (par.type == AnimatorControllerParameterType.Bool)
                {
                    bool newBoolValue = reader.ReadBool();
                    if (animatorEnabled)
                        animator.SetBool(par.nameHash, newBoolValue);
                }
            }
        }

        public override void OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            if (initialState)
            {
                // fix: https://github.com/MirrorNetworking/Mirror/issues/2852
                // serialize layerCount to be 100% sure we deserialize correct amount of bytes.
                // (255 layers should be enough for everyone, write it as byte)
                byte layerCount = (byte)animator.layerCount;
                writer.WriteByte(layerCount);

                // iterate on byte count. if it's >256, it won't break
                // serialization - just not serialize excess layers.
                for (int i = 0; i < layerCount; i++)
                {
                    AnimatorStateInfo st = animator.IsInTransition(i)
                        ? animator.GetNextAnimatorStateInfo(i)
                        : animator.GetCurrentAnimatorStateInfo(i);
                    writer.WriteInt(st.fullPathHash);
                    writer.WriteFloat(st.normalizedTime);
                    writer.WriteFloat(animator.GetLayerWeight(i));
                }
                WriteParameters(writer, true);
            }
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            if (initialState)
            {
                // fix: https://github.com/MirrorNetworking/Mirror/issues/2852
                // serialize layerCount to be 100% sure we deserialize correct amount of bytes.
                // mismatch shows error to make this super easy to debug.
                byte layerCount = reader.ReadByte();
                if (layerCount != animator.layerCount)
                {
                    Debug.LogError($"NetworkAnimator: serialized layer count={layerCount} does not match expected layer count={animator.layerCount}. Are you changing animators at runtime?", gameObject);
                    return;
                }

                for (int i = 0; i < layerCount; i++)
                {
                    int stateHash = reader.ReadInt();
                    float normalizedTime = reader.ReadFloat();
                    float weight = reader.ReadFloat();

                    animator.SetLayerWeight(i, weight);
                    animator.Play(stateHash, i, normalizedTime);
                }

                ReadParameters(reader);
            }
        }

        /// <summary>
        /// Causes an animation trigger to be invoked for a networked object.
        /// <para>If local authority is set, and this is called from the client, then the trigger will be invoked on the server and all clients. If not, then this is called on the server, and the trigger will be called on all clients.</para>
        /// </summary>
        /// <param name="triggerName">Name of trigger.</param>
        public void SetTrigger(string triggerName)
        {
            SetTrigger(Animator.StringToHash(triggerName));
        }

        /// <summary>
        /// Causes an animation trigger to be invoked for a networked object.
        /// </summary>
        /// <param name="hash">Hash id of trigger (from the Animator).</param>
        public void SetTrigger(int hash)
        {
            if (clientAuthority)
            {
                if (!isClient)
                {
                    Debug.LogWarning("Tried to set animation in the server for a client-controlled animator", gameObject);
                    return;
                }

                if (!isOwned)
                {
                    Debug.LogWarning("Only the client with authority can set animations", gameObject);
                    return;
                }

                if (isClient)
                    CmdOnAnimationTriggerServerMessage(hash);

                // call on client right away
                HandleAnimTriggerMsg(hash);
            }
            else
            {
                if (!isServer)
                {
                    Debug.LogWarning("Tried to set animation in the client for a server-controlled animator", gameObject);
                    return;
                }

                HandleAnimTriggerMsg(hash);
                RpcOnAnimationTriggerClientMessage(hash);
            }
        }

        /// <summary>
        /// Causes an animation trigger to be reset for a networked object.
        /// <para>If local authority is set, and this is called from the client, then the trigger will be reset on the server and all clients. If not, then this is called on the server, and the trigger will be reset on all clients.</para>
        /// </summary>
        /// <param name="triggerName">Name of trigger.</param>
        public void ResetTrigger(string triggerName)
        {
            ResetTrigger(Animator.StringToHash(triggerName));
        }

        /// <summary>Causes an animation trigger to be reset for a networked object.</summary>
        /// <param name="hash">Hash id of trigger (from the Animator).</param>
        public void ResetTrigger(int hash)
        {
            if (clientAuthority)
            {
                if (!isClient)
                {
                    Debug.LogWarning("Tried to reset animation in the server for a client-controlled animator", gameObject);
                    return;
                }

                if (!isOwned)
                {
                    Debug.LogWarning("Only the client with authority can reset animations", gameObject);
                    return;
                }

                if (isClient)
                    CmdOnAnimationResetTriggerServerMessage(hash);

                // call on client right away
                HandleAnimResetTriggerMsg(hash);
            }
            else
            {
                if (!isServer)
                {
                    Debug.LogWarning("Tried to reset animation in the client for a server-controlled animator", gameObject);
                    return;
                }

                HandleAnimResetTriggerMsg(hash);
                RpcOnAnimationResetTriggerClientMessage(hash);
            }
        }

        #region server message handlers

        [Command]
        void CmdOnAnimationServerMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            //Debug.Log($"OnAnimationMessage for netId {netId}");

            // handle and broadcast
            using (NetworkReaderPooled networkReader = NetworkReaderPool.Get(parameters))
            {
                HandleAnimMsg(stateHash, normalizedTime, layerId, weight, networkReader);
                RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
            }
        }

        [Command]
        void CmdOnAnimationParametersServerMessage(byte[] parameters)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            // handle and broadcast
            using (NetworkReaderPooled networkReader = NetworkReaderPool.Get(parameters))
            {
                HandleAnimParamsMsg(networkReader);
                RpcOnAnimationParametersClientMessage(parameters);
            }
        }

        [Command]
        void CmdOnAnimationTriggerServerMessage(int hash)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            // handle and broadcast
            // host should have already the trigger
            bool isHostOwner = isClient && isOwned;
            if (!isHostOwner)
            {
                HandleAnimTriggerMsg(hash);
            }

            RpcOnAnimationTriggerClientMessage(hash);
        }

        [Command]
        void CmdOnAnimationResetTriggerServerMessage(int hash)
        {
            // Ignore messages from client if not in client authority mode
            if (!clientAuthority)
                return;

            // handle and broadcast
            // host should have already the trigger
            bool isHostOwner = isClient && isOwned;
            if (!isHostOwner)
            {
                HandleAnimResetTriggerMsg(hash);
            }

            RpcOnAnimationResetTriggerClientMessage(hash);
        }

        [Command]
        void CmdSetAnimatorSpeed(float newSpeed)
        {
            // set animator
            animator.speed = newSpeed;
            animatorSpeed = newSpeed;
        }

        #endregion

        #region client message handlers

        [ClientRpc]
        void RpcOnAnimationClientMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
        {
            using (NetworkReaderPooled networkReader = NetworkReaderPool.Get(parameters))
                HandleAnimMsg(stateHash, normalizedTime, layerId, weight, networkReader);
        }

        [ClientRpc]
        void RpcOnAnimationParametersClientMessage(byte[] parameters)
        {
            using (NetworkReaderPooled networkReader = NetworkReaderPool.Get(parameters))
                HandleAnimParamsMsg(networkReader);
        }

        [ClientRpc(includeOwner = false)]
        void RpcOnAnimationTriggerClientMessage(int hash)
        {
            HandleAnimTriggerMsg(hash);
        }

        [ClientRpc(includeOwner = false)]
        void RpcOnAnimationResetTriggerClientMessage(int hash)
        {
            HandleAnimResetTriggerMsg(hash);
        }

        #endregion
    }
}
