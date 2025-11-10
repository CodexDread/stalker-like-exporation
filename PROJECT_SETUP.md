# Zone Survival - Project Setup Guide

## Overview
**Zone Survival** is a hardcore first-person survival shooter built with Unity DOTS ECS. This guide will help you set up the project for development.

## Prerequisites

### Required Software
- **Unity Editor**: 2022.3 LTS or newer
  - Download from: https://unity.com/download
- **Git**: For version control
- **.NET SDK**: 6.0 or newer (usually comes with Unity)

### Required Unity Packages

Install these packages via Unity Package Manager (`Window > Package Manager`):

1. **Entities** (com.unity.entities) - v1.0.0+
   - Core DOTS framework

2. **Unity.Mathematics** (com.unity.mathematics) - v1.2.0+
   - Math library optimized for DOTS

3. **Unity.Transforms** (com.unity.transforms) - v1.0.0+
   - Transform system for DOTS

4. **Unity.Physics** (com.unity.physics) - v1.0.0+ *(upcoming)*
   - Physics system for character controller

### Optional Packages
- **Unity.Rendering.Hybrid** - For rendering entities
- **Unity.Netcode** - For future multiplayer/co-op

## Project Structure

```
stalker-like-exporation/
├── Assets/
│   ├── Scenes/              # Unity scene files
│   ├── Scripts/
│   │   └── Character/       # Character controller (FIRST PASS COMPLETE)
│   │       ├── Components/  # ECS data components
│   │       ├── Systems/     # ECS logic systems
│   │       ├── Authoring/   # GameObject to ECS converters
│   │       └── README.md    # Character controller documentation
│   └── Prefabs/             # Reusable game objects
├── GDD.md                   # Game Design Document
├── CHANGELOG.md             # Development history
├── PROJECT_SETUP.md         # This file
└── README.md                # Project overview
```

## Quick Start

### 1. Clone Repository
```bash
git clone <repository-url>
cd stalker-like-exporation
```

### 2. Open in Unity
1. Launch Unity Hub
2. Click "Add" and select this project folder
3. Open the project with Unity 2022.3 LTS or newer
4. Wait for Unity to import assets and compile scripts

### 3. Install Required Packages
1. Open `Window > Package Manager`
2. Switch to "Unity Registry" view
3. Search and install:
   - Entities
   - Mathematics
   - Transforms

### 4. Create Test Scene
1. Create new scene: `File > New Scene`
2. Add a ground plane:
   - `GameObject > 3D Object > Plane`
   - Scale: (10, 1, 10)
   - Position: (0, 0, 0)
3. Create player character:
   - `GameObject > Create Empty`
   - Rename to "Player"
   - Add component: `PlayerCharacterAuthoring`
   - Position: (0, 1, 0)
4. Add camera as child:
   - Right-click Player > `3D Object > Camera`
   - Position: (0, 1.6, 0) - eye level
5. Save scene: `File > Save As` → `Assets/Scenes/TestScene.unity`

### 5. Configure Input
1. Open `Edit > Project Settings > Input Manager`
2. Verify these axes exist (they should by default):
   - **Horizontal**: A/D keys
   - **Vertical**: W/S keys
   - **Mouse X**: Mouse movement
   - **Mouse Y**: Mouse movement

If missing, add them manually.

### 6. Test the Character Controller
1. Press Play
2. Test controls:
   - **WASD** - Move
   - **Mouse** - Look
   - **Shift** - Sprint
   - **Ctrl** - Crouch
   - **Z** - Prone
   - **Space + A/D** - Dodge

See `Assets/Scripts/Character/README.md` for detailed testing instructions.

## Development Workflow

### Before Making Changes
1. **Check CHANGELOG.md** - See if feature was attempted before
2. **Read GDD.md** - Understand design specifications
3. **Create feature branch** - Use descriptive names

### While Developing
1. **Follow GDD specifications** - Stay true to design document
2. **Use ECS architecture** - Data-oriented design principles
3. **Document decisions** - Add comments explaining "why", not "what"
4. **Test frequently** - Verify changes don't break existing systems

### After Completing Changes
1. **Update CHANGELOG.md** - Document what was done and why
2. **Update relevant README files** - Keep documentation current
3. **Test all interactions** - Ensure new code works with existing systems
4. **Commit with clear messages** - Explain the change's purpose

## Current Project Status

### ✅ Completed (First Pass)
- [x] Character Movement System (WASD, states, acceleration)
- [x] First-Person Camera Controller
- [x] Stamina System (drain, regen, exhaustion)
- [x] Dodge System (2.5m, i-frames, cooldown - GDD compliant)
- [x] Encumbrance System (weight limits, movement penalties)
- [x] ECS Architecture Setup
- [x] Project Documentation

