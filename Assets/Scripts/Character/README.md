# Character Controller - Physics Integration Complete

## Overview
This is the **second iteration** of the character controller for **Zone Survival**, based on the specifications in `GDD.md`. The controller is built using Unity's DOTS (Data-Oriented Technology Stack) ECS architecture for maximum performance, now with full physics integration.

## Version History
- **v0.1.0** - First pass: Basic movement, camera, stamina, dodge, encumbrance
- **v0.2.0** - Physics integration: Ground detection, capsule height, jump mechanics, slope handling

## Features Implemented

### ✅ Core Movement
- **WASD Movement**: Full 8-directional movement
- **Movement States**: Idle, Walking, Sprinting, Crouching, Prone
- **Speed Variations**: Different speeds for each movement state
- **Smooth Acceleration/Deceleration**: Responsive but realistic movement feel

### ✅ First-Person Camera
- **Mouse Look**: Full 360° horizontal, limited vertical rotation
- **Pitch Clamping**: Prevents over-rotation (configurable limits)
- **FOV Changes**: Dynamic FOV shift when sprinting for speed sensation
- **Camera Offset**: Proper head height positioning

### ✅ Stamina System
- **Stamina Pool**: 100 points (configurable)
- **Sprint Drain**: 15 points/second while sprinting
- **Regeneration**: 10 points/second when not sprinting (faster when idle)
- **Exhaustion**: Cannot sprint again until stamina recovers to threshold (30 points)
- **Encumbrance Penalty**: Slower regen when overweight

### ✅ Dodge System (GDD Compliant)
- **Activation**: Jump while strafing (A or D keys)
- **Distance**: 2.5m quick displacement
- **Stamina Cost**: 15 points per dodge
- **Cooldown**: 1.5 seconds between dodges
- **I-Frames**: 0.2 seconds of invulnerability
- **Restrictions**:
  - Cannot dodge when overencumbered (>45kg)
  - Cannot dodge while prone
  - Cannot dodge without sufficient stamina

### ✅ Encumbrance System (GDD Compliant)
- **Base Carry Weight**: 30kg
- **Maximum Weight**: 60kg
- **Dodge Limit**: 45kg (as per GDD specifications)
- **Movement Penalties**: Speed reduced when carrying heavy loads
- **Skill Bonuses**: System ready for Strength/Endurance skill integration

### ✅ Physics System (NEW in v0.2.0)
- **Ground Detection**: Raycasting-based ground detection with sphere cast
- **Ground Snapping**: Smooth ground contact on slopes and steps
- **Slope Handling**: Movement projects onto ground normal, respects slope limits (45°)
- **Collision**: Physics-ready capsule configuration
- **Step Climbing**: Can climb steps up to 0.3m height

### ✅ Capsule Height Adjustment (NEW in v0.2.0)
- **Dynamic Height**: Capsule adjusts based on movement state
- **Standing**: 1.8m height
- **Crouching**: 1.2m height
- **Prone**: 0.5m height
- **Smooth Transitions**: Interpolated height changes
- **Camera Follows**: Camera offset adjusts to maintain eye-level view

### ✅ Jump Mechanics (NEW in v0.2.0)
- **Jump Force**: Configurable upward force (default: 5.0)
- **Jump Buffering**: Press jump 0.15s before landing - executes on land
- **Coyote Time**: 0.1s grace period after leaving ground edge
- **Jump Cooldown**: 0.2s between jumps to prevent spam
- **Stamina Cost**: 10 stamina per jump (configurable)
- **Restrictions**: Cannot jump while prone or without stamina

## Architecture

### ECS Components (Data)
Located in `Assets/Scripts/Character/Components/`:

**Core Movement:**
- **CharacterMovementData.cs** - Movement speeds, acceleration, velocity
- **CharacterStateData.cs** - Current movement state (idle/walk/sprint/crouch/prone)
- **StaminaData.cs** - Stamina pool, regen rates, exhaustion state
- **DodgeData.cs** - Dodge parameters, cooldowns, i-frame tracking
- **EncumbranceData.cs** - Weight limits, movement penalties
- **FirstPersonCameraData.cs** - Camera rotation, FOV, sensitivity
- **PlayerInputData.cs** - Captured input from keyboard/mouse

### ECS Systems (Logic)
Located in `Assets/Scripts/Character/Systems/`:

- **PlayerInputSystem.cs** - Captures keyboard/mouse input
- **FirstPersonCameraSystem.cs** - Handles camera rotation and FOV
- **StaminaSystem.cs** - Manages stamina drain and regeneration
- **EncumbranceSystem.cs** - Calculates weight penalties
- **DodgeSystem.cs** - Handles dodge activation and i-frames
- **CharacterMovementSystem.cs** - Main movement logic and state transitions

