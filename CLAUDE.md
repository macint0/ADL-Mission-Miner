# ADL Mission Miner — Plakod Hackathon Project

## Project Overview
A 2D rehabilitation game for stroke patients, Gold-Miner style. Designed for **hand and finger exercises** — the most neglected part of stroke rehab. Built in Unity, all player input goes through the **Plakod controller**. Calm, friendly, high-contrast — built for elderly / medical users.

## Team
- 2 members
- 1 coder (me), 1 non-coder

## Hardware — Sensors (2 inputs via Plakod)
| Sensor | Type | Mapped to | Motion |
|---|---|---|---|
| Conductive Copper Foil Tape | Digital | Button 1 | Finger pinch (thumb touches finger) |
| TBD (any digital sensor) | Digital | Button 2 | TBD |

## Input
- Plakod only exposes **button inputs** — no joystick/analog axis available
- **Plakod button** is the primary input (maps to Space)
- Also support **Mouse click** and **Touch tap** — same behavior as Space
- Do NOT use keyboard/mouse for gameplay — all game input must go through Plakod
- UI/menu can use keyboard/mouse
- Read inputs with `Input.GetButtonDown()` or `Input.GetKey(KeyCode.JoystickButton0)`

## Gameplay Loop
1. Mission text appears at top center ("Prepare for brushing teeth").
2. A rope + hook hang from a pivot at center-top. The hook auto-swings
   left↔right like a pendulum (±32°, ~0.55 Hz).
3. Player presses **Plakod button** once to **lock the angle**.
4. A power meter at bottom center starts charging — the marker sweeps
   left→right across yellow (too short), green (sweet spot), red (too long).
5. Player presses **Plakod button** again to **release**. The hook fires along the
   locked angle, traveling a distance proportional to where they stopped
   the meter (sweet-spot = max useful range).
6. If the hook touches a **target item** (mission-relevant), it grabs it and
   retracts; +score, sparkle particles. Wrong items don't penalize — hook just retracts empty.
7. Mission ends when all targets collected OR timer hits 0.

## Visual System

### Palette (use these exact hex values)
- Cream bg:        `#FBF5EC`
- Cream deep:      `#F2E9D8`
- Ink (text):      `#2E2A3A`
- Ink soft:        `#5A546A`
- Mint / deep:     `#B8E4D2` / `#6FBFA0`
- Peach / deep:    `#FFD2B8` / `#F2A37A`
- Lavender / deep: `#D9CBEF` / `#A88FD1`
- Butter / deep:   `#FFE9A8` / `#E8C25A`
- Sky / deep:      `#C7E1F2` / `#7FB3D9`
- Rose / deep:     `#F2C3CC` / `#D98896`
- Power meter:     yellow `#F5D26B`, green `#7BC796`, red `#E89090`

### Type
- Display / numbers: **Fraunces** (700/900). Use for score, timer, mission title.
- UI / body:         **Nunito** (600/800). Use for instructions, buttons, labels.
- Min body size: **18 px**. Score / timer: 30–64 px tabular numerals.

### Layout (16:9, design at 1920×1080)
- Top-left:    Timer chip (white pill, peach icon, "TIME" label, mm:ss value)
- Top-center:  Mission banner (white pill, butter "MISSION X OF N" tag, Fraunces title)
- Top-right:   Score chip (white pill, mint icon, "SCORE" label, value)
- Center-top:  Pivot dot + rope + hook
- Middle/lower: Items scattered on cozy wooden shelves
- Bottom-center: Power meter (~520 px wide, 36 px tall, rounded pill)

### Item Highlights
- Targets pulse with a butter-yellow halo (`#FFE9A8` glow) AND emit sparkle particles (use `sparkle.png`).
- Wrong items render plain — no color shift, no greying out.

## Sprites (in `Assets/Sprites/`)

All sprites are 512×512 transparent PNGs, paper-cutout sticker style.
Set Texture Type: **Sprite (2D and UI)**, Pixels Per Unit: 100.

| File              | Use                              | Pivot      |
|-------------------|----------------------------------|------------|
| `toothbrush.png`  | Mission: Brush teeth (target)    | Center     |
| `toothpaste.png`  | Mission: Brush teeth (target)    | Center     |
| `cup.png`         | Mission: Brush teeth (target)    | Center     |
| `soap.png`        | Mission: Wash hands (target)     | Center     |
| `comb.png`        | Mission: Groom (target)          | Center     |
| `hairbrush.png`   | Mission: Groom (target)          | Center     |
| `shoe.png`        | Mission: Leave house (target)    | Center     |
| `keys.png`        | Mission: Leave house (target)    | Center     |
| `bag.png`         | Mission: Leave house (target)    | Center     |
| `glasses.png`     | Mission: Leave house (target)    | Center     |
| `medication.png`  | Mission: Meds (target)           | Center     |
| `bottle.png`      | Mission: Meds (target)           | Center     |
| `banana.png`      | Mission: Meds (target)           | Center     |
| `hook.png`        | Hook + rope (rotates from top)   | **Top**    |
| `sparkle.png`     | Particle System texture          | Center     |

The rope is **part of the hook sprite** — pivot it at top so rotation looks correct.

## Missions to Ship

| Mission | Title              | Subtitle            | Targets                         |
|---------|--------------------|---------------------|---------------------------------|
| brush   | Morning routine    | Brush your teeth    | toothbrush, toothpaste, cup     |
| leave   | Leaving the house  | Get ready to go out | shoe, keys, bag, glasses        |
| meds    | Medication time    | Take your medicine  | medication, bottle, banana      |

In every mission, scatter ~5 non-target distractor items on the shelves, drawn from the other missions' item pools.

## Scenes / Scripts to Build

1. **MainMenu** — three mission cards (use the palette tints above).
2. **GameScene** — the core loop. Suggested components:
   - `HookSwinger` (state machine: SWINGING → LOCKED → CHARGING → FIRING → RETRACTING → IDLE)
   - `PowerMeter` (UI; emits the 0..1 release value)
   - `ItemSpawner` (places targets + distractors on shelves at scene load)
   - `Item` (MonoBehaviour with `bool isTarget`, glow + sparkle when target)
   - `MissionManager` (timer, score, win/lose, mission completion)
   - `InputRouter` (Plakod button / click / tap → single "PressAction" event)
3. **WinScreen / LoseScreen** — celebration, "Play again" / "Next mission" buttons.

## Behavior Tuning (expose in Inspector)
- Swing speed: ~0.55 Hz, amplitude ±32°
- Charge speed: full sweep in ~1.2 s (slow enough for limited dexterity)
- Sweet-spot zone: 40–70% of the meter
- Hook fire speed: 1500 px/s; retract speed: 900 px/s
- Mission time: 90 s
- Score: target = +50 base, +25 if released in green zone (precision bonus)

## Accessibility Floor
- All text ≥ 18 px on screen at 1920×1080
- All hit targets (buttons) ≥ 88×88 px
- High contrast on cream bg — keep ink `#2E2A3A` for body, never grey
- No flashing > 3 Hz anywhere (no strobing on win, calm animations only)
- Sound effects optional and toggleable

## Therapeutic Goals
- Finger pinching (thumb to finger opposition) via copper tape
- Progression = more reps, faster response, longer hold time over sessions
- Never punish slow or small movements — forgiving feel always

## Development Priorities
1. Confirm exact Plakod button mapping with organizers
2. Get Plakod input reading correctly in Unity via `InputRouter`
3. Keep mechanics simple — hackathon scope
4. Reference `ADL Mission Miner UI Kit.html` for live palette/layout previews
