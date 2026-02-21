![Of Taming and Breeding](https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/blob/main/_Assets/banner.png)

# Of Taming and Breeding (OTAB)

Ever wanted a tamed Abomination walking by your side?  
Pettable offspring?  
Lore-compliant lifecycles?  
Or something strange — like a Surtling offspring that might grow up into… coal?  

OTAB makes such systems possible.  

It can be used as plug-and-play.  
It also allows structured customization through server-defined data.  
The more you change — the more responsibility you take.  

---

## Extending Creature Behavior

Creatures in Valheim are more than combat targets.  
They have territory, instincts and limits.  

**Of Taming and Breeding** introduces structured control over:  

* how creatures are tamed  
* how reproduction works  
* how offspring grow  
* which limitations apply  
* which environmental conditions matter  

OTAB ships with preconfigured YAML data balanced for vanilla Valheim.  
You can install it and play immediately.  

But you can also modify those rules — carefully and intentionally.  

Not every creature is meant to be tameable.  
Not every reproduction path is meant to be trivial.  
OTAB allows you to define those boundaries explicitly.  

---

## Philosophy

OTAB builds on Valheim’s existing systems.  

It does not attempt to rewrite creature identity or progression.  
Instead, it exposes structured control over mechanics that are otherwise implicit.  

The aim is:  

* to respect vanilla balance and creature roles  
* to make taming and breeding intentional  
* to avoid accidental or automated ecosystems  
* to support long-term world stability  

Creatures should still feel like Valheim creatures —  
not generic pets or production units.  

If you want global “everything is tameable” gameplay shortcuts,  
other mods may be a better fit.  

If you want structured ecosystems with defined rules and limits,  
OTAB is designed for that.  

---

## Configuration & Documentation

All server-side creature definitions and behavior rules are structured in YAML files.  

Behavior is data-driven and explicit.  

Detailed setup instructions, examples and design explanations are available in the Wiki:  

👉 https://github.com/Vonny1412/Valheim.OfTamingAndBreeding/wiki

---

## Technical Model

OTAB follows a server-authoritative design.  

**Server:**  

* Loads YAML data
* Owns gameplay logic
* Synchronizes runtime data to clients

**Client:**  

* Receives synchronized data
* Displays related information
* Does not control gameplay decisions

Clients do not require local YAML files when connecting to a modded server.    
All necessary data is provided by the server.  

If a client connects to a server without OTAB installed,    
the mod switches into a passive compatibility mode    
and does not interfere with vanilla gameplay.  

This separation reduces desync risk and keeps multiplayer behavior consistent.  

---

## Designed for Long-Term Worlds

OTAB is especially suited for:  

* Long-running servers
* Lore-driven environments
* Carefully curated modpacks
* Admin-managed creature ecosystems

It provides structure and control — not shortcuts.  

---

**Now it’s time to write your own story of taming and breeding.**

---

*OTAB is also a personal learning project.  
Many systems evolved through experimentation and iteration while exploring Valheim’s internals.  
Stability, clarity and long-term maintainability are guiding goals — not absolute guarantees.*

*Created with ♥️*
