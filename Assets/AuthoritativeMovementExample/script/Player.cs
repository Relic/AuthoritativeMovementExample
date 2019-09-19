using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Linq;
using UnityEngine;

namespace AuthMovementExample
{
    /// <summary>
    ///		The networked component of the Player object
    ///		Handles all communications over the network
    ///		and updating of the entity's state
    /// </summary>
    public class Player : PlayerBehavior
    {
        #region Inspector
        [Tooltip("The movement speed of the player.")]
        public float Speed = 1.0f;
        [Tooltip("Sibling MonoBehavior representing the view.")]
        public GameObject View;
        #endregion

        private Rigidbody2D _rigidBody;
        private Collider2D _collider2D;
        private ContactFilter2D _noFilter;
        private readonly Collider2D[] _collisions = new Collider2D[20];

        private bool _networkReady;
        private bool _initialized;
        private bool _isLocalOwner;

        private InputFrame _currentInput;
        private InputListener _inputListener;

        // Last frame that was processed locally on this machine
        private uint _lastLocalFrame;
        // Last frame that was sent (server)/received (client) on the network
        private uint _lastNetworkFrame;
        // Calculates the current error between the Simulation and View
        private Vector2 _errorVector = Vector2.zero;
        // The interpolation timer for error interpolation
        private float _errorTimer;

        private void Awake()
        {
            _rigidBody = GetComponentInChildren<Rigidbody2D>();
            _collider2D = GetComponentInChildren<Collider2D>();
            _noFilter = new ContactFilter2D().NoFilter();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            _networkReady = true;
        }

        /// <summary>
        ///		Initialization method, sets all the initial parameters
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (!networkObject.IsServer)
            {
                if (networkObject.ownerNetId == 0)
                {
                    _initialized = false;
                    return _initialized;                    
                }
            }

            _isLocalOwner = networkObject.MyPlayerId == networkObject.ownerNetId;

            if (_isLocalOwner || networkObject.IsServer)
            {
                networkObject.positionInterpolation.Enabled = false;
                if (_inputListener == null)
                {
                    _inputListener = FindObjectsOfType<InputListener>()
                        .FirstOrDefault(x => x.networkObject.Owner.NetworkId == networkObject.ownerNetId);
                    if (_inputListener == null)
                    {
                        _initialized = false;
                        return _initialized;
                    }
                }
            }

            _initialized = true;
            return _initialized;
        }

        /// <summary>
        ///		Unity's Update method.
        ///		Handles network synchronization and Update of EntityState
        /// </summary>
        private void Update()
        {
            if (!_networkReady || !_initialized) return;
            
            // Set the networked fields in Update so we are
            // up to date per the last physics update
            if (networkObject.IsServer)
            {
                if (_lastNetworkFrame < _lastLocalFrame)
                {
                    networkObject.position = _rigidBody.position;
                    
                    _lastNetworkFrame = _lastLocalFrame;
                    networkObject.frame = _lastLocalFrame;
                } 
            } 
            // The local player has to smooth away some error
            else if (_isLocalOwner)
            {
                CorrectError();
            }
            // Update frame numbers and authoritatively set position if It's a remote player
            else
            {
                _lastLocalFrame = _lastNetworkFrame = networkObject.frame;
                View.transform.position = new Vector3(_rigidBody.position.x, _rigidBody.position.y, View.transform.position.z);
            }
        }

        /// <summary>
        ///		Unity's FixedUpdate method
        ///		Handles prediction, server processing, reconciliation
        ///		& FixedUpdate of state
        /// </summary>
        void FixedUpdate()
        {
            // Don't start until initialization is done, stop updating if the input is lost
            if (!_networkReady) return;
            if (!_initialized && !Initialize()) return;
            if ((networkObject.IsServer || _isLocalOwner) && _inputListener == null) return;

            #region Netcode Logic
            // Server Authority - snap the position on all clients to the server's position
            if (!networkObject.IsServer)
            {
                _rigidBody.position = networkObject.position;

                if (_isLocalOwner && networkObject.frame != 0 && _lastNetworkFrame <= networkObject.frame)
                {
                    _lastNetworkFrame = networkObject.frame;
                    Reconcile();
                }
            }

            if (!_isLocalOwner && !networkObject.IsServer) return;
            
            // Local client prediction & server authoritative logic
            if (_inputListener.FramesToPlay.Count <= 0) return;
            _currentInput = _inputListener.FramesToPlay.Pop();
            _lastLocalFrame = _currentInput.frameNumber;

            // Try to do a player update (if this fails, something's weird)
            try
            {
                PlayerUpdate(_currentInput);
            }
            catch (Exception e)
            {
                Debug.LogError("Malformed input frame.");
                Debug.LogError(e);
            }
            
            // Reconciliation only happens on the local client
            if (_isLocalOwner && !networkObject.IsServer) _inputListener.FramesToReconcile.Add(_currentInput);
            #endregion
        }

