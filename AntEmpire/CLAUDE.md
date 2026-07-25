# Ant Empire: Rise of the Queen

## Project Goal

Build a mobile 3D idle/colony-management game about ants.

The game focuses on retention:
- visible colony growth
- ant evolution with 5 visual stages
- offline progress
- daily and random events
- satisfying resource collection
- simple but expandable systems

Initial target platform:
- Android first
- iOS later

Engine:
- Unity
- C#
- Mobile-first performance

## Game Concept

The player starts with a queen and a tiny underground colony.
Ants collect resources on the surface and bring them back to the nest.
The player upgrades rooms, evolves ants, unlocks new ant roles, expands tunnels, and protects the colony from enemies.

## MVP Scope

The first playable version must include:

1. Boot scene
2. Main menu
3. Underground scene
4. Surface scene
5. Queen system
6. Worker ant system
7. Resource collection
8. Resources: Food, Leaves, Soil, DNA
9. Three rooms:
   - Queen Chamber
   - Food Storage
   - Larva Nursery
10. Worker ant evolution levels 1–5
11. Save/load system
12. Offline progress
13. One enemy event: Spider Attack
14. Basic HUD

## Architecture Rules

Use feature-based folders:

Assets/_Project/Scripts/Core
Assets/_Project/Scripts/SaveSystem
Assets/_Project/Scripts/Economy
Assets/_Project/Scripts/Ants
Assets/_Project/Scripts/Colony
Assets/_Project/Scripts/Rooms
Assets/_Project/Scripts/Resources
Assets/_Project/Scripts/Combat
Assets/_Project/Scripts/Events
Assets/_Project/Scripts/UI
Assets/_Project/Scripts/OfflineProgress
Assets/_Project/Scripts/Analytics
Assets/_Project/Scripts/Ads

## Coding Rules

- Use clean C#.
- Keep MonoBehaviour classes thin.
- Put game logic in plain C# services where possible.
- Use ScriptableObjects for configuration data.
- Do not use ScriptableObjects for player save data.
- Save player progress to JSON.
- All IDs must be stable strings, not display names.
- Do not hardcode upgrade values in MonoBehaviours.
- Use events or simple signals for UI updates.
- Mobile performance is important.
- Avoid expensive Update loops.
- Prefer coroutines, timers, or state machines.
- Do not create multiplayer.
- Do not create complex 3D art systems in MVP.
- Do not add unnecessary packages unless asked.
- Unity's JsonUtility cannot serialize dictionaries — SaveData uses
  serializable key/value entry lists instead. Keep it that way unless a
  JSON library is explicitly added.

## Core Systems

### Save System

Must save:
- resources
- ant counts
- ant evolution levels
- room levels
- queen level
- last save timestamp
- tutorial state

### Economy

Resources:
- Food
- Leaves
- Soil
- DNA

### Ant State Machine

Worker ant states:
- Idle
- SearchResource
- MoveToResource
- CollectResource
- ReturnToNest
- DepositResource

### Offline Progress

When the player returns, calculate rewards based on:
- time since last save
- worker count
- worker level
- storage capacity
- production modifiers

Show an offline rewards popup.

### Events

MVP event:
- Spider Attack

Later:
- Honey Drop
- Rain Event
- Golden Leaf
- Rare Beetle
- DNA Mutation Day

## Retention Requirements

Every early session should give the player something visible:
- new ant
- new room
- new tunnel
- new resource
- new evolution level
- new event

The first 10 minutes must be fast and rewarding.

## Art Direction

3D low-poly or stylized mobile-friendly look.
Ants must have visible evolution changes.
Underground colony should feel alive.
Surface should include leaves, tree roots, stones, and insects.

## What Not To Build Yet

Do not build:
- multiplayer
- large open world
- complex combat
- gacha
- clan systems
- NFT/blockchain
- advanced ads mediation
- huge procedural world
- complex story campaign

Focus on a working, fun MVP first.
