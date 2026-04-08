using UnityEngine;
using Unity.Cinemachine; 

public class Computer : MonoBehaviour
{
    [Header("Icona Prossimità (Prompt)")]
    public GameObject uiSprite;

    [Header("Cinemachine Camera")]
    public CinemachineCamera vcam; 

    [Header("Target di Puntamento")]
    public Transform playerTarget;
    public Transform computerTarget;

    [Header("Interfaccia Minigioco")]
    public GameObject minigamesUI;

    [Header("UI Giocatore (Vita)")]
    [Tooltip("Trascina qui i 3 elementi della vita, oppure l'oggetto Padre che li raggruppa")]
    public GameObject[] elementiVitaUI;

    [Header("Riferimento MagLockManager")]
    public MagLockManager minigiocoManager;

    [Header("Riferimento Player")]
    public MonoBehaviour playerMovementScript;

    [Header("Input")]
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode exitKey = KeyCode.Escape;

    private bool isPlayerInside = false;
    private bool isUsingComputer = false;
    
    // Memoria per salvare lo stato della vita prima di aprire il pc
    private bool[] statiVitaOriginali;

    private void Start()
    {
        if (uiSprite != null) uiSprite.SetActive(false);
        if (minigamesUI != null) minigamesUI.SetActive(false);
        UpdateCameraTarget(false);
    }

    private void Update()
    {
        if (isPlayerInside && !isUsingComputer && Input.GetKeyDown(interactionKey))
        {
            SetComputerState(true);
        }
        else if (isUsingComputer && Input.GetKeyDown(exitKey))
        {
            SetComputerState(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer"))
        {
            isPlayerInside = true;
            if (uiSprite != null && !isUsingComputer) uiSprite.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer"))
        {
            isPlayerInside = false;
            if (uiSprite != null) uiSprite.SetActive(false);
            
            if (isUsingComputer) SetComputerState(false);
        }
    }

    private void SetComputerState(bool state)
    {
        isUsingComputer = state;

        // Gestione Reset o Inizializzazione Minigioco
        if (minigiocoManager != null)
        {
            if (!state && !minigiocoManager.isCompleted)
            {
                minigiocoManager.ResetMinigioco();
            }
            else if (state && !minigiocoManager.isCompleted)
            {
                minigiocoManager.InizializzaMinigioco();
            }
        }

        // 1. Gestione Camera
        UpdateCameraTarget(state);

        // 2. Attivazione UI Minigioco
        if (minigamesUI != null) minigamesUI.SetActive(state);

        // 3. Disabilita movimento Player
        if (playerMovementScript != null) playerMovementScript.enabled = !state;

        // 4. Gestione Icona Prompt
        if (uiSprite != null) uiSprite.SetActive(!state && isPlayerInside);

        // 5. GESTIONE MEMORIA UI VITA
        if (state == true)
        {
            // STIAMO APRENDO IL PC: Salviamo lo stato attuale e poi nascondiamo
            
            // Inizializza l'array di memoria se non esiste ancora
            if (statiVitaOriginali == null || statiVitaOriginali.Length != elementiVitaUI.Length)
            {
                statiVitaOriginali = new bool[elementiVitaUI.Length];
            }

            for (int i = 0; i < elementiVitaUI.Length; i++)
            {
                if (elementiVitaUI[i] != null)
                {
                    // Memorizza se era acceso o spento
                    statiVitaOriginali[i] = elementiVitaUI[i].activeSelf; 
                    // Spegni l'oggetto
                    elementiVitaUI[i].SetActive(false);
                }
            }
        }
        else
        {
            // STIAMO CHIUDENDO IL PC: Ripristiniamo esattamente come stavano prima
            if (statiVitaOriginali != null)
            {
                for (int i = 0; i < elementiVitaUI.Length; i++)
                {
                    if (elementiVitaUI[i] != null)
                    {
                        // Rimetti lo stato salvato nella memoria
                        elementiVitaUI[i].SetActive(statiVitaOriginali[i]);
                    }
                }
            }
        }
    }

    private void UpdateCameraTarget(bool toComputer)
    {
        Transform target = toComputer ? computerTarget : playerTarget;
        
        if (vcam != null && target != null)
        {
            vcam.Follow = target;
            vcam.LookAt = target;
            vcam.ForceCameraPosition(target.position, target.rotation);
        }
    }
}