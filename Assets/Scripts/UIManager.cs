using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per ricaricare le scene

public class UIManager : MonoBehaviour
{
    [Header("Impostazioni Tasti (Opzionale)")]
    public KeyCode tastoRicarica = KeyCode.F5;
    public KeyCode tastoSbloccaMouse = KeyCode.Tab;

    void Update()
    {

        // Sblocca il mouse se premi il tasto assegnato (utile per debug o menù)
        if (Input.GetKeyDown(tastoSbloccaMouse))
        {
            SbloccaMouse();
        }
    }

    // --- METODI PUBBLICI DA ASSEGNARE AI PULSANTI DELLA UI ---

    public void Carica()
    {
        // Riporta il tempo alla normalità (nel caso il gioco fosse in pausa con Time.timeScale = 0)
        Time.timeScale = 1f;

        // Opzionale: Resetta i lucchetti del MagLockManager se serve
        MagLockManager.minigiochiCompletatiGlobali = 0; 

        // Carica la scena attualmente attiva
        SceneManager.LoadScene("Prologue");
        
        Debug.Log("Livello Ricaricato!");
    }
    public void Retry()
    {
        // Riporta il tempo alla normalità (nel caso il gioco fosse in pausa con Time.timeScale = 0)
        Time.timeScale = 1f;

        // Opzionale: Resetta i lucchetti del MagLockManager se serve
        MagLockManager.minigiochiCompletatiGlobali = 0; 

        // Carica la scena attualmente attiva
        SceneManager.LoadScene("Livello");
        
        Debug.Log("Livello Ricaricato!");
    }

    public void EsciDalGioco()
    {
        Debug.Log("Uscita dal gioco in corso...");
        
        // Chiude l'applicazione (Funziona solo nel gioco buildato/esportato)
        Application.Quit();

        // Questa riga ferma il gioco anche mentre lo stai testando nell'Editor di Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void MainMenu()
    {
        Debug.Log("Uscita dal gioco in corso...");
        
        SceneManager.LoadScene("MainMenu");
    }

    public void SbloccaMouse()
    {
        // Rende il mouse visibile
        Cursor.visible = true;
        // Scollega il mouse dal centro dello schermo (ferma il tracking)
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("Mouse Sbloccato");
    }

    public void BloccaMouse()
    {
        // Nasconde il mouse
        Cursor.visible = false;
        // Blocca di nuovo il mouse al centro per il tracking della visuale
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("Mouse Bloccato");
    }
}