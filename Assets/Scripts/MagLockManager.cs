using TMPro;
using UnityEngine;

public class MagLockManager : MonoBehaviour
{
    [Header("UI Condivisa (Canvas)")]
    public GameObject TXTtutorial;
    public GameObject TXTVittoria;
    
    [Header("Lucchetti SPECIFICI per questo PC")]
    public Rotazione2D[] locks;

    private int indiceLockAttuale = 0;
    
    // Variabile globale per contare i minigiochi completati
    public static int minigiochiCompletatiGlobali = 0; 
    
    [HideInInspector] public bool isCompleted = false; 

    // Array per salvare le rotazioni originali di partenza
    private Quaternion[] rotazioniIniziali;
    private bool rotazioniSalvate = false;

    void Start()
    {
        // Salva le rotazioni ESATTE dei lucchetti la prima volta che si avvia la scena
        if (!rotazioniSalvate)
        {
            rotazioniIniziali = new Quaternion[locks.Length];
            for (int i = 0; i < locks.Length; i++)
            {
                rotazioniIniziali[i] = locks[i].transform.rotation;
            }
            rotazioniSalvate = true;
        }
    }

    void Update()
    {
        if (isCompleted) return;

        if (Input.GetKeyDown(KeyCode.Space) && indiceLockAttuale < locks.Length)
        {
            Rotazione2D lockCorrente = locks[indiceLockAttuale];

            if (lockCorrente.inAreaCheck)
            {
                // 1. Ferma la rotazione
                lockCorrente.puoRuotare = false;
                
                // 2. INCASRTA IL PEZZO (Effetto Snap)
                lockCorrente.AllineaAlTrigger();
                
                // 3. Passa al prossimo
                indiceLockAttuale++;

                if (indiceLockAttuale < locks.Length)
                {
                    locks[indiceLockAttuale].puoRuotare = true;
                }
                else
                {
                    // VITTORIA
                    isCompleted = true; 
                    
                    // Incrementiamo PRIMA la variabile, così il testo legge il numero giusto
                    minigiochiCompletatiGlobali++;
                    Debug.Log("Minigiochi completati totali: " + minigiochiCompletatiGlobali);

                    if (TXTtutorial != null) TXTtutorial.SetActive(false);
                    
                    // Aggiorna il testo prima di accenderlo
                    AggiornaTestoVittoria();
                    
                    if (TXTVittoria != null) TXTVittoria.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Errore! Hai premuto spazio fuori dall'area.");
            }
        }
    }

    public void ResetMinigioco()
    {
        indiceLockAttuale = 0;
        isCompleted = false;
        InizializzaMinigioco();
    }

    public void InizializzaMinigioco()
    {
        // 1. Ripristina lo stato fisico dei lucchetti
        for (int i = 0; i < locks.Length; i++)
        {
            locks[i].puoRuotare = false;
            
            if (rotazioniSalvate)
            {
                locks[i].transform.rotation = rotazioniIniziali[i];
            }
        }

        if (locks.Length > 0 && !isCompleted)
        {
            locks[0].puoRuotare = true;
        }

        // 2. GESTIONE DELLA UI CONDIVISA
        if (!isCompleted)
        {
            if (TXTtutorial != null) TXTtutorial.SetActive(true);
            if (TXTVittoria != null) TXTVittoria.SetActive(false);
        }
        else
        {
            if (TXTtutorial != null) TXTtutorial.SetActive(false);
            
            // Assicura che anche riaprendo il PC il testo mostri il numero corretto aggiornato
            AggiornaTestoVittoria();
            
            if (TXTVittoria != null) TXTVittoria.SetActive(true);
        }
    }

    // --- FUNZIONE PER CAMBIARE IL TESTO AGGIORNATA ---
    private void AggiornaTestoVittoria()
    {
        if (TXTVittoria != null)
        {
            // Cerca il componente TextMeshPro sullo stesso oggetto o nei suoi figli
            TextMeshProUGUI testoTMP = TXTVittoria.GetComponent<TextMeshProUGUI>();
            if (testoTMP == null) 
            {
                testoTMP = TXTVittoria.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (testoTMP != null)
            {
                // Calcola i lock rimanenti
                int rimanenti = 3 - minigiochiCompletatiGlobali;
                if (rimanenti < 0) rimanenti = 0; 

                // Controlla se abbiamo finito tutto o se mancano ancora dei lucchetti
                if (rimanenti == 0)
                {
                    testoTMP.text = "ACCESS GRANTED\n0/3 LOCKS REMAINING - MAIN ENTRANCE UNLOCKED";
                }
                else
                {
                    testoTMP.text = "ACCESS GRANTED\n" + rimanenti + "/3 LOCKS REMAINING";
                }
            }
        }
    }
}