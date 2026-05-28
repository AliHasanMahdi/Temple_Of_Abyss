# Work Unique To My Branch

- Interaction System
  Approximate time: 1.5 hours
  Added reusable interaction scripts that are not present on the other branches: `Interactable`, `PickupInteractable`, `DoorInteractable`, and `CheckpointInteractable`. This also includes the aim-based `Press E ...` interaction prompt flow.

- Player State Visuals
  Approximate time: 1.0 hour
  Added the player visual state logic for idle, walk, run, jump, and death feedback on the cylinder body through `PlayerVisualAnimator`.

- Level01_Entrance Sandbox And Collectible Setup
  Approximate time: 2.0 hours
  Added sandbox/test content for `Level01_Entrance` that is specific to this branch, including scene/bootstrap setup for collectibles, door/checkpoint interactions, and sandbox geometry support.

- Collectible Behavior
  Approximate time: 0.5 hours
  Added collectible-specific behavior so aiming at pickups shows a collect prompt and pressing `E` collects them and removes them during play.

- Task Tracking
  Approximate time: 30 minutes
  Added and updated `todo.md` for completed work tracking, which is not present on the other branches.

- Branch Merging And Conflict Resolution
  Approximate time: 4.5 hours
  Merged `main` and the other project branches into `ahmed` branch, resolved conflicts across shared gameplay scripts and Unity scene content, and unified the imported mechanics so inventory, keys, doors, checkpoints, HUD flow, and Level 5 systems can be continued from one branch.

- Mechanics Integration Across Branches
  Approximate time: 3.0 hours
  Connected the imported gameplay systems so interaction, door keys, inventory key counts, door unlock behavior, checkpoints, HUD prompts, and Level 5 item flow work through the same shared gameplay path.

- Power-Up System Foundation
  Approximate time: 1.5 hours
  Added a reusable power-up system for instant collectible pickups. The current finished mechanics are `DoubleJump` with limited uses and `SpeedBoost` with time-based duration, wired through `PlayerPowerUps`, `PlayerMovement`, and `PowerUpPickup`.

- Bulls And Cows Utility
  Approximate time: 0.5 hours
  Added the `BullsAndCowsGame` utility with five-slot unique number generation, per-slot guess evaluation, and simple increment/decrement helpers for use in a minigame or puzzle.

- Main Menu Button Graphic Fallback
  Approximate time: 0.5 hours
  Added a runtime fallback in `MainMenu` so missing center/menu button graphics no longer appear as a large white box and the main menu stays usable while UI art references are unstable.

- Level 4 Bulls And Cows Scene Puzzle
  Approximate time: 3.0 hours
  Integrated the Bulls and Cows helper into `Level04_Vault` with real scene controls for increment, decrement, and submit. Added remembered color states per slot value, solved feedback audio, submit disable on success, and treasure/key release behavior tied to puzzle completion.

- Level 4 Exit Door And HUD Wiring
  Approximate time: 1.0 hour
  Wired the Level 4 exit door into the shared interaction flow so it can show a `Press E ...` prompt and move the player to `Level05_Chamber`. Also connected the Level 4 scene to the shared HUD and prompt setup needed by the existing gameplay UI path.

- Level 4 Lever Sequence Puzzle
  Approximate time: 1.5 hours
  Added a dedicated Level 4 lever puzzle that generates a true/false sequence of five levers, guarantees it is not all false, prints the sequence to the console for debug, and unlocks a falling wall plus a dropped key when the lever states match.

- Level 4 Unique Door-Key Pairs
  Approximate time: 1.0 hour
  Separated the two Level 4 dropped-key door pairs so they use distinct save and unlock IDs: `Door_092/Key_092` and `Door_095/Key_095`. Also renamed the released key objects in the scene to match those unique IDs.

## Total Approximate Time

20.5 hours
