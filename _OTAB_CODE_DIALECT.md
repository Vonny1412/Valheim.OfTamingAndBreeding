# OTAB – Valheim Code Dialect Guidelines

OTAB intentionally follows **Valheim's gameplay-code dialect** where practical.

---

## Core Principles

### 1. Prefer explicit code over abstraction-heavy solutions

Gameplay logic should stay close to where it is used.

---

### 2. Prefer early returns and early continues

Keep control flow flat instead of deeply nested.

---

### 3. Use named arguments for optional parameters and unclear bool arguments

Example:

```csharp
RandomMovement(dt, centerPoint, snapToGround: true);
SetAlerted(alert: true);
AvoidFire(dt, null, superAfraid: true);
```

---

### 4. Cache frequently used references locally

Especially:

* transforms
* positions
* components
* repeated field access

---

### 5. Cache Unity components once when they are used repeatedly

Prefer `Awake()`-time caching over repeated `GetComponent()` calls in runtime loops.

Example:

```csharp
private void Awake()
{
    m_nview = GetComponent<ZNetView>();
    m_character = GetComponent<Character>();
}
```

---

### 6. Prefer small, project-specific helpers over generic utility abstraction

Helpers should solve recurring **Valheim-like problems**, not create frameworks.

---

### 7. Keep runtime data structures simple

Avoid unnecessary architectural complexity for gameplay data.

---

### 8. Prefer gameplay-oriented names over abstract API names

Examples:

* `HuntPlayer`
* `AvoidFire`
* `IdleMovement`
* `StayNearSpawn`

---

### 9. Accept small repetition when it improves immediate readability

Do not abstract too early.

---

### 10. Write code that is easy to inspect while debugging live gameplay behavior

Clarity of behavior is more important than theoretical elegance.

---

## Gameplay Math Conventions

### 11. Prefer XZ distance for gameplay logic

Valheim often ignores vertical distance for gameplay decisions.

This is especially common for:

* creature AI
* item search
* aggro checks
* movement decisions

Example:

```csharp
float dist = Utils.DistanceXZ(posA, posB);
```

This avoids unwanted vertical influence (e.g. cliffs, terrain height).

---

### 12. Prefer squared distances for comparisons

When only comparing distances, avoid square roots.

Example:

```csharp
float dx = a.x - b.x;
float dz = a.z - b.z;
float distSqr = dx * dx + dz * dz;

if (distSqr < range * range)
{
    // inside range
}
```

This avoids unnecessary `Mathf.Sqrt()` calls in frequently executed code.

---

## OTAB Interpretation

If a piece of code can be made:

* **shorter but less obvious**, or
* **slightly more repetitive but easier to read in-place**

prefer the second option.

The goal is **not “modern C# purity”**.

The goal is to **sound like Valheim**.

---

## Additional OTAB Note

When OTAB extends vanilla behavior, prefer solutions that:

* reuse vanilla logic
* preserve vanilla flow
* override only the decision point that must change

Do **not replace a full vanilla system** when changing only one input or condition is enough.

---
