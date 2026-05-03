# 🎮 Space Landing Game

## Overview

A 2D physics-based space landing game built with Unity, where the player controls a spaceship to safely land on designated platforms while managing fuel and avoiding environmental hazards.

## 🚀 Gameplay Features

### Physics-based Movement
- Spaceship controlled using Rigidbody2D
- Realistic thrust and rotation mechanics

### Fuel Management System
- Fuel decreases when applying thrust
- Players must collect fuel pickups to continue flying

### Landing System
Landing is evaluated based on:
- Speed
- Angle

**Different outcomes:**
- ✅ Successful landing
- ❌ Crash (too fast / wrong angle / wrong area)

### Environmental Hazards
- 🌪️ **Wind zones** that push the player off course
- ☄️ **Moving asteroids** that require timing and avoidance

### UI System
- Main menu
- Pause screen
- Success screen
- Fail screen
- Game over screen

### Audio Feedback
Sound effects for:
- Pickup collection
- Landing events
- Gameplay actions

## 🎮 Controls

| Action | Key |
|--------|-----|
| Thrust | Move Up |
| Rotate Left | Left Arrow |
| Rotate Right | Right Arrow |

**Objective:** Land safely before running out of fuel

## 🧠 Technical Highlights

- Event-driven architecture for gameplay systems
- Modular prefab-based level design
- Unity Input System integration
- Clean separation between gameplay logic and UI
- Physics interactions using Rigidbody2D

## 📹 Gameplay Demo

(Add a short gameplay video or GIF here)

## 🛠️ How to Run

1. Clone the repository:
   ```bash
   git clone https://github.com/TrongPhat14/SpaceP
2. Open the project in Unity Hub

3. Open the main scene and press Play

## 🎯 What I Learned
- Designing gameplay systems with player feedback
- Balancing difficulty using environmental hazards
- Working with Unity physics for responsive controls
- Structuring a small-scale game project cleanly

## 👤 Author
Trọng Phát