### Authoring Components
Located in `Assets/Scripts/Character/Authoring/`:

- **PlayerCharacterAuthoring.cs** - Converts GameObject to ECS entity with all components

## Setup Instructions

### 1. Unity Project Setup
- Unity Version: 2022.3 LTS or newer
- Required Packages:
  - `com.unity.entities`
  - `com.unity.transforms`
  - `com.unity.mathematics`

### 2. Create Player Character

#### Method A: Manual Setup
1. Create a new GameObject in your scene
2. Add the `PlayerCharacterAuthoring` component
3. Configure values in the Inspector (defaults are set to GDD specifications)
4. The GameObject will be converted to an ECS entity at runtime

#### Method B: Prefab Setup
1. Create a Capsule GameObject (Scale: 1, 1.8, 1 for human-sized character)
2. Add `PlayerCharacterAuthoring` component
3. Add a Camera as a child object (will be controlled by FirstPersonCameraSystem)
4. Save as prefab in `Assets/Prefabs/`

### 3. Input Configuration
The controller uses Unity's legacy Input system. Ensure these axes are configured in `Edit > Project Settings > Input`:

- **Horizontal**: A/D keys (or arrow keys)
- **Vertical**: W/S keys (or arrow keys)
- **Mouse X**: Mouse movement X-axis
- **Mouse Y**: Mouse movement Y-axis

### 4. Testing the Controller

#### Basic Movement Test
1. Create a test scene with a ground plane
2. Add the Player Character prefab/GameObject
3. Enter Play mode
4. Test controls:
   - **W/A/S/D** - Move in all directions
   - **Left Shift** - Sprint (watch stamina drain)
   - **Left Ctrl** - Crouch
   - **Z** - Prone
   - **Space** - Jump (not fully implemented in first pass)

#### Dodge System Test
1. While moving with **A** or **D**, press **Space** to dodge
2. Observe:
   - 2.5m displacement in strafe direction
   - Stamina consumption (15 points)
   - 1.5 second cooldown before next dodge
   - Cannot dodge if stamina < 15

#### Encumbrance Test
1. Modify `currentWeight` in the Inspector while playing
2. Test values:
   - **0-30kg**: Normal movement
   - **30-45kg**: Reduced speed, can still dodge
   - **45-60kg**: Heavily reduced speed, **cannot dodge**
   - **>60kg**: Severe penalties

## Controls Reference

| Input | Action |
|-------|--------|
| **W** | Move Forward |
| **A** | Move Left |
| **S** | Move Backward |
| **D** | Move Right |
| **Mouse** | Look Around |
| **Left Shift** | Sprint (hold) |
| **Left Ctrl** | Crouch (hold) |
| **Z** | Prone (hold) |
| **Space** | Jump |
| **Space + A/D** | Dodge Left/Right |

## Configuration Values (GDD Compliant)

### Default Speeds
- **Walk**: 3.5 m/s
- **Sprint**: 6.5 m/s
- **Crouch**: 1.8 m/s
- **Prone**: 0.8 m/s

### Stamina
- **Maximum**: 100 points
- **Regen Rate**: 10/second (15/second when idle)
- **Sprint Drain**: 15/second
- **Dodge Cost**: 15 points

### Dodge
- **Distance**: 2.5m (GDD specification)
- **Duration**: 0.3 seconds
- **Cooldown**: 1.5 seconds (GDD specification)
- **I-Frames**: 0.2 seconds (GDD specification)

### Encumbrance
- **Base Max**: 30kg (GDD specification)
- **Absolute Max**: 60kg (GDD specification)
- **Dodge Limit**: 45kg (GDD specification)

## Known Limitations (Current v0.2.0)

### Resolved in v0.2.0 ✅
- ✅ **Ground Detection**: Now fully implemented with raycasting
- ✅ **Capsule Height Changes**: Smooth transitions between stances
- ✅ **Jump Mechanics**: Complete with buffering and coyote time

### Still Not Implemented
- ❌ **Full Physics Collision**: Unity.Physics integration partial (collision detection ready, no rigid body dynamics)
- ❌ **Animation Integration**: No animation system
- ❌ **Sound Effects**: No audio feedback
- ❌ **Skill System Integration**: Stamina/weight bonuses from skills not connected
- ❌ **Camera Bob/Sway**: No head bob or weapon sway

### Requirements for Production
- Add physics collision responses (pushback, bumping)
- Integrate animation system for visual feedback
- Add footstep sounds and audio cues
- Connect to skill progression system

## Next Steps for Development

