# Forge Networking Remastered Authoritative Movement Example

Created using:

- Unity 2017.2.0f3
- Forge Networking Remastered - Github build

## What is this?

An implementation of Authoritative Server Movement in Forge Networking Remastered.

The example shows client-side prediction and reconciliation of inputs, and server-side authoritative processing (the server has the final say on game state).

## How to use it?

Clone the project and open it in Unity. Import the latest Github version of Forge Networking Remastered (this project uses [this commit](https://github.com/BeardedManStudios/ForgeNetworkingRemastered/commit/9fe861ec6f29751d74a15add1679c6a343e22e89)). **NOTE: when you import Forge Networking Remastered do NOT import "Assets/Beaded Man Studios Inc/Generated" or you will nuke the generated network object implementations!**

If needed, add the prefabs of the network objects (GameManager, InputListener, Player) to the NetworkManager prefab.

Build and run two or more instances of the build (ensure the two scenes are Forge's MultiplayerMenu.unity and this example's AuthoritativeMovementExample.unity).

Host on one instance and then join on the others and you can move on any client that didn't host using the Arrow keys.

## Functional Overview

This project is a very similar implementation to [g-klein's AuthoritativeMovementDemo](https://github.com/g-klein/ForgeAuthoritativeMovementDemo).

The main difference is how reconciliation is done in that an RPC is not used to send the game states processed on the server to the client. Instead the unreliable network object fields are used. Since missing one or two server updates isn't a big deal, RPCs aren't needed.

Also, some optimizations were done in both the Player and InputListener for reconciliation and when/how Inputs should be sent/processed.

Additionally, this build is deterministic. ~~and Physics2D (not sure about 3D, untested) rigid body physics work as far as I've tested so far.~~ **I've changed the build to decouple Unity's Physics2D. The build now uses a very simple collision detection & resolution system using the Collider2D.OverlapCollider and Collider2D.Distance methods and direct updating of the rigidbody's position. I did this so I could simulate physics on individual objects allowing all inputs in the queue to be processed at once server side.**

(Outdated - will do more testing soon.) This build was tested by me both locally (to/from localhost) and with a server hosted on a Linux VPS with ~100ms ping from my location. Clumsy Lagswitch was used to do further network testing (especially for higher pings).

## Changelog

July 13, 2018:

- Added error interpolation for movement to smooth away errors due to networked collision between players if both players are moving.
- Fixed some bugs (namely the spawning issue which was caused by using the playerConnected event instead of the playerAccepted event for spawning).
- Refactored code.

## Resources

[g-klein's AuthoritativeMovementDemo](https://github.com/g-klein/ForgeAuthoritativeMovementDemo)

[Gabriel Gambetta](http://www.gabrielgambetta.com/client-server-game-architecture.html)

[Gaffer On Games'](https://gafferongames.com/)

[Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

[Fast Paced Multiplayer Implementation: Smooth Server Reconciliation](https://fouramgames.com/blog/fast-paced-multiplayer-implementation-smooth-server-reconciliation#smoothing)

## To-do

1. Lag detection & handling - if excessive lag is detected:
	- The local client should mitigate processing time from excessive reconciliation, wait for the last processed server update and snap to there.
	- The server should stop processing inputs, extrapolate the player forward along the current movement direction until ping is either lowered (snap to position and start processing inputs again) or too high/too unstable (drop the player).

3. Add a small time delay to incoming inputs on the server to smooth out processing processing (this may not actually be required, Forge may already do this, but I want to experiement a little time permitting).

## Known Issues

1. At high pings, non-owner clients see choppiness in the interpolation. Smoother interpolation which adapts to the ping is needed.

## Disclaimer

This is a demo build and is by no means perfect. Use this at your own risk and take the time to understand how it works. Making your own improvements is encouraged and if you feel that you can contribute something back to this example feel free to contact me on the Forge Networking Discord or submit an issue.

**Thanks to the members of the Forge Networking Discord who helped me extensively with ideas, discussion and feedback as I worked on this.**