        /// <summary>
        /// Move the player's simulation (rigid body)
        /// </summary>
        /// <param name="input"></param>
        private void Move(InputFrame input)
        {
            // Move the player, clamping the movement so diagonals aren't faster
            Vector2 translation =
                Vector2.ClampMagnitude(new Vector2(input.horizontal, input.vertical) * Speed * Time.fixedDeltaTime, Speed);
            _rigidBody.position += translation;
            _rigidBody.velocity = translation;
        }

        /// <summary>
        /// Detect and resolve collisions with a simple overlap check
        /// </summary>
        private void PhysicsCollisions()
        {
            // We don't want to be pushed if we aren't moving.
            if (_rigidBody.velocity == Vector2.zero) return;
            
            // Collision detection - get a list of colliders the player's collider overlaps with
            int numColliders = Physics2D.OverlapCollider(_collider2D, _noFilter, _collisions);

            // Collision Resolution - for each of these colliders check if that collider and the player overlap
            for (int i = 0; i < numColliders; ++i)
            {
                ColliderDistance2D overlap = _collider2D.Distance(_collisions[i]);

                // If the colliders overlap move the player
                if (overlap.isOverlapped) _rigidBody.position += overlap.normal * overlap.distance;
            }
        }

        /// <summary>
        /// Player update composed of movement and collision processing
        /// </summary>
        /// <param name="input"></param>
        private void PlayerUpdate(InputFrame input)
        {
            // Set the velocity to zero, move the player based on the next input, then detect & resolve collisions
            _rigidBody.velocity = Vector2.zero;
            if (input != null && input.HasInput)
            {
                Move(input);
                PhysicsCollisions();
            }
        }

        /// <summary>
        ///		Reconcile inputs that haven't yet been
        ///		authoritatively processed by the server
        /// </summary>
        private void Reconcile()
        {
            // Remove any inputs up to and including the last input processed by the server
            _inputListener.FramesToReconcile.RemoveAll(f => f.frameNumber < networkObject.frame);
            
            // Replay them all back to the last input processed by client prediction
            if (_inputListener.FramesToReconcile.Count > 0)
            {
                for (Int32 i = 0; i < _inputListener.FramesToReconcile.Count; ++i)
                {
                    _currentInput = _inputListener.FramesToReconcile[i];
                    PlayerUpdate(_currentInput);
                }
            }

            // The error vector measures the difference between the predicted & server updated sim position (this one)
            // and the view position (the position of the MonoBehavior holding your renderer/view)
            _errorVector = _rigidBody.position - (Vector2)View.transform.position;
            _errorTimer = 0.0f;
        }

        /// <summary>
        ///		Interpolate away errors between the simulation
        ///		and render positions of the entity over time
        /// </summary>
        private void CorrectError()
        {
            // If we have a measurable error
            if (_errorVector.magnitude >= 0.00001f)
            {
                // Determine the weight, or amount we interpolate towards the simulation position
                float weight = Math.Max(0.0f, 0.75f - _errorTimer);
               
                // Interpolate towards the simulation position
                Vector2 newViewPosition = (Vector2)View.transform.position * weight + _rigidBody.position * (1.0f - weight);
                View.transform.position = new Vector3(newViewPosition.x , newViewPosition.y, View.transform.position.z);
               
                // Increase the timer - makes the weight smaller meaning more weight towards the simulation position
                // This is so that the bigger the error gets, or the longer it takes to smooth,
                // the more is smoothed away on the next frame
               _errorTimer += Time.fixedDeltaTime;

                // New error vector, always the difference between sim and view
                _errorVector = _rigidBody.position - (Vector2)View.transform.position;

                // If the error is REALLY small we can discount the rest
                if (_errorVector.magnitude >= 0.00001f) return;
                _errorVector = Vector2.zero;
                _errorTimer = 0.0f;
            }
            else
            {
                View.transform.position = new Vector3(_rigidBody.position.x, _rigidBody.position.y, View.transform.position.z);
            }
        }
    }
}
