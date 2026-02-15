![Of Taming and Breeding](https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/blob/main/_Assets/banner.png)

# Of Taming and Breeding (OTAB)

**Of Taming and Breeding (OTAB)** is a mod for **Valheim** that provides a  
**data-driven framework for taming, breeding and creature behavior**.

It is designed for players and server administrators who want  
**more control**, **clearer rules**, and **stable long-term worlds**  
without relying on heavy automation or global overhauls.

OTAB does not try to “make everything tameable”.  
Instead, it focuses on **intentional, configurable creature behavior**.

---

> **Configuration & Documentation**
>  
> OTAB exposes many systems through configuration and data files.  
> Detailed setup instructions, examples and design explanations are available in the Wiki:
>  
> 👉 https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/wiki

---

## What OTAB Is (and Is Not)

**OTAB aims to be:**

* Server-authoritative where possible
* Explicit in configuration and data
* Focused on controlled taming and reproduction
* Suitable for curated or long-running worlds

**OTAB is not:**

* A “make everything tameable” mod
* A riding or mount system
* A one-click gameplay shortcut

---

## Ecosystem & Mod Compatibility

OTAB is designed to integrate cleanly into existing Valheim mod setups and provides  
**optional integration points** for selected mods, without bundling or replacing them:

* **Wacky’s Database** – late recipe registration and data-driven items
* **Creature Level & Loot Control (CLLC)** – lifecycle-aware trait and effect inheritance
* **Seasons** – adaptive handling of modified durations

Each integration is optional and explicitly scoped.

A full compatibility overview (compatible / incompatible / untested mods)  
is maintained in the Wiki.

---

## A Note on AllTameable & Similar Mods

OTAB is **not intended as a replacement** for mods like  
**AllTameable** or **AllTameableTamingOverhaul**.

Those mods primarily focus on:

* Making many or all creatures tameable
* Minimal setup
* Immediate gameplay changes

OTAB takes a different approach.

Rather than enabling taming globally, OTAB is built around:

* Respecting vanilla creature identity and progression
* Explicit acquisition paths (items, eggs, offspring, growth)
* Partner- and offspring-dependent reproduction logic

If you want a fast “everything is tameable” experience,  
AllTameable-style mods are a great fit.

If you want **designed ecosystems with controlled reproduction paths**,  
OTAB is built for that.

---

## Core Systems (Overview)

OTAB replaces parts of Valheim’s implicit behavior with **configurable systems**, such as:

* Intentional taming rules (e.g. avoiding accidental tames)
* Component-based creature behavior control
* Extended procreation logic with partner binding
* Optional self-breeding and sibling chains
* Offspring growth and lifecycle handling
* Egg-based reproduction with environmental requirements

The exact behavior depends on configuration and data definitions.

---

## Server vs Client Model

**Server:**

* Loads YAML data
* Owns gameplay logic
* Synchronizes world data to clients

**Client:**

* Receives synchronized data
* Displays related information
* Does not decide gameplay logic

This separation helps reduce desyncs and keeps multiplayer behavior consistent.

---

## Designed with Long-Term Worlds in Mind

OTAB is especially suited for:

* Long-running servers
* Roleplay or lore-driven worlds
* Modpacks with curated progression
* Admin-managed ecosystems

It provides **tools**, not shortcuts.

---

**Now it’s time to write your own story of taming and breeding.**

---

*OTAB is also a personal learning project.  
Many systems evolved through experimentation and iteration while exploring Valheim’s internals.  
Stability, clarity and long-term maintainability are guiding goals — not absolute guarantees.*

*Created with ♥️*
