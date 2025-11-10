# Character Controller Physics - Testing Guide

## Version 0.2.0 - Physics Integration Complete

This guide covers testing the newly implemented physics features:
- Ground detection with raycasting
- Capsule height adjustment for stances
- Jump mechanics with buffering and coyote time
- Slope handling and ground snapping

---

## Prerequisites

Before testing, ensure:
1. Unity.Physics package is installed (`com.unity.physics`)
2. Scene has a ground plane with physics collision
3. Player character has `PlayerCharacterAuthoring` component attached
4. All new components are properly baked to ECS entity

---

## Test 1: Ground Detection

### Setup
1. Create a test scene with varied terrain:
   - Flat ground plane at Y=0
   - Some raised platforms (0.5m, 1m, 2m heights)
   - A ramp/slope (30° angle)

### Test Cases

#### TC1.1: Basic Ground Detection
**Steps:**
1. Place character at Y=2 (slightly above ground)
2. Enter Play mode
3. Character should fall and detect ground

**Expected Results:**
- Character falls due to gravity
- `IsGrounded` becomes `true` when landing
- Character stops falling at ground level
- Console shows no errors

#### TC1.2: Platform Edges
**Steps:**
1. Walk character to edge of platform
2. Continue walking off edge

**Expected Results:**
- Character leaves ground smoothly
- `IsGrounded` becomes `false`
- Coyote time activates (0.1s grace period)
- Character begins falling

#### TC1.3: Slope Detection
**Steps:**
1. Walk up a 30° slope
2. Observe movement behavior

**Expected Results:**
- Character moves smoothly up slope
- `GroundAngle` reports ~30°
- Movement is projected onto slope normal
- No jittering or floating

**Debug Values to Monitor:**
```
GroundDetectionData.IsGrounded: true/false
GroundDetectionData.GroundAngle: 0-90°
GroundDetectionData.GroundDistance: 0-0.1m when grounded
```

---

## Test 2: Capsule Height Adjustment

### Test Cases

#### TC2.1: Standing to Crouching
**Steps:**
1. Start in standing state
2. Press and hold Ctrl (crouch)
3. Observe capsule height change

**Expected Results:**
- Height smoothly transitions from 1.8m → 1.2m
- Camera position lowers smoothly
- FOV remains stable
- Character doesn't clip through ground

**Debug Values:**
```
CharacterPhysicsData.CurrentHeight: 1.8 → 1.2
CharacterStateData.CurrentHeight: 1.8 → 1.2
FirstPersonCameraData.CameraOffset.y: ~1.6 → ~1.08
```

#### TC2.2: Crouching to Prone
**Steps:**
1. From crouch state
2. Press Z (prone)
3. Observe height change

**Expected Results:**
- Height transitions from 1.2m → 0.5m
- Camera lowers significantly
- Movement speed reduces to 0.8 m/s
- Cannot dodge while prone

**Debug Values:**
```
CharacterPhysicsData.CurrentHeight: 1.2 → 0.5
CharacterStateData.CurrentState: Crouching → Prone
```

#### TC2.3: Rapid Stance Changes
**Steps:**
1. Rapidly press Ctrl and Z
2. Alternate between states quickly

**Expected Results:**
- Smooth interpolation between all states
- No capsule clipping or teleporting
- Camera follows smoothly
- No system errors

#### TC2.4: Prone to Standing Transition
**Steps:**
1. Go prone (Z key)
2. Release Z and Ctrl
3. Observe return to standing

**Expected Results:**
- Height transitions 0.5m → 1.8m
- Smooth camera rise
- Movement speed returns to normal
- Character doesn't penetrate ceiling if under low obstacle

---

## Test 3: Jump Mechanics

### Test Cases

#### TC3.1: Basic Jump
**Steps:**
1. Stand on ground
2. Press Space
3. Observe jump

**Expected Results:**
- Character launches upward with `JumpForce` velocity
- `IsGrounded` becomes `false`
- Stamina decreases by `JumpStaminaCost` (10 points)
- Jump cooldown activates (0.2s)
- Character lands smoothly

**Debug Values:**
```
JumpData.IsJumping: false → true → false (on land)
JumpData.CanJump: true → false → true (after cooldown)
StaminaData.Current: decreases by 10
CharacterMovementData.Velocity.y: ~5.0 (jump force)
```

#### TC3.2: Jump Buffering
**Steps:**
1. Jump off a platform
2. While in air, press Space ~0.1s before landing
3. Observe behavior on landing

**Expected Results:**
- Jump input is buffered
- Character jumps immediately upon landing
- No need to time jump perfectly
- Feels responsive

