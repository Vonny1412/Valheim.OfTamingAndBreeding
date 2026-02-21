![Of Taming and Breeding](https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/blob/main/_Assets/banner.png)

# Of Taming and Breeding (OTAB)

Ever wanted a tamed Abomination walking by your side?  
Pettable offspring?  
Lore-compliant lifecycles?  
Or something strange — like a Surtling offspring that might grow up into… coal?

OTAB makes such systems possible.

It can be used as plug-and-play.  
It also allows deeper customization through server-defined data.  
The more you change — the more responsibility you take.

---

## What OTAB Does

OTAB expands how creatures in Valheim can be tamed, reproduced and managed.

Creatures are more than combat targets.  
They have territory, instincts and limits.

OTAB introduces explicit control over:

* taming conditions  
* reproduction logic  
* offspring growth  
* environmental requirements  
* behavioral limitations  

The mod ships with balanced default YAML data for vanilla gameplay.  
You can install it and play immediately.

Or you can redefine those rules with intention and control.

Not every creature is meant to be tameable.  
Not every lifecycle is meant to be trivial.

---

## Philosophy

OTAB builds on Valheim’s existing creature systems.

It does not attempt to replace vanilla mechanics or creature identity.  
Instead, it makes hidden systems configurable and transparent.

The aim is to:

* respect balance and progression  
* ensure taming and breeding require thought  
* prevent uncontrolled ecosystem growth  
* maintain stability in persistent worlds  

Creatures should still feel like Valheim creatures —  
not generic pets or production units.

If you prefer global “everything is tameable” gameplay changes,  
other mods may be a better fit.

---

## Configuration & Technical Model

OTAB follows a server-authoritative design.

All creature definitions and behavior rules are structured in YAML files and loaded by the server.

These data files define how taming, breeding and lifecycle systems behave.  
The server owns all gameplay logic and synchronizes the relevant runtime data to connected clients.

OTAB can also be used in singleplayer.  
In that case, your local installation loads and applies its own YAML data.

When connecting to a multiplayer server, however, local data is ignored.  
Only the server’s data definitions are used.

The server is always authoritative.

Clients do not require local YAML files when joining a modded server.  
They receive synchronized data and display related information, but do not control gameplay decisions.

If a client connects to a server without OTAB installed,  
the mod switches into a passive compatibility mode  
and does not interfere with vanilla gameplay.

Detailed setup instructions, configuration examples and design explanations are available in the Wiki:

👉 https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/wiki


---

## Designed for Persistent Worlds

OTAB is especially suited for:

* dedicated multiplayer servers  
* lore-driven environments  
* curated modpacks  
* administrator-managed ecosystems  

It provides structure and control for long-lived worlds.

---

**Now it’s time to write your own story of taming and breeding.**

---

*OTAB is also a personal learning project.  
Many systems evolved through experimentation and iteration while exploring Valheim’s internals.  
Stability, clarity and long-term maintainability are guiding goals — not absolute guarantees.*

*Created with ♥️*
