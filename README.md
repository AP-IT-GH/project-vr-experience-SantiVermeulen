# Tutorial: AI-Racer in VR met Unity ML-Agents

**Velocity Duel VR** is een virtual reality racegame waarin een AI-agent, getraind met Reinforcement Learning, leert om een uitdagende en dynamische tegenstander te worden voor een menselijke speler.

## Inleiding

Virtual Reality biedt een ongeëvenaarde immersie voor racegames, maar de ervaring wordt vaak beperkt door voorspelbare, gescripte AI-tegenstanders. 
Dit project doorbreekt die conventie door gebruik te maken van Unity's ML-Agents om een PPO-agent (Proximal Policy Optimization) te trainen. 
De AI leert niet alleen de optimale racelijn, 
maar ook emergent gedrag zoals het ontwijken van obstakels en het strategisch gebruiken van een turbo. Het resultaat is een tegenstander 
die zich aanpast en elke race uniek maakt, wat zorgt voor een dynamische en herspeelbare ervaring.

Aan het einde van deze tutorial heeft u een werkende Unity-omgeving opgezet waarin een ML-Agents PPO-model is geconfigureerd en getraind. 
U zult begrijpen hoe u de observaties, acties en het beloningssysteem definieert om een agent te leren racen op een circuit. Tot slot kunt u de trainingsresultaten analyseren met TensorBoard en 
heeft u een functioneel prototype van een adaptieve race-AI.

## Methoden

### Installatie

Voor dit project werden de volgende softwareversies gebruikt. De installatie volgt de standaardprocedures.

*   **Unity:** 2021.3.16f1 (LTS)
*   **Unity ML-Agents:** Release 19 (`com.unity.ml-agents` v2.3.0-exp.3)
*   **Python:** 3.8.10
*   **ML-Agents Python package (`mlagents`):** 0.30.0
*   **TensorFlow:** 2.10.0

### Verloop van de simulatie of het spel

De flow van een enkele race is als volgt:

1.  **Grid-Start:** De speler en de AI-agent spawnen op naastliggende startposities.
2.  **Countdown:** Een `RaceManager` script initieert een 3-2-1-GO countdown. Bij "GO" worden de besturing en dynamische obstakels (zoals uitschuivende slagbomen) geactiveerd.
3.  **Race Loop:**
    *   **Speler:** Bestuurt het voertuig via VR-controllers (thumbstick voor sturen, triggers voor gas/rem).
    *   **AI-agent:** Neemt autonoom beslissingen op basis van zijn observaties om te sturen, accelereren, remmen en een turbo te gebruiken op optimale momenten.
4.  **Finish:** Wanneer een racer de finish-lijn passeert na het voltooien van het vereiste aantal rondes, eindigt de race.
5.  **Reset:** De omgeving wordt gereset voor een nieuwe race. Tijdens training gebeurt dit automatisch na een finish, een crash of een timeout.

### Observaties, Acties en Beloningen

| Type          | Beschrijving                                                                                                                                                                                                                                                                                                        |
| :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Observaties** | **Vector (7) + Raycasts (30):**<br> • **Raycasts**: 30 stralen die afstanden tot de circuitranden en obstakels detecteren.<br> • **Vector Observaties**: <br>    - Richting naar volgende checkpoint (X, Z).<br>    - Lokale snelheid van de auto (voorwaarts, zijwaarts).<br>    - Rotatiesnelheid rond de Y-as.<br>    - Vorige acties (stuur, gas/rem) voor een feedback-loop. |
| **Acties**      | **Continue & Discrete (2 Branches):**<br> • **Branch 0 (Continu, size 2):** `[sturen (-1 tot 1), gas/rem (-1 tot 1)]`<br> • **Branch 1 (Discrete, size 2):** `[geen actie, gebruik turbo]`                                                                                                                          |
| **Beloningen**  | • **`+1.0`**: Bij het succesvol passeren van een checkpoint.<br>• **`-1.0`**: Bij een botsing met een muur.<br>• **`-1.0`**: Als de agent van de baan valt (`FallDetector`).<br>• **`-0.5`**: Als de agent te lang vastzit (timeout).<br>• **`+5.0`**: Grote bonusbeloning voor het voltooien van een volledige ronde.<br>• **`+0.01`**: Kleine, continue "breadcrumb" beloning voor het dichterbij komen van het volgende checkpoint.<br>• **`-0.001`**: Kleine, constante straf per stap om snelheid te motiveren. |

### Beschrijving van de objecten

*   **AI Agent (Vehicle):** Een `GameObject` met een `Rigidbody` en `WheelCollider`s voor de fysica. Bevat het `AgentCarController.cs` script, een `Decision Requester` en een `Ray Perception Sensor 3D`.
*   **Player (Vehicle):** Een identiek voertuig, maar bestuurd door een `PlayerController.cs` script dat VR-input verwerkt.
*   **Race Track:** Een 3D-model van het circuit met `Mesh Collider`s die de `Wall` tag hebben.
*   **Checkpoints:** Een reeks onzichtbare `Box Collider`s (ingesteld als `Is Trigger`) die de voortgang van de racers meten.
*   **RaceManager / TrackCheckpoints:** Een centraal script dat de voortgang van alle racers bijhoudt, de ranglijst bepaalt en de race-logica beheert.

### Beschrijvingen van de gedragingen van de objecten

