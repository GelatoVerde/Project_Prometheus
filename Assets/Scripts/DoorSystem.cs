using UnityEngine;
using System.Collections;

public class DoorSystem : MonoBehaviour
{
    public float time = 2f;
    public bool isOpen = false;
    private Animator anim;
    
    // Variabile per contare quanti personaggi sono sul "tappetino" della porta
    private int entitiesInside = 0; 
    
    // Riferimento alla Coroutine per poterla fermare
    private Coroutine closeRoutine; 

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer") || collision.CompareTag("Enemy"))
        {
            entitiesInside++; // Aggiungiamo 1 al contatore

            // Se c'è almeno una persona, ci assicuriamo che la porta sia aperta
            if (entitiesInside > 0)
            {
                isOpen = true;
                anim.Play("Open");

                // Se la porta si stava per chiudere (timer in corso), fermiamo il timer!
                if (closeRoutine != null)
                {
                    StopCoroutine(closeRoutine);
                    closeRoutine = null;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("SeePlayer") || collision.CompareTag("Enemy"))
        {
            entitiesInside--; // Togliamo 1 dal contatore

            // Per sicurezza evitiamo che il contatore vada sotto zero
            if (entitiesInside <= 0) 
            {
                entitiesInside = 0;
                
                // Non c'è più nessuno, avviamo il timer per chiudere
                closeRoutine = StartCoroutine(CloseDoorRoutine());
            }
        }
    } 

    // È buona norma dare nomi specifici alle Coroutine per non confondersi con classi di Unity come WaitForSeconds
    IEnumerator CloseDoorRoutine()
    {
        yield return new WaitForSeconds(time);
        
        // Assicuriamoci che nel frattempo non sia entrato nessuno
        if (entitiesInside == 0) 
        {
            isOpen = false;
            anim.Play("Closed");
        }
    }
}