# Anisac

A 2D pixel-art roguelite / action platformer built solo in Unity — currently in the combat prototype (vertical slice) stage.

<!--
Add your GIFs here once exported, e.g.:
![Combo System](docs/gifs/combo-system.gif)
![Attack & Damage Pipeline](docs/gifs/attack-enemy.gif)
-->

## About

Anisac is a solo-developed 2D roguelite focused on fast, responsive combat. This project is also my personal learning ground for gameplay programming in Unity/C# — every core system (state handling, combo resolution, damage pipeline) is built from scratch rather than relying on built-in tools like Animator Controller state machines.

## Features

- **Custom combo system** — input buffer + combo resolver chain player attacks into multi-hit combos based on timing, instead of resetting on every input
- **Unified damage pipeline** — player and enemies share one Hitbox/Health system through an `IDamageable` interface, so combat logic isn't duplicated per character
- **State-driven enemy behavior** — enemies handle detection, chase, attack, hurt, and death through dedicated behavior controllers
- **Elite enemy variant** — extended enemy type using the same `ActionSystem` as the player, with melee, dash, and ranged (projectile) attacks
- **Data-driven design** — attacks, combos, and movement stats are configured via ScriptableObjects for fast iteration without touching code

## Tech Stack

- Unity 6000.3.11f1
- C#
- Universal Render Pipeline (URP) 17.3.0
- Unity Input System 1.19.0

## Project Status

🚧 Active development — currently building out the core combat vertical slice (player/enemy combat loop) before expanding into full game systems.

## Demo

*(GIFs / clips showcasing the combo system and combat pipeline go here)*

## Getting Started

1. Clone the repository
2. Open the project in Unity 6000.3.11f1 (or later compatible version)
3. Open `Assets/_Projects/Scenes/SampleScene.unity`
4. Press Play

## Author

**Hieu (Tu Duong Hieu)**
- Email: duonghieu0402@gmail.com
- LinkedIn: [linkedin.com/in/tu-duonghieu-fgw-hcm-711121226](https://linkedin.com/in/tu-duonghieu-fgw-hcm-711121226)
