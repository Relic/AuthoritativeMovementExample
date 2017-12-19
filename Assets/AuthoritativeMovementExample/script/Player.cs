using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Linq;
using UnityEngine;

namespace AuthMovementExample
{
    /*
     * The networked player object
     * 
     * Server-owned but we can find the local owner by comparing the NetworkIds
     * 
     * Clientside does prediction and replaying of inputs from latest server update (reconciliation)
     * Serverside does processing of inputs sent from clients and has authority over the player object's state
     */
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : PlayerBehavior
    {
        #region Inspector
        [Tooltip("The movement speed of the player.")]
        public float Speed = 1.0f;
        #endregion

        private Rigidbody2D _rigidBody;

        private bool _setup = false;
        private bool _isLocalOwner = false;

        private InputFrame _currentInput = null;
        private InputListener _inputListener;
        private uint _lastPredictionFrame = 0;
        private uint _lastServerFrame = 0;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Set the networked fields in Update so we are
            // up to date per the last physics update
            if (networkObject.IsServer)
            {
                if (_currentInput != null) networkObject.frame = _currentInput.frameNumber;
                networkObject.position = transform.position;
            }
        }

        void FixedUpdate()
        {
            // Check if this client is the local owner
            _isLocalOwner = networkObject.MyPlayerId == networkObject.ownerNetId;

            #region Setup            
            // Initial setup - only do this once
            if ((_isLocalOwner || networkObject.IsServer) && !_setup)
            {
                // Interpolation on the predicted client and server does weird things
                // Only interpolate on non-owner clients
                networkObject.positionInterpolation.Enabled = false;
                _setup = true;
            }

            // Get the input listener if it doesn't exist and this isn't a remote client
            if (_inputListener == null) _inputListener = FindObjectsOfType<InputListener>().FirstOrDefault(x => x.networkObject.Owner.NetworkId == networkObject.ownerNetId);
            #endregion

            #region Netcode Logic
            // Server Authority - snap the position on all clients to the server's position
            if (!networkObject.IsServer)
            {
                transform.position = networkObject.position;
            }

            // Client owner Reconciliation & Prediction
            if (_isLocalOwner)
            {
                if (_inputListener != null)
                {
                    // Reconciliation - only do this if the server update is current or new
                    if (networkObject.frame != 0 && _lastServerFrame <= networkObject.frame)
                    {
                        _lastServerFrame = networkObject.frame;
                        Reconcile();
                    }

                    // Prediction
                    if (_inputListener.FramesToPlay.Count > 0)
                    {
                        InputFrame input = _inputListener.FramesToPlay[0];
                        _lastPredictionFrame = input.frameNumber;
                        PlayerUpdate(input);
                        _inputListener.FramesToPlay.RemoveAt(0);
                    }
                }
            }
            // Server Processing
            else if (networkObject.IsServer)
            {
                // Reset the current input - we don't want to re-use it if there no inputs in the queue
                _currentInput = null;

                if (_inputListener != null)
                {
                    // Process 1 input each frame - the same rate as the client
                    if (_inputListener.FramesToPlay.Count > 0)
                    {
                        _currentInput = _inputListener.FramesToPlay[0];
                        
                        // Try-catch is a good idea to handle weird serialization/deserialization errors
                        try
                        {
                            PlayerUpdate(_currentInput);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e + " (Serverside input processing - Player.cs line 104)");
                        }
                        _inputListener.FramesToPlay.RemoveAt(0);
                    }
                }
            }
            #endregion
        }

        private void Move(InputFrame input)
        {
            _rigidBody.velocity = new Vector2(input.horizontal, input.vertical) * Speed;
        }

        private void PlayerUpdate(InputFrame input)
        {
            _rigidBody.velocity = Vector2.zero;
            if (input.HasInput) Move(input);
        }

        private void Reconcile()
        {
            // Remove any inputs up to and including the last input processed by the server
            _inputListener.FramesToReconcile.RemoveAll(f => f.frameNumber <= networkObject.frame);
            
            // Replay them all back to the last input processed by client prediction
            if (_inputListener.FramesToReconcile.Count > 0)
            {
                // Turn off (and back on) auto simulation for similar reasons to the server
                Physics2D.autoSimulation = false;
                foreach (InputFrame input in _inputListener.FramesToReconcile)
                {
                    // Don't replay frames that haven't been predicted yet if there are any
                    if (input.frameNumber > _lastPredictionFrame) break;
                    PlayerUpdate(input);
                    Physics2D.Simulate(Time.fixedDeltaTime);
                }
                Physics2D.autoSimulation = true;
            }
        }
    }
}