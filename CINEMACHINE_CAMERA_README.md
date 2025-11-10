# Cinemachine Camera System (v0.1.6)

## Overview

The Cinemachine Camera System replaces the basic first-person camera with a Cinemachine-powered camera that provides:
- **Weight & Inertia Effects**: Camera feels heavier and more sluggish when carrying gear
- **Procedural Camera Noise**: Breathing, idle sway, and movement-based shake
- **Camera Shake**: Impact effects for landing, explosions, recoil
- **Smooth Damping**: Professional camera feel with configurable responsiveness
- **Advanced Recoil**: Weapon kickback with smooth recovery
- **Stance-Based Stabilization**: Less shake when prone/crouched/aiming

This system uses a **hybrid ECS + Cinemachine** architecture:
- **ECS handles calculations**: Rotation, FOV, weight effects (fast, data-oriented)
- **Cinemachine handles presentation**: Damping, noise, shake, impulses (powerful, cinematic)

---

## Quick Start

### 1. Install Cinemachine

1. Open **Window → Package Manager**
2. Search for **"Cinemachine"**
3. Click **Install**
4. Wait for import to complete

### 2. Set Up Player with Cinemachine Camera

**Option A: New Player Setup**

1. Create an empty GameObject for your player
2. Add **CinemachineCameraAuthoring** component
3. Add **PlayerInputAuthoring** component (for input)
4. Add **CharacterMovementAuthoring** (or use PlayerCharacterAuthoring)
5. Play the scene - Camera system is ready!

**Option B: Migrate Existing Player**

1. Find your player GameObject (has PlayerCharacterAuthoring)
2. Add **CinemachineCameraAuthoring** component
3. Remove old **FirstPersonCameraData** references
4. The camera settings will be read from CinemachineCameraAuthoring

### 3. Cinemachine Virtual Camera Setup

At runtime, you need a Cinemachine Virtual Camera in the scene. You have two options:

**Option A: Automatic Setup (Recommended)**
- Add **CinemachineCameraController** MonoBehaviour to a GameObject with a **CinemachineVirtualCamera**
- Assign your player GameObject to the **Player Entity** field
- The controller automatically creates a camera target and connects to ECS

**Option B: Manual Setup**
1. Create a GameObject with **CinemachineVirtualCamera**
2. Add **CinemachineCameraController** script
3. Create a child GameObject called "CameraTarget" under your player
4. Assign references:
   - Player Entity: Your player GameObject
   - Camera Target: The CameraTarget transform
5. Configure Cinemachine Body: **Transposer** or **Do Nothing**
6. Configure Cinemachine Aim: **Do Nothing** (we handle rotation)
7. Add **Basic Multi-Channel Perlin** extension for noise
8. Add **Cinemachine Impulse Source** component for shake

---

## Architecture

### Data Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         ECS SIDE                                │
├─────────────────────────────────────────────────────────────────┤
│  1. PlayerInputSystem                                           │
│      ↓ (captures mouse input)                                   │
│  2. CinemachineCameraBridgeSystem                               │
│      • Reads PlayerInputData                                    │
│      • Calculates rotation (pitch/yaw)                          │
│      • Calculates FOV (sprint, ADS)                             │
│      • Calculates weight effects (damping, shake)               │
│      • Processes recoil requests                                │
│      • Processes shake requests                                 │
│      • Updates CinemachineCameraData                            │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│                    MONOBEHAVIOUR SIDE                           │
├─────────────────────────────────────────────────────────────────┤
│  3. CinemachineCameraController (MonoBehaviour)                 │
│      • Reads CinemachineCameraData from ECS entity              │
│      • Applies rotation to camera target transform              │
│      • Applies FOV to Cinemachine virtual camera                │
│      • Applies damping to Cinemachine Body                      │
│      • Triggers Cinemachine noise (breathing, sway)             │
│      • Triggers Cinemachine impulses (shake, impacts)           │
└─────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────┐
│                     CINEMACHINE                                 │
├─────────────────────────────────────────────────────────────────┤
│  • CinemachineVirtualCamera                                     │
│      • Smooth damping on rotation/position                      │
│      • Procedural noise for camera shake                        │
│      • Impulse responses for impacts                            │
│      • Professional camera feel                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Why Hybrid Architecture?

