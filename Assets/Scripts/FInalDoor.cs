using UnityEngine;

public class FinalDoor : MonoBehaviour
{
    [Header("Sprite della Porta")]
    [Tooltip("L'oggetto con la grafica della porta CHIUSA")]
    public GameObject spritePortaChiusa;
    [Tooltip("L'oggetto con la grafica della porta APERTA")]
    public GameObject spritePortaAperta;

    [Header("Luci della Porta")]
    public GameObject[] luciRosse;
    public GameObject[] luciVerdi;

    // Variabili di memoria e stato
    private int minigiochiPrecedenti = -1;
    private bool isPlayerInside = false;
    private bool isPortaAperta = false; // Ricorda se abbiamo già aperto la porta

    private void Start()
    {
        // Stato iniziale: porta chiusa visibile, porta aperta nascosta
        if (spritePortaAperta != null) spritePortaAperta.SetActive(false);
        if (spritePortaChiusa != null) spritePortaChiusa.SetActive(true);
    }

    private void Update()
    {
        int completatiAttuali = MagLockManager.minigiochiCompletatiGlobali;

        // 1. Aggiorna le luci se il punteggio globale cambia
        if (completatiAttuali != minigiochiPrecedenti)
        {
            AggiornaLuci(completatiAttuali);
            minigiochiPrecedenti = completatiAttuali;
        }

        // 2. Controlla l'apertura automatica
        // Se il player è nel trigger, la porta è chiusa, e ci sono 3 lock risolti
        if (isPlayerInside && !isPortaAperta && completatiAttuali >= 3)
        {
            ApriPorta();
        }
    }

    private void AggiornaLuci(int completati)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < luciRosse.Length && i < luciVerdi.Length)
            {
                if (i < completati)
                {
                    if (luciRosse[i] != null) luciRosse[i].SetActive(false);
                    if (luciVerdi[i] != null) luciVerdi[i].SetActive(true);
                }
                else
                {
                    if (luciRosse[i] != null) luciRosse[i].SetActive(true);
                    if (luciVerdi[i] != null) luciVerdi[i].SetActive(false);
                }
            }
        }
    }

    private void ApriPorta()
    {
        isPortaAperta = true;

        // Cambia gli sprite
        if (spritePortaChiusa != null) spritePortaChiusa.SetActive(false);
        if (spritePortaAperta != null) spritePortaAperta.SetActive(true);

        Debug.Log("Hai aperto la porta automaticamente!");
        
        // (Opzionale) Se la porta aveva un BoxCollider separato che bloccava 
        // fisicamente il passaggio, potresti disabilitarlo qui.
        // Esempio: GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer"))
        {
            isPlayerInside = false;
        }
    }
}