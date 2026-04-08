using UnityEngine;
using Unity.Cinemachine; // Se usi versioni vecchie di Unity: using Cinemachine;

public class Computer : MonoBehaviour
{
    [Header("Icona Prossimità (Prompt)")]
    [Tooltip("L'icona 'Premi E' che appare quando sei vicino al PC")]
    public GameObject uiSprite;

    [Header("Cinemachine Camera")]
    [Tooltip("La Virtual Camera unica che deve spostarsi tra Player e PC")]
    public CinemachineCamera vcam; 

    [Header("Target di Puntamento")]
    [Tooltip("Il Transform del Player (solitamente assegnato a Follow/LookAt)")]
    public Transform playerTarget;
    [Tooltip("Un oggetto vuoto posizionato dove la cam deve inquadrare il PC")]
    public Transform computerTarget;

    [Header("Interfaccia Minigioco")]
    [Tooltip("Il Canvas o il Pannello UI del minigioco")]
    public GameObject minigamesUI;

    [Header("Riferimento Player")]
    [Tooltip("Trascina qui il componente del movimento del Player per disattivarlo")]
    public MonoBehaviour playerMovementScript;

    [Header("Input")]
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode exitKey = KeyCode.Escape;

    private bool isPlayerInside = false;
    private bool isUsingComputer = false;

    private void Start()
    {
        // Reset iniziale: UI e Icone spente
        if (uiSprite != null) uiSprite.SetActive(false);
        if (minigamesUI != null) minigamesUI.SetActive(false);
        
        // La camera inizia seguendo il player
        UpdateCameraTarget(false);
    }

    private void Update()
    {
        // Logica di attivazione
        if (isPlayerInside && !isUsingComputer && Input.GetKeyDown(interactionKey))
        {
            SetComputerState(true);
        }
        // Logica di uscita
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
            
            // Se il player esce dal trigger mentre usa il PC (es. teletrasporto), resettiamo
            if (isUsingComputer) SetComputerState(false);
        }
    }

    private void SetComputerState(bool state)
    {
        isUsingComputer = state;

        // 1. Gestione Camera
        UpdateCameraTarget(state);

        // 2. Attivazione UI Minigioco
        if (minigamesUI != null) minigamesUI.SetActive(state);

        // 3. Disabilita movimento Player
        if (playerMovementScript != null) playerMovementScript.enabled = !state;

        // 4. Gestione Icona Prompt (Scompare quando usi il PC)
        if (uiSprite != null) uiSprite.SetActive(!state && isPlayerInside);
    }

    private void UpdateCameraTarget(bool toComputer)
    {
        Transform target = toComputer ? computerTarget : playerTarget;
        
        if (vcam != null && target != null)
        {
            vcam.Follow = target;
            vcam.LookAt = target;

            // Forza il passaggio istantaneo senza scivolamenti (Blend Cut)
            vcam.ForceCameraPosition(target.position, target.rotation);
        }
    }
}