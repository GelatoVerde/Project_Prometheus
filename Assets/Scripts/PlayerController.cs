using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; 

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float xInput;
    private float yInput;   
    private Vector2 moveVelocity; 
    private bool isDead = false; // Il "lucchetto" per la morte

    [Header("Health (Vita)")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth; 
    [SerializeField] private float regenDelay = 3f; // Tempo di attesa per rigenerare
    private Coroutine regenCoroutine; // Riferimento per resettare il timer

    [Header("Health UI & Game Over")]
    [SerializeField] private GameObject health1Image; 
    [SerializeField] private GameObject health2Image; 
    [SerializeField] private GameObject health3Image; 
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private AudioSource gameOverMusic; // NUOVO: Trascina qui la musica del Game Over

    [Header("Damage Effects")]
    [SerializeField] private Image damageFlashPanel; 
    [SerializeField] private float flashDuration = 0.3f; 
    
    private CinemachineImpulseSource impulseSource;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float speedRun = 8f;
    [SerializeField] private float direction;

    [Header("Stamina (Corsa)")]
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float runStaminaCost = 20f;
    [SerializeField] private float staminaRecoveryRate = 15f;
    [SerializeField] private float exhaustionDelay = 3f; 
    [SerializeField] private Image StaminaBar;
    private float staminaDelayTimer = 0f; 

    [Header("Battery (Torcia)")]
    [SerializeField] private float battery = 100f; 
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainRate = 10f; 
    [SerializeField] private float batteryRecoveryRate = 30f; 
    [SerializeField] private float batteryEmptyDelay = 1.5f; 
    [SerializeField] private Image BatteryBar;
    [SerializeField] private GameObject flashlightObject; 
    
    private bool isFlashlightOn = false;
    private float batteryDelayTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        stamina = maxStamina; 
        battery = maxBattery;
        currentHealth = maxHealth; 

        if (flashlightObject != null) flashlightObject.SetActive(false);

        if (damageFlashPanel != null)
        {
            Color c = damageFlashPanel.color;
            c.a = 0f;
            damageFlashPanel.color = c;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverMusic != null) gameOverMusic.Stop();
        
        UpdateHealthUI();
    }
   
    void Update()
    {
        HandleInputAndStamina();
        HandleFlashlight();
        HandAnimation();
        UpdateUIBars();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveVelocity;
    }

    public void TakeDamage(int damageAmount)
    {
        // Se il giocatore è GIA' morto, ignora completamente il resto del codice!
        if (isDead) return; 

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        if (currentHealth > 0) regenCoroutine = StartCoroutine(RegenHealthRoutine());

        if (impulseSource != null) impulseSource.GenerateImpulse(); 

        if (damageFlashPanel != null) StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0) Die();
    }

    // --- NUOVA COROUTINE: RIGENERAZIONE ---
    private IEnumerator RegenHealthRoutine()
    {
        // Aspetta 3 secondi dopo l'ultimo danno ricevuto
        yield return new WaitForSeconds(regenDelay);

        // Continua a rigenerare finché non arrivi alla vita massima
        while (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHealthUI();
            Debug.Log("Vita rigenerata! Attuale: " + currentHealth);
            
            // Aspetta altri 3 secondi per il prossimo punto vita
            yield return new WaitForSeconds(regenDelay);
        }
    }

    private void UpdateHealthUI()
    {
        if (currentHealth >= 3)
        {
            if (health1Image != null) health1Image.SetActive(true);
            if (health2Image != null) health2Image.SetActive(false);
            if (health3Image != null) health3Image.SetActive(false);
        }
        else if (currentHealth == 2)
        {
            if (health1Image != null) health1Image.SetActive(false);
            if (health2Image != null) health2Image.SetActive(true);
            if (health3Image != null) health3Image.SetActive(false);
        }
        else if (currentHealth == 1)
        {
            if (health1Image != null) health1Image.SetActive(false);
            if (health2Image != null) health2Image.SetActive(false);
            if (health3Image != null) health3Image.SetActive(true);
        }
        else
        {
            if (health1Image != null) health1Image.SetActive(false);
            if (health2Image != null) health2Image.SetActive(false);
            if (health3Image != null) health3Image.SetActive(false);
        }
    }

    private void Die()
    {
        isDead = true; 
        Debug.Log("Il giocatore è morto!");
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // --- BLOCCA TUTTI I NEMICI TOTALMENTE ---
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            // 1. Ferma le coroutine in corso (niente attacchi o suoni "fantasma" in ritardo)
            enemy.StopAllCoroutines();
            
            // 2. Inchioda il nemico sul posto fermando la sua fisica
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;
            
            // 3. Ferma la musica dell'inseguimento
            AudioSource[] enemySources = enemy.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in enemySources)
            {
                source.Stop(); 
            }
            
            // 4. Disabilita l'intelligenza artificiale
            enemy.enabled = false; 
        }

        if (gameOverMusic != null)
        {
            gameOverMusic.Play();
        }
        
        // 5. Cambiamo il tag al giocatore così i nemici lo ignorano fisicamente
        gameObject.tag = "Untagged";
        
        rb.linearVelocity = Vector2.zero;
        this.enabled = false; 
    }

    // (Tutte le altre funzioni HandleInput, Flashlight, etc. rimangono uguali...)
    private IEnumerator DamageFlashRoutine() { /* ... già presente ... */ 
        Color c = damageFlashPanel.color;
        c.a = 0.4f; 
        damageFlashPanel.color = c;
        float elapsed = 0f;
        while (elapsed < flashDuration) {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.4f, 0f, elapsed / flashDuration);
            damageFlashPanel.color = c;
            yield return null;
        }
        c.a = 0f;
        damageFlashPanel.color = c;
    }

    private void HandleInputAndStamina() {
        xInput = Input.GetAxisRaw("Horizontal"); 
        yInput = Input.GetAxisRaw("Vertical");
        bool isMoving = (xInput != 0 || yInput != 0);
        if (isMoving) FacingDirection();
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && isMoving) {
            currentSpeed = speedRun; 
            stamina -= Time.deltaTime * runStaminaCost;
            staminaDelayTimer = 0f; 
            if (stamina <= 0) stamina = 0;
        } else {
            currentSpeed = speed; 
            if (stamina < maxStamina) {
                if (stamina == 0 && staminaDelayTimer < exhaustionDelay)
                    staminaDelayTimer += Time.deltaTime; 
                else
                    stamina += Time.deltaTime * staminaRecoveryRate;
                if (stamina > maxStamina) stamina = maxStamina;
            }
        }
        moveVelocity = new Vector2(xInput, yInput).normalized * currentSpeed;
    }

    private void HandleFlashlight() {
        if (Input.GetKeyDown(KeyCode.Mouse0) && battery > 0) {
            isFlashlightOn = !isFlashlightOn; 
            if (flashlightObject != null) flashlightObject.SetActive(isFlashlightOn);
        }
        if (isFlashlightOn) {
            battery -= Time.deltaTime * batteryDrainRate;
            batteryDelayTimer = 0f; 
            if (battery <= 0) {
                battery = 0;
                isFlashlightOn = false;
                if (flashlightObject != null) flashlightObject.SetActive(false);
            }
        } else {
            if (battery < maxBattery) {
                if (battery == 0 && batteryDelayTimer < batteryEmptyDelay)
                    batteryDelayTimer += Time.deltaTime; 
                else
                    battery += Time.deltaTime * batteryRecoveryRate;
                if (battery > maxBattery) battery = maxBattery;
            }
        }
    }

    void HandAnimation() {
        anim.SetFloat("velocityX", moveVelocity.x);
        anim.SetFloat("velocityY", moveVelocity.y);
        anim.SetFloat("FacingDirection", direction);
    }

    private void FacingDirection() {
        if (xInput < 0) direction = 1;
        else if (xInput > 0) direction = 2;
        else if (yInput < 0) direction = 3;
        else if (yInput > 0) direction = 4;
    }

    private void UpdateUIBars() {
        if (StaminaBar != null) StaminaBar.fillAmount = stamina / maxStamina;
        if (BatteryBar != null) BatteryBar.fillAmount = battery / maxBattery;
    }
}