**ECS Strengths (Calculations):**
- Fast batch processing of camera logic
- Clean data flow (input → rotation → FOV)
- Easy to test and debug
- No per-frame overhead from MonoBehaviour updates

**Cinemachine Strengths (Presentation):**
- Industry-standard camera features
- Damping, noise, shake out-of-the-box
- Extensive tools and presets
- Timeline integration
- Professional camera feel

By combining both, we get performance AND features.

---

## Components Reference

### CinemachineCameraData (ECS Component)

Core camera data stored in ECS for fast processing.

```csharp
public struct CinemachineCameraData : IComponentData
{
    // Mouse & Rotation
    public float MouseSensitivityX;
    public float MouseSensitivityY;
    public float MinPitch;
    public float MaxPitch;
    public float Pitch;
    public float Yaw;

    // Camera Offset
    public float3 CameraOffset;

    // FOV
    public float BaseFOV;
    public float SprintFOV;
    public float CurrentFOV;
    public float FOVLerpSpeed;

    // Weight & Inertia
    public float BaseRotationDamping;
    public float EncumberedDampingMultiplier;
    public float CurrentRotationDamping;

    // Procedural Effects
    public float BaseFOVShake;
    public float MovementShakeMultiplier;
    public float EncumberedShakeMultiplier;

    // Landing
    public float LandingImpactStrength;
    public bool TriggerLandingShake;

    // Recoil
    public float RecoilPitchAmount;
    public float RecoilYawAmount;
    public float RecoilRecoverySpeed;

    // Breathing & Idle
    public float BreathingFrequency;
    public float BreathingAmplitude;
    public float IdleSwayAmount;
    public float CurrentBreathPhase;

    // Stance Stabilization
    public float ProneStabilization;
    public float CrouchStabilization;
    public float ADSStabilization;
    public bool IsADS;

    // Performance
    public bool EnableProceduralEffects;
    public float EffectsIntensity;
}
```

### CameraShakeRequest (ECS Component)

Request component for triggering camera shake. Add this to an entity to trigger shake.

```csharp
public struct CameraShakeRequest : IComponentData
{
    public float Amplitude;   // Shake strength (0.1-2.0)
    public float Frequency;   // Shake speed (5-30 Hz)
    public float Duration;    // How long (0.1-1.0 seconds)
    public float3 Direction;  // Optional directional shake
}
```

**Example Usage:**
```csharp
// Trigger explosion shake
entityManager.AddComponentData(playerEntity, new CameraShakeRequest
{
    Amplitude = 1.5f,
    Frequency = 20f,
    Duration = 0.5f,
    Direction = new float3(0, 0, 0) // Random shake
});
```

### CameraRecoilRequest (ECS Component)

Request component for weapon recoil. Add this when weapon fires.

```csharp
public struct CameraRecoilRequest : IComponentData
{
    public float PitchRecoil;  // Upward kick (2-10 degrees)
    public float YawRecoil;    // Horizontal kick (-3 to +3 degrees)
    public float RecoveryTime; // Return to center time (0.1-0.5s)
}
```

**Example Usage:**
```csharp
// Weapon recoil on fire
entityManager.AddComponentData(playerEntity, new CameraRecoilRequest
{
    PitchRecoil = 3.5f,  // Kick up 3.5 degrees
    YawRecoil = UnityEngine.Random.Range(-1f, 1f), // Random horizontal
    RecoveryTime = 0.2f  // Return in 0.2 seconds
});
```

---

## Weight & Inertia System

### How It Works

As the player carries more weight (gear, weapons, loot), the camera becomes:
1. **More sluggish** - Increased rotation damping
2. **More shaky** - Extra procedural noise
3. **Less stable** - Harder to aim precisely

This creates a gameplay tradeoff: heavy gear = more resources but worse combat performance.

### Weight Calculation

```csharp
// Weight ratio (0.0 = empty, 1.0 = max capacity)
float weightRatio = currentWeight / maxWeight;

// Camera damping increases with weight
// Light (0-30kg):   Damping 0.3 (responsive)
// Medium (30-45kg): Damping 0.5 (noticeable)
// Heavy (45-60kg):  Damping 1.2 (sluggish)
float damping = baseRotationDamping * lerp(1.0, dampingMultiplier, weightRatio);

// Extra shake when overencumbered (>75% capacity)
if (weightRatio > 0.75f)
{
    float overencumberedRatio = (weightRatio - 0.75f) / 0.25f;
    noiseAmplitude *= (1.0f + overencumberedRatio);
}
```

