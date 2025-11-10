# Weapons System Documentation (v0.4.0)

## Overview

Complete implementation of a Tarkov-style modular weapons system with two-stage durability, part-based customization, jamming mechanics, and easy-to-use authoring tools for adding weapons and parts.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Architecture](#architecture)
3. [Two-Stage Durability System](#two-stage-durability-system)
4. [Adding Weapons](#adding-weapons)
5. [Adding Parts](#adding-parts)
6. [Weapon Stats Calculation](#weapon-stats-calculation)
7. [Combat Systems](#combat-systems)
8. [Part Compatibility](#part-compatibility)
9. [File Reference](#file-reference)

---

## Quick Start

### Method 1: ScriptableObject Database (EASIEST)

**Create a Weapon:**
1. Right-click in Project window
2. Select `Create -> Zone Survival -> Weapons -> Weapon Definition`
3. Name it (e.g., "AK74_Definition")
4. Configure all properties in Inspector
5. Done! Reference this asset in spawning systems

**Create a Part:**
1. Right-click in Project window
2. Select `Create -> Zone Survival -> Weapons -> Weapon Part Definition`
3. Name it (e.g., "LongBarrel_Definition")
4. Configure part type, modifiers, and compatibility
5. Done! Can be attached to weapons

### Method 2: Authoring Components

**Create a Weapon in Scene:**
1. Create empty GameObject
2. Add `WeaponAuthoring` component
3. Configure values in Inspector
4. Enter Play mode - weapon entity created automatically

**Create a Part in Scene:**
1. Create empty GameObject
2. Add `WeaponPartAuthoring` component
3. Set part type and modifiers
4. Attach to weapon's part buffer (via code or tool)

### Method 3: Runtime Spawning (Advanced)

```csharp
// Spawn weapon from WeaponDefinition
EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
Entity weaponEntity = entityManager.CreateEntity();
weaponDefinition.ApplyToWeapon(weaponEntity, entityManager);
```

---

## Architecture

### Component Hierarchy

```
Weapon Entity
├── ItemData (base item properties)
├── WeaponItemData (weapon-specific stats)
├── WeaponStateData (runtime state)
├── WeaponPartElement[] (buffer of attached parts)
└── WeaponPartSlotDefinition[] (defines which parts can be attached)

Part Entity
└── WeaponPartData (part properties and modifiers)
```

### System Update Order

```
1. PlayerInputSystem (captures input)
   ↓
2. WeaponStatsCalculationSystem (calculates final stats from parts + condition)
   ↓
3. WeaponReloadSystem (handles reload and unjam)
   ↓
4. WeaponEquipSystem (equip/holster weapons)
   ↓
5. WeaponFiringSystem (fires weapon, applies jam chance)
```

---

## Two-Stage Durability System

### Stage 1: Overall Weapon Condition

Stored in `ItemData.Condition` (0.0 - 1.0)

**Affects:**
- All stats globally (accuracy, recoil, damage, range)
- Base jam chance (increases when condition < 70%)
- Overall weapon reliability

**Degradation:**
- Lost per shot based on `WeaponItemData.DegradationPerShot`
- Can be restored via cleaning/repair

### Stage 2: Individual Part Condition

Stored in `WeaponPartData.Condition` (0.0 - 1.0) for EACH part

**Part-Specific Effects:**

| Part Type | Affects When Degraded |
|-----------|----------------------|
| **Barrel** | Accuracy, Range, Jam Chance |
| **Firing Pin** | Jam Chance (misfires) |
| **Bolt** | Jam Chance (cycling issues) |
| **Receiver** | Overall Reliability |
| **Magazine** | Feed Reliability |
| **Stock** | Recoil, Ergonomics |
| **Grip** | Recoil, Ergonomics |
| **Scope** | Accuracy |
| **Muzzle** | Recoil, Accuracy |

**Critical Parts:**
If Barrel, Firing Pin, or Bolt are missing or broken (condition = 0), weapon CANNOT fire.

### Combined Jam Calculation

```
Final Jam Chance = Base Jam Chance
                 + Overall Condition Penalty (if < 70%)
                 + Barrel Condition Penalty
                 + Firing Pin Condition Penalty
                 + Bolt Condition Penalty
                 + Magazine Condition Penalty
                 + Receiver Condition Penalty
```

Maximum jam chance is capped at 95% (always 5% chance to fire).

---

## Adding Weapons

### Using WeaponDefinition (Recommended)

**1. Create Definition Asset**
```
Assets/
  Weapons/
    Definitions/
      AK74.asset (WeaponDefinition)
```

**2. Configure Properties**

```yaml
Weapon Identity:
  WeaponID: 1001
  WeaponName: "AK-74"
  WeaponType: AssaultRifle

Item Properties:
  Category: Weapon
  Rarity: Common
  Weight: 3.5 kg
  GridWidth: 4
  GridHeight: 2
  BaseValue: 15000 RU
  StartingCondition: 1.0

Damage:
  BaseDamage: 45
  ArmorPenetration: 0.6

Fire Rate:
  FireRate: 600 RPM
  AvailableFireModes: Semi | Auto
  DefaultFireMode: Semi
  BurstCount: 3

Magazine:
  MagazineSize: 30
  StartingAmmo: 30
  StartingReserveAmmo: 90

Accuracy & Recoil:
  BaseAccuracy: 0.75
  RecoilMultiplier: 1.2
  AimDownSightTime: 0.35s

Range:
  EffectiveRange: 300m
  MaxRange: 800m

Durability & Jamming:
  DegradationPerShot: 0.0001
  BaseJamChance: 0.01 (1%)
```

**3. Reference in Code**

```csharp
public WeaponDefinition ak74Definition;

void SpawnWeapon()
{
    // Create weapon from definition
    GameObject weaponGO = new GameObject("AK74");
    WeaponAuthoring authoring = weaponGO.AddComponent<WeaponAuthoring>();
    ak74Definition.ApplyToAuthoring(authoring);
}
```

### Common Weapon Presets

**Assault Rifle (AK-74)**
- Damage: 45
- Fire Rate: 600 RPM
- Accuracy: 0.75
- Range: 300m
- Modes: Semi/Auto

**SMG (MP5)**
- Damage: 30
- Fire Rate: 800 RPM
- Accuracy: 0.7
- Range: 150m
- Modes: Semi/Burst/Auto

**Sniper Rifle (SVD)**
- Damage: 85
- Fire Rate: 60 RPM
- Accuracy: 0.95
- Range: 800m
- Modes: Semi

**Shotgun (TOZ-34)**
- Damage: 120
- Fire Rate: 60 RPM
- Accuracy: 0.6
- Range: 50m
- Modes: Semi

**Pistol (Makarov)**
- Damage: 35
- Fire Rate: 300 RPM
- Accuracy: 0.65
- Range: 50m
- Modes: Semi

---

## Adding Parts

### Using WeaponPartDefinition (Recommended)

**1. Create Definition Asset**
```
Assets/
  Weapons/
    Parts/
      Barrels/
        LongBarrel_AK.asset (WeaponPartDefinition)
```

**2. Configure Properties**

```yaml
Part Identity:
  PartID: 2001
  PartName: "Extended Barrel"
  PartType: Barrel
  MountType: Integrated

Condition:
  StartingCondition: 1.0
  MaxCondition: 1.0
  DegradationRate: 0.00005

Physical Properties:
  Weight: 0.8 kg
  IsVisible: true

Performance Modifiers:
  AccuracyModifier: +0.1 (10% better accuracy)
  RecoilModifier: +0.05 (5% more recoil)
  RangeModifier: +50m
  ErgoModifier: +0.05 (slower ADS)
  DamageModifier: +0.02 (2% more damage)

Reliability:
  JamChanceWhenDegraded: 0.02 (2% when < 50%)

Compatibility:
  CompatibleWeapons: [AK74, AKM, AK103]
```

### Part Presets

The `WeaponPartAuthoring` component includes quick presets:

- **Custom**: Manual configuration
- **LowQuality**: -10% accuracy, +10% recoil, -20m range
- **Standard**: Neutral (all zeros)
- **HighQuality**: +10% accuracy, -5% recoil, +30m range
- **Tactical**: Optimized for CQB (-10% recoil, -10% ADS time)
- **Sniper**: Optimized for range (+20% accuracy, +100m range)

### Attaching Parts to Weapons

**At Authoring Time:**
```csharp
// In WeaponAuthoring Inspector
[Header("Part Slots")]
public WeaponPartSlotDefinitionAuthoring[] PartSlots = new[]
{
    new WeaponPartSlotDefinitionAuthoring
    {
        SlotType = WeaponPartType.Barrel,
        Required = true,
        MountType = PartMountType.Integrated,
        MaxCount = 1
    },
    // ... more slots
};
```

**At Runtime:**
```csharp
// Get weapon entity's part buffer
DynamicBuffer<WeaponPartElement> partsBuffer = entityManager.GetBuffer<WeaponPartElement>(weaponEntity);

// Create part entity
Entity partEntity = CreatePartEntity(partDefinition);

// Attach to weapon
partsBuffer.Add(new WeaponPartElement
{
    PartEntity = partEntity,
    SlotType = WeaponPartType.Barrel,
    IsRequired = true
});

// Stats will automatically recalculate next frame
```

---

## Weapon Stats Calculation

The `WeaponStatsCalculationSystem` runs every frame for equipped weapons and calculates final stats:

### Calculation Flow

```
1. Start with Base Weapon Stats (from WeaponItemData)
   ↓
2. Apply Overall Condition Modifier (from ItemData.Condition)
   - All stats *= condition (0.5 at 0%, 1.0 at 100%)
   ↓
3. Apply Part-Specific Modifiers
   - Iterate through all attached parts
   - Add each part's modifiers (accuracy, recoil, etc.)
   - Factor in part condition for critical parts
   ↓
4. Calculate Jam Chance
   - Base jam chance + overall condition penalty + part penalties
   ↓
5. Check Critical Parts
   - If Barrel/FiringPin/Bolt missing → jam chance = 100%
   ↓
6. Clamp Final Values
   - Accuracy: 0.0 - 1.0
   - Recoil: minimum 0.1
   - Damage/Range: minimum 0
   - Jam Chance: 0.0 - 0.95 (max 95%)
   ↓
7. Update WeaponStateData.Calculated* Fields
```

### Example Calculation

**AK-74 with Extended Barrel**

```
Base Stats:
- Accuracy: 0.75
- Recoil: 1.2
- Range: 300m
- Jam Chance: 0.01

Overall Condition: 0.8 (80%)
- Accuracy: 0.75 * 0.9 = 0.675
- Recoil: 1.2 / 0.9 = 1.33
- Range: 300 * 0.9 = 270m
- Jam Chance: 0.01 + 0.0 (condition > 70%)

Extended Barrel Modifiers:
- Accuracy: +0.1
- Recoil: +0.05
- Range: +50m

Final Calculated Stats:
- Accuracy: 0.675 + 0.1 = 0.775
- Recoil: 1.33 + 0.05 = 1.38
- Range: 270 + 50 = 320m
- Jam Chance: 0.01
```

---

## Combat Systems

### Firing System

**Fire Modes:**
- **Safe**: Cannot fire
- **Semi**: One shot per trigger pull
- **Burst**: 3 rounds per trigger pull
- **Auto**: Continuous fire while held
- **Bolt Action**: Manual cycling required

**Fire Rate Limiting:**
```csharp
float roundsPerMinute = 600f;
float secondsPerRound = 60f / roundsPerMinute;
weaponState.FireCooldown = secondsPerRound;
```

**Jamming Check:**
```csharp
float jamChance = weaponState.CalculatedJamChance;

// Extra jam chance for dirty weapons
if (weaponState.ShotsSinceCleaning > 500)
{
    jamChance += 0.01f; // +1% per 500 rounds
}

if (random.NextFloat() < jamChance)
{
    weaponState.IsJammed = true;
}
```

### Reload System

**Reload Triggers:**
- R key (manual reload)
- Auto-reload when magazine empty and fire button pressed

**Reload Time:**
```csharp
float baseReloadTime = 2.5f;
weaponState.ReloadTime = baseReloadTime * weaponState.CalculatedErgo;
```

**Ammo Transfer:**
```csharp
int ammoNeeded = MaxMagazineCapacity - CurrentMagazineAmmo;
int ammoToLoad = min(ammoNeeded, ReserveAmmo);
CurrentMagazineAmmo += ammoToLoad;
ReserveAmmo -= ammoToLoad;
```

### Unjamming System

**Unjam Trigger:**
- R key while jammed (2 seconds to clear)

**Unjam Time:**
```csharp
weaponState.UnjamTime = 2.0f; // Fixed 2 seconds
weaponState.UnjamProgress += deltaTime / UnjamTime;
if (UnjamProgress >= 1.0f)
{
    weaponState.IsJammed = false;
}
```

### Equip System

**Equip Triggers:**
- 1-0 keys (quick slots)
- Pressing same key toggles holster

**Draw Time:**
```csharp
weaponState.HolsterSpeed = weaponState.CalculatedErgo;
weaponState.DrawProgress += deltaTime / HolsterSpeed;
```

---

## Part Compatibility

### Slot Definition System

Each weapon defines which parts it can accept:

```csharp
public struct WeaponPartSlotDefinition : IBufferElementData
{
    public WeaponPartType SlotType;      // What type of part
    public PartMountType RequiredMount;  // How it attaches
    public bool IsRequired;              // Must have this part to function
    public int MaxCount;                 // How many (1 barrel, 3 rails, etc.)
}
```

### Mount Types

- **Integrated**: Built into weapon, cannot be removed
- **Picatinny**: Standard NATO rail
- **MLok**: M-LOK mounting system
- **KeyMod**: KeyMod mounting system
- **Proprietary**: Weapon-specific (e.g., AK rail systems)
- **Universal**: Fits any weapon of this type

### Compatibility Checking (Future System)

```csharp
bool CanAttachPart(Entity weaponEntity, Entity partEntity)
{
    var part = GetComponent<WeaponPartData>(partEntity);
    var slots = GetBuffer<WeaponPartSlotDefinition>(weaponEntity);

    foreach (var slot in slots)
    {
        if (slot.SlotType == part.PartType &&
            slot.RequiredMount == part.MountType)
        {
            return true;
        }
    }

    return false;
}
```

---

## File Reference

### Components (7 files)

**`WeaponPartType.cs`**
- Enums for part types and mount types
- 13 part types: Barrel, Receiver, Bolt, FiringPin, Trigger, Magazine, Grip, Stock, Scope, Muzzle, Rail, Laser, Flashlight
- 6 mount types: Integrated, Picatinny, MLok, KeyMod, Proprietary, Universal

**`WeaponPartData.cs`**
- Individual part component
- Stores: ID, name, type, mount, condition, weight, modifiers, jam contribution
- `WeaponPartElement` buffer: Attached parts on weapon
- `WeaponPartSlotDefinition` buffer: Compatible part slots

**`WeaponStateData.cs`**
- Runtime weapon state
- Equip state, fire mode, ammo counts, firing state, reload state, jam state
- Calculated stats (updated by WeaponStatsCalculationSystem)
- `EquippedWeaponTag`: Marks currently equipped weapon

### Systems (4 files)

**`WeaponStatsCalculationSystem.cs`** (242 lines)
- Calculates final weapon stats from base + condition + parts
- Two-stage durability implementation
- Critical part missing check
- Updates all Calculated* fields in WeaponStateData

**`WeaponFiringSystem.cs`** (180 lines)
- Handles weapon firing
- Fire mode processing (safe, semi, burst, auto)
- Fire rate limiting
- Jam chance roll
- Ammo consumption

**`WeaponReloadSystem.cs`** (130 lines)
- Reload and unjam handling
- Manual and auto-reload
- Reload time calculation (affected by ergonomics)
- Unjam progress tracking

**`WeaponEquipSystem.cs`** (150 lines)
- Equip/holster weapons from quick slots
- Draw animation progress
- Holster state management

### Authoring (2 files)

**`WeaponAuthoring.cs`** (280 lines)
- Unity Inspector-based weapon creation
- All weapon properties configurable
- Bakes to: ItemData, WeaponItemData, WeaponStateData
- Part slot definition
- EASY WEAPON CREATION: Just add component, configure, play

**`WeaponPartAuthoring.cs`** (180 lines)
- Unity Inspector-based part creation
- Preset system (LowQuality, Standard, HighQuality, Tactical, Sniper)
- All modifiers configurable
- Bakes to: WeaponPartData
- EASY PART CREATION: Add component, select preset or customize

### Database (2 files)

**`WeaponDefinition.cs`** (ScriptableObject)
- Asset-based weapon configuration
- Create via: Create -> Zone Survival -> Weapons -> Weapon Definition
- Reusable, version-control friendly
- Can be loaded at runtime for modding

**`WeaponPartDefinition.cs`** (ScriptableObject)
- Asset-based part configuration
- Create via: Create -> Zone Survival -> Weapons -> Weapon Part Definition
- Compatibility system with WeaponDefinition
- Part library management

---

## Integration with Existing Systems

### Player Input (v0.3.1 Unified System)

Weapons read from `PlayerInputData`:
```csharp
input.ValueRO.InteractPressed // Fire button (temporary)
input.ValueRO.WeaponSlotPressed // 1-0 keys for quick slots
input.ValueRO.SprintPressed // R key for reload (temporary)
```

**TODO:** Add dedicated weapon inputs to `PlayerInputData`:
- `FirePressed`/`FireHeld`
- `ReloadPressed`
- `FireModeTogglePressed`
- `InspectWeaponPressed`

### Inventory System (v0.3.0)

Weapons stored in inventory as items:
- Use `InventoryData` for storage
- Use `QuickSlotsData.WeaponSlots` for quick access
- Weapon weight contributes to `EncumbranceData.CurrentWeight`

### Character Controller (v0.1.0 - v0.2.0)

Weapon stats can affect movement:
- High recoil weapons → apply camera shake to `FirstPersonCameraData`
- Heavy weapons → slow ADS movement speed
- Weapon weight → adds to encumbrance

---

## Future Enhancements

### Planned Features

1. **Projectile System**
   - Raycast hit detection
   - Bullet penetration
   - Ballistics simulation

2. **Weapon Modding UI**
   - Visual workbench
   - Drag-and-drop part attachment
   - Real-time stat preview

3. **Cleaning System**
   - Cleaning kits as consumables
   - Restores condition
   - Resets `ShotsSinceCleaning` counter

4. **Weapon Animations**
   - Fire animations
   - Reload animations
   - Inspect animations

5. **Advanced Jamming Types**
   - Misfire (firing pin issue)
   - Failure to feed (magazine issue)
   - Failure to eject (bolt issue)
   - Stovepipe (receiver issue)

6. **Weapon Repair System**
   - Swap broken parts
   - Repair at workbenches
   - Skill-based repair quality

---

## Performance Considerations

- **WeaponStatsCalculationSystem**: Only runs for equipped weapons
- **WeaponFiringSystem**: Only processes when weapon drawn
- **Part Buffers**: Dynamic size, efficient memory usage
- **Jam Rolls**: Single random roll per shot, minimal overhead

**Expected Performance:**
- Weapon Stats Calculation: ~0.1ms (1 equipped weapon)
- Weapon Firing: ~0.05ms per shot
- Reload System: ~0.02ms when active
- Equip System: ~0.03ms during transitions

**Total Addition**: <0.2ms per frame for active combat

---

## GDD Compliance

**Weapon System (GDD lines 470-525):**
- ✅ Multiple weapon types (pistol, SMG, AR, sniper, shotgun, melee)
- ✅ Condition/degradation system
- ✅ Jamming mechanics
- ✅ Modular attachment system (Tarkov-style)
- ✅ Ammo tracking (magazine + reserve)
- ✅ Fire modes (safe, semi, burst, auto)

**Item System Integration:**
- ✅ Weapons stored in Tetris-grid inventory
- ✅ Weight affects encumbrance
- ✅ Condition displayed in UI
- ✅ Trading/economy ready

---

## Developer Notes

### Two-Stage Durability Importance

The two-stage durability system is CRITICAL for Tarkov-style gameplay:

1. **Overall condition** provides simple feedback to players
2. **Part-specific condition** creates depth and customization
3. **Both affect jamming** creates emergent gameplay (swap out worn firing pin to reduce jams)
4. **Part replacement** gives value to looted parts
5. **Repair vs Replace** economic choices

### Why This Architecture?

- **ECS Performance**: Handles hundreds of weapons efficiently
- **Data-Driven**: All stats in data components, easy to balance
- **Modular**: Parts are independent entities
- **Extensible**: Easy to add new part types or weapons
- **Designer-Friendly**: ScriptableObject workflow familiar to Unity devs
- **Mod-Friendly**: Can load weapon/part definitions at runtime

### Common Pitfalls

❌ **Don't**: Modify `WeaponStateData.Calculated*` fields directly
✅ **Do**: Let `WeaponStatsCalculationSystem` calculate them

❌ **Don't**: Add parts directly to `WeaponPartElement` buffer without checking compatibility
✅ **Do**: Use compatibility checking system (when implemented)

❌ **Don't**: Forget to set `IsRequired = true` for critical parts (barrel, bolt, firing pin)
✅ **Do**: Mark critical parts as required in `WeaponPartSlotDefinition`

---

## Quick Reference: Adding Your First Weapon

**5-Minute Weapon Creation:**

1. Create new WeaponDefinition asset
2. Set name: "My Custom AK"
3. Set stats (copy from AK-74 example above)
4. Save asset
5. Reference in loot spawn system

**Done!** Weapon is now in the game with full functionality:
- Two-stage durability ✓
- Jamming mechanics ✓
- Fire/reload/equip ✓
- Part attachment ready ✓
- Inventory integration ✓

---

**END OF DOCUMENTATION**

For questions or issues, refer to CHANGELOG.md for version history and known limitations.
