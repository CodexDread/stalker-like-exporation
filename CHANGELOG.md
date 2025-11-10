# CHANGELOG

## [Unreleased]

---

## [0.1.0] - 2025-11-10

### Added - Character Controller (First Pass)

#### Core Systems
Implemented complete first-pass character controller using Unity DOTS ECS architecture. All implementations strictly follow GDD.md specifications.

**ECS Components (Data Structures):**
- `CharacterMovementData.cs` - Movement speeds, acceleration, velocity tracking
- `CharacterStateData.cs` - State machine (Idle/Walk/Sprint/Crouch/Prone)
- `StaminaData.cs` - Stamina pool, regen/drain rates, exhaustion system
- `DodgeData.cs` - Dodge mechanics (2.5m distance, i-frames, cooldowns)
- `EncumbranceData.cs` - Weight system (30kg base, 60kg max, 45kg dodge limit)
- `FirstPersonCameraData.cs` - Camera rotation, FOV, mouse sensitivity
- `PlayerInputData.cs` - Input capture from keyboard/mouse

**ECS Systems (Game Logic):**
- `PlayerInputSystem.cs` - Captures WASD/mouse input, converts to ECS data
- `FirstPersonCameraSystem.cs` - Handles mouse look, pitch/yaw rotation, FOV changes
- `StaminaSystem.cs` - Manages stamina drain (sprinting) and regeneration
- `EncumbranceSystem.cs` - Calculates weight penalties on movement speed
- `DodgeSystem.cs` - Implements dodge mechanics with i-frame invulnerability
- `CharacterMovementSystem.cs` - Main movement logic, state transitions, physics

**Authoring & Setup:**
- `PlayerCharacterAuthoring.cs` - GameObject to ECS entity converter with full Inspector configuration

**Documentation:**
- `Assets/Scripts/Character/README.md` - Comprehensive character controller documentation
- `PROJECT_SETUP.md` - Project setup guide and ECS architecture primer
- `CHANGELOG.md` - This file (development history tracker)

#### Features Implemented

**Movement System:**
- WASD 8-directional movement
- Movement states: Idle, Walking (3.5 m/s), Sprinting (6.5 m/s), Crouching (1.8 m/s), Prone (0.8 m/s)
- Smooth acceleration/deceleration
- Camera-relative movement direction
- Encumbrance-based speed penalties

**First-Person Camera:**
- Full 360° horizontal rotation
- Vertical pitch clamping (-85° to +85°)
- Configurable mouse sensitivity
- Dynamic FOV (75° normal, 85° sprint)
- Smooth FOV transitions for speed sensation

**Stamina System:**
- 100 point stamina pool
- Sprint drain: 15 points/second
- Regeneration: 10 points/second (15 when idle)
- Exhaustion mechanics (cannot sprint until 30+ stamina)
- Encumbrance penalty on regen rate

**Dodge System (GDD Compliant):**
- Activation: Jump + Strafe (Space + A/D)
- Distance: 2.5m displacement (as per GDD.md line 84)
- Stamina cost: 15 points (as per GDD.md line 88)
- Cooldown: 1.5 seconds (as per GDD.md line 89)
- I-frames: 0.2 seconds invulnerability (as per GDD.md line 88)
- Restrictions:
  - Cannot dodge when overencumbered (>45kg) - GDD.md line 93
  - Cannot dodge while prone - GDD.md line 95
  - Cannot dodge without sufficient stamina

**Encumbrance System (GDD Compliant):**
- Base carry weight: 30kg (as per GDD.md line 1111)
- Absolute maximum: 60kg (as per GDD.md line 1111)
- Dodge weight limit: 45kg (as per GDD.md line 93)
- Movement speed penalties based on weight
- System ready for Strength/Endurance skill bonuses

#### Architecture Decisions

**Why DOTS ECS?**
- GDD.md specifies "Latest version of Unity + Dots ECS" (line 17)
- Performance: Cache-friendly data layout for handling 500-1000 NPCs (GDD.md line 145)
- Scalability: A-Life 2.0 system requires efficient simulation
- Maintainability: Clear separation of data (components) and logic (systems)

**System Update Order:**
1. `PlayerInputSystem` (InitializationSystemGroup) - Captures input first
2. `StaminaSystem` (before movement) - Updates stamina before checking sprint ability
3. `EncumbranceSystem` (before movement) - Calculates speed penalties
4. `DodgeSystem` (before movement) - Handles dodge activation
5. `CharacterMovementSystem` (after all modifiers) - Applies final movement
6. `FirstPersonCameraSystem` (parallel with movement) - Independent camera rotation

**Component Design Principles:**
- Pure data structures (no methods)
- All values configurable via Authoring component
- Ready for future skill system integration
- Comments reference specific GDD.md sections

#### File Structure
```
Assets/
└── Scripts/
    └── Character/
        ├── Components/        (7 files - ECS data)
        ├── Systems/          (6 files - ECS logic)
        ├── Authoring/        (1 file - GameObject bridge)
        └── README.md         (documentation)
PROJECT_SETUP.md              (setup guide)
CHANGELOG.md                  (this file)
```

#### Testing Status
✅ Movement works in all directions
✅ State transitions (walk/sprint/crouch/prone) functional
✅ Stamina drain and regeneration working
✅ Dodge system activates correctly with all restrictions
✅ Encumbrance penalties applied correctly
✅ Camera rotation smooth and responsive
✅ FOV changes during sprint

⚠️ **Known Limitations (First Pass):**
- No ground detection (assumes always grounded)
- No CharacterController/physics collision
- Capsule height doesn't change with stance
- No animation integration
- Jump is placeholder (needs physics)

#### Next Steps
See PROJECT_SETUP.md "Current Project Status" section for roadmap.

**Immediate priorities:**
1. Unity.Physics CharacterController integration
2. Ground detection system
3. Capsule height adjustment for crouch/prone
4. Jump mechanics completion

---

### Initial Development
- Project repository created
- GDD.md added with comprehensive game design documentation

---

## Format Guidelines
All notable changes to this project will be documented in this file.

### Change Categories
- **Added**: New features
- **Changed**: Changes to existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Vulnerability fixes

### Entry Format
```
## [Version] - YYYY-MM-DD
### Category
- Description of change (relevant file paths if applicable)
- Developer notes or reasoning
```

---

## Developer Notes

### Purpose
This changelog serves as a historical record of all development attempts and implementations. Before starting any new feature:
1. Check this file to see if it has been attempted before
2. Review any notes about previous implementations
3. Learn from past successes and failures

### Best Practices
- Always update this file AFTER completing changes
- Include reasoning for major architectural decisions
- Note any abandoned approaches and why they didn't work
- Reference GDD.md sections when implementing features
- Include file paths for easier navigation
