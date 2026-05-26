# Sprint Log 5

## Current Ongoing Items
Approximate time remaining: 12.0+ hours

- Place and test the new `Level04_Vault` power-up pickups using `PowerUpPickup` for `DoubleJump` and `SpeedBoost`
- Hook those pickups into the actual `Level04_Vault` scene layout and balance their values
- Continue Level 5 scene completion work across Sprint 5 to Sprint 8 items below
- Finish full scene-level verification for inventory, keys, checkpoints, hazards, and UI feedback after the recent branch merge

## Recently Completed Foundation Work
Approximate time: 5.0 hours

- Merged the other branches into this branch and resolved the shared gameplay/script conflicts
- Connected inventory, keys, doors, checkpoints, HUD, and Level 5 progression systems together
- Added the reusable power-up framework for instant collectible pickups with `DoubleJump` uses and `SpeedBoost` duration support
- Added the `BullsAndCowsGame` helper script with number generation, guess evaluation, and increment/decrement helpers
- Removed the wall pass-through mechanic from the power-up plan

## Abyss Vault Core Environment And Layout
Approximate time: 4.0 hours

Create the full Level 5 scene layout as the main playable environment. Build the main chamber, collapsed corridors, trap hallway, water section, boulder escape path, and exit room, while establishing atmosphere, verticality, and a basic lighting setup.

## Navigation Path And Exploration Flow
Approximate time: 2.0 hours

Design the intended player route through the level and support it with visual guidance such as torches, broken walls, light sources, and collectible trails. Add side exploration routes, traversal spaces, and optional detours that make the scene feel less linear.

## Environmental Hazard Zones
Approximate time: 2.0 hours

Set up water hazard spaces, unstable floors, falling debris zones, damage triggers, and fall kill zones. These hazards should already be placed inside the scene so the level flow can be tested with danger and failure states.

## Checkpoint Placement
Approximate time: 1.0 hour

Place checkpoint objects across the level, connect activation feedback, and verify save and respawn flow for the new scene. The goal is to make the level safe to iterate on without replaying the entire section each time.

## Collectibles And Hidden Areas Setup
Approximate time: 2.0 hours

Add collectible placement for coins, artifact fragments, and rare rewards, and build at least one hidden room or secret entrance in the level. This sprint should establish exploration rewards early so later polish has real content to support.

## Total Approximate Time
11.0 hours

# Sprint Log 6

## Physics Boulder Trap
Approximate time: 4.0 hours

Implement a rolling boulder trap using Rigidbody-based behavior, trigger activation, collision damage, audio feedback, and a full escape corridor sequence. If possible, support crate interaction so the player can redirect or influence the trap instead of only running from it.

## Moving Platform System
Approximate time: 3.0 hours

Create moving stone platforms for traversal challenges and add timing-based movement sections. Include at least one falling or resetting variation so the system can support more than a single scripted platform.

## Trap Interaction Mechanics
Approximate time: 3.0 hours

Add player interaction with trap logic through crates, switches, levers, or activation states. The player should be able to influence hazards rather than only avoid them, making the level feel more systemic and less static.

## Water Slowdown System
Approximate time: 2.0 hours

Integrate movement slowdown or altered traversal behavior in water sections, along with splash feedback and ambient sound. This should turn water into a real gameplay modifier instead of only visual dressing.

## Trap Damage And Death Feedback
Approximate time: 2.0 hours

Improve the presentation of damage, death, and respawn by connecting screen effects, trap-specific feedback, and clearer death flow. This sprint should make dangerous sections easier to read and more polished to play.

## Total Approximate Time
14.0 hours

# Sprint Log 7

## Pressure Plate Puzzle
Approximate time: 3.0 hours

Build a pressure plate puzzle sequence with door unlock logic, success feedback, and reset behavior. If possible, add a wrong-order or failure case that ties puzzle mistakes back into the trap systems already built for the level.

## Power-Up System Integration
Approximate time: 3.0 hours

Integrate usable power-ups into the level, such as speed boost, double jump, trap immunity, and health restore. The code foundation for collectible-triggered `DoubleJump` and `SpeedBoost` is done, but Level 4 placement, balance, testing, and any extra power-up types still need to be finished.

## Secret Room Puzzle
Approximate time: 2.0 hours

Create an optional hidden puzzle that opens a secret room or vault section and rewards exploration with a stronger collectible or special pickup. This should feel like bonus content rather than part of the mandatory route.

## Lighting And Post Processing
Approximate time: 3.0 hours

Polish the level atmosphere with fog, bloom, color grading, emissive materials, torch flicker, and stronger contrast. The goal is to make the scene presentation feel intentional and visually complete rather than only functional.

## UI And Feedback Polish
Approximate time: 2.0 hours

Improve player-facing feedback for damage, pickups, checkpoints, puzzle completion, and inventory presentation. This sprint should make the level’s interactions more readable and increase overall game feel.

## Total Approximate Time
13.0 hours

# Sprint Log 8

## Audio Integration
Approximate time: 3.0 hours

Finalize level audio by adding ambient cave sound, water loops, boulder rumble, trap sounds, puzzle solve cues, secret room ambience, and footstep support where still missing. This sprint should bring the scene much closer to presentation quality.

## Performance Optimization
Approximate time: 3.0 hours

Optimize the level for stable play by reviewing lighting, collider usage, particle systems, and unnecessary physics cost. The goal is to reduce frame drops and make the level reliable during testing and demo.

## Full Gameplay Testing
Approximate time: 3.0 hours

Run full end-to-end testing of checkpoints, puzzles, hazards, collectibles, power-ups, respawn flow, secret content, and level completion. This sprint should confirm that the level can be played start to finish without broken progression.

## Bug Fixing And Balancing
Approximate time: 4.0 hours

Fix gameplay bugs such as collision issues, physics glitches, trap timing problems, UI problems, spawn errors, softlocks, and audio overlap. Rebalance trap difficulty, puzzle timing, collectible placement, and power-up duration based on test results.

## Final Build And Documentation
Approximate time: 2.0 hours

Prepare the level for submission by exporting a build, taking screenshots, recording gameplay evidence, and organizing sprint proof for the final demo. This should leave the level ready for presentation and reporting.

## Total Approximate Time
15.0 hours

# Cross-Branch Integration

## Merge Branches And Resolve Conflicts
Approximate time: 3.0 hours

Merge the gameplay and scene work from the other branches into this branch, resolve conflicts in shared scripts and Unity scene files, and leave the branch in a usable integrated state for continuing Level 5 development. This includes bringing in inventory, door, checkpoint, HUD, menu, and Level 5 mechanics so the scene can be completed on one branch.

# Usage Note

When a task is fully completed, move or copy its finished implementation details into the matching sprint section in `log.md` using the same short summary style as `Sprint Log 4`.