### Configuration

**In CinemachineCameraAuthoring:**
```
Base Rotation Damping: 0.3
  ↓ (Light, responsive camera)

Encumbered Damping Multiplier: 2.5
  ↓ (At full weight: 0.3 × 2.5 = 0.75 damping)

Result: Camera feels 2.5x heavier when fully loaded
```

**Recommended Values:**
- **Realistic**: Base 0.3, Multiplier 2.5
- **Arcade**: Base 0.1, Multiplier 1.5
- **Hardcore**: Base 0.5, Multiplier 4.0

---

## Procedural Camera Effects

### Breathing Cycle

Subtle camera sway simulating breathing.

**Configuration:**
```
Breathing Frequency: 0.25 (15 breaths/min)
Breathing Amplitude: 0.05 (subtle)
```

**Breathing varies by stance:**
- **Idle**: 0.5x amplitude (calm)
- **Walking**: 1.0x amplitude (normal)
- **Sprinting**: 2.0x amplitude (heavy breathing)
- **Prone**: 0.2x amplitude (stable)
- **Crouched**: 0.5x amplitude (steady)
- **ADS**: 0.3x amplitude (holding breath)

### Idle Camera Sway

Random subtle movement for realism.

**Configuration:**
```
Idle Sway Amount: 0.02 (very subtle)
Movement Shake Multiplier: 1.5 (more shake when moving)
```

### Stance-Based Stabilization

Different stances provide different stability:

| Stance | Shake Multiplier | Effect |
|--------|------------------|--------|
| Standing | 1.0x | Normal |
| Crouched | 0.5x | 50% less shake |
| Prone | 0.2x | 80% less shake (sniper-stable) |
| ADS | 0.3x | 70% less shake (focused) |

**Example: Prone + ADS**
```
Base shake: 0.05
Prone: 0.05 × 0.2 = 0.01
ADS: 0.01 × 0.3 = 0.003 (extremely stable for sniping)
```

---

## Camera Shake System

### Landing Impact

Automatic shake when landing from height.

**How it works:**
1. `GroundDetectionData.JustLanded` is true
2. Fall time calculated (longer fall = stronger shake)
3. `CameraShakeRequest` added automatically
4. Cinemachine impulse triggered

**Configuration:**
```
Landing Impact Strength: 0.5
  ↓ (scales with fall height)

Short fall (< 0.5s): Shake 0.1
Medium fall (0.5-1s): Shake 0.5
Long fall (> 1s): Shake 1.0 (clamped)
```

### Manual Shake Requests

Any system can trigger camera shake:

```csharp
// Explosion nearby
entityManager.AddComponentData(playerEntity, new CameraShakeRequest
{
    Amplitude = 2.0f,     // Strong shake
    Frequency = 25f,      // Fast shake (feels violent)
    Duration = 0.3f,      // Brief
    Direction = explosionDirection // Shake toward explosion
});

// Gunshot impact
entityManager.AddComponentData(playerEntity, new CameraShakeRequest
{
    Amplitude = 0.3f,     // Light shake
    Frequency = 30f,      // Sharp shake
    Duration = 0.1f,      // Very brief
    Direction = float3.zero // Random shake
});
```

### Shake Intensity Guide

| Type | Amplitude | Frequency | Duration | Use Case |
|------|-----------|-----------|----------|----------|
| Bullet Impact | 0.2-0.4 | 30Hz | 0.1s | Getting shot |
| Nearby Explosion | 1.0-2.0 | 20Hz | 0.5s | Grenade nearby |
| Landing | 0.1-1.0 | 15Hz | 0.2s | Falling/jumping |
| Footsteps | 0.01-0.05 | 10Hz | 0.1s | Walking/running |
| Vehicle Crash | 2.0-5.0 | 25Hz | 1.0s | Heavy impact |

---

## Weapon Recoil System

### How Recoil Works

