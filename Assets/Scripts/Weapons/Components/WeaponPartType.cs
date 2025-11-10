using Unity.Entities;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Defines types of weapon parts for Tarkov-style modular system
    /// Each part type has specific effects on weapon performance
    /// </summary>
    public enum WeaponPartType : byte
    {
        // Core Parts (required for weapon to function)
        Barrel = 0,
        Receiver = 1,
        Bolt = 2,
        FiringPin = 3,
        Trigger = 4,

        // Magazine Parts
        Magazine = 5,

        // Attachments (optional, improve performance)
        Grip = 6,
        Stock = 7,
        Scope = 8,
        Muzzle = 9,
        Rail = 10,
        Laser = 11,
        Flashlight = 12
    }

    /// <summary>
    /// Compatibility system - defines which parts can be attached to which weapons
    /// </summary>
    public enum PartMountType : byte
    {
        Integrated = 0,     // Built into weapon, cannot be removed
        Picatinny = 1,      // Standard rail system
        MLok = 2,           // M-LOK mounting
        KeyMod = 3,         // KeyMod mounting
        Proprietary = 4,    // Weapon-specific mount
        Universal = 5       // Fits any weapon of this type
    }
}
