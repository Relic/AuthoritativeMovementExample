# Forge Networking Remastered Authoritative Movement Example

Created using:

- Unity 2017.2.0f3
- Forge Networking Remastered - Github build

## What is this?

An implementation of Authoritative Server Movement in Forge Networking Remastered.

The example shows client-side prediction and reconciliation of inputs, and server-side authoritative processing (the server has the final say on game state).

## How to use it?

Clone the project and open it in Unity. Import the latest Github version of Forge Networking Remastered (this project uses [this commit](https://github.com/BeardedManStudios/ForgeNetworkingRemastered/commit/9fe861ec6f29751d74a15add1679c6a343e22e89)).

Build and run two or more instances of the build (ensure the two scenes are Forge's MultiplayerMenu.unity and this example's AuthoritativeMovementExample.unity).

Host on one instance and then join on the others and you can move on any client that didn't host using the Arrow keys.

## Functional Overview

This project is a very similar implementation to [g-klein's AuthoritativeMovementDemo](https://github.com/g-klein/ForgeAuthoritativeMovementDemo).

The main difference is how reconciliation is done in that an RPC is not used to send the game states processed on the server to the client. Instead the unreliable network object fields are used since missing one or two server updates isn't a big deal.

Also, some optimizations were done in both the Player and InputListener for reconciliation and when Inputs should be sent.

Additionally, this build is deterministic and Physics2D (not sure about 3D, untested) rigid body physics work just fine as far as I've tested so far.

## Resources

[g-klein's AuthoritativeMovementDemo](https://github.com/g-klein/ForgeAuthoritativeMovementDemo)

[Gabriel Gambetta](http://www.gabrielgambetta.com/client-server-game-architecture.html)

[Gaffer On Games'](https://gafferongames.com/)

[Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

## Disclaimer/Issues

This is a demo build and is by no means perfect. Use this at your own risk and take the time to understand how it works. Making your own improvements is encouraged and if you feel that you can contribute something back to this example feel free to contact me on the Forge Networking Discord or submit an issue.

**Thanks to the members of the Forge Networking Discord who helped me extensively with ideas, discussion and feedback as I worked on this.**