### 🚧 In Progress
- [ ] Physics Integration (CharacterController, ground detection)
- [ ] Capsule Height Adjustment (crouch/prone)
- [ ] Jump Mechanics

### 📋 Planned
- [ ] Animation System
- [ ] Weapon System
- [ ] Inventory System
- [ ] Health System (body parts, damage types)
- [ ] A-Life System
- [ ] Anomaly System
- [ ] Artifact System
- [ ] PDA System
- [ ] UI/HUD System
- [ ] Save/Load System

See GDD.md for full feature list.

## ECS Architecture Primer

### What is ECS?
Entity Component System (ECS) is a data-oriented design pattern that separates:
- **Entities**: Unique IDs (like GameObject IDs)
- **Components**: Pure data (structs, no logic)
- **Systems**: Pure logic (processes components)

### Why Use ECS?
- **Performance**: Cache-friendly data layout
- **Scalability**: Easily handles thousands of entities
- **Maintainability**: Clear separation of data and logic
- **Parallelization**: Systems can run in parallel automatically

### How It Works in This Project

#### 1. Components (Data)
```csharp
// Pure data, no methods
public struct StaminaData : IComponentData
{
    public float Current;
    public float Maximum;
    public float RegenRate;
}
```

#### 2. Systems (Logic)
```csharp
// Processes all entities with StaminaData
public partial class StaminaSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var stamina in SystemAPI.Query<RefRW<StaminaData>>())
        {
            // Process stamina regen/drain
        }
    }
}
```

#### 3. Authoring (GameObject to ECS)
```csharp
// Converts GameObject to ECS entity
public class PlayerCharacterAuthoring : MonoBehaviour
{
    public float maxStamina = 100f;

    class Baker : Baker<PlayerCharacterAuthoring>
    {
        public override void Bake(PlayerCharacterAuthoring authoring)
        {
            AddComponent(entity, new StaminaData { Maximum = authoring.maxStamina });
        }
    }
}
```

## Coding Standards

### Naming Conventions
- **Components**: `[Feature]Data.cs` (e.g., `StaminaData.cs`)
- **Systems**: `[Feature]System.cs` (e.g., `StaminaSystem.cs`)
- **Authoring**: `[Feature]Authoring.cs` (e.g., `PlayerCharacterAuthoring.cs`)

### File Organization
- Components in `Components/` folder
- Systems in `Systems/` folder
- Authoring in `Authoring/` folder
- One class per file

### Comments
- **XML Doc Comments**: For public APIs
- **Inline Comments**: For complex logic or GDD references
- **GDD References**: Link to specific sections when implementing features

Example:
```csharp
/// <summary>
/// Dodge system implementation
/// Based on GDD.md Combat Mechanics - Dodge System (lines 82-96)
/// </summary>
```

## Troubleshooting

### Build Errors
**Problem**: "Entities package not found"
**Solution**: Install via Package Manager (`Window > Package Manager`)

**Problem**: "Baker class not recognized"
**Solution**: Update Entities package to v1.0.0+

### Runtime Issues
**Problem**: Character doesn't move
**Solution**:
1. Check Entity Debugger (`Window > Entities > Hierarchy`)
2. Verify components are attached to entity
3. Check systems are running in System window

**Problem**: Input not working
**Solution**:
1. Verify Input Manager settings (`Edit > Project Settings > Input`)
2. Check `PlayerInputSystem` is running
3. Ensure old Input System is enabled (not new Input System)

### Performance Issues
**Problem**: Low FPS
**Solution**:
1. Check system update order (avoid unnecessary dependencies)
2. Use Burst compiler for systems (add `[BurstCompile]` attribute)
3. Profile with Unity Profiler (`Window > Analysis > Profiler`)

## Resources

### Documentation
- **Unity DOTS**: https://docs.unity3d.com/Packages/com.unity.entities@latest
- **Unity Mathematics**: https://docs.unity3d.com/Packages/com.unity.mathematics@latest
- **ECS Samples**: https://github.com/Unity-Technologies/EntityComponentSystemSamples

### Learning Resources
- Unity Learn: DOTS tutorials
- YouTube: Unity DOTS overview videos
- GDD.md: Game design specifications

## Support

### Getting Help
1. Check `CHANGELOG.md` for similar issues
2. Read relevant README files in feature folders
3. Review GDD.md for design clarification
4. Check Unity DOTS documentation

### Reporting Issues
When reporting bugs, include:
1. Unity version
2. Steps to reproduce
3. Expected vs actual behavior
4. Relevant component/system code
5. Console errors (if any)

---

**Project**: Zone Survival
**Engine**: Unity 2022.3+ with DOTS ECS
**Status**: Early Development - Character Controller Complete
**Last Updated**: 2025-11-10
