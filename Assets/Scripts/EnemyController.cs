using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    // Variabili Pubbliche
    public float velocityX;
    public float velocityY;
    public int isfacing;

    // Componenti
    private Rigidbody2D rb;
    private Animator anim;

    // Target e Waypoints
    private Transform target;
    public Transform WaypointParent;
    private Transform[] waypoints;
    private int currentWaypointIndex;

    [Header("Movement")]
    [SerializeField] private float speed = 2.7f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private bool loopWaypoints;

    // --- SISTEMA DI ATTACCO ---
    [Header("Combat")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1f; 
    
    // NUOVO: SUONO ATTACCO
    [SerializeField] private AudioSource attackAudioSource; // Trascina qui l'AudioSource dell'attacco
    
    private bool isAttacking = false; 

    [Header("AI & Vision")]
    [SerializeField] private bool inSight;
    [SerializeField] private bool inRange;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask visionMask;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float avoidanceDistance = 1.5f;
    [SerializeField] private float avoidanceWeight = 4f; 
    [SerializeField] private float agentRadius = 0.4f;   
    [SerializeField] private LayerMask obstacleMask;
    
    private Vector2 lastAimDirection = Vector2.down; 

    [Header("Breadcrumbs (Ritorno)")]
    [SerializeField] private float breadcrumbSpacing = 1f; 
    [SerializeField] private float waitBeforeReturnTime = 1f;
    private List<Vector2> breadcrumbs = new List<Vector2>(); 
    private bool isReturning = false; 
    private bool wasChasing = false; 
    private bool isWaitingToReturn = false; 

    // Variabili per l'investigazione
    private bool isInvestigating = false; 
    private Vector2 lastKnownPlayerPosition; 

    [Header("Sphere Indicator")]
    [SerializeField] private Transform indicatorSphere; 
    [SerializeField] private float sphereHeight = 1.5f; 
    [SerializeField] private float sphereOffsetX = 0f;  

    // --- VARIABILI AUDIO E FADE ---
    [Header("Audio Phases (Assign AudioSources)")]
    [SerializeField] private AudioSource phase0Source; 
    [SerializeField] private AudioSource phase1Source;
    [SerializeField] private AudioSource phase2Source;
    [SerializeField] private AudioSource phase3Source; 
    
    [SerializeField] private float phase1Distance = 15f;
    [SerializeField] private float phase2Distance = 7f;
    [SerializeField] private float fadeDuration = 2.5f;     
    [SerializeField] private float ambientMaxVolume = 0.5f; 
    [SerializeField] private float maxVolume = 1f;          

    // --- VARIABILI UI ---
    [Header("UI Effects")]
    [SerializeField] private Image tensionUIImage; 

    private int currentPhase = -1; 
    private bool isWaiting;

    // Riferimenti alle Coroutine
    private Coroutine fade0;
    private Coroutine fade1;
    private Coroutine fade2;
    private Coroutine fade3;
    private Coroutine fadeUI; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) target = playerObj.transform;

        if (WaypointParent != null)
        {
            waypoints = new Transform[WaypointParent.childCount];
            for (int i = 0; i < WaypointParent.childCount; i++)
            {
                waypoints[i] = WaypointParent.GetChild(i);
            }
        }
        
        SetupAudioSource(phase0Source);
        SetupAudioSource(phase1Source);
        SetupAudioSource(phase2Source);
        SetupAudioSource(phase3Source);

        if (tensionUIImage != null)
        {
            Color c = tensionUIImage.color;
            c.a = 0f;
            tensionUIImage.color = c;
        }
    }

    private void SetupAudioSource(AudioSource source)
    {
        if (source != null)
        {
            source.volume = 0;
            source.loop = true;
            source.Play(); 
        }
    }

    void Update()
    {
        if (target == null) return;

        // --- 1. SISTEMA DI VISIONE E DISTANZA ---
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > maxDistance)
        {
            inRange = false;
            inSight = false;
        }

        if (inRange)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            RaycastHit2D ray = Physics2D.Raycast(transform.position, directionToTarget, maxDistance, visionMask);
            
            if (ray.collider != null)
                inSight = ray.collider.CompareTag("SeePlayer");
            else
                inSight = false;
        }

        bool isChasing = inSight && inRange;

        if (wasChasing && !isChasing)
        {
            isInvestigating = true;
        }

        // --- 2. MACCHINA A STATI ---
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else if (isChasing)
        {
            isReturning = false; 
            isWaitingToReturn = false; 
            isInvestigating = false; 
            lastKnownPlayerPosition = target.position; 
            
            Chase();
            RecordBreadcrumb(); 
        }
        else if (isInvestigating) 
        {
            InvestigateLastPosition();
            RecordBreadcrumb(); 
        }
        else if (isReturning)
        {
            if (CanSeeWaypoint())
            {
                breadcrumbs.Clear();
                isReturning = false;
            }
            else
            {
                ReturnViaBreadcrumbs(); 
            }
        }
        else if (!isWaitingToReturn && !isWaiting && waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint();
            breadcrumbs.Clear(); 
        }
        else if (isWaiting || isWaitingToReturn || (waypoints == null || waypoints.Length == 0))
        {
            rb.linearVelocity = Vector2.zero;
        }

        wasChasing = isChasing;

        // --- 3. AGGIORNAMENTO VARIABILI PUBBLICHE ---
        velocityX = rb.linearVelocity.x;
        velocityY = rb.linearVelocity.y;

        CheckDirection();
        HandAnimation();

        // --- 4. GESTIONE AUDIO E UI ---
        ManagePhases(isChasing, distance);
    }

    void LateUpdate()
    {
        if (indicatorSphere != null)
        {
            indicatorSphere.position = new Vector2(transform.position.x + sphereOffsetX, transform.position.y + sphereHeight);
            indicatorSphere.rotation = Quaternion.identity; 
        }
    }

    // --- LOGICA DI COLLISIONE E DANNO ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SeePlayer") && !isAttacking)
        {
            StartCoroutine(AttackCooldownRoutine(collision.gameObject));
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SeePlayer") && !isAttacking)
        {
            StartCoroutine(AttackCooldownRoutine(collision.gameObject));
        }
    }

    IEnumerator AttackCooldownRoutine(GameObject playerObj)
    {
        isAttacking = true; 
        rb.linearVelocity = Vector2.zero; 

        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(attackDamage); 
            
            // NUOVO: SUONO ATTACCO
            // Facciamo partire il suono del colpo
            if (attackAudioSource != null)
            {
                attackAudioSource.Play();
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
    // ------------------------------------------

    private void ManagePhases(bool isChasing, float distanceToPlayer)
    {
        int newPhase = 0;

        if (isChasing) newPhase = 3;
        else if (distanceToPlayer <= phase2Distance) newPhase = 2;
        else if (distanceToPlayer <= phase1Distance) newPhase = 1;
        else newPhase = 0;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            
            if (fade0 != null) StopCoroutine(fade0);
            if (fade1 != null) StopCoroutine(fade1);
            if (fade2 != null) StopCoroutine(fade2);
            if (fade3 != null) StopCoroutine(fade3);
            
            fade0 = StartCoroutine(FadeAudio(phase0Source, currentPhase == 0 ? ambientMaxVolume : 0));
            fade1 = StartCoroutine(FadeAudio(phase1Source, currentPhase == 1 ? maxVolume : 0));
            fade2 = StartCoroutine(FadeAudio(phase2Source, currentPhase == 2 ? maxVolume : 0));
            fade3 = StartCoroutine(FadeAudio(phase3Source, currentPhase == 3 ? maxVolume : 0));

            if (tensionUIImage != null)
            {
                if (fadeUI != null) StopCoroutine(fadeUI);

                float targetAlpha = 0f;
                if (currentPhase == 0) targetAlpha = 0f;
                else if (currentPhase == 1) targetAlpha = 0.33f; 
                else if (currentPhase == 2) targetAlpha = 0.5f;  
                else if (currentPhase == 3) targetAlpha = 1f;    

                fadeUI = StartCoroutine(FadeUIAlpha(tensionUIImage, targetAlpha));
            }
        }
    }

    IEnumerator FadeAudio(AudioSource source, float targetVolume)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    IEnumerator FadeUIAlpha(Image img, float targetAlpha)
    {
        if (img == null) yield break;

        Color startColor = img.color;
        float startAlpha = startColor.a;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            img.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        img.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }

    private bool CanSeeWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return false;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 direction = targetWaypoint.position - transform.position;
        float distToWaypoint = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distToWaypoint, obstacleMask);
        return hit.collider == null;
    }

    private void Chase()
    {
        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
        Vector2 finalDirection = ApplyObstacleAvoidance(desiredDirection);
        rb.linearVelocity = finalDirection * speed;
    }

    private void InvestigateLastPosition()
    {
        Vector2 desiredDirection = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
        Vector2 finalDirection = ApplyObstacleAvoidance(desiredDirection);
        rb.linearVelocity = finalDirection * speed;

        if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 0.5f)
        {
            isInvestigating = false; 
            StartCoroutine(WaitBeforeReturningRoutine()); 
        }
    }

    private void MoveToWaypoint()
    {
        Transform moveTo = waypoints[currentWaypointIndex];
        Vector2 desiredDirection = ((Vector2)moveTo.position - (Vector2)transform.position).normalized;
        Vector2 finalDirection = ApplyObstacleAvoidance(desiredDirection);
        
        rb.linearVelocity = finalDirection * speed;

        if (Vector2.Distance(transform.position, moveTo.position) < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitBeforeReturningRoutine()
    {
        isWaitingToReturn = true;
        rb.linearVelocity = Vector2.zero; 
        
        yield return new WaitForSeconds(waitBeforeReturnTime);

        if (isWaitingToReturn) 
        {
            isWaitingToReturn = false;
            isReturning = true; 
        }
    }

    private void RecordBreadcrumb()
    {
        if (breadcrumbs.Count == 0 || Vector2.Distance(transform.position, breadcrumbs[breadcrumbs.Count - 1]) >= breadcrumbSpacing)
        {
            breadcrumbs.Add(transform.position);
            if (breadcrumbs.Count > 100) breadcrumbs.RemoveAt(0);
        }
    }

    private void ReturnViaBreadcrumbs()
    {
        if (breadcrumbs.Count == 0)
        {
            isReturning = false;
            return;
        }

        Vector2 targetPos = breadcrumbs[breadcrumbs.Count - 1];
        Vector2 desiredDirection = (targetPos - (Vector2)transform.position).normalized;
        Vector2 finalDirection = ApplyObstacleAvoidance(desiredDirection);
        rb.linearVelocity = finalDirection * speed;

        if (Vector2.Distance(transform.position, targetPos) < 0.5f)
        {
            breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
        }
    }

    private Vector2 ApplyObstacleAvoidance(Vector2 currentDirection)
    {
        lastAimDirection = currentDirection;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, agentRadius, currentDirection, avoidanceDistance, obstacleMask);

        if (hit.collider != null)
        {
            Vector2 wallNormal = hit.normal;

            Vector2 slideDirection = new Vector2(-wallNormal.y, wallNormal.x); 
            if (Vector2.Dot(slideDirection, currentDirection) < 0)
            {
                slideDirection = -slideDirection;
            }

            float distanceRatio = 1f - (hit.distance / avoidanceDistance);
            Vector2 pushAway = wallNormal * (avoidanceWeight * distanceRatio);

            return (slideDirection + pushAway).normalized;
        }

        return currentDirection;
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero; 
        
        yield return new WaitForSeconds(waitTime);

        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointIndex = loopWaypoints ? 
                (currentWaypointIndex + 1) % waypoints.Length : 
                Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);
        }

        isWaiting = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SeePlayer"))
        {
            inRange = true;
        }
    }

    private void CheckDirection()
    {
        if (rb.linearVelocity.y >= 1.1f) isfacing = 1;
        else if (rb.linearVelocity.y <= -1.1f) isfacing = 2;
        else if (rb.linearVelocity.x >= 1f) isfacing = 3;
        else if (rb.linearVelocity.x <= -1f) isfacing = 4;
    }

    void HandAnimation()
    {
        if (anim != null)
        {
            anim.SetFloat("velocityX", rb.linearVelocity.x);
            anim.SetFloat("velocityY", rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        
        Gizmos.DrawRay(transform.position, lastAimDirection * avoidanceDistance);
        Gizmos.DrawWireSphere((Vector2)transform.position + (lastAimDirection * avoidanceDistance), agentRadius);
    }
}