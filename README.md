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

• Clear separation of responsibilities between systems
• Consistent `MOSRPG_` naming convention
• Improved modularity and extensibility
• Reworked combat and respawn logic
• Reduced script coupling and circular dependencies
• More explicit inspector-driven workflows

---

## 🧩 Basic Setup

1. Download the desired release as a **Unity Package**.
2. Import it into your active Unity project.
3. Open the **Example Scene** from the `Scenes` folder **OR** drag the preassembled prefab into your hierarchy.
4. If dragging the prefab into an existing scene, **zero the parent transform position**.

You will now have:
• Core MOSRPG systems
• Example items
• Test enemy
• Basic UI and respawn setup

From here, you can freely customize layouts, values, prefabs, and logic.

---

## ❤️ MOSRPG_ResourceManager

Central authority for health state management for both players & enemy AI.

• Tracks current and maximum health
• Differentiates between player and enemy entities
• Sends death events
• Interfaces with `MOSRPG_RespawnManager`
* UI health displays
* Damage sources

This script ensures health logic remains consistent and synchronized across network interactions.

---

## 🔄 MOSRPG_RespawnManager

Unified respawn logic for both players and enemies.

• Stores spawn points or checkpoints
• Handles delayed respawns
• Resets health and state on respawn
• Supports multiple respawn zones

This system allows designers to create **checkpoint-style respawning** independent of VRChat's menu respawn.

---

## 🎒 MOSRPG_InventoryManager

Manages player inventory logic and UI.

• Defines inventory size
• Stores item references
• Handles cycling, selection, and storage
• Controls raycast-based spawning
• Assigns inventory icons
• Keybind support for cycling and usage

The Inventory Manager is designed to be **agnostic of item behavior**, delegating logic to item-specific scripts.

---

## ⚔️ MOSRPG_Weapon

Handles equippable items and combat stats. Executes damage logic when a valid hit occurs.

• Defines inventory sprite
• Sets weapon collider references
• Controls minimum and maximum damage
• Manages damage cooldown intervals
• Applies damage to valid `MOSRPG_ResourceManager` targets
• Respects cooldowns and damage ranges
• Supports player and enemy usage
• Optional movement slowdown effects

The Gear Manager separates **equipment data** from **damage execution**, allowing weapons to be stored without necessarily being active.

Weapons can now exist purely as inventory or world objects **without automatically dealing damage**, allowing safer storage and staged activation.

---

## 🧱 MOSRPG_Interactable

Handles player interaction with world objects and defines how items may be used.

• Pickup logic
• Interaction gating
• Delegation to Inventory or Item systems
• Connects items to `MOSRPG_ResourceManager`
• Applies healing or effects
• Determines whether the item is consumed
• Assigns inventory UI icon

This allows consumables (e.g., potions) and utility items to share the same framework.

This script keeps interaction logic clean and reusable across items, weapons, and environment objects. This can be used to along with the MOSRPG_RoleManager to create role-gated doors & items.

Also used to create role buttons, allowing you to set which team a player joins.

---

## 📍 MOSRPG_Checkpoint

Defines respawn locations that integrate with the unified respawn system.

• Registers a respawn point with MOSRPG_RespawnManager
• Updates the active checkpoint when entered
• Supports checkpoint-style progression

Checkpoints allow creators to control where players return after death or falling out of bounds, independent of VRChat’s default respawn behavior.

---

## 💰 MOSRPG_EconomyManager

Manages player currency and basic economy interactions.

• Tracks player currency values 
• Allows currency collection and spending 
• Provides a centralized reference for shops or rewards

This system is intentionally lightweight and can be expanded to support vendors, upgrades, or rewards.

---

## 🎭 MOSRPG_RoleManager

Assigns and manages player roles using indexed definitions.

• Assigns roles based on index values
• Supports single or multiple role configurations
• Exposes role data for conditional logic
• Defines role-based spawn locations

Roles can be used to:
• Control player abilities
• Restrict equipment
• Gate interactions
• Spawn players at designated locations when the game begins

---

## 👥 MOSRPG_LobbyPlayerList

Displays active players in the lobby UI.

• Tracks joined players
• Updates UI name lists dynamically
• Reflects lobby state before game start

This helps players understand who is present and ready before a session begins and can also be used to reflect who is active in the game on the player's HUD.

---

## ▶️ MOSRPG_StartGameButton

Controls game initialization from the lobby

• Allows the instance master to start the game
• Optional majority-vote or instant-start logic
• Connects with MOSRPG_RoleManager
• Defines lobby spawn location
• Sets game duration • Handles automatic game reset

This system acts as the bridge between lobby setup and active gameplay, ensuring roles, spawns, and timers are initialized consistently.

---

## 🤝 Contributions

MOSRPG is designed to be:
• Modular
• Community-driven
• Easily configurable

Feel free to:
• Submit pull requests
• Open issues
• Fork and customize

---

## 🙏 Credits & Acknowledgements

**Project Author:**
DankPopNLocks

**Contributors:**
Autumn,
TheXev

**AI Development Assistance:**
ChatGPT by OpenAI

ChatGPT was used for:
• Architectural planning
• Debugging assistance
• Refactoring guidance
• Documentation drafting

Human review, testing, and integration were always performed by the project author.

---

## 📜 License

This project is open source under the standard MIT License. Please refer to the repository for further licensing details.
