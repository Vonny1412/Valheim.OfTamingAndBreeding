![Of Taming and Breeding](_Assets/otab_logo.png)

# Of Taming and Breeding (OTAB)

**Of Taming and Breeding (OTAB)** is a mod for **Valheim** that provides a
**data-driven framework for taming, breeding and creature behavior**.

It is designed for players and server administrators who want
**full control**, **predictable mechanics**, and **long-term world stability** —
without hidden automation or performance-heavy systems.

OTAB does not try to “make everything tameable”.
Instead, it gives you the tools to **design intentional, believable creature behavior**.

---

## Core Philosophy

OTAB is built around a few core principles:

* **Server-authoritative logic**
* **Explicit configuration over implicit behavior**
* **No hidden automation**
* **Performance first**
* **Vanilla-compatible, but not vanilla-limited**

Everything in OTAB is **opt-in, explicit and traceable**.

---

## Data-Driven Everything (YAML-Based)

OTAB uses **structured YAML files** to define behavior instead of hardcoded rules.

You can fully control (related to taming and breeding):

* Creatures
* Eggs
* Offsprings
* Recipes (via Wacky’s Database)
* Translations

No recompiling, no guessing, no hidden defaults.

> If something exists, it is because **you explicitly defined it**.

---

## Component-Based Creature Control

OTAB treats creature behavior as **explicit components**.

For each creature you decide:

* Patch existing behavior
* Inherit vanilla behavior
* Remove behavior entirely

Examples:

* A creature that is tameable but **cannot reproduce**
* A passive offspring that **never becomes an adult**
* Egg-like items out of **any** other item even wood

Nothing happens implicitly.

---

## Explicit Taming Rules (No Accidental Tames)

One of OTAB’s most important features:

### `RequireFoodDroppedByPlayer`

When enabled:

* Only **player-dropped food** counts for taming and breeding
* World food (drops from combat, corpses, environment) is ignored

This prevents classic problems like:

* Wolves taming themselves after killing a deer
* Animals breeding uncontrollably because they ate random drops
* “Why is this thing suddenly tamed?” moments

Taming always happens **because a player intended it**.

> **Note:**  
> This setting does **not** prevent animals from eating world food.
>  
> Animals will still consume nearby food items, but **eating non player-dropped food will NOT start or progress the fed/taming timer**.
>  
> In short: *they eat it, but they don’t get “fed” from it.*

---

## Smarter Food Searching (Non-Robotic AI)

Vanilla Valheim food search logic is deterministic:

> “Always go to the nearest food.”

OTAB introduces an **optional weighted food search system**:

* Nearby food has higher weight
* Distant food has lower weight
* Selection is randomized, not deterministic

Result:

* Animals feel **less robotic**
* Movement looks more natural
* No perfectly synchronized behavior patterns

The system is simple, fast, and predictable — without feeling artificial.

---

## Advanced Procreation System (Beyond Vanilla)

OTAB replaces Valheim’s simplistic breeding logic with a **fully configurable system**.

### Partner Selection

* Weighted partner lists
* Partner memory (mourning behavior)
* Partner recheck intervals
* Self-reproduction support (no-partner breeding)

### Offspring Selection

* Weighted offspring pools
* Partner-dependent offspring
* Optional no-partner offspring
* Level inheritance & level-up chances
* Maximum offspring chains per pregnancy

Breeding becomes **intentional, configurable and extensible**.

---

## Multiple Offsprings per Pregnancy (Sibling Chains)

OTAB introduces an optional **siblings-per-pregnancy system**, extending Valheim’s one-offspring-per-pregnancy behavior.

Instead of each pregnancy always resulting in exactly one offspring, OTAB allows:

* Multiple offsprings from a single pregnancy
* Controlled chaining of births
* Explicit limits and probabilities

### How it Works

Each pregnancy can:

* Produce **one or more offsprings**
* Continue producing siblings based on a configurable chance
* Stop automatically after reaching a defined maximum

This allows natural-looking outcomes such as:

* Single births (most common)
* Occasional twins
* Rare larger litters — without becoming uncontrollable

### Configuration Control

You can explicitly define:

* Maximum number of offsprings per pregnancy
* Chance for an additional sibling

### Why This Matters

This system enables:

* More believable animal reproduction
* Better population pacing
* Rare but memorable breeding events
* Fine-grained balancing for long-running worlds

It avoids the extremes of Vanilla’s rigid single-offspring rule

---

## Offsprings Are NOT Just Small Adults

In OTAB, **offsprings are their own lifecycle stage**.

You can decide:

* Whether they ever grow up
* How long they stay small
* What they grow into (or if they grow at all)
* Whether they inherit tame state

This enables:

* Permanent baby creatures
* Decorative companions
* Lore-driven “juvenile forms”
* Mini-boss aspects or peaceful remnants

---

## Egg System with Real Control

Eggs in OTAB are not just timers.

You can define:

* Multiple possible hatch results
* Weighted outcomes
* Environmental requirements (fire, roof, cover)
* Tamed or wild hatchlings
* Visual control (particles, lights, effects)
* Custom items via cloning

---

## Clean Separation: Server vs Client

OTAB clearly separates responsibilities:

### Server

* Loads YAML data
* Owns game logic
* Is always authoritative
* Syncs encrypted data to clients

### Client

* Receives cached data
* Cannot influence logic
* Displays synced translations

This guarantees:

* Deterministic multiplayer behavior
* No desyncs
* No client-side cheating
* No “it works for me” problems

---

## Encrypted Cache System (Performance First)

OTAB uses a **custom encrypted cache system**:

* Server generates cache per world
* Clients load from cache only
* No directory watching
* No file system polling

Changes only apply after restart — **by design**.

This keeps:

* CPU usage low
* GC pressure minimal
* Startup predictable
* Long-running servers stable

Performance is not an afterthought.
It is a core design goal.

### About Encryption

The cache encryption used by OTAB is **not meant as a security feature**.

Its primary purpose is to **discourage casual data inspection and spoilers**, such as:
* Looking up future offspring
* Revealing rare outcomes
* Inspecting hidden mechanics before they naturally occur in gameplay

The data **can still be extracted by determined users**, and OTAB does not attempt to fully prevent this.

Encryption in OTAB is about **preserving discovery and surprise**, not about enforcing secrecy or preventing modding.

---

## Zero Runtime Guessing

OTAB deliberately avoids:

* File watchers
* Auto-reloading
* Hot-swapping configs
* Implicit fallbacks

Why?

Because predictable behavior beats convenience when running a real server.

If something changes:  
→ restart  
→ everything is consistent again  

---

## Designed for Long-Term Worlds

OTAB is especially suited for:

* Long-running servers
* Roleplay worlds
* Modpacks
* Carefully balanced progression
* Admin-curated experiences

It gives you **tools**, not shortcuts.

---

## In Short

OTAB is for you if you want:

* Full control instead of magic behavior
* Clear rules instead of surprises
* Performance instead of convenience hacks
* Living creatures instead of scripted machines
* A system you can reason about

It is not a “press button → everything tameable” mod.

It is a **framework for intentional creature design**.

---

**Now it’s time to write your own story of taming and breeding.**
