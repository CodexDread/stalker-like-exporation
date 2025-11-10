# ZONE SURVIVAL - Game Design Document
## Version 2.0

---

## Executive Summary

**Zone Survival** is a hardcore first-person survival shooter set in an exclusion zone filled with radiation, anomalies, and mutants. The game combines atmospheric world design with A-Life 2.0 systems, deep survival mechanics inspired by GAMMA modification, and tactical gunplay from Escape from Tarkov, all wrapped in a distinctive PSX-inspired aesthetic.

**Core Pillars:**
- Immersive survival simulation with PSX-era visual style
- Dynamic living world (A-Life 2.0)
- Realistic weapon handling and customization
- Emergent storytelling through faction warfare
- Minimal HUD with physical/PDA-based information systems
- Open-world exploration without extraction requirements

---

## Art Direction

### PSX-Inspired Visual Style (Asset-Focused)
- **Texture Resolution**: 128x128 to 256x256 textures for authentic low-res aesthetic
- **Polygon Count**: Low-poly models (500-2000 triangles for characters, 100-500 for props)
- **Texture Style**: Limited color palettes, no PBR, flat shading available
- **Animation**: Lower frame count animations for authentic feel

### Render Settings (Player Adjustable)
- **Render Distance**: 
  - Low: 50m (heavy fog, best performance)
  - Medium: 150m (moderate fog)
  - High: 300m (light fog)
  - Ultra: 500m+ (minimal fog, demanding)
- **Fog Density**: Separate slider from render distance
- **LOD Distance**: Adjustable model detail transitions

### Optional PSX Effects (Toggleable)
Players can enable "Retro Mode" with these optional effects:
- **Vertex Snapping**: Subtle geometry wobble (off by default)
- **Affine Texture Mapping**: PS1-style texture warping (off by default)
- **Dithering**: Transparency effect (minimal setting available)
- **CRT Filter**: Simple scanlines only, no distortion (off by default)
- **Frame Limiter**: 30 FPS option for authentic feel (60+ FPS default)

### Core Visual Features (Always On)
- **Vertex Lighting**: Simple, performance-friendly lighting model
- **Sharp Shadows**: Hard-edged shadows for clarity
- **Particle Effects**: Sprite-based for explosions and effects
- **Color Depth**: Full color by default (15-bit mode optional)

### Visual Clarity Priority
- Clean, readable environments despite low-poly aesthetic
- Clear enemy silhouettes 
- Distinct faction colors and designs
- No visual effects that impair gameplay
- UI elements always crisp and readable

---

## Core Gameplay Loop

### Primary Loop (Open World)
1. **Prepare** - Check equipment, repair gear, plan route
2. **Explore** - Navigate the open zone, discover new areas
3. **Survive** - Manage resources, avoid/engage threats
4. **Establish** - Set up camps, claim territories
5. **Progress** - Complete quests, upgrade gear, build reputation

### Meta Loop (Multiple Sessions)
1. Build reputation with factions
2. Establish and upgrade permanent bases
3. Complete storyline quests
4. Master zone knowledge and artifact combinations
5. Influence faction warfare outcomes
6. Unlock new areas through progression

---

## Combat Mechanics

### Dodge System
- **Activation**: Jump while strafing (A or D keys)
- **Effect**: Momentum burst in strafe direction (2.5m quick displacement)
- **Stamina Cost**: 15 stamina points per dodge
- **Cooldown**: 1.5 seconds between dodges
- **Benefits**: 
  - Brief i-frames (0.2 seconds)
  - Breaks mutant target lock
  - Can dodge through small anomalies
  - Maintains weapon ready state
- **Limitations**:
  - Cannot dodge while overencumbered (>45kg)
  - Reduced distance when injured
  - No dodging while prone
  - Momentum carries into landing (can stumble if poorly timed)

---

## World Design

### The Zone
- **Size**: 100 km² open world divided into 16 interconnected regions
- **Setting**: Post-disaster Eastern European exclusion zone
- **Time Period**: Near-future (2030s)
- **Day/Night Cycle**: Real-time 24-hour cycle with weather system

### Environmental Hazards

#### Radiation
- **Zones**: Background (0.1-10 mSv/h), Hot Spots (10-100 mSv/h), Critical (100+ mSv/h)
- **Protection**: Suits, artifacts, medication (RadAway, Potassium Iodide)
- **Effects**: Gradual health degradation, mutation chance, hallucinations at extreme levels

#### Anomalies
**Types:**
- **Gravitational**: Vortex, Springboard, Whirligig
- **Thermal**: Burner, Comet, Scorcher
- **Chemical**: Fruit Punch, Gas Cloud, Acid Rain
- **Electrical**: Electro, Tesla, Storm
- **Psi**: Brain Scorcher, Controller Field, Phantom zones

**Detection Methods:**
- Bolt throwing system
- Detector devices (Echo, Bear, Veles)
- Visual/audio cues
- Mutant behavior patterns

### Emissions (Blowouts)
- **Frequency**: Every 24-72 in-game hours (randomized)
- **Warning Signs**: 
  - Animals fleeing (2 hours before)
  - Sky color changes (1 hour before)
  - PDA emergency broadcast (30 minutes before)
- **Safe Zones**: Underground areas, reinforced buildings, psi-shelters
- **Effects**: Instant death if exposed, anomaly redistribution, new artifact spawns

---

## A-Life 2.0 System

### Core Components

#### Population Management
- **NPCs**: 500-1000 active NPCs simulated simultaneously
- **Offline Simulation**: Simplified calculations for distant NPCs
- **Online Simulation**: Full AI for NPCs within 500m of player

#### NPC Behaviors
**Daily Routines:**
- Sleep cycles (6-8 hours)
- Eating schedules (3 times daily)
- Work activities (scavenging, patrolling, trading)
- Social interactions (campfire stories, trading, fights)

**Goal System:**
- Short-term: Find food, seek shelter, avoid danger
- Medium-term: Complete missions, acquire gear, build reputation
- Long-term: Faction objectives, territory control, wealth accumulation

#### Dynamic Events
- Faction raids and territory battles
- Mutant migrations and hunting packs
- Artifact hunts with multiple stalker groups
- Emission-triggered evacuations
- Random encounters and emergent stories

---

## Factions

### Major Factions

#### 1. The Prospectors Union
- **Philosophy**: Organized scavengers seeking fortune through cooperation
- **Relations**: Neutral traders, defend against raiders
- **Benefits**: Shared stash network, market information, group expeditions
- **Base**: Trade Station, Central Market

#### 2. Iron Guard
- **Philosophy**: Ex-military peacekeepers maintaining order
- **Relations**: Hostile to criminals, protective of civilians
- **Benefits**: Military surplus, vehicle access, fortified outposts
- **Base**: Firebase Alpha, Checkpoint Networks

#### 3. The Technicians
- **Philosophy**: Engineers and mechanics maintaining zone infrastructure
- **Relations**: Trade repairs for supplies, neutral to conflicts
- **Benefits**: Advanced weapon modifications, vehicle repairs, generator access
- **Base**: Power Plant, Radio Relay Stations

