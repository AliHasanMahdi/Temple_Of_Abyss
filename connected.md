# Connected Systems

This file records the gameplay connections that were added so the merged branch works as one connected set of mechanics instead of separate branch-specific systems.

## 1. Shared interaction flow

The project now uses one common interaction path for the main gameplay mechanics:

- `PlayerInteraction` raycasts from the player view and looks for `IPlayerInteractable`
- `IPlayerInteractable` is the common contract for gameplay objects that can be used with `E`
- `Interactable` now implements `IPlayerInteractable`, so the older generic interactable objects still work

This means the same prompt and input path is now used by:

- key pickups
- main door interactions
- Level 5 key/gem/door mechanics
- lever and button interactions
- existing generic interactables

## 2. Interaction and UI/inventory protection

`PlayerInteraction` was updated so it stops interaction while:

- the pause menu is open
- the inventory UI is open
- the game is time-scaled to `0`

This prevents using doors, pickups, or puzzle objects while the inventory is open.

`InventoryManager` now exposes `IsOpen` so the interaction system can check it directly.

## 3. Key pickup -> player keys -> inventory -> save system

`AN_DoorKey` was moved off its own local `E` polling and now uses the shared interaction system.

When a key is collected, it now updates all connected systems together:

1. adds the key to `AN_HeroInteractive`
2. refreshes the visible inventory through `InventoryManager`
3. registers the pickup with `SaveSystem` as pending until checkpoint save
4. shows HUD feedback through `HUDManager`

Result:

- the player actually owns the key
- the inventory UI reflects the key count
- the key pickup survives correctly after checkpoints
- if the player dies before checkpoint, the key can still respawn as intended

## 4. Main doors -> key usage -> inventory sync -> save system

`AN_DoorScript` now uses the shared interaction system instead of handling `E` itself.

Its key logic is connected to the real player inventory flow:

- doors consume keys through `AN_HeroInteractive.UseKey(...)`
- after use, `InventoryManager.SyncKeysFromPlayer()` updates the inventory UI
- unlocked doors are registered into `SaveSystem` so checkpoint saves preserve progress
- failed unlock attempts show HUD feedback

Result:

- key counts decrease correctly
- the inventory UI updates immediately
- unlocked doors stay consistent with checkpoint saving

## 5. Level 5 doors and quest items

The Level 5 scripts were connected to the same interaction path:

- `Level5AN_DoorScript`
- `Level05QuestItem`
- `Level05OfferingPedestal`
- `Level05PuzzlePedestal`
- `Level05ChamberDoor`

These now use the shared player interaction and are tied into inventory/HUD behavior:

- quest items collect into `PersistentInventory`
- pedestals consume required gems from `PersistentInventory`
- chamber doors consume their required special keys from `PersistentInventory`
- every successful change refreshes `InventoryManager`
- failure/success messages go through `HUDManager`

Result:

- Level 5 pickups, puzzle rewards, offerings, and doors now behave like one connected system
- inventory display stays in sync with the actual held items

## 6. Buttons and levers

`AN_Button` was connected to the shared interaction system for normal buttons and levers.

Pressing `E` through the interaction system can now:

- trigger remote doors
- toggle lever animation
- fire connected spike targets

This makes buttons/levers use the same prompt-and-raycast interaction model as doors and pickups.

## 7. Intentional exception: valves

Valves were left on hold interaction instead of being forced into the single-press interaction path.

Reason:

- valves are continuous controls
- they need hold behavior, not a one-frame press event

So the valve path still uses hold-to-rotate, while standard buttons/levers use the shared interact system.

## 8. Duplicate script cleanup

An unused duplicate `Assets/AN_DoorScript.cs` was removed.

The scenes and prefabs reference the active door script in:

- `Assets/Scripts/Door/AN_DoorScript.cs`

Removing the duplicate avoids hidden compile/integration problems and prevents stale code from expecting old APIs that no longer exist.

## 9. Current connected gameplay chain

The main connected chain is now:

`PlayerInteraction`
-> `IPlayerInteractable`
-> pickup/door/button/pedestal/chamber object
-> inventory state (`AN_HeroInteractive` or `PersistentInventory`)
-> inventory UI (`InventoryManager`)
-> player feedback (`HUDManager`)
-> persistence (`SaveSystem` where applicable)

## 10. Manual checks to do in Unity

Recommended quick checks after opening Unity:

1. collect a red/blue key and confirm the inventory count updates
2. unlock a key door and confirm the key count decreases
3. open the inventory and confirm you cannot interact while it is open
4. collect a Level 5 quest item and confirm it appears in the inventory UI
5. use a Level 5 gem pedestal and confirm the gem disappears from inventory
6. use a lever/button and confirm it still drives the linked door/trap
7. test a valve separately and confirm hold interaction still works

## 11. Remaining legacy note

There is still an older unused script at:

- `Assets/Scripts/Level5 Scripts/Door.cs`

It still has its own local `E` polling, but it is not referenced by the active scene/prefab links I checked. The connected gameplay path now uses the newer Level 5 scripts listed above.
