# MOSRPG

**MOSRPG (Modular Open Source RPG for VRChat)** is a community-focused framework designed to make building RPG-style mechanics in VRChat worlds easier, more modular, and more extensible. It is built for **Unity 2022.3.22f1** and **VRChat Base & Worlds SDK 3.10.1**, using **UdonSharp** and Udon Behaviours.

🔗 **Latest Release (v0.3):**
[https://github.com/DankPopNLocks/MOSRPG/releases/tag/Main](https://github.com/DankPopNLocks/MOSRPG/releases/tag/Main)

---

## ⚠️ IMPORTANT DISCLAIMER

Portions of this project were developed with the assistance of **AI (ChatGPT by OpenAI)**. The author has reviewed, tested, corrected, and integrated the generated code using their own experience and judgment.

This project is shared **as-is** to provide a solid foundation for others to learn from, expand upon, and improve. If AI-assisted development is a concern for you, please refrain from using this repository.

Contributions, refactors, and improvements from the community are welcome.

---

**Key changes v0.3**

1. Clear separation of responsibilities between systems
2. Consistent `MOSRPG_` naming convention
3. Improved modularity and extensibility
4. Reworked combat and respawn logic
5. Reduced script coupling and circular dependencies
6. More explicit inspector-driven workflows

---

## 🧩 Basic Setup

1. Download the desired release as a **Unity Package**.
2. Import it into your active Unity project.
3. Open the **Example Scene** from the `Scenes` folder **OR** drag the preassembled prefab into your hierarchy.
4. If dragging the prefab into an existing scene, **zero the parent transform position**.

You will now have:
1. Core MOSRPG systems
2. Example items
3. Test enemy
4. Basic UI and respawn setup

From here, you can freely customize layouts, values, prefabs, and logic.

---

## ❤️ MOSRPG_ResourceManager

Central authority for health state management for both players & enemy AI.

1. Tracks current and maximum health
2. Differentiates between player and enemy entities
3. Sends death events
4. Interfaces with `MOSRPG_RespawnManager`
5. UI health displays
6. Damage sources

This script ensures health logic remains consistent and synchronized across network interactions.

---

## 🔄 MOSRPG_RespawnManager

Unified respawn logic for both players and enemies.

1. Stores spawn points or checkpoints
2. Handles delayed respawns
3. Resets health and state on respawn
4. Supports multiple respawn zones

This system allows designers to create **checkpoint-style respawning** independent of VRChat's menu respawn.

---

## 🎒 MOSRPG_InventoryManager

Manages player inventory logic and UI.

1. Defines inventory size
2. Stores item references
3. Handles cycling, selection, and storage
4. Controls raycast-based spawning
5. Assigns inventory icons
6. Keybind support for cycling and usage

The Inventory Manager is designed to be **agnostic of item behavior**, delegating logic to item-specific scripts.

---

## ⚔️ MOSRPG_Weapon

Handles equippable items and combat stats. Executes damage logic when a valid hit occurs.

1. Defines inventory sprite
2. Sets weapon collider references
3. Controls minimum and maximum damage
4. Manages damage cooldown intervals
5. Applies damage to valid `MOSRPG_ResourceManager` targets
6. Respects cooldowns and damage ranges
7. Supports player and enemy usage
8. Optional movement slowdown effects

Weapons can now exist purely as inventory or world objects **without automatically dealing damage**, allowing safer storage and staged activation.

---

## 🧱 MOSRPG_Interactable

Handles player interaction with world objects and defines how items may be used.

1. Pickup logic
2. Interaction gating
3. Delegation to Inventory or Item systems
4. Connects items to `MOSRPG_ResourceManager`
5. Applies healing or effects
6. Determines whether the item is consumed
7. Assigns inventory UI icon

This allows consumables (e.g., potions) and utility items to share the same framework.

This script keeps interaction logic clean and reusable across items, weapons, and environment objects. This can be used to along with the MOSRPG_RoleManager to create role-gated doors & items.

Also used to create role buttons, allowing you to set which team a player joins.

---

## 📍 MOSRPG_Checkpoint

Defines respawn locations that integrate with the unified respawn system.

1. Registers a respawn point with MOSRPG_RespawnManager
2. Updates the active checkpoint when entered
3. Supports checkpoint-style progression

Checkpoints allow creators to control where players return after death or falling out of bounds, independent of VRChat’s default respawn behavior.

---

## 💰 MOSRPG_EconomyManager

Manages player currency and basic economy interactions.

1. Tracks player currency values 
2. Allows currency collection and spending 
3. Provides a centralized reference for shops or rewards

This system is intentionally lightweight and can be expanded to support vendors, upgrades, or rewards.

---

## 🎭 MOSRPG_RoleManager

Assigns and manages player roles using indexed definitions.

1. Assigns roles based on index values
2. Supports single or multiple role configurations
3. Exposes role data for conditional logic
4. Defines role-based spawn locations

Roles can be used to:
1. Control player abilities
2. Restrict equipment
3. Gate interactions
4. Spawn players at designated locations when the game begins

---

## 👥 MOSRPG_LobbyPlayerList

Displays active players in the lobby UI.

1. Tracks joined players
2. Updates UI name lists dynamically
3. Reflects lobby state before game start

This helps players understand who is present and ready before a session begins and can also be used to reflect who is active in the game on the player's HUD.

---

## ▶️ MOSRPG_StartGameButton

Controls game initialization from the lobby

1. Allows the instance master to start the game
2. Optional majority-vote or instant-start logic
3. Connects with MOSRPG_RoleManager
4. Defines lobby spawn location
5. Sets game duration
6. Handles automatic game reset

This system acts as the bridge between lobby setup and active gameplay, ensuring roles, spawns, and timers are initialized consistently.

---

## 🤝 Contributions

MOSRPG is designed to have:
1. Modular & inspector Focused creative workflow
2. Community inspired feature integration
3. Easily configurable scripting with clear documentation

Feel free to:
1. Submit pull requests
2. Open issues
3. Fork and customize
4. Use for commercial projects as long as it is not a direct replicative of this source material (you must create your own game and do -not- sell this source!) Please credit https://github.com/DankPopNLocks/MOSRPG/ as the source, thank you!

---

## 🙏 Credits & Acknowledgements

**Project Author:**
DankPopNLocks

**Contributors:**
Autumn,
TheXev

**Patreon Supporters**
Thank you to everyone who supports my development work, I hope these tools help you and others!
www.patreon.com/DankPopNLocks

**AI Development Assistance:**
ChatGPT by OpenAI

ChatGPT was used for:
1. Architectural planning
2. Debugging assistance
3. Refactoring guidance
4. Documentation drafting

Human review, testing, and integration were always performed by the project author.

---

## 📜 License

This project is open source under the standard MIT License. Please refer to the repository for further licensing details.
