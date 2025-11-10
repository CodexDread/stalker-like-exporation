using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Component for weapon visual effects configuration
    /// Muzzle flash, shell ejection, smoke, etc.
    /// </summary>
    public struct WeaponVisualEffectsData : IComponentData
    {
        // Muzzle Flash
        public int MuzzleFlashPrefabID;      // Particle effect prefab
        public float MuzzleFlashDuration;    // How long flash lasts (0.1s typical)
        public float MuzzleFlashScale;       // Size multiplier

        // Shell Ejection
        public bool EjectsShells;            // Does this weapon eject shells?
        public int ShellPrefabID;            // Shell casing prefab
        public float3 EjectionOffset;        // Local offset from weapon
        public float3 EjectionVelocity;      // Initial velocity (local space)
        public float EjectionDelay;          // Delay after firing (0.1s for most guns)

        // Smoke
        public bool HasSmoke;                // Gun smoke effect?
        public int SmokePrefabID;
        public float SmokeDuration;

        // Tracer
        public bool HasTracer;               // Visible bullet trail?
        public int TracerPrefabID;
        public float TracerSpeed;            // Visual speed (m/s)
        public float TracerLifetime;         // How long tracer lasts

        // Active effects tracking
        public float MuzzleFlashTimer;       // Current flash timer
        public float ShellEjectionTimer;     // Time until next shell ejects
    }

    /// <summary>
    /// Request tag - added to weapon entity when it fires
    /// Visual effects system processes these and spawns effects
    /// </summary>
    public struct WeaponFireEffectRequest : IComponentData
    {
        public float3 MuzzlePosition;
        public quaternion MuzzleRotation;
        public float3 HitPosition;           // Where bullet landed (for tracers)
        public bool DidHit;                  // Did bullet hit something?
    }
}
