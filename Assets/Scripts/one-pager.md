# Velocity Duel VR – Adaptive Rival Racing

### Team

| Naam  
| -----------------
| Santi Vermeulen  
| Bas De Jongh
| Steven Van Rossum

---

## Logica (draaiboek)

1. **Grid-Start** – Speler en AI-rivaal spawnen op naast­liggende start­plekken op een racing track.
2. **Countdown** – 3-2-1-GO; race-manager activeert dynamische obstakels bv: (speed-pads, Uitschuivende slagbomen(balken die kort open- en dichtklappen op de baan)).
3. **Race Loop** –
   - **Player-lane**: speler bestuurt cockpit via head tilt (g-force look) + thumb-stick / triggers.
   - **AI-lane**: PPO-agent neemt bochten, ontwijkt drones en gebruikt turbo op optimale punten.
   - **Rubber-band AI**: reward shaping houdt de kloof spannend maar fair.
4. **Finish** – HUD toont rondetijden, hit-count, skill-rating update; replay van mooiste inhaal­actie.
5. **Adaptieve moeilijkheid** – volgende track-layout & AI-skillschaling gebaseerd op ELO-verschil.

---

## Waarom AI?

- **Dynamische tegenstander** in plaats van statische ghost; past zich aan spelers­niveau aan.
- **Gedrags­diversiteit**: agent leert driftlijnen, slipstreamen, agressief inhalen.
- **Type agent** – _Single-Agent Reinforcement Learning_ (PPO in Unity ML-Agents) + _Self-play curriculum_ (agent racet tegen oudere checkpoints om strategieën te verfijnen).

---

## Waarom VR?

- **Diepte & snelheids­perceptie** maken bochten en drift realistisch – flatscreen mist de vestibulaire cues.
- **Cockpit-immersion** vergroot flow-gevoel; haptische feedback (controller rumble) versterkt bocht-g-force.

---

## Interactie

| Actor        | Input                                                | Output                                     |
| ------------ | ---------------------------------------------------- | ------------------------------------------ |
| Speler       | Thumb-stick sturen, triggers gas/rem, head-tilt lean | Audio cues, haptics, HUD rondetijd         |
| AI-agent     | Observations: 30 raycasts + rigid-body state         | Acties: `steer`, `accel`, `brake`, `turbo` |
| Race-manager | Lap-triggers, checkpoint events                      | Obstakel-spawning, moeilijkheids­tuning    |

---
