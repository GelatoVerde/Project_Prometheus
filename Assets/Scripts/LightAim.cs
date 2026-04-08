using UnityEngine;

public class LightAim : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Impostazioni")]
    [Tooltip("Modifica questo valore se la luce punta di lato invece che verso il cursore (spesso è -90 o 90)")]
    public float angleOffset = -90f; 

    void Start()
    {
        // Troviamo la telecamera principale per calcolare la posizione del mouse
        mainCamera = Camera.main;
        Cursor.visible = false;

        // Ti consiglio anche questa riga: "intrappola" il cursore invisibile 
        // dentro la finestra di gioco così non clicchi fuori per sbaglio!
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        // 1. Prendiamo la posizione del mouse sullo schermo e la convertiamo nel mondo di gioco
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // Assicuriamoci che la Z sia a 0 (fondamentale nel 2D!)
        mousePosition.z = 0f;

        // 2. Calcoliamo la direzione: dal nostro oggetto verso il mouse
        Vector2 lookDirection = mousePosition - transform.position;

        // 3. Calcoliamo l'angolo in gradi usando un po' di trigonometria (Atan2)
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        // 4. Applichiamo la rotazione sull'asse Z (quello della profondità nel 2D)
        transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
    }
}