using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Combat
{
    /// <summary>
    /// Component for entities that can take damage (NPCs, players, destructibles)
    /// </summary>
    public struct HealthData : IComponentData
    {
        public float CurrentHealth;
        public float MaxHealth;
        public bool IsDead;
        public bool IsInvulnerable;          // For dodge i-frames, etc.

        // Armor
        public float ArmorValue;             // 0-100, reduces damage
        public float ArmorDurability;        // Armor degrades when hit

        // Recent damage tracking (for UI feedback)
        public float DamageTakenThisFrame;
        public float TimeSinceLastDamage;
    }

    /// <summary>
    /// Tag component for entities hit this frame
    /// Processed by damage application system, then removed
    /// </summary>
    public struct DamageEvent : IComponentData
    {
        public float Damage;
        public float ArmorPenetration;       // 0.0-1.0, bypasses armor
        public float3 HitPosition;           // World position of hit
        public float3 HitDirection;          // Direction of damage
        public Entity Attacker;              // Who caused the damage
        public DamageType Type;              // Bullet, explosion, melee, etc.
    }

    /// <summary>
    /// Types of damage for different effects
    /// </summary>
    public enum DamageType : byte
    {
        Bullet = 0,
        Explosion = 1,
        Melee = 2,
        Fall = 3,
        Anomaly = 4,
        Radiation = 5,
        Bleed = 6
    }

    /// <summary>
    /// Hitbox component - defines vulnerable areas on entity
    /// Multiple hitboxes per entity for headshots, body shots, etc.
    /// </summary>
    public struct HitboxData : IComponentData
    {
        public Entity ParentEntity;          // Reference to the entity this hitbox belongs to
        public HitboxType Type;              // Head, torso, limb
        public float DamageMultiplier;       // Headshot = 2.0x, leg = 0.5x, etc.
    }

    public enum HitboxType : byte
    {
        Head = 0,
        Torso = 1,
        ArmLeft = 2,
        ArmRight = 3,
        LegLeft = 4,
        LegRight = 5
    }
}
