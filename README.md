# SpaceP - 2D Space Landing Game

SpaceP is a 2D physics-based space landing game built with Unity. The player controls a small spaceship, manages limited fuel, avoids hazards, collects resources, and lands safely on designated platforms.

This project is my main Unity portfolio project. It focuses on complete gameplay flow, mobile build delivery, Firebase-backed online features, and Android APK testing.

## Demo

- Gameplay demo: https://drive.google.com/drive/folders/1CTMMpAY1NcgH4oJ6Y69sNDRRw6_i_gyu
- Repository: https://github.com/TrongPhat14/SpaceP

## Gameplay Features

- 15 handcrafted levels with increasing difficulty.
- Rigidbody2D spaceship movement with thrust, rotation, gravity, and collision handling.
- Fuel system with limited fuel, fuel pickups, and fuel-based decision making.
- Landing evaluation based on impact speed and ship stability.
- Score, coin rewards, save/load, level progression, and game completion flow.
- Shop upgrades with 4 upgrade categories.
- Touch controls for Android and keyboard/gamepad-friendly input through Unity Input System.
- Interactive first-level control tutorial.
- Mechanic tutorial popups for new hazards and level mechanics.

## Hazards and Mechanics

- Moving meteors that cause instant crash.
- Key, lock, and split-door mechanics.
- Static laser enemies.
- Player-following laser enemies.
- Wind zones that push the player in a fixed direction.
- Narrow terrain routes and landing challenges.

## Online and Mobile Features

- Firebase Realtime Database leaderboard.
- Player name validation and global rank display.
- Firebase Analytics events for level start, failure reason, retry, completion, game completion, and leaderboard submission.
- Google AdMob rewarded ads with UMP consent, coin rewards, and daily view limits.
- Android APK build and real-device testing.

## Technical Highlights

- Unity 6.3.10f1 project using C#.
- Rigidbody2D-based gameplay physics.
- ScriptableObject-based shop upgrade data and landing scoring configuration.
- Coroutine-driven gameplay timing and UI sequences.
- DOTween UI animation polish.
- Object Pooling for enemy projectiles to reduce repeated runtime instantiation.
- Modular prefab-based level design.
- GitHub Actions CI using GameCI for Android APK builds.
- Firebase and AdMob configuration excluded from Git and injected locally/through secrets.

## Tech Stack

- Unity, C#
- Unity Input System
- Rigidbody2D, Collider2D, Trigger logic
- ScriptableObject, Coroutine
- DOTween
- Firebase Realtime Database
- Firebase Analytics
- Google AdMob
- Git, GitHub Actions, GameCI

## Controls

| Action | Keyboard | Mobile |
| --- | --- | --- |
| Thrust | Up Arrow / bound input action | Up touch button |
| Rotate Left | Left Arrow / bound input action | Left touch button |
| Rotate Right | Right Arrow / bound input action | Right touch button |

Objective: land safely before running out of fuel.

## How to Run

1. Clone the repository:

   ```bash
   git clone https://github.com/TrongPhat14/SpaceP.git
   ```

2. Open the project with Unity Hub.
3. Use Unity `6000.3.10f1` or a compatible Unity 6 version.
4. Open the main scene from `Assets/Scenes`.
5. Press Play.

## Local Configuration Notes

Firebase and AdMob configuration files are intentionally not committed to the repository.

To build online features locally, add your own Firebase and AdMob configuration files in the expected Unity locations. Without these files, offline gameplay can still be inspected, but Firebase leaderboard, analytics, and ads will not work correctly.

## Assets and Licenses

Third-party asset, font, and audio usage is documented in:

- [docs/licenses.md](docs/licenses.md)

Only assets intended for game/commercial use should be kept in the final release build.

## What I Learned

- Building a complete Unity gameplay loop from menu to game completion.
- Designing mobile-friendly touch controls and UI flows.
- Structuring gameplay, UI, save data, shop upgrades, and online systems.
- Integrating Firebase leaderboard and analytics into a Unity game.
- Setting up Android builds, AdMob rewarded ads, and GitHub Actions CI.
- Improving runtime behavior with projectile object pooling and focused profiling.

## Author

Nguyen Hoang Trong Phat
