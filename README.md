# Riptide FNA Tank
An FNA Game featuring a networked client / server multiplayer model using the Riptide network library.

## Overview

A short demo project of the Atari classic, Tank - highlighting three core concepts:
- A simple 2d game based on the **FNA Framework** low level game library - no Unity or Godot here.
- A **pure** ECS for the world / entity management.
- A **client / server** networked multiplayer element.

### ECS 

> Entity component system (ECS) is a software architectural pattern mostly used in video game development for the representation of game world objects. An ECS comprises entities composed from components of data, with systems which operate on entities' components.
> 
> ECS follows the principle of composition over inheritance, meaning that every entity is defined not by a type hierarchy, but by the components that are associated with it. Systems act globally over all entities which have the required components. 

[MoonTools.ECS](https://gitea.moonside.games/MoonsideGames/MoonTools.ECS) is in the author's own words, "A very simple ECS system."

It could be considered a 'Pure' ECS:
- **E**ntity - Nothing more than a number - acts as an 'indexer' into the various component collections.
- **C**omponent - Data, no behaviour - components in MoonTools.ECS are limited to unmanaged values types only, no class references are allowed here.
- **S**ystem - Functions that operate on entities that conform to a certain set of components.
  
  > e.g. A system to move entities in the world could query for entities with both Position and Velocity components.

### Network Multiplayer

Implemented using a client / (authoritative) server approach based on the [Overwatch Gameplay Architecture and Netcode](https://www.youtube.com/watch?v=W3aieHjyNvw).

> Overwatch is a multimedia franchise centered on a series of multiplayer first-person shooter video games developed by Blizzard Entertainment. Overwatch was released in 2016 with a successor, Overwatch 2, released in 2022.

The engine showcases the following features:
* 100% server authority.
* Client-side prediction and reconciliation.
* Lag compensation inc sever rollback `No need to lead shots`.

The Overwatch implementation is based on (amonst other things) the [Quake-III-Arena](https://github.com/id-Software/Quake-III-Arena/) networking model.

> Quake III Arena is a 1999 multiplayer-focused first-person shooter developed by id Software

* [Quake III Arena](https://github.com/id-Software/Quake-III-Arena/) networking model

* [Quake 3 Source Code Review: Network Model](https://fabiensanglard.net/quake3/network.php) _by Fabien Sanglard_

## Getting Started

Clone the source code and build the app - all dependencies should be included in the project.

You should then be able to launch two instances of the game to test locally.

## Credits

Inspired by:
- [Tank](https://en.wikipedia.org/wiki/Tank_(video_game)) by Atari

Frameworks:
- [FNA](https://github.com/FNA-XNA/FNA) - _An XNA4 reimplementation that focuses solely on developing a fully accurate XNA4 runtime for the desktop._
- [MoonTools.ECS](https://gitea.moonside.games/MoonsideGames/MoonTools.ECS) _by MoonsideGames_
- [Riptide](https://github.com/RiptideNetworking/Riptide) - _Riptide Networking is a lightweight C# networking library primarily designed for use in multiplayer games. It can be used in Unity as well as in other .NET environments such as console applications._

Fonts:
- [Squared Display](https://www.dafont.com/squared-display.font) _by Vikas Kumar_
