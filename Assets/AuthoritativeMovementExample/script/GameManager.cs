using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections.Generic;

namespace AuthMovementExample
{
    /// <summary>
    /// The singleton Game object which oversees the whole game state
    /// </summary>
    public class GameManager : GameManagerBehavior
    {
        // Singleton instance
        public static GameManager Instance;

        // List of players
        private readonly Dictionary<uint, PlayerBehavior> _playerObjects = new Dictionary<uint, PlayerBehavior>();

        private bool _networkReady;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            DontDestroyOnLoad(Instance);
        }
        
        protected override void NetworkStart()
        {
            base.NetworkStart();

            if (NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Networker.playerAccepted += (player, sender) =>
                {
                    // Instantiate the player on the main Unity thread, get the Id of its owner and add it to a list of players
                    MainThreadManager.Run(() =>
                    {
                        PlayerBehavior p = NetworkManager.Instance.InstantiatePlayer();
                        p.networkObject.ownerNetId = player.NetworkId;
                        _playerObjects.Add(player.NetworkId, p);
                    });
                };

                NetworkManager.Instance.Networker.playerDisconnected += (player, sender) =>
                {
                    // Remove the player from the list of players and destroy it
                    PlayerBehavior p = _playerObjects[player.NetworkId];
                    _playerObjects.Remove(player.NetworkId);
                    p.networkObject.Destroy();
                };
            }
            else
            {
                // This is a local client - it needs to listen for input
                NetworkManager.Instance.InstantiateInputListener();
            }

            _networkReady = true;
        }
		
		// Force NetworkStart to happen - a work around for NetworkStart not happening
        // for objects instantiated in scene in the latest version of Forge
        private void FixedUpdate()
        {
            if (!_networkReady && networkObject != null)
            {
                NetworkStart();
            }
        }
    }
}