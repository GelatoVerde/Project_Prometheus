using TMPro;
using UnityEngine;

public class MagLockManager : MonoBehaviour
{
    public GameObject TXTtutorial;
    public GameObject TXTVittoria;
    [Header("Assegna i 3 Lock qui")]
    public Rotazione2D[] locks;

    private int indiceLockAttuale = 0;

    void Start()
    {
        foreach (Rotazione2D l in locks)
        {
            l.puoRuotare = false;
        }

        if (locks.Length > 0)
        {
            locks[0].puoRuotare = true;
        }
    }

    void Update()
    {
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
                    TXTtutorial.SetActive(false);
                    TXTVittoria.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Errore! Hai premuto spazio fuori dall'area.");
            }
        }
    }
}