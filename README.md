# ADL Mission Miner

A 2D rehabilitation game for stroke patients — Gold Miner style.

Designed to target **hand and wrist exercises**, the most neglected part of stroke rehabilitation. Players perform real therapeutic gestures — finger pinch and wrist extension — to swing a hook and collect mission-relevant items.

---

## Gameplay

1. A mission appears at the top: *"Brush your teeth"*, *"Get ready to go out"*, etc.
2. A rope and hook swing left and right like a pendulum.
3. **Finger pinch** to lock the swing angle.
4. A power meter charges — sweep through yellow → green → red.
5. **Wrist extension** to fire. Distance depends on where you stopped the meter.
6. Hook grabs target items and retracts. Collect all targets before time runs out.

---

## Missions

| Mission | Title | Targets |
|---|---|---|
| Brush teeth | Morning routine | toothbrush, toothpaste, cup |
| Leave house | Leaving the house | shoe, keys, bag, glasses |
| Take meds | Medication time | medication, bottle, banana |

Each mission scatters ~5 distractor items from other missions' pools.

---

## Input

Two physical gestures drive all gameplay. Mouse click and touch tap are supported as development/testing fallbacks.

| Gesture | Maps to | In-game action |
|---|---|---|
| Finger pinch (thumb to finger) | Button 1 | Lock swing angle |
| Wrist extension | Button 2 | Release power / fire hook |

Keyboard is for UI/menu only — not used during gameplay.

---

## Therapeutic Goals

- **Finger pinching** — thumb-to-finger opposition, activates intrinsic hand muscles
- **Wrist extension** — counters the flexor spasticity pattern common after stroke
- Both movements are mapped to meaningful in-game actions so patients associate the gesture with a real ADL task
- Progression through more reps, faster response, and longer hold time across sessions
- Forgiving mechanics — slow or small movements are never penalised

---

## Technical Details

- **Engine:** Unity (2D)
- **Input:** `InputReader.cs` — routes both gesture buttons, mouse click, and touch tap into two distinct game actions
- **Hook state machine:** SWINGING → LOCKED → CHARGING → FIRING → RETRACTING → IDLE
- **UI:** TextMeshPro, custom palette, Fraunces + Nunito fonts

### Key Scripts

| Script | Role |
|---|---|
| `HookController` | Pendulum swing, fire, and retract logic |
| `InputReader` | Unified input routing (gesture buttons / mouse / touch) |
| `MissionManager` | Timer, score, win/lose, mission flow |
| `ItemSpawner` | Places targets + distractors on shelves at scene load |
| `CollectibleItem` | Item behaviour — glow, sparkle, target flag |
| `ChargeMeterUI` | Power meter sweep UI |
| `MissionData` | ScriptableObject for mission config |
| `MainMenu` | Mission selection screen |

---

## Tuning (Inspector-exposed)

| Parameter | Default |
|---|---|
| Swing frequency | ~0.55 Hz, ±32° |
| Charge sweep time | ~1.2 s (slow for limited dexterity) |
| Sweet-spot zone | 40–70% of meter |
| Hook fire speed | 1500 px/s |
| Retract speed | 900 px/s |
| Mission timer | 90 s |
| Target score | +50, +25 precision bonus (green zone) |

---

## Accessibility

- All text ≥ 18 px at 1920×1080
- All interactive targets ≥ 88×88 px
- High-contrast ink `#2E2A3A` on cream `#FBF5EC` background
- No flashing above 3 Hz — calm animations throughout
- Forgiving mechanics — slow or small movements are never penalised

---

## Project Structure

```
Assets/
  Scripts/
    HookController.cs
    InputReader.cs
    MissionManager.cs
    ItemSpawner.cs
    CollectibleItem.cs
    MissionData.cs
    MainMenu.cs
    UI/
      ChargeMeterUI.cs
    Editor/
      GameSceneBuilder.cs
      MainMenuBuilder.cs
      MissionDataCreator.cs
      SpritePlaceholderCreator.cs
  Sprites/
    toothbrush.png, toothpaste.png, cup.png
    soap.png, comb.png, hairbrush.png
    shoe.png, keys.png, bag.png, glasses.png
    medication.png, bottle.png, banana.png
    hook.png, sparkle.png
```

---

## Team

2-person team.

- 1 programmer
- 1 designer / non-coder

---

## License

Hackathon project — see repository for details.
