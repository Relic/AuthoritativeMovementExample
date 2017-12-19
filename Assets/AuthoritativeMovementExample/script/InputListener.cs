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
        #region Inspector
        [Tooltip("Send every input as it happens or use frameSyncRate?")]
        public bool sendSingleInputs = true;
        [Tooltip("Frequency of inputs to server (fixed update ticks) - ignored if Send Single Frames is true.")]
        public int frameSyncRate = 5;
        #endregion

        #region Public Properties
        [HideInInspector]
        public List<InputFrame> FramesToPlay;
        [HideInInspector]
        public List<InputFrame> FramesToReconcile;
        #endregion

        private uint _frameNumber;
        private List<InputFrame> _framesToSend;
        private InputFrame _inputFrame;
        private InputFrame _lastInputFrame;

        private void Start()
        {
            FramesToPlay = new List<InputFrame>();
            FramesToReconcile = new List<InputFrame>();
            _framesToSend = new List<InputFrame>();
        }

        private void Update()
        {
            /*
             * Poll the input in Update
             *
             * I've heard some discussions about this vs polling in
             * FixedUpdate on the Forge Networking Discord
             *
             * From what I can tell it is an opinionated choice
             * and there's no benefit/problem one way or the other,
             * although theoretically you are one input behind in
             * Update since FixedUpdate runs before Update per:
             * https://docs.unity3d.com/Manual/ExecutionOrder.html
             */
            _lastInputFrame = _inputFrame;
            _inputFrame = new InputFrame()
            {
                horizontal = Input.GetAxisRaw("Horizontal"),
                vertical = Input.GetAxisRaw("Vertical")
            };
        }

        private void FixedUpdate()
        {
            // If this is a client store and send the current polled input for processing
            if (!networkObject.IsServer)
            {
                _frameNumber++;

                // Only send the input if it has buttons pressed or has changed from the previous input state
                if (_inputFrame != null && (_inputFrame.HasInput || _lastInputFrame == null || _inputFrame != _lastInputFrame))
                {
                    _inputFrame.frameNumber = _frameNumber;

                    if (networkObject.IsOwner)
                    {
                        _framesToSend.Add(_inputFrame);
                        FramesToPlay.Add(_inputFrame);
                        FramesToReconcile.Add(_inputFrame);
                    }
                }

                // Send the input(s) if needed
                // I added an option to use the frame sync rate and send a List or
                // send every input and only send InputFrames
                if (_framesToSend.Count > 0 && (sendSingleInputs || _frameNumber % frameSyncRate == 0))
                {
                    byte[] bytes = {};
                    if (sendSingleInputs) bytes = ByteArrayUtilities.ObjectToByteArray(_framesToSend[0]);
                    else bytes = ByteArrayUtilities.ObjectToByteArray(_framesToSend);

                    networkObject.SendRpc(RPC_SYNC_INPUTS, Receivers.Server, new object[] { bytes });

                    if (sendSingleInputs) _framesToSend.RemoveAt(0);
                    else _framesToSend.Clear();
                }
            }
        }

        // RPC for receiving InputFrames from the clients
        public override void SyncInputs(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                var bytes = args.GetNext<Byte[]>();
                if (sendSingleInputs)
                {
                    InputFrame nextInputFrame = (InputFrame)ByteArrayUtilities.ByteArrayToObject(bytes);
                    FramesToPlay.Add(nextInputFrame);
                }
                else
                {
                    List<InputFrame> networkInputFrames = (List<InputFrame>)ByteArrayUtilities.ByteArrayToObject(bytes);
                    FramesToPlay.AddRange(networkInputFrames);
                }
            }
        }
    }
}