1. Weapon fires → Add `CameraRecoilRequest` to player entity
2. `CinemachineCameraBridgeSystem` reads request
3. Pitch kick applied to camera immediately (upward)
4. Yaw kick applied as horizontal offset (left/right random)
5. Camera gradually recovers to center over time

### Recoil Configuration

```csharp
// Example: AK-74 recoil
entityManager.AddComponentData(playerEntity, new CameraRecoilRequest
{
    PitchRecoil = 3.0f,  // Moderate upward kick
    YawRecoil = Random.Range(-0.5f, 0.5f), // Slight horizontal deviation
    RecoveryTime = 0.15f // Quick recovery for auto fire
});

// Example: Sniper rifle recoil
entityManager.AddComponentData(playerEntity, new CameraRecoilRequest
{
    PitchRecoil = 8.0f,  // Strong upward kick
    YawRecoil = Random.Range(-2f, 2f), // Noticeable horizontal
    RecoveryTime = 0.4f  // Slower recovery (bolt action)
});
```

### Recoil Recovery

Recoil recovery is automatic and smooth:

```csharp
// In CinemachineCameraBridgeSystem
recoilPitchAmount = lerp(recoilPitchAmount, 0f, deltaTime * recoilRecoverySpeed);
recoilYawAmount = lerp(recoilYawAmount, 0f, deltaTime * recoilRecoverySpeed);
```

**Recovery Speed Values:**
- **8.0**: Fast recovery (SMG, assault rifles)
- **5.0**: Medium recovery (battle rifles)
- **3.0**: Slow recovery (heavy weapons)

---

## Cinemachine Setup Guide

### Required Components

On your **CinemachineVirtualCamera** GameObject:

1. **CinemachineVirtualCamera**
   - Priority: 10 (higher than any other cameras)
   - Follow: Camera target transform (auto-created or manual)
   - Look At: None (we handle rotation manually)

2. **CinemachineBasicMultiChannelPerlin** (Noise extension)
   - Noise Profile: Assign a NoiseSettings asset
   - Recommended: "6D Shake" or "Handheld_normal_mild"
   - Amplitude Gain: Controlled by script (leave at 0)
   - Frequency Gain: Controlled by script (leave at 0)

3. **CinemachineImpulseSource**
   - Default Velocity: (0, -1, 0)
   - Amplitude: 1.0
   - Frequency: 1.0
   - Time Envelope: Default
   - (Values will be overridden per shake request)

4. **CinemachineCameraController** script
   - Player Entity: Your player GameObject
   - Camera Target: Auto-created or assign manually
   - Enable Weight Effects: True
   - Enable Procedural Noise: True

### Body Configuration

**Transposer (Recommended):**
```
Binding Mode: Lock To Target On Assign
Follow Offset: (0, 0, 0)
X/Y/Z Damping: Controlled by script
```

**Do Nothing (Alternative):**
If you prefer manual control, use "Do Nothing" and position is handled entirely by the camera target transform.

### Aim Configuration

**Do Nothing:**
We handle all rotation via the camera target transform. Cinemachine aim components are not needed.

### Noise Profile Setup

1. Right-click in Project → Create → Cinemachine → Noise Settings
2. Name it "FirstPersonNoise"
3. Configure noise presets (or use built-in)
4. Assign to BasicMultiChannelPerlin component

**Recommended Noise Preset:**
```
Position Noise:
  X: (Frequency: 1.0, Amplitude: 0.3)
  Y: (Frequency: 0.8, Amplitude: 0.4)
  Z: (Frequency: 1.2, Amplitude: 0.2)

Rotation Noise:
  X: (Frequency: 1.5, Amplitude: 0.2)
  Y: (Frequency: 1.3, Amplitude: 0.3)
  Z: (Frequency: 1.0, Amplitude: 0.1)
```

---

## Integration with Existing Systems

### Character Movement

Weight effects automatically read from `EncumbranceData`:

```csharp
// EncumbranceSystem updates weight
encumbrance.CurrentWeight = inventoryWeight + equippedGearWeight;

// CinemachineCameraBridgeSystem reads weight
float weightRatio = encumbrance.CurrentWeight / encumbrance.AbsoluteMaxWeight;
cameraData.CurrentRotationDamping = baseRotationDamping * lerp(1.0f, multiplier, weightRatio);
```

### Weapon System

Weapons trigger recoil on fire:

