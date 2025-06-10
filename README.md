# Tutorial: AI-Racer in VR met Unity ML-Agents

**The Perfect Lap** is een virtual reality racegame waarin een AI-agent, getraind met Reinforcement Learning, leert om een uitdagende tegenstander te worden voor een menselijke speler.

## Inleiding

Virtual Reality biedt natuurlijk ongelofelijk veel mogelijkheden voor singleplayer racing games. Wij gaan ervoor zorgen dat een player tegen een AI kan racen, dit doen we door een Unity ML-agent te maken om een PPO-agent te trainen. De AI gaat zo goed mogelijk proberen om de perfecte racelijn te leren en de perfecte lap neer te zetten die jij dan gaat moeten verslagen.

Aan het einde van deze tutorial heeft u een werkende Unity-omgeving opgezet waarin een ML-Agents PPO-model is geconfigureerd en getraind.
U zult begrijpen hoe u de observaties, acties en het beloningssysteem definieert om een agent te leren racen op een circuit. Tot slot kunt u de trainingsresultaten analyseren met TensorBoard en
heeft u een functioneel prototype van een adaptieve race-AI.

## Methoden

### Installatie

Voor dit project werden de volgende softwareversies gebruikt. De installatie volgt de standaardprocedures.

- **Unity:** 6000.0.36f1
- **Unity ML-Agents:** 3.0.0
- **Python:** 3.9.18
- **ML-Agents Python package:** 0.30.0
- **PyTorch:** 1.7.1
- **Protobuf:** 3.20

### Verloop van de simulatie of het spel

De flow van een enkele race is als volgt:

1.  **Grid-Start:** De speler en de AI-agent spawnen op naastliggende startposities.
2.  **Countdown:** Een `RaceManager` script initieert een 3-2-1-GO countdown. Bij "GO" worden de autos geactiveerd.
3.  **Race Loop:**
    - **Speler:** Bestuurt het voertuig via VR-controllers (thumbstick voor sturen, triggers voor gas/rem).
    - **AI-agent:** Neemt autonoom beslissingen op basis van zijn observaties om te sturen, accelereren en remmen.
4.  **Finish:** Wanneer een racer de finish-lijn passeert na het voltooien van het vereiste aantal rondes, eindigt de race.
5.  **Reset:** De omgeving wordt gereset voor een nieuwe race. Tijdens training gebeurt dit automatisch na een finish of een timeout.

### Observaties, Acties en Beloningen

| Type            | Beschrijving                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Observaties** | **Vector (7) + Raycasts (30):**<br> • **Raycasts**: 30 stralen die afstanden tot de circuitranden en obstakels detecteren.<br> • **Vector Observaties**: <br> - Richting naar volgende checkpoint (X, Z).<br> - Lokale snelheid van de auto (voorwaarts, zijwaarts).<br> - Rotatiesnelheid rond de Y-as.<br> - Vorige acties (stuur, gas/rem) voor een feedback-loop.                                                                                                                                                |
| **Acties**      | **Continuous Actions** `[sturen, gas/rem]`                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| **Beloningen**  | • **`+1.0`**: Bij het succesvol passeren van een checkpoint.<br>• **`-1.0`**: Bij een botsing met een muur.<br>• **`-1.0`**: Als de agent van de baan valt (`FallDetector`).<br>• **`-0.5`**: Als de agent te lang vastzit (timeout).<br>• **`+5.0`**: Grote bonusbeloning voor het voltooien van een volledige ronde.<br>• **`+0.01`**: Kleine, continue "breadcrumb" beloning voor het dichterbij komen van het volgende checkpoint.<br>• **`-0.001`**: Kleine, constante straf per stap om snelheid te motiveren. |

### Beschrijving van de objecten

- **AI Agent (Vehicle):** Een `GameObject` met een `Rigidbody` en `WheelCollider`s voor de fysica. Bevat het `AgentCarController.cs` script, een `Decision Requester` en een `Ray Perception Sensor 3D`.
- **Player (Vehicle):** Een identiek voertuig, maar bestuurd door een `PlayerController.cs` script dat VR-input verwerkt.
- **Race Track:** Een 3D-model van het circuit met `Mesh Collider`s.
- **Checkpoints:** Een reeks onzichtbare `Box Collider`s (ingesteld als `Is Trigger`) die de voortgang van de racers meten.
- **RaceManager / TrackCheckpoints:** Een centraal script dat de voortgang van alle racers bijhoudt, de ranglijst bepaalt en de race-logica beheert.

### Beschrijvingen van de scripts

- **`AgentCarController.cs`:** Dit script, dat erft van `Agent`, is het brein van de AI. Het verzamelt observaties (`CollectObservations`), vertaalt de beslissingen van het neuraal netwerk naar fysieke krachten op de wielen (`OnActionReceived`), en verwerkt beloningen en straffen.
- **`TrackCheckpoints.cs`:** Dit script beheert een lijst van alle checkpoints. Het registreert elke auto (speler en AI) en houdt per auto bij welk checkpoint de volgende is en in welke ronde ze zitten. Dit script is de "scheidsrechter" van de race.
- **`VRCarController.cs`:** Leest de input van de VR-controllers via Unity's Input System en past de corresponderende krachten toe op de `WheelCollider`s van de spelerauto.

### Afwijkingen van de one-pager

- Dynamische obstakels
- Turbo

## Resultaten

De agent werd getraind voor 2.938.700 stappen bij level 1 en 6.764.400 stappen bij level 2.

[Screenshot van tensorboard](./tensorboard.png)

### Beschrijving van de Tensorboard grafieken

De grafiek toont de "Environment/Episode Length" over tijd (aantal stappen). Er zijn twee trainingsruns weergegeven: "TrainingOvernight_Level2_v11/RacerLvL2" (blauw/Level 2) en "TrainingOvernight_v3/Racer" (grijs/Level 1). De y-as (Environment/Episode Length) geeft aan hoe lang een episode duurt, wat in de context van reinforcement learning vaak gerelateerd is aan hoe goed de agent presteert. De x-as representeert het aantal trainingsstappen (steps).

### Opvallende waarnemingen tijdens het trainen

#### Level 1 (grijs).

Bij de racer van lvl 1 zien we dat het allemaal een stuk vlotter gaat (natuurlijk omdat het de map een stuk gemakkelijker is).

#### Level 2 (blauw)

We zien dat de RacerLVL2 in het begin veel meer moeite heeft met op gang komen, maar naargelang de tijd word hij wel stabieler en stabieler.

## Conclusie

In dit project hebben we succesvol een PPO-agent getraind om autonoom in een VR-omgeving te racen door een beloningssysteem en een set van relevante observaties te ontwerpen. De resultaten tonen aan dat de agent niet alleen leerde de baan te voltooien, maar zijn prestaties ook significant optimaliseerde over miljoenen trainingsstappen. Het eindmodel is in staat om snelle rondetijden neer te zetten en vormt een competente tegenstander.

Dit project demonstreert de kracht van Reinforcement Learning voor het creëren van geloofwaardige NPC's in games. In plaats van een vast, voorspelbaar pad te volgen, ontwikkelde de agent een eigen, effectieve rijstijl op basis van de gestelde regels en beloningen.

## Bronvermelding

https://www.youtube.com/watch?v=DU-yminXEX0
