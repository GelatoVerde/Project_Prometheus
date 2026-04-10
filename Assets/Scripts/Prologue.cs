using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GestoreIntro : MonoBehaviour
{
    [Header("Testi dell'Intro")]
    [Tooltip("Trascina qui le tue 5 scritte nell'ordine in cui devono apparire")]
    public GameObject[] scritte;
    
    [Header("Tempistiche")]
    public float tempoTraScritte = 2f;
    public float tempoAttesaFinale = 3f;
    
    [Header("Transizione")]
    [Tooltip("Crea una Image nera nel Canvas che copra tutto lo schermo e trascinala qui")]
    public Image pannelloNero;
    [Tooltip("Velocità del fade to black (1 = ci mette 1 secondo, 0.5 = 2 secondi, 2 = mezzo secondo)")]
    public float velocitaFade = 1f;
    [Tooltip("Il nome esatto della scena da caricare (es: Level_01)")]
    public string nomeProssimaScena;

    void Start()
    {
        // 1. Spegne tutte le scritte all'avvio per sicurezza
        foreach (GameObject scritta in scritte)
        {
            if (scritta != null) scritta.SetActive(false);
        }
        
        // 2. Assicura che il pannello nero sia trasparente all'inizio
        if (pannelloNero != null)
        {
            Color c = pannelloNero.color;
            c.a = 0f;
            pannelloNero.color = c;
            
            // Attiva il GameObject del pannello nel caso fosse spento
            pannelloNero.gameObject.SetActive(true);
        }

        // 3. Fa partire la sequenza cinematica
        StartCoroutine(Sequenza());
    }

    IEnumerator Sequenza()
    {
        // Mostra le scritte una alla volta
        for (int i = 0; i < scritte.Length; i++)
        {
            if (scritte[i] != null)
            {
                scritte[i].SetActive(true);
            }

            // Se NON è l'ultima scritta aspetta 2 secondi, altrimenti aspetta i 3 secondi finali
            if (i < scritte.Length - 1)
            {
                yield return new WaitForSeconds(tempoTraScritte);
            }
            else
            {
                yield return new WaitForSeconds(tempoAttesaFinale);
            }
        }

        // Inizia il fade to black e aspetta che finisca prima di procedere
        yield return StartCoroutine(FadeToBlack());

        // Carica la scena successiva
        SceneManager.LoadScene(nomeProssimaScena);
    }

    IEnumerator FadeToBlack()
    {
        if (pannelloNero == null)
        {
            Debug.LogWarning("Attenzione: Non hai assegnato il Pannello Nero! Salto il fade.");
            yield break;
        }

        Color c = pannelloNero.color;
        float tempo = 0f;

        // Aumenta l'alpha da 0 (trasparente) a 1 (tutto nero) gradualmente
        while (tempo < 1f)
        {
            tempo += Time.deltaTime * velocitaFade;
            c.a = Mathf.Lerp(0f, 1f, tempo);
            pannelloNero.color = c;
            
            // Aspetta il prossimo frame
            yield return null;
        }
    }
}