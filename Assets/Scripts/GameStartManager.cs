using UnityEngine;
using System.Collections;
using TMPro; 

public class GameStartManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    // De tijd in seconden voordat de race start
    [SerializeField] private float countdownTime = 3.0f;

    // Een lijst met alle auto's die bestuurd moeten worden
    private CarController playerCar;
    private AgentCarController aiCar;

    void Start()
    {
        // Zoek de auto's in de scene
        playerCar = FindFirstObjectByType<CarController>();
        aiCar = FindFirstObjectByType<AgentCarController>();

        // Zorg ervoor dat de besturing is uitgeschakeld bij de start
        if (playerCar != null) playerCar.canControl = false;
        if (aiCar != null) aiCar.canControl = false;

        // Start de countdown coroutine
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        // Wacht een seconde voordat de countdown begint
        yield return new WaitForSeconds(1f);

        float currentTime = countdownTime;
        while (currentTime > 0)
        {
            // Toon het afgeronde getal (3, 2, 1)
            countdownText.text = Mathf.CeilToInt(currentTime).ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        // Toon "GO!"
        countdownText.text = "GO!";

        // Geef de besturing vrij!
        if (playerCar != null) playerCar.canControl = true;
        if (aiCar != null) aiCar.canControl = true;

        // Wacht nog een seconde en verberg dan de tekst
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}