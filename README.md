# Armor Repair

A bespoke version of [Armor Repair](https://github.com/citizenSnippy/ArmorRepair) for BattleTech Extended Tactics.

## What's Changed

### ✨ Repair Costs

* **Set repair costs to tabletop values.**

The mod uses the same armor and structure repair costs as tabletop, with Endo Steel and Ferro-Fibrous armor being much more expensive to repair than standard structure and armor.

Compared to Vanilla/BEX, repairs are now significantly cheaper in both time and C-Bills, but the costs add up quickly on each mission now that armor is not reapplied for free.

  <details>
    <summary>Vanilla/BEX vs Armor Repair</summary>

    Taking a 100-ton mech with standard structure and armor:
    * Repairing one ton of structure damage now costs 4,000 C-Bills (38,050 C-Bills in Vanilla/BEX).
    * Repairing one ton of armor damage now costs 10,000 C-Bills (0 C-Bills in Vanilla/BEX).
    
    Taking a 100-ton mech with Endo Steel chassis and Ferro-Fibrous armor:
    * Repairing one ton of structure damage now costs 32,000 C-Bills (38,050 C-Bills in Vanilla/BEX).
    * Repairing one ton of armor damage now costs 20,000 C-Bills (0 C-Bills in Vanilla/BEX).

	> [!NOTE]
    > Endo Steel and Ferro-Fibrous armor cost three times more before the technologies are reintroduced in 3040.

  </details>

### ✨ Tonnage Scaling

* **Changed tonnage scaling for structure repairs to be more lenient than BEX.**

The mod increases structure repair times by 1-4x based on mech tonnage. This ensures that heavier mechs take longer to repair, but not as long as BEX, while maintaining tabletop structure repair costs.

## Usage

### Installation

Download the [latest release](https://github.com/AkiraBrahe/ArmorRepair/releases/latest) of the mod and unpack it into your `Battletech\Mods` folder after installing BattleTech Extended Tactics.

> [!IMPORTANT]
> Advanced MechLab integrates and expands on Armor Repair, making it incompatible.

## Settings

| Setting                                                        | Default | Description                                                                                                                                                                               |
| :------------------------------------------------------------- | :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Enable Auto-Repair Prompt                                      | True    | Toggle the auto-repair prompt after each battle where Yang summarises repair costs and asks if you want him to auto-repair your mechs.                                                    |
| Auto-Repair Mechs With Destroyed Components                    | True    | Toggle the auto-repair of mechs with destroyed components post-battle.<br>_**Note:** Disabling this lets you see what components were destroyed and allows you to replace them manually._ |
| Auto-Repair Structure                                          | True    | Toggle the auto-repair of structure damage post-battle.<br>_**Note:** Structure damage takes a long time to repair, so you may want to disable this if you are on a tight schedule._      |
| Scale Structure Repair Time By Tonnage                         | True    | Toggle tonnage scaling on structure repair times.<br>_**Note:** Heavier mechs take up to 4x longer to repair when enabled._                                                               |
| Prototype Endo Steel / Ferro-Fibrous<br>Repair Cost Multiplier | 3       | Multiplier applied when repairing Prototype Endo Steel or Ferro-Fibrous armor.                                                                                                            |
| Clan-Tech Repair Cost Multiplier                               | 1.5     | Multiplier applied when repairing Clan mechs.                                                                                                                                             |
| Armor Repair Cost By Tag                                       |         | Multipliers applied when repairing armor on mechs with a specific tag.                                                                                                                    |
| Structure Repair Cost By Tag                                   |         | Multipliers applied when repairing structure on mechs with a specific tag.                                                                                                                |