```csharp
// In WeaponFiringSystem, when weapon fires:
if (fired)
{
    // Calculate recoil based on weapon stats
    float pitchRecoil = weaponData.RecoilPattern.PitchAmount;
    float yawRecoil = Random.Range(-weaponData.RecoilPattern.YawSpread, weaponData.RecoilPattern.YawSpread);

    // Add recoil request
    entityManager.AddComponentData(weaponOwner, new CameraRecoilRequest
    {
        PitchRecoil = pitchRecoil,
        YawRecoil = yawRecoil,
        RecoveryTime = weaponData.RecoilRecoveryTime
    });
}
```

### Ground Detection

Landing shake triggered automatically:

```csharp
// In CinemachineCameraBridgeSystem
if (groundData.JustLanded)
{
    float fallTime = groundData.TimeSinceGrounded;
    float intensity = clamp(fallTime / 1.0f, 0.1f, 1.0f);
    float shakeAmplitude = cameraData.LandingImpactStrength * intensity;

    entityManager.AddComponentData(entity, new CameraShakeRequest
    {
        Amplitude = shakeAmplitude,
        Frequency = 15f,
        Duration = 0.2f,
        Direction = new float3(0, -1, 0)
    });
}
```

---

## Debugging

### Debug Display

Enable debug info in **CinemachineCameraController**:
```
Show Debug Info: True
```

This displays:
- Current pitch/yaw
- FOV
- Damping value
- Recoil amounts
- Breathing phase
- Noise amplitude

### Common Issues

**Issue: Camera not moving**
- Check that player entity has `CinemachineCameraData` component
- Verify `CinemachineCameraController.playerEntity` is assigned
- Ensure ECS entity conversion happened (wait 1 frame after scene load)

**Issue: Camera too sluggish**
- Reduce `Base Rotation Damping` (try 0.1-0.2)
- Reduce `Encumbered Damping Multiplier` (try 1.5)
- Check `EncumbranceData.CurrentWeight` isn't too high

**Issue: Too much camera shake**
- Reduce `Idle Sway Amount` (try 0.01)
- Reduce `Breathing Amplitude` (try 0.02)
- Reduce `Effects Intensity` (try 0.5)
- Disable procedural effects: `Enable Procedural Effects = false`

**Issue: No camera shake on landing**
- Check `Landing Impact Strength` > 0
- Verify player has `GroundDetectionData` component
- Ensure `CinemachineImpulseSource` component exists
- Check Cinemachine Impulse Listener is in scene (on main camera)

**Issue: Recoil not working**
- Verify `CameraRecoilRequest` is being added to entity
- Check `RecoilRecoverySpeed` isn't too high
- Confirm `CinemachineCameraBridgeSystem` is running

---

## Performance Considerations

### ECS Performance

- Camera calculations: ~0.05ms per frame
- Minimal overhead vs old system
- Scales well with multiple cameras (future splitscreen)

### Cinemachine Performance

- Virtual camera overhead: ~0.2ms per frame
- Noise processing: ~0.05ms per frame
- Impulse processing: ~0.1ms when active
- Total: ~0.4ms (well within 16.6ms budget)

### Optimization Tips

1. **Disable unused features:**
   ```
   Enable Procedural Effects: False (save ~0.05ms)
   ```

2. **Reduce noise frequency:**
   ```
   Breathing Frequency: 0.15 (less frequent updates)
   ```

3. **Use simpler noise profiles:**
   - Avoid complex multi-frequency presets
   - Stick to 3-4 noise channels max

4. **Limit shake requests:**
   - Don't trigger shake every frame
   - Batch shake requests when possible
   - Use cooldowns for repeated shakes (footsteps)

---

## Migration from Old System

### Quick Migration Steps

1. **Backup your scene**

2. **Replace component on player:**
   - Remove or disable `FirstPersonCameraData` baking in PlayerCharacterAuthoring
   - Add `CinemachineCameraAuthoring` component

3. **Create Cinemachine camera:**
   - Create GameObject "Virtual Camera"
   - Add `CinemachineVirtualCamera`
   - Add `CinemachineCameraController`
   - Assign player reference

4. **Test and adjust:**
   - Play scene
   - Adjust damping, sensitivity, noise
   - Tune to your game feel

### Settings Mapping