**Debug Values:**
```
JumpData.JumpBufferCounter: 0 → 0.15 → decreasing → 0
```

#### TC3.3: Coyote Time
**Steps:**
1. Walk off platform edge (don't jump)
2. Press Space within 0.1s after leaving ground
3. Observe jump execution

**Expected Results:**
- Jump executes even though not grounded
- Feels forgiving and responsive
- Grace period allows late jump input

**Debug Values:**
```
JumpData.CoyoteTimeCounter: 0.1 → decreasing
GroundDetectionData.IsGrounded: false
JumpData.CanJump: true (due to coyote time)
```

#### TC3.4: Jump Spam Prevention
**Steps:**
1. Mash Space key rapidly
2. Try to jump repeatedly

**Expected Results:**
- Only one jump executes at a time
- 0.2s cooldown between jumps
- Cannot spam jump infinitely
- Stamina drains per jump

#### TC3.5: Jump While Crouching
**Steps:**
1. Crouch (hold Ctrl)
2. Press Space to jump
3. Observe behavior

**Expected Results:**
- Can jump from crouch
- May have slightly reduced height (optional)
- Automatically stands when jumping

#### TC3.6: Jump While Prone (Should Fail)
**Steps:**
1. Go prone (Z key)
2. Press Space
3. Observe no jump occurs

**Expected Results:**
- Jump does NOT execute
- Character remains prone
- No stamina consumed
- Logical restriction enforced

#### TC3.7: Insufficient Stamina Jump
**Steps:**
1. Sprint until stamina < 10 points
2. Try to jump
3. Observe behavior

**Expected Results:**
- Jump does NOT execute
- No stamina consumed
- Character stays grounded
- Stamina gate works correctly

---

## Test 4: Slope Handling

### Setup
Create slopes of varying angles:
- 15° (easy walkable)
- 30° (moderate)
- 45° (slope limit)
- 60° (too steep)

### Test Cases

#### TC4.1: Walking Up Slopes
**Steps:**
1. Walk forward up 30° slope
2. Observe movement

**Expected Results:**
- Movement projects onto slope surface
- Speed may reduce slightly on steep slopes
- Character doesn't slide backward
- Smooth climbing motion

#### TC4.2: Walking Down Slopes
**Steps:**
1. Walk forward down 30° slope
2. Observe ground contact

**Expected Results:**
- Character stays grounded (no floating)
- Smooth descent
- Ground snapping keeps contact
- No bouncing or jittering

#### TC4.3: Slope Limit (45°)
**Steps:**
1. Approach 45° slope (at slope limit)
2. Try to walk up

**Expected Results:**
- Can barely walk up (very slow)
- Character may struggle or slide
- Respects `SlopeLimit` parameter

#### TC4.4: Too Steep (60°)
**Steps:**
1. Approach 60° slope (exceeds limit)
2. Try to walk up

**Expected Results:**
- Character cannot climb
- Slides backward or stops
- `GroundAngle` reports 60°
- Movement blocked by steep angle

---

## Test 5: Physics Integration

### Test Cases

#### TC5.1: Jumping on Slopes
**Steps:**
1. Stand on 30° slope
2. Jump
3. Observe trajectory

**Expected Results:**
- Jump launches perpendicular to slope (optional)
- OR launches straight up (simpler)
- Lands back on slope smoothly
- No clipping through slope

#### TC5.2: Falling from Height
**Steps:**
1. Place character at Y=10
2. Let fall
3. Observe landing

**Expected Results:**
- Falls with gravity (20 m/s²)
- Terminal velocity reached
- Lands with ground detection
- `JustLanded` flag triggers
- Can jump again immediately after landing

#### TC5.3: Dodge While Grounded
**Steps:**
1. Stand on ground
2. Strafe (A or D) + Jump (dodge)
3. Observe interaction

**Expected Results:**
- Dodge executes (2.5m displacement)
- Ground detection still works
- Can land mid-dodge
- No conflicts with jump system

---

## Test 6: Edge Cases

### Test Cases

#### TC6.1: Teleporting to High Altitude
**Steps:**
1. Using Unity Inspector, set Position.Y = 50
2. Observe behavior

**Expected Results:**
- Character enters free-fall
- `IsGrounded` = false
- Gravity applies immediately
- Lands safely when reaching ground

#### TC6.2: Ground Disappearing
**Steps:**
1. Stand on a platform
2. Disable the platform GameObject
3. Observe behavior

**Expected Results:**
- Character immediately becomes ungrounded
- Begins falling
- Coyote time activates briefly
- No crashes or errors

#### TC6.3: Rapid Height Transitions
**Steps:**
1. Set `HeightTransitionSpeed` to 50
2. Rapidly change stances
3. Observe capsule behavior

**Expected Results:**
- Very fast transitions
- No clipping or glitches
- Systems handle high speed changes
- Camera follows correctly

#### TC6.4: Zero Stamina Jump
**Steps:**
1. Set `JumpStaminaCost` to 0 in Inspector
2. Jump repeatedly
3. Observe stamina

**Expected Results:**
- Jumping doesn't cost stamina
- Can jump freely
- Cooldown still applies
- Optional cost system works

---

## Debug Visualization (Recommended)

### Add to Scene for Testing

Create a simple debug script to visualize:

```csharp
// Pseudo-code for debug visualization
void OnDrawGizmos()
{
    // Ground check ray
    Gizmos.color = isGrounded ? Color.green : Color.red;
    Gizmos.DrawLine(position, position + Vector3.down * groundCheckDistance);

    // Capsule height
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(position, capsuleRadius);
    Gizmos.DrawWireSphere(position + Vector3.up * currentHeight, capsuleRadius);

    // Ground normal
    if (isGrounded)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(position, groundNormal * 2f);
    }
}
```

---

## Performance Testing

### Metrics to Monitor

1. **Frame Rate**: Should remain 60+ FPS
2. **System Update Time**: Each system < 1ms
3. **Memory**: No allocations per frame
4. **GC**: No garbage collection during gameplay

### Tools
- Unity Profiler (`Window > Analysis > Profiler`)
- Entity Debugger (`Window > Entities > Hierarchy`)
- Physics Debugger (`Window > Analysis > Physics Debugger`)

---

## Common Issues & Solutions

### Issue 1: Character Falls Through Ground
**Cause**: Ground has no physics collider
**Solution**: Add `PhysicsShapeAuthoring` to ground GameObject

### Issue 2: Jittery Ground Detection
**Cause**: `GroundCheckDistance` too large
**Solution**: Reduce to 0.1m or less

### Issue 3: Cannot Jump
**Cause**: Multiple possible causes
**Solutions**:
- Check stamina level (need 10+ points)
- Verify not in cooldown (0.2s)
- Ensure not prone
- Check `CanJump` flag in debugger

### Issue 4: Capsule Not Changing Height
**Cause**: `CapsuleHeightSystem` not running
**Solution**: Verify system appears in System window

### Issue 5: Floating Above Ground
**Cause**: Ground check offset incorrect
**Solution**: Set `groundCheckOffset` to (0, 0, 0) or bottom of capsule

---

## Test Checklist

Mark each test as you complete it:

**Ground Detection:**
- [ ] TC1.1: Basic ground detection
- [ ] TC1.2: Platform edges
- [ ] TC1.3: Slope detection

**Capsule Height:**
- [ ] TC2.1: Standing to crouching
- [ ] TC2.2: Crouching to prone
- [ ] TC2.3: Rapid stance changes
- [ ] TC2.4: Prone to standing

**Jump Mechanics:**
- [ ] TC3.1: Basic jump
- [ ] TC3.2: Jump buffering
- [ ] TC3.3: Coyote time
- [ ] TC3.4: Jump spam prevention
- [ ] TC3.5: Jump while crouching
- [ ] TC3.6: Jump while prone (fail)
- [ ] TC3.7: Insufficient stamina

**Slope Handling:**
- [ ] TC4.1: Walking up slopes
- [ ] TC4.2: Walking down slopes
- [ ] TC4.3: Slope limit (45°)
- [ ] TC4.4: Too steep (60°)

**Physics Integration:**
- [ ] TC5.1: Jumping on slopes
- [ ] TC5.2: Falling from height
- [ ] TC5.3: Dodge while grounded

**Edge Cases:**
- [ ] TC6.1: Teleporting to altitude
- [ ] TC6.2: Ground disappearing
- [ ] TC6.3: Rapid height transitions
- [ ] TC6.4: Zero stamina jump

---

## Success Criteria

Physics integration is considered complete when:
- ✅ All test cases pass
- ✅ No console errors during testing
- ✅ Frame rate remains 60+ FPS
- ✅ Character movement feels smooth and responsive
- ✅ Jump buffering and coyote time improve feel
- ✅ Capsule height transitions are smooth
- ✅ Ground detection is reliable on all terrains

---

**Version**: 0.2.0
**Date**: 2025-11-10
**Status**: Physics implementation complete, ready for testing
