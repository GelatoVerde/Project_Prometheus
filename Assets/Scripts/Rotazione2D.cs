using UnityEngine;

public class Rotazione2D : MonoBehaviour
{
    [Header("Impostazioni di Rotazione")]
    public float velocita = 100f;
    public bool sensoOrario = true;
    
    [Header("Allineamento al Successo")]
    [Tooltip("Gradi da aggiungere per renderlo perpendicolare (es: 0, 90, -90).")]
    public float offsetAngolo = 0f;

    [Header("Stato (Gestito dal Manager)")]
    public bool puoRuotare = false;
    public bool inAreaCheck = false;

    private Transform triggerCorrente; // Salva il bersaglio da copiare

    void Update()
    {
        if (!puoRuotare) return;

        float direzione = sensoOrario ? -1f : 1f;
        transform.Rotate(0f, 0f, direzione * velocita * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Check"))
        {
            inAreaCheck = true;
            triggerCorrente = collision.transform; // Memorizza il trigger toccato
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Check"))
        {
            inAreaCheck = false;
            triggerCorrente = null; // Dimentica il trigger quando esce
        }
    }

    // Nuova funzione richiamata dal Manager per l'effetto "Snap"
    public void AllineaAlTrigger()
    {
        if (triggerCorrente != null)
        {
            // Prende la rotazione del trigger sull'asse Z e aggiunge il tuo offset
            Vector3 rotazioneTrigger = triggerCorrente.eulerAngles;
            transform.eulerAngles = new Vector3(0f, 0f, rotazioneTrigger.z + offsetAngolo);
        }
    }
}