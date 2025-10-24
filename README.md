# Spirit of the Dark Cards
*A turn-based deck-building adventure built in Unity.*

https://frostyshrek.github.io/Game-prototype.3.1-B/build/index.html

---

## Overview
**Spirit of the Dark Cards** is a prototype that blends the tactical depth of a **turn-based card system** with the intensity and atmosphere of a **combat experience**.  
Instead of attacking directly, players build powerful **card combos**, chaining abilities, buffs, and finishers to overcome enemies strategically.  
Outside of combat, players explore the **Glade**.

---

## Core Gameplay Loop
1. **Explore** the overworld to find enemies.  
2. **Enter Battle** when approaching an enemy, switching to a tactical card combat scene.  
3. **Play Cards** in sequence to form combos.
4. **Defeat Enemies** to earn keys and progress the story.  
5. **Return to the Glade** to rest, and prepare for the next encounter.  


## How to Play

### Objective
Defeat enemies in **turn-based card battles** by playing and chaining cards strategically.  
Explore the **Glade** to discover enemies and collect **Ancient Keys** needed to progress the story.

---

### Starting the Game
1. Open the Page Link at the top of the README File.  
2. Use the player controls to move around the environment.  
3. When you approach an enemy, a **battle scene** will automatically load.  
4. Once in battle, use your **cards** to attack, defend, and combo against the enemy.

---

### Battle System
- The battle uses a **turn-based system**.  
- Each turn, you can **select cards** from your hand to play in sequence.  
- Chaining cards together creates **combos**, increasing damage or triggering special effects. 
- When you end your turn, the **enemy takes their turn**, performing attacks or buffs.

#### Card Types
| Type | Description |
|------|--------------|
| **Attack Cards** | Deal damage to enemies. May have elemental attributes (Fire, Water, etc.) |
| **Buff Cards** | Temporarily increase damage, defense, or stamina regeneration. |
| **Debuff Cards** | Inflict damage-over-time or reduce enemy power. |
| **Utility Cards** | Heal, draw more cards, or alter your next combo sequence. |

#### Combos
- Some cards have **combo bonuses** when played after specific types (e.g., *Buff → Attack → Finisher*).  
- Experiment with different orders to maximize effects.
- Combo order is shown visually by a small **number badge** on selected cards.

---

### Controls
| Action | Key / Input |
|---------|--------------|
| **Move Player (in Glade)** | `W A S D` or Arrow Keys |
| **Interact** | `E` |
| **Look Around** | Mouse movement |
| **Pause / Resume** | `Esc` |
| **Select Card (in Battle)** | Left-click on a card |
| **End Turn** | Click the **End Turn** button |
| **Return to Game / Close Pause Menu** | Click **Return to Game** or press `Esc` again |

---

### Health and Damage
- Both the **player** and **enemy** have visible **HP bars** floating above them.  
- When HP reaches **0**, the character is defeated.  
- Defeating the enemy returns the player to the **Glade**.  
- Losing resets the player to the **last checkpoint**.

---

### Progression
- Some enemies drop **Ancient Keys** after being defeated.  
- These keys unlock new areas and are essential for progressing the story.  
- Explore the environment for  that grant additional rewards.  

---

### Between Battles
- After a battle, you’ll return to the **Glade**.    
- Each enemy encounter will only trigger once unless progress is reset (for prototype testing, `AutoClearPrefs.cs` resets all data on Play).

---

### Prototype Notes
- This version is a **prototype**, focused on demonstrating core combat and card systems.  
- Deck editing, multiple heroes, and puzzle mechanics are in progress.  
- Saved data resets automatically each run during the prototype phase.

---

### Win Condition
Defeat all enemies in the Glade, collect the **Ancient Key** and open the Chest to complete the current version of the story.

---

## Future Vision

Looking beyond the prototype, **Spirit of the Dark Cards** aims to evolve into a large-scale, exploration-driven RPG combining the **strategic depth of deck-building** with the **narrative and atmosphere of Soulslike games**.

### Long-Term Goals
- **Massive Card Pool:**  
  Expand the deck system to feature **100+ unique cards**, each with distinct mechanics, elemental attributes, and combo potential.

- **Hero Archetypes:**  
  Introduce multiple hero classes, each with personalized starting decks, passive traits, and ultimate abilities that define playstyle.

- **Deck Synergy & Loadouts:**  
  Develop systems that reward intelligent card combinations, allowing players to discover powerful synergies between attacks, buffs, and elemental effects.

- **World & Exploration:**  
  Build out a fully explorable world filled with **puzzles, environmental challenges, and hidden areas** that reward exploration with **Ancient Keys** and lore.

- **Secret Quests & Bosses:**  
  Add hidden encounters, optional storylines, and secret bosses that test the player’s mastery of deck strategy and timing.

- **Progression & Customization:**  
  Allow deck upgrades, hero progression trees, and resource management between encounters to encourage experimentation and replayability.

- **Atmosphere & Storytelling:**  
  Maintain the **mystery, tone, and difficulty** inspired by Dark Souls — a world that reveals itself through exploration, choice, and persistence.


## Contributors
- Mark Attieh > frostyshrek
- Wei Zhou > WilliamZhou-AUV
- Soi Chen > SoiChen
- Aadil Munshi > adzmunshi 