*   **`AgentCarController.cs`:** Dit script, dat erft van `Agent`, is het brein van de AI. Het verzamelt observaties (`CollectObservations`), vertaalt de beslissingen van het neuraal netwerk naar fysieke krachten op de wielen (`OnActionReceived`), en verwerkt beloningen en straffen.
*   **`TrackCheckpoints.cs`:** Dit script beheert een lijst van alle checkpoints. Het registreert elke auto (speler en AI) en houdt per auto bij welk checkpoint de volgende is en in welke ronde ze zitten. Dit script is de "scheidsrechter" van de race.
*   **`PlayerController.cs`:** Leest de input van de VR-controllers via Unity's Input System en past de corresponderende krachten toe op de `WheelCollider`s van de spelerauto.

### Afwijkingen van de one-pager

In de one-pager werd een geavanceerd *Self-play curriculum* en een adaptieve moeilijkheidsgraad op basis van een ELO-systeem voorgesteld. Voor deze implementatie is de focus gelegd op het creëren van één robuuste, capabele *single-agent*. De AI traint dus op een statisch circuit om eerst de fundamentele vaardigheid van het racen te leren. Het self-play en de ELO-rating zijn complexe vervolgstappen die een solide basis-agent vereisen, wat het doel van deze tutorial is.

## Resultaten

De agent werd getraind voor 20 miljoen stappen op een complex circuit. De resultaten werden gemonitord via TensorBoard.

![Cumulative Reward over time](https://i.imgur.com/gK6kS8k.png "Cumulative Reward")
*Figuur 1: Cumulatieve Beloning. De gemiddelde beloning per episode stijgt significant naarmate de training vordert, met een duidelijke doorbraak rond 5 miljoen stappen en een afvlakking richting de 20 miljoen stappen.*

![Episode Length over time](https://i.imgur.com/L79EayV.png "Episode Length")
*Figuur 2: Episodelengte. De gemiddelde lengte van een episode neemt gestaag af, wat aangeeft dat de agent de race sneller en efficiënter leert voltooien.*

### Beschrijving van de Tensorboard grafieken

*   **Environment / Cumulative Reward:** Deze grafiek toont de gemiddelde totale beloning die de agent per trainingsronde (episode) verzamelt. De duidelijke stijgende trend, gevolgd door een plateau, indiceert een succesvol leerproces. De agent leert om acties die tot een hogere beloning leiden (zoals checkpoints halen en finishen) te verkiezen boven acties die tot straf leiden (botsen, vastzitten).
*   **Environment / Episode Length:** Deze grafiek toont het gemiddelde aantal stappen dat een agent nodig heeft om een episode te voltooien. Omdat een snellere ronde minder stappen vereist, is de dalende trend een sterk positief signaal. Het toont aan dat de agent niet alleen de baan leert volgen, maar dit ook steeds sneller doet.

### Opvallende waarnemingen tijdens het trainen

In de vroege fase (0-3M stappen) vertoonde de agent chaotisch gedrag en kon hij nauwelijks op de baan blijven. Een eerste doorbraak was zichtbaar rond 5 miljoen stappen, waar de agent voor het eerst consistent rondes begon te voltooien, zij het erg langzaam en hoekig. De grootste verbetering vond plaats tussen 10 en 20 miljoen stappen, waar de agent emergent gedrag vertoonde: hij leerde om voor scherpe bochten af te remmen en de ideale racelijn te benaderen, een gedrag dat niet expliciet was geprogrammeerd maar voortkwam uit het beloningssysteem. Het toevoegen van de "breadcrumb" beloning (voor het naderen van een checkpoint) bleek cruciaal om de initiële, willekeurige exploratiefase te doorbreken.

## Conclusie

In dit project hebben we succesvol een PPO-agent getraind om autonoom in een VR-omgeving te racen door een beloningssysteem en een set van relevante observaties te ontwerpen. De resultaten tonen aan dat de agent niet alleen leerde de baan te voltooien, maar zijn prestaties ook significant optimaliseerde over miljoenen trainingsstappen. Het eindmodel is in staat om snelle rondetijden neer te zetten en vormt een competente tegenstander.

Dit project demonstreert de kracht van Reinforcement Learning voor het creëren van dynamische en geloofwaardige NPC's in games. In plaats van een vast, voorspelbaar pad te volgen, ontwikkelde de agent een eigen, effectieve rijstijl op basis van de gestelde regels en beloningen. Dit opent de deur naar rijkere en meer herspeelbare game-ervaringen, waarbij de uitdaging zich organisch aanpast aan de omgeving.

Voor de toekomst zijn er diverse verbeteringen mogelijk. De meest voor de hand liggende stap is de implementatie van het oorspronkelijk geplande self-play curriculum, waarbij de agent tegen oudere versies van zichzelf racet om defensieve en offensieve strategieën te leren. Daarnaast kan een meer geavanceerd voertuigfysica-model en het toevoegen van meer dynamische obstakels, zoals andere voertuigen, de complexiteit en het realisme verder verhogen, wat leidt tot een nog intelligentere en meer menselijke AI-rivaal.

## Bronvermelding

De algemene opzet van de ML-Agents implementatie is gebaseerd op de principes en voorbeelden uit de officiële Unity ML-Agents documentatie en de Obelix tutorial van de cursus.

Juliani, A., Berges, V., Vckay, E., Gao, Y., Henry, H., Mattar, M., & Lange, D. (2020). Unity: A general platform for intelligent agents. *arXiv preprint arXiv:1809.02627*. Geraadpleegd van https://github.com/Unity-Technologies/ml-agents
