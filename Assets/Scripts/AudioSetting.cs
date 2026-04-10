using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [Header("Impostazioni UI")]
    [Tooltip("Trascina qui lo Slider della UI che controllerà il volume")]
    public Slider sliderVolume;

    // --- LA MAGIA È QUI ---
    // Questo metodo viene eseguito in automatico da Unity nel momento esatto 
    // in cui il gioco viene avviato, PRIMA ancora che carichi qualsiasi scena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CaricaVolumeGlobaleAllAvvio()
    {
        // Applica il volume salvato a tutto il gioco in background
        AudioListener.volume = PlayerPrefs.GetFloat("VolumeGenerale", 1f);
    }

    void Start()
    {
        // Quando entri in una scena con questo script (es. il Menu), 
        // aggiorna solo l'aspetto visivo dello Slider per farlo combaciare.
        if (sliderVolume != null)
        {
            sliderVolume.value = PlayerPrefs.GetFloat("VolumeGenerale", 1f);
            
            // "Ascolta" ogni volta che il giocatore muove lo slider
            sliderVolume.onValueChanged.AddListener(ImpostaVolume);
        }
    }

    public void ImpostaVolume(float nuovoVolume)
    {
        // Modifica il volume generale (0.0 = muto, 1.0 = massimo)
        AudioListener.volume = nuovoVolume;

        // Salva il nuovo valore nella memoria del PC
        PlayerPrefs.SetFloat("VolumeGenerale", nuovoVolume);
        PlayerPrefs.Save();
    }
}