#### 4. Nomad Collective
- **Philosophy**: Mobile survivors adapted to zone life
- **Relations**: Trade with all, avoid conflicts
- **Benefits**: Guide services, hidden paths, mobile camps
- **Base**: Caravan routes, Seasonal camps

#### 5. The Wardens
- **Philosophy**: Environmental researchers documenting the zone
- **Relations**: Neutral observers, document everything
- **Benefits**: Zone maps, anomaly predictions, weather forecasts
- **Base**: Observatory, Field Stations

#### 6. Black Tide Company
- **Philosophy**: Corporate extraction team, profit-focused
- **Relations**: Business only, eliminate competitors
- **Benefits**: High-grade military gear, helicopter supply drops, contracts
- **Base**: Corporate FOB, Helipad Sites

#### 7. Scrap Hounds
- **Philosophy**: Anarchist raiders and thieves
- **Relations**: Hostile to organized factions, raid suppliers
- **Benefits**: Black market, ambush intel, stolen goods
- **Base**: Junkyard Fortress, Hidden Dens

#### 8. The Covenant of Ash
- **Philosophy**: Death cult believing the zone is cleansing humanity
- **Relations**: Hostile to all "impure" outsiders
- **Benefits**: Fanatic devotion (no morale loss), ritual artifacts, emission prediction
- **Base**: The Burned Cathedral, Ash Fields

#### 9. The Shroud (Secret Faction - Rare Encounters)
- **Philosophy**: Cannibalistic ex-special forces who've gone completely feral in the deep zone
- **Relations**: Kill everything that moves, take trophies, leave warnings
- **Benefits**: NONE - Cannot trade, cannot ally, cannot reason with
- **Encounter Rate**: 1-2% chance in deep zone, increase during fog/night
- **Behavior**: 
  - Hunt in coordinated 4-6 person kill teams
  - Use military tactics with savage brutality
  - Set traps and psychological warfare (hanging bodies, recordings of victims)
  - Take ears/fingers as trophies
  - Leave cryptic messages written in blood
  - Wear makeshift armor from victim's gear
  - Never speak, only use hand signals and clicks
- **Warning Signs**:
  - Sudden animal/mutant silence
  - Finding mutilated bodies arranged in patterns
  - Hearing distant clicking sounds
  - Radio static increases dramatically
  - Other NPCs flee without explanation
- **Combat Characteristics**:
  - Elite-level AI (headshots, flanking, suppression)
  - Modified weapons with illegal ammo (hollow points, incendiary)
  - Ignore morale/suppression effects
  - Will pursue across multiple zones
  - Plant mines and booby traps during chase
  - One always tries to go for melee kills
- **Loot**: 
  - High-tier equipment (badly maintained)
  - Human meat (causes severe morale loss if identified)
  - Trophy collections (can be traded for massive rep with other factions)
  - Cryptic maps to their weapon caches
