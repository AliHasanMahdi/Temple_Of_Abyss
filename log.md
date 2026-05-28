# Sprint Log 4

## Interaction System
Approximate time: 1.5 hours

Implemented the interaction layer for gameplay objects. Added reusable `Interactable`, `PickupInteractable`, `DoorInteractable`, and `CheckpointInteractable` scripts, and connected them to an aim-based prompt flow so the player sees `Press E ...` text when targeting interactable objects.

## Player State Visuals
Approximate time: 1.0 hour

Added simple visual player state feedback using `PlayerVisualAnimator`. The cylinder player body now reflects core movement states such as idle, walk, run, jump, and death to make movement and status changes more readable during testing.

## Level01_Entrance Sandbox And Collectible Setup
Approximate time: 2.0 hours

Added sandbox/test content for `Level01_Entrance` specific to this branch. This includes collectible placement support, door and checkpoint interaction setup, and sandbox geometry content intended to make the level more usable for testing movement and interaction systems.

## Collectible Behavior
Approximate time: 0.5 hours

Added collectible gameplay behavior so the player can aim at pickups, see a collect prompt, press `E` to collect them, and remove them during play once collected.

## Task Tracking
Approximate time: 0.5 hours

Updated project task tracking by adding and maintaining `todo.md` entries for the completed work on this branch.

## Total Approximate Time
5.5 hours

# Sprint Log 7

## Power-Up System Foundation
Approximate time: 1.5 hours

Implemented the reusable power-up framework for collectible-triggered gameplay effects. Added `PlayerPowerUps` and connected it into `PlayerMovement` so the player can receive limited-use `DoubleJump` and timed `SpeedBoost` effects immediately on pickup. The wall pass-through idea was removed from the implementation.

## Branch Mechanics Integration
Approximate time: 3.0 hours

Unified the imported systems from the merged branches so door keys, inventory key counts, door unlocking, HUD prompts, checkpoints, and Level 5 item flow work through the same connected gameplay path instead of separate disconnected mechanics.

## Bulls And Cows Utility
Approximate time: 0.5 hours

Added the `BullsAndCowsGame` helper with five-slot unique number generation, per-position guess result evaluation, and simple increment/decrement helpers for future puzzle or minigame use.

## Level 4 Bulls And Cows Puzzle Integration
Approximate time: 3.0 hours

Integrated the Bulls and Cows system into `Level04_Vault` using real scene controls for increase, decrease, and submit. Added remembered digit-color feedback, solved audio, submit disable on success, and puzzle-driven release behavior for a dropped key.

## Level 4 Lever Sequence Puzzle
Approximate time: 1.5 hours

Implemented a lever-based Level 4 puzzle that generates a random true/false sequence across the lever set, guarantees at least one `true`, prints the sequence to the console for debugging, and releases `Wall_A (78)` plus a dropped key when the lever states match.

## Level 4 Unique Door-Key Progression
Approximate time: 2.0 hours

Finished the Level 4 progression wiring so the exit-side door and the two dropped-key doors use the shared interaction path correctly. The two puzzle reward doors now use distinct key-door IDs, `Door_092/Key_092` and `Door_095/Key_095`, so each dropped key is tied to its own door only.

## Main Menu UI Cleanup
Approximate time: 0.5 hours

Added a runtime fallback for the main menu so missing central/button images no longer appear as large white UI boxes, keeping the menu readable and usable while the sprite references are being stabilized.

## Current Ongoing Work
Approximate time remaining: 12.0+ hours

Scene placement, balancing, and testing for the new Level 4 power-up pickups are still not finished, and the larger Sprint 5 to Sprint 8 level-building work remains in progress.

## Total Approximate Time
5.5 hours completed in Sprint 4
12.0 hours completed in Sprint 7 follow-up tasks