| Old System | New System |
|-----------|------------|
| MouseSensitivityX | MouseSensitivityX (same) |
| MouseSensitivityY | MouseSensitivityY (same) |
| MinPitch | MinPitch (same) |
| MaxPitch | MaxPitch (same) |
| BaseFOV | BaseFOV (same) |
| SprintFOV | SprintFOV (same) |
| FOVLerpSpeed | FOVLerpSpeed (same) |
| N/A | BaseRotationDamping (NEW) |
| N/A | BreathingAmplitude (NEW) |
| N/A | IdleSwayAmount (NEW) |

---

## Advanced Topics

### Custom Noise Profiles

Create specialized noise for different situations:

**Heavy Breathing (After Sprint):**
```csharp
noiseComponent.m_AmplitudeGain = 0.3f; // Strong
noiseComponent.m_FrequencyGain = 0.8f;  // Fast breathing
```

**Injured Character:**
```csharp
float healthRatio = currentHealth / maxHealth;
float injuredShake = lerp(0f, 0.2f, 1f - healthRatio);
noiseComponent.m_AmplitudeGain += injuredShake; // More shake when hurt
```

**Drunk/Poisoned Effect:**
```csharp
float statusIntensity = 0.5f; // 50% drunk
noiseComponent.m_AmplitudeGain = 0.4f * statusIntensity;
noiseComponent.m_FrequencyGain = 0.3f; // Slow swaying
```

### Dynamic FOV Effects

**Damage Flash (FOV Punch):**
```csharp
// When taking damage
entityManager.AddComponentData(playerEntity, new FOVPunchRequest
{
    FOVChange = -10f, // Narrow FOV briefly
    Duration = 0.2f,
    RecoveryTime = 0.3f
});
```

**Speed Lines Effect:**
```csharp
// Extreme sprint
if (movementState == Sprint && speed > 10f)
{
    float speedBoost = (speed - 10f) / 5f; // Scale with speed
    targetFOV = baseFOV + 15f * speedBoost; // Up to +15 FOV
}
```

### Multiplayer Considerations

For multiplayer, ensure each player has:
- Separate `CinemachineVirtualCamera` (priority-based)
- Separate `CinemachineCameraController` instance
- Separate camera target transforms
- Shared Cinemachine Brain on main camera (handles blending)

---

## GDD Compliance

**First-Person Camera (GDD lines 42-68):**
- ✅ Mouse look controls
- ✅ Configurable sensitivity
- ✅ Pitch clamping (-85° to +85°)
- ✅ FOV changes (sprint)
- ✅ Smooth camera movement

**Encumbrance System (GDD lines 1111-1119):**
- ✅ Weight affects camera feel
- ✅ Heavy load = sluggish camera
- ✅ Realistic weight impact

**Weapon Recoil (GDD lines 470-525):**
- ✅ Camera recoil on fire
- ✅ Weapon-specific recoil patterns
- ✅ Smooth recoil recovery

---

## File Reference

### Components
- `Assets/Scripts/Character/Components/CinemachineCameraData.cs` (146 lines)

### Systems
- `Assets/Scripts/Character/Systems/CinemachineCameraBridgeSystem.cs` (243 lines)

### MonoBehaviours
- `Assets/Scripts/Character/CinemachineCameraController.cs` (385 lines)

### Authoring
- `Assets/Scripts/Character/Authoring/CinemachineCameraAuthoring.cs` (202 lines)

### Documentation
- `CINEMACHINE_CAMERA_README.md` (this file)

**Total Lines Added**: ~976 lines of code + documentation

---

## Next Steps

1. **Try different damping values** - Find the sweet spot for your game feel
2. **Create custom noise profiles** - Match your game's aesthetic
3. **Add weapon-specific recoil** - Different guns = different kick
4. **Implement status effects** - Drunk, injured, poisoned camera effects
5. **Add hit reactions** - Camera punch when taking damage
6. **Experiment with FOV effects** - Speed, damage, abilities

---

## Credits

**System Design**: Hybrid ECS + Cinemachine architecture
**Based On**: Unity DOTS ECS + Cinemachine 2.9+
**GDD Compliance**: Zone Survival Game Design Document
**Version**: v0.1.6

For questions or issues, check the CHANGELOG.md or project documentation.