- **Zone Legend Status**:
  - NPCs refuse to talk about them directly
  - Referenced only as "The Shroud" or "The Silent Ones"
  - Survivors become permanently paranoid (won't travel alone)
  - Even hardened mercenaries avoid their territory
  - Finding their mark (bloody handprint) causes NPCs to evacuate

### Special Shroud Mechanics
- **First Encounter**: Always scripted for maximum horror (find aftermath first)
- **Stalking Phase**: They follow player for 10-30 minutes before attacking
- **No Negotiation**: Cannot be reasoned with, no dialogue options
- **Permadeath Risk**: Being downed by Shroud has 50% chance of instant execution
- **Bounty System**: Other factions pay extreme amounts for proof of Shroud kills

### Reputation System
- **Range**: -1000 to +1000 per faction
- **The Shroud Exception**: No reputation tracking - permanent hostile
- **Effects**: 
  - Trade prices (±50% based on reputation)
  - Mission availability
  - Safe passage through territories
  - Faction-specific equipment access
- **Reputation Changes**:
  - Completing missions (+10 to +100)
  - Killing faction members (-50 to -200)
  - Helping in firefights (+20 to +50)
  - Trading (+1 to +5 per transaction)
  - Killing a Shroud member (+100 with ALL other factions)

---

## Character System

### Attributes
- **Health**: 100 HP (divided into body parts)
- **Stamina**: Affects sprint duration, carrying capacity
- **Hunger**: 0-100 (death at 0)
- **Thirst**: 0-100 (death at 0)
- **Sleep**: 0-100 (penalties below 30)
- **Radiation**: 0-1000 mSv (effects start at 100)
- **Psi-Health**: Mental resistance to anomalies

### Skills (Passive Progression)

#### Physical Skills
- **Endurance** (0-100)
  - Level 0-25: Base stamina
  - Level 26-50: +25% stamina, +5kg carry weight
  - Level 51-75: +50% stamina, +10kg carry weight, reduced sprint drain
  - Level 76-100: +75% stamina, +15kg carry weight, sprint while injured
  - Progression: Running (1xp/km), Carrying heavy loads (1xp/10min over 30kg)

- **Strength** (0-100)
  - Level 0-25: Base melee damage
  - Level 26-50: +20% melee damage, +5kg carry capacity
  - Level 51-75: +40% melee damage, +10kg carry, faster heavy weapon handling
  - Level 76-100: +60% melee damage, +15kg carry, throw items further
  - Progression: Melee combat (5xp/kill), carrying maximum weight (1xp/5min)

- **Agility** (0-100)
  - Level 0-25: Base movement
  - Level 26-50: +10% movement speed, faster dodging
  - Level 51-75: +15% movement speed, reduced fall damage, wall climbing
  - Level 76-100: +20% movement speed, silent running, parkour moves
  - Progression: Dodging (2xp per successful dodge), Sprint-jumping (1xp per 10 jumps)

#### Combat Skills
- **Marksmanship** (0-100)
  - Level 0-25: Base accuracy
  - Level 26-50: -20% recoil, +10% ADS speed
  - Level 51-75: -40% recoil, +20% ADS speed, reduced sway
  - Level 76-100: -60% recoil, instant ADS, perfect breath control
  - Progression: Headshots (10xp), center mass hits (2xp), long range kills (5xp)

- **Weapon Handling** (0-100)
  - Level 0-25: Base reload speed
  - Level 26-50: +20% reload speed, quick magazine check
  - Level 51-75: +40% reload speed, tactical reload, jam clearing
  - Level 76-100: +60% reload speed, dual wielding pistols, blind reload
  - Progression: Reloads in combat (3xp), weapon swaps (1xp), clearing jams (5xp)

- **Explosives** (0-100)
  - Level 0-25: Basic throwing
  - Level 26-50: +30% throw distance, trajectory preview
  - Level 51-75: +50% throw distance, cook grenades, mine detection
  - Level 76-100: +70% throw distance, craft IEDs, disarm explosives
  - Progression: Grenade kills (10xp), mine placement (5xp), disarming (15xp)

#### Survival Skills
- **Medicine** (0-100)
  - Level 0-25: Basic healing
  - Level 26-50: +25% healing efficiency, diagnose injuries
  - Level 51-75: +50% healing, craft medicine, surgery success +30%
  - Level 76-100: +75% healing, immunity crafting, resurrect recently dead
  - Progression: Healing self (2xp), healing others (5xp), successful surgery (10xp)

- **Metabolism** (0-100)
  - Level 0-25: Normal digestion
  - Level 26-50: -20% hunger/thirst rate, resist food poisoning
  - Level 51-75: -40% hunger/thirst, eat raw meat safely, alcohol resistance
  - Level 76-100: -60% hunger/thirst, nutrition from anything, toxin immunity
  - Progression: Eating varied foods (1xp), surviving starvation (5xp/day)

- **Resistance** (0-100)
  - Level 0-25: Base resistances
  - Level 26-50: +20% radiation/psi resistance, faster recovery
  - Level 51-75: +40% all resistances, immunity to weak anomalies
  - Level 76-100: +60% all resistances, walk through radiation, psi-shield
  - Progression: Surviving anomalies (3xp), radiation exposure (1xp/hour), psi attacks (5xp)

#### Technical Skills
- **Engineering** (0-100)
  - Level 0-25: Basic repairs
  - Level 26-50: +30% repair quality, craft basic mods
  - Level 51-75: +60% repair quality, advanced mods, overclock devices
  - Level 76-100: +90% repair quality, experimental mods, reverse engineering
  - Progression: Successful repairs (3xp), crafting (5xp), modding (7xp)

- **Electronics** (0-100)
  - Level 0-25: Use basic devices
  - Level 26-50: Hack terminals, boost detector range +25%
  - Level 51-75: Craft devices, detector range +50%, drone control
  - Level 76-100: Master hacking, detector +100%, EMP immunity
  - Progression: Hacking (10xp), device crafting (15xp), detector use (1xp/artifact)

- **Chemistry** (0-100)
  - Level 0-25: Basic crafting
  - Level 26-50: Craft stimulants, identify chemicals, +25% yield
  - Level 51-75: Advanced drugs, poison crafting, +50% yield
  - Level 76-100: Experimental compounds, artifact synthesis, +100% yield
  - Progression: Crafting chems (5xp), identifying substances (3xp)

#### Zone Skills
- **Anomaly Expert** (0-100)
  - Level 0-25: See anomaly edges
  - Level 26-50: +30% artifact spawn chance, safe paths visible
  - Level 51-75: +60% artifact chance, manipulate weak anomalies
  - Level 76-100: +100% artifact chance, create temporary anomalies
  - Progression: Navigate anomalies (2xp), collect artifacts (10xp)

- **Mutant Hunter** (0-100)
  - Level 0-25: Basic knowledge
  - Level 26-50: See weak points, +20% damage to mutants
  - Level 51-75: Predict behavior, +40% damage, taming chance
  - Level 76-100: Control weak mutants, +60% damage, mount large mutants
  - Progression: Mutant kills (5xp), rare kills (20xp), successful taming (50xp)

- **Zone Attunement** (0-100)
  - Level 0-25: Feel emissions coming
  - Level 26-50: Sense nearby artifacts, weather prediction
  - Level 51-75: Communicate with zone, find hidden stashes
  - Level 76-100: Emission immunity, teleport between anomalies
  - Progression: Time in zone (1xp/day), surviving emissions (20xp)

#### Social Skills
- **Leadership** (0-100)
  - Level 0-25: Basic commands
  - Level 26-50: Recruit NPCs, +2 squad size
  - Level 51-75: Inspire morale, +4 squad size, faction influence
  - Level 76-100: Command outposts, +6 squad, start faction wars
  - Progression: Leading squads (5xp/mission), winning battles (10xp)

- **Trading** (0-100)
  - Level 0-25: See base prices
  - Level 26-50: -10% buy prices, +10% sell prices
  - Level 51-75: -20% buy, +20% sell, exclusive items
  - Level 76-100: -30% buy, +30% sell, establish trade routes
  - Progression: Successful trades (1xp), big deals (10xp), new contacts (5xp)

- **Stealth** (0-100)
  - Level 0-25: Basic sneaking
  - Level 26-50: -30% detection radius, silent takedowns
  - Level 51-75: -60% detection, invisibility in shadows, disguises
  - Level 76-100: -90% detection, temporary cloaking, perfect mimicry
  - Progression: Stealth kills (10xp), undetected infiltration (20xp)

---

## Health & Medical System (Tarkov-Inspired)

### Body Part System
**Parts**: Head, Thorax, Stomach, Left Arm, Right Arm, Left Leg, Right Leg

**Damage Types:**
- **Ballistic**: Bullets, shrapnel
- **Blunt**: Melee, falling
- **Bleeding**: Requires bandaging
- **Fractures**: Requires splints/surgery
- **Radiation**: Whole-body damage
- **Psi**: Head/mental damage

### Medical Items

#### Immediate Care
- **Bandages**: Stop light bleeding (5 seconds)
- **Tourniquets**: Stop heavy bleeding (3 seconds)
- **Hemostatic**: Stop all bleeding (7 seconds)
- **Morphine**: Temporary pain relief (instant)
- **Adrenaline**: Stamina boost, temporary HP (instant)

#### Treatment
- **Medkits**: Restore HP to body parts
  - AI-2: 50 HP total (civilian)
  - IFAK: 100 HP total (military)
  - Grizzly: 200 HP total (advanced)
- **Surgical Kits**: Fix fractures, remove bullets
- **Splints**: Temporary fracture fix

#### Long-term
- **Antibiotics**: Prevent infection
- **Painkillers**: Extended pain management
- **RadAway**: Radiation reduction
- **Psi-blockers**: Mental protection
- **Vitamins**: Boost natural healing

---

## Weapon System

### Weapon Categories

#### Pistols
- Makarov PM, Glock 17, Desert Eagle, FN Five-Seven
- **Pros**: Lightweight, quick draw
- **Cons**: Low damage, limited range

#### SMGs
- MP5, PP-19 Bizon, P90, Vector
- **Pros**: High fire rate, compact
- **Cons**: Limited range, low penetration

#### Assault Rifles
- AK-74M, M4A1, SCAR-L, AN-94
- **Pros**: Versatile, modular
- **Cons**: Weight, maintenance requirements

#### Sniper Rifles
- Mosin Nagant, SVD, Barrett M82
- **Pros**: Long range, high damage
- **Cons**: Weight, slow fire rate

#### Shotguns
- TOZ-34, SPAS-12, Saiga-12
- **Pros**: Devastating close range
- **Cons**: Limited range, heavy ammo

### Weapon Customization (Tarkov-Style)

#### Attachment Points
- **Barrel**: Muzzle devices, suppressors
- **Optics**: Iron sights to 8x scopes
- **Handguard**: Grips, lasers, flashlights
- **Stock**: Fixed, folding, adjustable
- **Magazine**: Standard, extended, drum
- **Internal**: Trigger group, gas system

#### Weapon Condition
- **Durability**: 0-100% (jams increase below 70%)
- **Degradation Factors**:
  - Environmental (rain, mud, radiation)
  - Usage (rounds fired)
  - Ammo quality
- **Maintenance**:
  - Cleaning kits restore 10-20%
  - Repair kits restore 30-50%
  - Gunsmith service: Full restoration

### Ballistics
- **Bullet Physics**: Gravity drop, travel time
- **Penetration System**: Material-based (wood, metal, armor)
- **Ricochet Chance**: Based on angle and surface
- **Fragmentation**: Chance for additional damage

---

## Survival Mechanics

### Hunger System
- **Depletion Rate**: -1 per 30 minutes (base rate)
- **Increased by**: Running, fighting, cold weather
- **Effects**:
  - 75-50%: Slower stamina regeneration
  - 50-25%: Reduced carry weight
  - 25-0%: Health degradation
- **Food Types**:
  - Canned goods (+20-30)
  - Cooked meals (+40-60)
  - MREs (+50, includes hydration)
  - Mutant meat (+10-40, radiation risk)

### Thirst System
- **Depletion Rate**: -1 per 20 minutes
- **Increased by**: Running, bleeding, radiation
- **Effects**:
  - 75-50%: Reduced stamina cap
  - 50-25%: Blurred vision
  - 25-0%: Rapid health loss
- **Drinks**:
  - Water (clean: +40, dirty: +30 with infection risk)
  - Energy drinks (+35, stamina boost)
  - Alcohol (+20, radiation -10, accuracy penalty)

### Sleep System
- **Depletion Rate**: -1 per 40 minutes awake
- **Rest Options**:
  - Sleeping bag: 6 hours minimum, vulnerable
  - Bed: 4 hours minimum, safe zones only
  - Stimulants: Temporary delay (-30 to sleep need)
- **Effects**:
  - 75-50%: Slower reactions
  - 50-25%: Hallucinations
  - 25-0%: Random blackouts

### Temperature
- **Factors**: Weather, clothing, fire proximity
- **Effects**:
  - Hypothermia: Stamina drain, shaking
  - Hyperthermia: Increased thirst, reduced carrying capacity

---

## Artifact System

### Artifact Categories

#### Protective
- **Jellyfish**: +2 radiation protection
- **Stone Flower**: +3 chemical protection
- **Pellicle**: +4 physical protection
- **Bubble**: +6 impact protection

#### Healing
- **Stone Blood**: +1 HP/minute
- **Soul**: +2 HP/minute
- **Crystal**: Limb regeneration

#### Enhancement
- **Flash**: +15% movement speed
- **Moonlight**: +20% night vision
- **Compass**: Anomaly detection +50m
- **Goldfish**: +30kg carry capacity

#### Offensive
- **Thorn**: Damage reflection
- **Flame**: Thermal damage aura
- **Battery**: Electrical discharge

### Artifact Crafting System

#### Equipment Tiers

##### Artifact Melter Tiers
- **Tier 1 (Basic Melter)**
  - Success rate: 40%
  - Max 3 artifacts combined
  - Power: Manual generator
  - Cost: 15,000 RU
  
- **Tier 2 (Advanced Melter)**
  - Success rate: 60%
  - Max 5 artifacts combined
  - Power: Electric generator
  - Special: Recipe preview
  - Cost: 50,000 RU
  
- **Tier 3 (Experimental Melter)**
  - Success rate: 80%
  - Max 7 artifacts combined
  - Power: Anomaly core
  - Special: Custom recipes, mutation chance
  - Cost: 150,000 RU

##### Artifact Containers Tiers
- **Tier 1 (Lead Box)**: 2 slots, -50% radiation
- **Tier 2 (SEVA Container)**: 4 slots, -75% radiation
- **Tier 3 (Anomalous Case)**: 6 slots, -100% radiation, preserves charge

#### Complete Artifact Recipes

##### Protective Combinations
- **Radiation Shield**: 3x Jellyfish + Lead Plate = +10 radiation protection
- **Chemical Barrier**: 3x Stone Flower + Copper Sulfate = +10 chemical protection
- **Kevlar Skin**: 3x Pellicle + Carbon Fiber = +15 physical protection
- **Shock Absorber**: 3x Bubble + Rubber Compound = +20 impact protection
- **Psi Screen**: 2x Moonlight + 1x Controller Brain = +25 psi protection
- **Elemental Ward**: 1 of each protective type + Mercury = +5 all protections
- **Zone Aegis**: 5x mixed protective + Anomaly Core = +30 all protections

##### Healing Combinations
- **Blood Stone**: 3x Stone Blood + Medical Alcohol = +3 HP/minute
- **Soul Crystal**: 3x Soul + Morphine = +5 HP/minute + pain immunity
- **Regenerator**: 3x Crystal + Stem Cells = Limb regrowth over time
- **Life Spring**: 2x Soul + 2x Crystal + Holy Water = Full heal every emission
- **Phoenix Heart**: 5x healing artifacts + Pseudogiant Heart = Resurrect once on death

##### Enhancement Combinations
- **Speed Demon**: 3x Flash + Adrenaline = +30% movement speed
- **Night Stalker**: 3x Moonlight + Cat Eyes = Perfect night vision
- **Zone Compass**: 3x Compass + Detector Parts = +100m all detection
- **Hercules**: 3x Goldfish + Testosterone = +50kg carry capacity
- **Predator**: 2x Flash + 2x Moonlight + Bloodsucker Tentacle = Invisibility when still
- **Quantum Leap**: 3x Flash + Spatial Anomaly Sample = Short-range teleportation
- **Time Dilation**: 4x varied enhancement + Chronos Fragment = Slow time for 3 seconds

##### Offensive Combinations
- **Thorn Field**: 3x Thorn + Iron Filings = 360° damage reflection
- **Inferno**: 3x Flame + Napalm = Fire aura + immunity to fire
- **Tesla Coil**: 3x Battery + Copper Wire = Chain lightning on hit
- **Toxic Avenger**: 2x Chemical artifacts + Poison Gland = Poison cloud on kill
- **Psi Storm**: 3x Psi artifacts + Controller Brain = AoE psi damage
- **Gravity Well**: 3x Gravitational + Lead Core = Pull enemies toward you
- **Antimatter**: 5x offensive + Unstable Isotope = Explosion on death

##### Utility Combinations
- **Emission Predictor**: 2x Compass + Weather Station Parts = 2-hour emission warning
- **Artifact Magnet**: 3x any artifacts + Electromagnet = Auto-collect nearby artifacts
- **Zone Whisper**: 2x Psi + Radio Parts = Understand mutant communication
- **Stalker's Luck**: 4x random + Four-leaf Clover = +50% rare loot chance
- **Dimensional Pocket**: 3x Spatial + Void Sample = +20kg weightless storage
- **Second Wind**: 3x Stamina artifacts + Epinephrine = Instant stamina refill
- **Echo Location**: 2x Compass + Bat Ear = See through walls (10m)

##### Experimental/Unstable
- **Wild Card**: 7x random artifacts = Completely random powerful effect
- **Paradox**: Mix opposing types (Fire+Ice, etc.) = Unpredictable results
- **Void Touch**: 5x Spatial + Black Hole Fragment = Delete matter on touch
- **Zone's Child**: 1 of every artifact type = Become partially anomalous
- **Prometheus**: 7x Flame + Nuclear Core = Mini-emission on command

---

## Base Building System

### Building Types

#### Personal Hideouts
- **Size**: Single room to small building
- **Features**: Storage, sleeping, basic crafting
- **Defense**: Locks, traps, reinforced doors
- **Cost**: 10,000-50,000 RU

#### Faction Outposts
- **Size**: Multi-room buildings
- **Features**: Traders, medical bay, armory
- **Defense**: Guards, watchtowers, walls
- **Cost**: 100,000-500,000 RU

### Furniture & Installations

#### Storage Tiers
**Tier 1:**
- **Wooden Crate**: 50kg capacity, 100 HP
- **Backpack Hanger**: 4 backpack storage
- **Ammo Box**: 30kg, ammo only

**Tier 2:**
- **Metal Locker**: 100kg capacity, lockable, 500 HP
- **Weapon Rack**: 8 weapon display/storage
- **Medical Cabinet**: 40kg, medicine only, preserves items

**Tier 3:**
- **Vault**: 200kg, biometric lock, 2000 HP
- **Armory Wall**: 16 weapons, 8 armor sets
- **Anomalous Stash**: 150kg, dimensional storage

#### Detector Tiers
**Tier 1:**
- **Echo Detector**: 20m range, beeps only
- **Response time**: 2 seconds
- **Cost**: 5,000 RU

**Tier 2:**
- **Bear Detector**: 40m range, directional display
- **Response time**: 1 second
- **Shows artifact type**
- **Cost**: 15,000 RU

**Tier 3:**
- **Veles Detector**: 75m range, 3D display
- **Response time**: Instant
- **Shows exact location and strength**
- **Cost**: 50,000 RU

### Crafting Station Recipes

#### Workbench Crafting (Weapon/Armor Repair & Mods)

##### Tier 1 Workbench
**Weapon Mods:**
- **Improvised Suppressor**: 2x Steel Pipe + Cloth + Wire = Basic suppressor
- **Extended Magazine**: Original Mag + Spring + Steel Plate = +10 capacity
- **Makeshift Scope**: 2x Bottle Glass + Pipe + Tape = 2x magnification
- **Reinforced Stock**: Wood + Steel Bracket + Screws = -10% recoil
- **Barrel Shroud**: Sheet Metal + Screws = -15% heat buildup

**Armor Mods:**
- **Steel Plates**: 4x Steel Plate + Straps = +5 ballistic protection
- **Lead Lining**: Lead Sheets + Sewing Kit = +3 radiation protection
- **Padding**: Foam + Cloth + Thread = +3 impact protection
- **Rubber Coating**: Rubber + Adhesive = +2 electrical protection

##### Tier 2 Workbench  
**Weapon Mods:**
- **Professional Suppressor**: Titanium Tube + Baffles + Thread = -70% noise
- **Drum Magazine**: 3x Magazines + Spring Kit + Welder = +50 capacity
- **Red Dot Sight**: LED + Battery + Glass + Mount = Fast acquisition
- **Muzzle Brake**: Steel Block + Precision Tools = -25% recoil
- **Rail System**: Aluminum Rail + Mounting Hardware = 3 attachment points
- **Match Trigger**: Trigger Assembly + Springs + Polish = +15% accuracy

**Armor Mods:**
- **Ceramic Plates**: Ceramic Sheets + Kevlar + Straps = +10 ballistic protection
- **Hazmat Layer**: Chemical Suit + Integration Kit = +8 chemical protection
- **Exo-Frame**: Servo Motors + Frame + Battery = +20kg carry capacity
- **Climate Control**: Cooling Unit + Heating Coil + Thermostat = Temperature immunity

##### Tier 3 Workbench
**Weapon Mods:**
- **Smart Scope**: Digital Scope + Rangefinder + CPU = Auto-ranging + wind calculation
- **Electromagnetic Accelerator**: EM Coils + Capacitor = +30% bullet velocity
- **Adaptive Suppressor**: Smart Materials + Sensors = -90% noise, no penalty
- **Neural Link**: Brain Interface + Weapon CPU = Thought-controlled firing
- **Anomalous Infusion**: Artifact Fragment + Weapon = Random magical property

**Armor Mods:**
- **Nanoweave**: Nanofibers + Programming Unit = Self-repairing armor
- **Active Camo**: Optical Fibers + CPU + Battery = 30 second invisibility
- **Power Armor Core**: Fusion Battery + Servo System = +50kg carry, +speed
- **Psi Amplifier**: Controller Brain + Helmet = Psi abilities unlocked

#### Chemistry Set Recipes (Medical & Chemical Crafting)

##### Tier 1 Chemistry
**Medical:**
- **Bandage**: Cloth + Alcohol = Stops bleeding
- **Antiseptic**: Alcohol + Iodine = Prevents infection
- **Painkiller**: Aspirin + Codeine = -pain for 5 minutes
- **Antirad**: Iodine + Potassium = -50 radiation
- **Stimulant**: Caffeine + Sugar = +stamina for 10 minutes
- **Antidote**: Charcoal + Herbs = Cures poison

**Chemicals:**
- **Molotov**: Bottle + Gasoline + Cloth = Fire grenade
- **Smoke Bomb**: Sugar + Potassium Nitrate = 30 second smoke
- **Flash Powder**: Magnesium + Oxidizer = Blinding effect
- **Acid**: Battery Acid + Concentrate = Armor damage

##### Tier 2 Chemistry
**Medical:**
- **Morphine**: Opium + Process Kit = Strong painkiller
- **Adrenaline**: Epinephrine + Enhancer = Instant stamina + speed
- **Blood Bag**: Blood + Preservative + IV Kit = +50 HP over time
- **RadAway**: Complex Chelators + IV Solution = -200 radiation
- **Antibiotics**: Penicillin + Broad Spectrum = Cures infections
- **Psi-Block**: Neurochemicals + Stabilizer = 1 hour psi immunity
- **Stimpak**: Stem Cells + Accelerant + Injector = Instant 75 HP

**Chemicals:**
- **Thermite**: Aluminum + Iron Oxide = Burns through anything
- **Nerve Gas**: Classified Chemicals = Area denial weapon
- **Corrosive Cloud**: Acid + Dispersant = DOT area effect
- **Cryo Grenade**: Liquid Nitrogen + Detonator = Freeze enemies

##### Tier 3 Chemistry
**Medical:**
- **Phoenix Serum**: Rare Compounds + Mutant DNA = Revive from death once
- **Evolution Virus**: Modified FEV + Stabilizer = Permanent stat boost
- **Neural Enhancer**: Nootropics + Nanobots = +20 all skills permanently
- **Immortality Dose**: Philosopher's Stone + Elixir = Stop aging for 1 year
- **Panacea**: Ultimate Compound = Cure everything, full restore

**Chemicals:**
- **Antimatter Bomb**: Particle Accelerator + Containment = Massive explosion
- **Psi Emitter**: Controller Extract + Amplifier = Mind control gas
- **Temporal Grenade**: Chronos Particles + Device = Time stop field
- **Singularity**: Compressed Matter + Activator = Mini black hole

#### Cooking Station Recipes

##### Tier 1 Cooking
- **Grilled Meat**: Any Meat + Fire = +30 hunger, +5 HP
- **Boiled Water**: Dirty Water + Fire = Clean water
- **Vegetable Stew**: 3x Vegetables + Water + Salt = +40 hunger, +10 HP
- **Dried Meat**: Raw Meat + Salt + Time = Preserved food
- **Mushroom Soup**: Mushrooms + Water + Herbs = +35 hunger, +radiation resistance
- **Energy Bar**: Grains + Sugar + Nuts = +25 hunger, +stamina

##### Tier 2 Cooking
- **Hunter's Feast**: Prime Meat + Vegetables + Spices = +60 hunger, +20 HP, +morale
- **Zone Goulash**: Mutant Meat + Special Process = +50 hunger, mutation resistance
- **Stalker's Brew**: Coffee + Energy Drink + Herbs = No sleep need for 12 hours
- **Immunity Soup**: Rare Herbs + Antibodies = Disease immunity 24 hours
- **Ration Pack**: Multiple Preserved Foods = Complete meal, long shelf life
- **Artifact Tea**: Artifact Dust + Hot Water = Random beneficial effect

##### Tier 3 Cooking
- **Ambrosia**: Divine Ingredients = +100 all needs, +50 HP, blessed status
- **Dragon Steak**: Chimera Meat + Master Preparation = Permanent +5 strength
- **Philosopher's Meal**: Alchemical Foods = Permanent +5 intelligence
- **Zone's Bounty**: Every Food Type + Artifact = Reset all stats to max

#### Electronics Bay Recipes

##### Tier 1 Electronics
- **Radio**: Circuit Board + Speaker + Antenna = Communication device
- **Flashlight Upgrade**: LED + Battery Pack = Extended duration
- **Motion Sensor**: Sensor + Circuit + Alarm = Intruder detection
- **GPS Tracker**: GPS Module + Battery = Location tracking
- **Night Vision (Basic)**: IR LEDs + Camera + Display = Gen 1 NVG
- **Bug Detector**: RF Scanner + Display = Find listening devices

##### Tier 2 Electronics
- **Drone**: Motors + Camera + Controller = Reconnaissance drone
- **EMP Grenade**: Capacitor Bank + Coil + Trigger = Disables electronics
- **Advanced PDA**: CPU Upgrade + Memory + Software = Enhanced PDA functions
- **Turret Controller**: AI Module + Targeting System = Automated defense
- **Artifact Scanner**: Specialized Sensors + Computer = Artifact analysis
- **Comm Jammer**: Transmitter + Frequency Gen = Block communications
- **Thermal Vision**: Thermal Sensor + Display + Mount = Heat detection

##### Tier 3 Electronics
- **AI Companion**: Advanced AI + Robot Body = Permanent companion
- **Teleporter Beacon**: Quantum Entangler + Power Source = Fast travel point
- **Force Field Generator**: Energy Projector + Shield Matrix = Protective barrier
- **Mind Reader**: Neural Interface + Decoder = Read NPC thoughts
- **Time Machine (Small)**: Temporal Circuit + Power Core = Rewind 1 minute
- **Dimensional Portal**: Reality Anchor + Rift Generator = Access parallel zones

### Base Management
- **Power**: 
  - Tier 1: Manual generator (fuel: 1L = 12 hours)
  - Tier 2: Solar + Battery (daylight charging)
  - Tier 3: Anomaly harvester (infinite power)
- **Maintenance**: Degradation over time, requires materials
- **NPCs**: Hire guards, traders, mechanics
- **Raids**: Periodic attacks based on wealth/faction relations

---

## PDA System

The PDA is a physical device that must be pulled out and viewed (not a HUD overlay):

### Physical Device Properties
- **Animation**: Takes 1 second to pull out and put away
- **Vulnerability**: Can be used while moving but not while shooting
- **Damage**: Can be broken by EMP or physical damage (requires repair)
- **Upgrades**: Better models have faster processors, more features
- **Battery**: Requires charging (solar or generator)

### PDA Functions

#### Map
- **Exploration**: Fog of war system
- **Markers**: Custom waypoints, discovered locations
- **Intel**: Faction territories, anomaly fields
- **Real-time**: Shows detected NPCs within range

#### Journal
- **Main Quests**: Story progression
- **Side Quests**: Faction/NPC missions
- **Completed**: Archive with rewards
- **Notes**: Collectible lore documents

#### Contacts
- **Stalkers**: Met NPCs with reputation status
- **Traders**: Inventory refresh timers
- **Mission Givers**: Available jobs
- **Radio Frequency**: Faction communications

#### Encyclopedia
- **Artifacts**: Properties, locations, combinations
- **Mutants**: Behaviors, weaknesses, loot
- **Anomalies**: Types, detection, navigation
- **Weapons**: Stats, compatible mods
- **Factions**: Relations, territories, leaders

#### Statistics
- **Personal**: Kills, deaths, distance traveled
- **Economic**: Money earned/spent, items traded
- **Survival**: Days survived, emissions weathered
- **Achievements**: Unlocked milestones

### Communication
- **Radio Channels**: Faction chatter, emergency broadcasts
- **Direct Messages**: NPC communications
- **Emission Warnings**: Automated alerts
- **SOS Beacon**: Call for help (limited uses)

---

## Stash System

### Types

#### Personal Stashes
- **Backpack Cache**: 25kg, above ground
- **Buried Stash**: 50kg, underground
- **Blue Box**: 75kg, in buildings
- **Safe**: 100kg, requires combination

#### Mechanics
- **Placement**: Designated spots or custom locations
- **Security**: Can be discovered by NPCs if poorly hidden
- **Sharing**: Coordinate sharing with other players/NPCs
- **Decay**: Exposed stashes degrade over time

---

## Quest System

### Main Storyline

#### Act 1: Arrival (10 hours)
- Enter the Zone illegally
- Learn basic survival
- Discover the anomaly crisis
- Choose initial faction alignment

#### Act 2: Investigation (15 hours)
- Uncover conspiracy within Military
- Explore deep Zone laboratories
- Encounter Monolith forces
- Discover truth about Emissions

#### Act 3: Resolution (10 hours)
- Faction war escalation
- Assault on CNPP
- Multiple endings based on choices:
  - Destroy the Zone
  - Control the Zone
  - Merge with the Zone
  - Escape with secrets

### Side Quest Types

#### Fetch Quests
- Retrieve artifacts from anomaly fields
- Recover lost equipment
- Find specific mutant parts
- Locate missing stalkers

#### Elimination
- Clear mutant nests
- Assassinate faction leaders
- Defend locations from raids
- Hunt legendary mutants

#### Exploration
- Map uncharted territories
- Find secret laboratories
- Discover artifact spawns
- Document anomaly behaviors

#### Social
- Mediate faction disputes
- Escort VIPs through dangerous areas
- Deliver messages between settlements
- Build faction reputation

### Dynamic Quests
- **Generation**: Based on A-Life events
- **Time Limits**: 24-72 hours typical
- **Consequences**: Failed quests affect reputation
- **Rewards**: Scaled to difficulty and faction standing

---

## UI/UX Design

### Physical Item UI Philosophy
Instead of traditional HUD elements, players must use physical items that occupy hands and require active viewing:

#### Navigation & Orientation
- **Compass** (Physical Item):
  - Must be held in hand to check direction
  - Takes up weapon slot while viewing
  - Can be attached to wrist (still requires looking down)
  - Magnetic interference near anomalies
  
- **Wristwatch** (Physical Item):
  - Check time by looking at wrist animation
  - Shows day/night cycle position
  - Can have alarm function for emissions
  - Different tiers: Basic → Digital → Smart

- **Map** (Physical Item):
  - Pull out paper map that fills screen
  - Mark locations with pencil
  - Can be damaged by water/fire
  - Requires light source at night

#### Detection & Analysis
- **Detectors** (Physical Items):
  - Must be actively held and watched
  - Screen on device shows readings
  - Different models show different info
  - Cannot use weapons while detecting

- **Geiger Counter** (Physical Item):
  - Audible clicks for radiation
  - Must look at device for exact readings
  - Handheld or belt-mounted options

- **Dosimeter** (Physical Item):
  - Pen-style radiation tracker
  - Check accumulated dose by examining
  - Wrist-mounted version available

#### Health & Status
- **Medical Scanner** (Physical Item):
  - Handheld device showing body part status
  - Takes 3 seconds to scan self
  - Shows injuries, radiation, infections
  
- **Thermometer** (Physical Item):
  - Check body temperature
  - Environmental temperature reading
  - Mercury or digital versions

- **Blood Pressure Cuff** (Physical Item):
  - Check cardiovascular health
  - Indicates stamina capacity
  - Manual or automatic versions

#### Equipment Status
- **Weapon Inspection** (Animation):
  - Press and hold key to physically examine weapon
  - Check chamber, magazine count visually
  - See dirt/damage on model
  
- **Ammo Check** (Physical Action):
  - Remove magazine to see bullets
  - Weight-based estimation when shaking
  - Tracer rounds for count visualization

- **Armor Inspection** (Animation):
  - Look down to see vest condition
  - Check plates by patting chest
  - Visible damage model

### Minimal HUD Mode
When enabled, only critical contextual information appears:

#### Always Hidden
- Health bars
- Stamina meters  
- Ammo counters
- Minimap
- Crosshair (except when aiming)

#### Contextual Display (Optional)
- **Interaction Prompts**: When near objects
- **Critical Warnings**: "EMISSION IMMINENT" text only
- **Death State**: Bleedout timer when downed

### Inventory System
- **Grid-based**: Tetris-style organization
- **Weight Limit**: 30kg base, 60kg maximum
- **Quick Slots**: 
  - 1-0 for weapons and equipment
  - F1-F4 for consumables (meds, food, etc.)
- **Physical Inspection**: 
  - 3D model viewing for all items
  - Rotate to check condition
  - Read labels on consumables

### Immersion Options
Players can customize their experience:

#### Hardcore Mode (No UI)
- All information via physical items only
- No interaction prompts
- No subtitles or damage indicators
- Sound-based feedback only

#### Standard Mode  
- Minimal contextual UI
- Interaction prompts available
- Subtitle option
- Damage direction indicators

#### Accessibility Mode
- Traditional HUD elements available
- Health/stamina bars
- Ammo counter
- Compass on screen
- Status icons

### Physical Item Tiers

#### Tier 1 (Basic)
- **Analog Compass**: Magnetic needle
- **Mechanical Watch**: Wind-up mechanism  
- **Paper Map**: Static, markable
- **Echo Detector**: Basic beeps

#### Tier 2 (Advanced)
- **Digital Compass**: Degree readout
- **Digital Watch**: Stopwatch, alarms
- **Laminated Map**: Water-resistant
- **Bear Detector**: LCD display

#### Tier 3 (High-Tech)
- **GPS Compass**: Coordinate display
- **Smart Watch**: Multiple functions
- **Electronic Map**: Tablet device
- **Veles Detector**: Full GUI

### Controls
- **Movement**: WASD + sprint/walk/crouch/prone
- **Combat**: ADS toggle/hold options
- **Interaction**: Hold to search, tap for quick action
- **Inventory**: Tab for full, I for quick access
- **PDA**: P key with mouse navigation
- **Weapons/Equipment**: 1-0 number keys
- **Quick Items**: F1-F4 (consumables, meds, grenades, etc.)
- **Inspect**: Hold R to check weapon, Hold C to check compass
- **Physical Items**: Hold G for detector, H for map, T for watch

---

## Mutants

### Common Mutants

#### Blind Dogs
- **Behavior**: Pack hunters, 3-8 members
- **Weakness**: Fire, loud noises
- **Loot**: Dog tail, meat

#### Fleshes
- **Behavior**: Ambush predator
- **Weakness**: Headshots
- **Loot**: Flesh eye, meat

#### Boars
- **Behavior**: Charge attacks
- **Weakness**: Sidestepping
- **Loot**: Boar hoof, thick hide

### Uncommon Mutants

#### Bloodsuckers
- **Behavior**: Invisible stalking
- **Weakness**: Shotguns, UV light
- **Loot**: Bloodsucker tentacles

#### Snorks
- **Behavior**: Military training remnants
- **Weakness**: Leg shots
- **Loot**: Snork foot, gas mask

#### Poltergeists
- **Behavior**: Telekinetic attacks
- **Weakness**: EMP grenades
- **Loot**: Poltergeist skin

### Rare Mutants

#### Controllers
- **Behavior**: Psi-attacks, NPC control
- **Weakness**: Psi-protection, range
- **Loot**: Controller brain

#### Pseudogiants
- **Behavior**: Tank, ground slam
- **Weakness**: Explosives, .50 cal
- **Loot**: Pseudogiant eye

#### Chimeras
- **Behavior**: Extreme agility, high damage
- **Weakness**: Sustained fire
- **Loot**: Chimera claw

---

## Trading & Economy

### Currency
- **Rubles (RU)**: Standard currency
- **Artifacts**: High-value trades
- **Ammunition**: Emergency currency
- **Rations**: Survival trades

### Dynamic Pricing
- **Supply/Demand**: Prices fluctuate based on availability
- **Faction Relations**: ±50% based on reputation
- **Emissions**: Price spikes post-emission
- **Wartime**: Equipment prices increase during conflicts

### Trade Goods

#### Common (100-1000 RU)
- Food, water, medical supplies
- Standard ammunition
- Basic tools and parts

#### Uncommon (1000-10,000 RU)
- Weapons, armor
- Advanced medical
- Detectors

#### Rare (10,000-100,000 RU)
- High-tier weapons
- Exoskeletons
- Rare artifacts

#### Legendary (100,000+ RU)
- Unique weapons
- Combined artifacts
- Experimental equipment

---

## Technical Systems

### Save System
- **Automatic**: Every 10 minutes, entering/leaving areas
- **Manual**: Unlimited saves (except Ironman mode)
- **Checkpoint**: Before major events

### Difficulty Modes

#### Rookie
- 50% damage taken
- Abundant resources
- Simplified survival needs
- Map markers for objectives

#### Stalker
- Standard damage model
- Balanced resources
- Full survival mechanics
- Limited UI assistance

#### Veteran
- 150% damage taken
- Scarce resources
- Harsh survival requirements
- No UI assistance

#### Ironman
- Permanent death
- Single save file
- No UI assistance
- Achievement multipliers

### Performance Optimization
- **LOD System**: 5 levels of detail
- **Dynamic Loading**: Stream world sections
- **AI LOD**: Simplified distant AI
- **Occlusion Culling**: Hide unseen objects

---

## Audio Design

### Environmental
- **Ambient**: Wind, rain, distant gunfire
- **Anomalies**: Unique signatures for detection
- **Wildlife**: Birds fleeing before emissions
- **Geiger Counter**: Dynamic radiation levels

### Combat
- **Weapons**: Realistic firing sounds
- **Suppressed**: Authentic suppressor sounds
- **Impacts**: Material-based hit sounds
- **Movement**: Equipment rattling, footsteps

### Communication
- **Radio Static**: Distance-based quality
- **Languages**: Russian/English with subtitles
- **Faction Chatter**: Ambient conversations
- **Emergency Broadcasts**: Clear priority audio

### Music
- **Dynamic**: Intensity based on situation
- **Ambient**: Atmospheric exploration
- **Combat**: Escalating battle themes
- **Safe Zones**: Calm, guitar campfire songs

---

## Multiplayer (Co-op Only)

### Co-op Campaign Mode
- **Players**: 2-4 person squads
- **Shared World**: Host's world persists, guests can bring character progress
- **Progression**: 
  - Individual character skills and inventory
  - Shared faction reputation
  - Shared base ownership
  - Individual stash boxes
- **Difficulty Scaling**: 
  - +25% mutant health per additional player
  - +50% mutant spawns with 3-4 players
  - Better loot chances with more players
  - Faction battles scale to party size

### Co-op Mechanics
- **Revival System**: 
  - 60 second bleedout timer
  - Requires medical supplies to revive
  - Dead players respawn at nearest base
- **Loot Distribution**:
  - Individual loot for containers
  - Artifact detection shared within 50m
  - Trading between players allowed
- **Base Building**:
  - Collaborative construction
  - Shared resource pool for base
  - Individual rooms/storage possible
- **Communication**:
  - Voice chat with proximity
  - Radio for long-distance
  - Gesture system for stealth

### Drop-in/Drop-out
- Join friend's game anytime
- AI companion takes over when player leaves
- Progress saved for both host and guests
- No pause in co-op (real-time survival)

---

## Post-Launch Content Plan

### Year 1 DLC

#### Quarter 1: "Underground"
- New area: Metro system (20km² underground network)
- 5 hours additional story
- New mutant types (Cave variants)
- Underground base building
- New faction: The Tunnel Rats

#### Quarter 2: "Industrial Zone"  
- New area: Factory complex (15km² industrial wasteland)
- Vehicle system introduction (cars, bikes, trucks)
- New faction: The Foundry Workers
- Crafting system expansion with industrial machinery
- New anomaly type: Industrial accidents

#### Quarter 3: "Winter Has Come"
- Seasonal weather system (snow, blizzards, ice)
- Temperature survival expansion
- New anomaly types (ice-based, frozen time)
- Winter clothing and equipment
- Frozen mutant variants
- Ice fishing and new food sources

#### Quarter 4: "The Swamplands"
- New area: Toxic swamps (25km²)
- Boat traversal system
- New faction: The Bog Dwellers
- Amphibious mutants
- Swamp-specific artifacts
- Disease and parasite system

### Year 2 Focus
- **Mod Support**: 
  - Full Steam Workshop integration
  - Official mod tools release
  - Community content highlights
  - Mod compatibility framework
  
- **Expanded A-Life**:
  - Wildlife ecosystem
  - Faction politics system
  - Dynamic economy
  - Generational NPCs (children, aging)
  
- **Quality of Life**:
  - Photo mode
  - New Game+ mode
  - Arena/training mode
  - Hardcore survival mode
  
- **Community Features**:
  - Shared world events
  - Community challenges
  - Leaderboards (PvE only)
  - Base visiting system

---

## Platform Requirements

### Minimum Specs
- **OS**: Windows 10 64-bit
- **CPU**: Intel i5-8400 / AMD Ryzen 5 2600
- **RAM**: 12 GB
- **GPU**: GTX 1060 6GB / RX 580 8GB
- **Storage**: 150 GB SSD space

### Recommended Specs
- **OS**: Windows 11 64-bit
- **CPU**: Intel i7-10700K / AMD Ryzen 7 3700X
- **RAM**: 16 GB
- **GPU**: RTX 3070 / RX 6700 XT
- **Storage**: 150 GB NVMe SSD

### Ultra Specs
- **OS**: Windows 11 64-bit
- **CPU**: Intel i9-12900K / AMD Ryzen 9 5900X
- **RAM**: 32 GB
- **GPU**: RTX 4080 / RX 7900 XTX
- **Storage**: 150 GB NVMe SSD

---

## Accessibility Features

### Visual
- Colorblind modes (3 types)
- UI scaling options
- Subtitles with speaker tags
- High contrast mode

### Audio
- Visual sound indicators
- Closed captions
- Volume channel separation
- Spatial audio options

### Controls
- Full rebinding support
- Hold/toggle options
- Difficulty adjustments
- Auto-aim assistance options

### Comfort
- FOV slider (60-120)
- Motion blur toggle
- Head bob adjustment
- Arachnophobia mode (replace spiders)

---

## Closing Notes

Zone Survival aims to create the definitive hardcore survival shooter experience by combining the best elements of S.T.A.L.K.E.R., GAMMA, and Escape from Tarkov. The focus on emergent gameplay through the A-Life system, combined with deep survival mechanics and tactical combat, will provide players with unique stories and experiences in every playthrough.

The game respects player intelligence and agency, providing minimal hand-holding while offering robust systems that reward mastery and experimentation. Every journey into the Zone should feel dangerous, unpredictable, and ultimately rewarding for those who survive.

**"Good hunting, Stalker."**

---

*End of Game Design Document v1.0*