### High Priority (COMPLETED in v0.2.0) ✅
1. ✅ **Physics Integration**
   - ✅ Unity.Physics components added
   - ✅ Proper ground detection implemented
   - ✅ Jump mechanics completed
   - ⚠️ Collision responses (partial - needs rigid body dynamics)

2. ✅ **Capsule Height Adjustment**
   - ✅ Standing: 1.8m
   - ✅ Crouching: 1.2m
   - ✅ Prone: 0.5m

3. ✅ **Camera Position Updates**
   - ✅ Camera offset adjusts based on stance
   - ✅ Smooth transitions between heights

### New High Priority

### Medium Priority
4. **Animation System**
   - Integrate animation controller
   - Blend movement states
   - Add dodge animation

5. **Input System Upgrade**
   - Migrate to new Unity Input System
   - Add rebindable controls
   - Add controller support

6. **UI Integration**
   - Connect stamina to UI bar
   - Show movement state indicator
   - Display weight/encumbrance

### Low Priority (Polish)
7. **Audio Feedback**
   - Footstep sounds
   - Stamina breathing effects
   - Dodge whoosh sound

8. **Camera Effects**
   - Head bob during movement
   - Camera shake for impacts
   - Smooth stance transitions

## Testing Game Systems

This character controller provides the foundation needed to test other game systems:

- ✅ **Combat Systems**: Movement and dodging ready for enemy engagement
- ✅ **Inventory System**: Encumbrance system ready for item weight
- ✅ **Stamina-based Activities**: Framework ready for climbing, melee, etc.
- ✅ **Skill System**: Multipliers ready for Endurance, Agility, Strength skills
- ✅ **Anomaly Navigation**: Dodge system ready for anomaly avoidance
- ✅ **Survival Mechanics**: Movement penalties ready for hunger/thirst debuffs

## Performance Notes

This ECS implementation is highly optimized:
- All data is cache-friendly (struct components)
- Systems process entities in parallel where possible
- No GameObject overhead during gameplay
- Suitable for large numbers of NPCs using same systems

## Troubleshooting

### Character doesn't move
- Check that `PlayerInputSystem` is running (look in Entity Debugger)
- Verify Input axes are configured in Project Settings
- Ensure `PlayerCharacterAuthoring` baked successfully

### Dodge not working
- Check stamina level (need 15+ points)
- Verify cooldown timer has expired (1.5s)
- Ensure you're not prone
- Check weight is ≤45kg

### Stamina drains too fast/slow
- Adjust `sprintDrainRate` and `staminaRegenRate` in Inspector
- Default values are from GDD but can be tuned for feel

## GDD References

This implementation directly follows these GDD sections:
- **Combat Mechanics - Dodge System** (lines 82-96)
- **Character System** (lines 284-428)
- **Controls** (lines 1162-1171)
- **UI/UX Design** (lines 1021-1171)

## File Structure
```
Assets/
└── Scripts/
    └── Character/
        ├── Components/             (10 files)
        │   ├── CharacterMovementData.cs
        │   ├── CharacterStateData.cs
        │   ├── PlayerInputData.cs
        │   ├── StaminaData.cs
        │   ├── DodgeData.cs
        │   ├── EncumbranceData.cs
        │   ├── CharacterPhysicsData.cs      [NEW v0.2.0]
        │   ├── GroundDetectionData.cs       [NEW v0.2.0]
        │   ├── JumpData.cs                  [NEW v0.2.0]
        │   └── FirstPersonCameraData.cs
        ├── Systems/                (9 files)
        │   ├── PlayerInputSystem.cs
        │   ├── FirstPersonCameraSystem.cs
        │   ├── StaminaSystem.cs
        │   ├── EncumbranceSystem.cs
        │   ├── DodgeSystem.cs
        │   ├── GroundDetectionSystem.cs     [NEW v0.2.0]
        │   ├── JumpSystem.cs                [NEW v0.2.0]
        │   ├── CapsuleHeightSystem.cs       [NEW v0.2.0]
        │   └── CharacterMovementSystem.cs   [UPDATED v0.2.0]
        ├── Authoring/              (1 file)
        │   └── PlayerCharacterAuthoring.cs  [UPDATED v0.2.0]
        ├── README.md (this file)            [UPDATED v0.2.0]
        └── PHYSICS_TESTING.md               [NEW v0.2.0]
```

---

## Additional Documentation

- **PHYSICS_TESTING.md** - Comprehensive testing guide for all physics features
- **PROJECT_SETUP.md** (root folder) - Unity setup and ECS architecture guide
- **CHANGELOG.md** (root folder) - Complete development history

---

**Version**: v0.2.0 - Physics Integration Complete
**Date**: 2025-11-10
**Status**: Physics systems operational, ground detection working, jump mechanics complete
**Previous**: v0.1.0 - First pass (basic movement, camera, stamina)
