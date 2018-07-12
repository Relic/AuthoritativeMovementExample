using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AuthMovementExample
{
    /*
     * Polls the client's input and send its to the server
     * 
     * Client-owned
     */
    public class InputListener : InputListenerBehavior
    {
        #region Public Properties
        [HideInInspector]
        public List<InputFrame> FramesToPlay;
        [HideInInspector]
        public List<InputFrame> FramesToReconcile;
        #endregion

        private bool _networkReady;
        
        private uint _frameNumber;
        private InputFrame _inputFrame = InputFrame.Empty;

        private void Start()
        {
            FramesToPlay = new List<InputFrame>();
            FramesToReconcile = new List<InputFrame>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            _networkReady = true;
        }

        /// <summary>
        ///		Polling of inputs is handled in Update since they
        ///		reset every frame making weird things happen in FixedUpdate
        /// </summary>
        private void Update()
        {
            if (!_networkReady) return;

            if (!networkObject.IsServer && networkObject.IsOwner)
            {
                _inputFrame = new InputFrame
                {
                    right = Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.RightArrow),
                    down = Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.DownArrow),
                    left = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.LeftArrow),
                    up = Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.UpArrow),
                    horizontal = Input.GetAxisRaw("Horizontal"),
                    vertical = Input.GetAxisRaw("Vertical")
                };   
            }
        }

        /// <summary>
        ///		Store the input polled from Update and send it
        ///		to the server for authoritative processing
        /// </summary>
        private void FixedUpdate()
        {
            if (!_networkReady) return;
            
            // If this is a client store and send the current polled input for processing
            if (!networkObject.IsServer && networkObject.IsOwner)
            {
                _inputFrame.frameNumber = _frameNumber++;
                FramesToPlay.Add(_inputFrame);

                byte[] bytes = ByteArray.Serialize(_inputFrame);
                networkObject.SendRpc(RPC_SYNC_INPUTS, Receivers.Server, bytes);
            }
        }

        /// <summary>
        ///		Send input state to the server for processing
        /// </summary>
        /// <param name="args"></param>
        public override void SyncInputs(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                var bytes = args.GetNext<Byte[]>();
                InputFrame newest = (InputFrame) ByteArray.Deserialize(bytes);
                FramesToPlay.Add(newest);
            }
        }
    }
}