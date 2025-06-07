# Velocity Duel VR - Tutorial

## Inleiding

Velocity Duel VR is een adaptief VR-racingspel waarbij een AI-agent leert racen tegen een menselijke speler. Het project combineert Virtual Reality technologie met Reinforcement Learning om een dynamische en uitdagende race-ervaring te creëren.

In deze tutorial leer je hoe je een VR-racingspel kunt opzetten met een AI-agent die zich aanpast aan het niveau van de speler. Je leert de basis van Unity VR development, het implementeren van een PPO-agent met Unity ML-Agents, en hoe je deze twee componenten kunt integreren tot een coherente race-ervaring.

## Methoden

### Installatie
- Unity 2022.3 LTS
- Unity ML-Agents 2.3.0
- XR Interaction Toolkit 2.5.2
- Python 3.9.7
- PyTorch 2.0.1

### Projectstructuur

#### Core Components
1. **Race Manager**
   - Beheert race-status en events
   - Controleert checkpoints en rondetijden
   - Activeert dynamische obstakels
   - Reset mechanisme bij val van de baan

2. **VR Player Controller**
   - Head-tilt voor g-force simulatie
   - Thumb-stick voor sturen
   - Triggers voor gas/rem
   - Automatische respawn bij val van de baan

3. **AI Agent (PPO)**
   - Observations: 30 raycasts + rigid-body state
   - Actions: steer, accel, brake, turbo
   - Reward shaping voor rubber-banding

### AI Training Setup

#### Observations
- 30 raycasts voor omgevingsperceptie
  - 10 raycasts vooruit (verschillende hoeken)
  - 10 raycasts links
  - 10 raycasts rechts
  - Raycast lengte: 20 meter
  - Detecteert muren, obstakels en bochten
- Rigid-body state (velocity, position, rotation)
- Track checkpoints status
  - Volgende checkpoint positie
  - Afstand tot volgende checkpoint
  - Hoek tussen huidige richting en checkpoint
- Obstacle positions

#### Track Layout
- Checkpoints op strategische punten:
  - Begin van elke bocht
  - Midden van scherpe bochten
  - Einde van elke bocht
- Checkpoint radius: 5 meter
- Visuele markers voor AI training:
  - Rode lijnen voor bocht-waarschuwing
  - Groene markers voor ideale race-lijn

#### Corner Detection
- Raycast pattern voor bocht-detectie:
  - Dichte raycast cluster vooruit (5 raycasts)
  - Verspreide raycasts naar zijkanten
  - Langere raycasts in bochten
- Bocht-classificatie:
  - Scherpe bocht: > 90 graden
  - Normale bocht: 45-90 graden
  - Lichte bocht: < 45 graden

#### Actions
- Steering: [-1, 1] continuous
- Acceleration: [0, 1] continuous
- Brake: [0, 1] continuous
- Turbo: binary

#### Rewards
- Base reward: 0.1 per frame
- Checkpoint bonus: +1.0
- Collision penalty: -0.5
- Off-track penalty: -0.3 (triggered when vehicle height drops below track level)
- Reset penalty: -1.0 (when vehicle falls off track and needs to respawn)
- Corner handling bonus: +0.2 (for smooth cornering)
- Speed bonus: proportional to velocity
- Rubber-banding: dynamic scaling based on player-AI gap

### Training Process
1. Curriculum learning met zelf-play
   - Eerst leren van rechte stukken
   - Dan lichte bochten
   - Vervolgens scherpe bochten
   - Tot slot complete circuits
2. Progressive difficulty scaling
3. Behavior cloning from human demonstrations
4. Fine-tuning with PPO
5. Reset handling:
   - Vehicle respawns at last checkpoint
   - Velocity reset to zero
   - Brief pause before continuing (2 seconds)
   - Additional penalty to discourage falling off

## Resultaten

### Training Metrics
- Average reward per episode
- Success rate in completing laps
- Collision frequency
- Average speed maintained

### Performance Analysis
- AI adaptation to player skill level
- Learning curve progression
- Behavior diversity in racing lines

## Conclusie

Velocity Duel VR demonstreert de effectieve integratie van VR technologie met reinforcement learning voor het creëren van een adaptieve race-ervaring. De PPO-agent toont aan dat het mogelijk is om een AI te trainen die zowel uitdagend als eerlijk is tegenover menselijke spelers.

De resultaten tonen dat de agent succesvol verschillende race-strategieën kan leren en zich kan aanpassen aan het niveau van de speler. De rubber-banding mechanisme zorgt voor spannende races zonder frustrerende moeilijkheidsgraden.

### Toekomstig Werk
- Implementatie van meerdere AI-personalities
- Uitbreiding van de observatie-ruimte met meer sensorische input
- Optimalisatie van de reward functie voor meer natuurlijk gedrag
- Integratie van meer geavanceerde VR-interacties

## Bronnen
- Unity ML-Agents Documentation (2023)
- Unity XR Interaction Toolkit Documentation (2023)
- Schulman, J., et al. (2017). Proximal Policy Optimization Algorithms 