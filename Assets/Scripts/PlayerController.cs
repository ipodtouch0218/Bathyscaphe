using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    AudioSource source;
    SpriteRenderer spriteRenderer;    
    Animator animator;
    Rigidbody2D body;
    public float xSpeed = 1, ySpeed = 1;
    public float sinkGravity = 0.2f, sinkMaxSpeed = 1f;
    public float cameraDist = 2f, zoomMin = -10f, zoomMax = -2f;
    float zoom = -10f;
    public CameraFollower cam;
    public MapManager mapManager;

    public int health = 100, scoreOnHand, scoreDeposited, maxDepth;
    public TextMeshProUGUI healthText, scoreText, scoreDepositText, depthText, insuranceText, infoText, abandonTxt;
    public AudioClip collect, impact, splash, explosion, deposit;
    public AudioClip[] ambient;
    float audioTimer;
    bool dead;
    public GameObject explodePrefab, splashPrefab, areaLight;
    public FollowMouse spotlight;
    Vector3 lastPos;
    public GameObject pausePanel, deathPanel, depositWarnings, pauseWarnings;
    bool paused;
    float deadTimer = 2f, depthTimer = 3f;
    public GameObject wasd; 

    void Start() {
        lastPos = transform.position;
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
        audioTimer = Random.Range(15f,30f);

        animator.SetInteger("armor", GameManager.INSTANCE.armorUpgrade);
        health += (GameManager.INSTANCE.armorUpgrade * 20);
        
        xSpeed *= (GameManager.INSTANCE.engineUpgrade * 0.25f) + 1f; 
        ySpeed *= (GameManager.INSTANCE.engineUpgrade * 0.25f) + 1f; 

        maxDepth = ((GameManager.INSTANCE.depthUpgrade) * 200) + 150;

        int modifier = (int) (GameManager.INSTANCE.lightUpgrade * 10);
        Light2D light = spotlight.GetComponent<Light2D>();
        light.pointLightInnerAngle = 48 + modifier;
        light.pointLightInnerRadius = 22 + modifier;
        light.pointLightOuterAngle = 71 + modifier;
        light.pointLightOuterRadius = 30 + modifier;
    }

    void Update() {
        zoom = Mathf.Max(Mathf.Min(zoom + Input.mouseScrollDelta.y, zoomMax), zoomMin);
        
        float offX = Mathf.Min(Mathf.Max(body.velocity.x, -cameraDist), cameraDist);
        float offY = Mathf.Min(Mathf.Max(body.velocity.y, -cameraDist), cameraDist);
        Vector2 mouseOff = (Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)) - transform.position)/10f;
        cam.offset = (new Vector3(offX, offY, zoom) + new Vector3(Mathf.Clamp(mouseOff.x, -3, 3), Mathf.Clamp(mouseOff.y, -3, 3), 0));

        healthText.text = "Health: " + health;
        scoreText.text = "On Hand: $" + scoreOnHand;
        scoreDepositText.text = "Deposited: $" + scoreDeposited;
        depthText.text = "Depth: " + (int) (-Mathf.Min(0,transform.position.y*0.5f)) + "m (max " + maxDepth + "m)";

        if (dead) {
            deathPanel.SetActive((deadTimer -= Time.deltaTime) <= 0);
            return;
        }

        if (Input.GetButtonDown("Pause")) {
            paused = !paused;
            pausePanel.SetActive(paused);
            Time.timeScale = (paused ? 0f : 1f);
            if (transform.position.y > -100) {
                abandonTxt.text = "Quit";
            } else {
                abandonTxt.text = "Abandon";
            }
            AudioListener.pause = paused;
        }

        animator.SetFloat("vel", Mathf.Max(0.2f, Mathf.Abs(body.velocity.magnitude/2f)));
        spriteRenderer.flipX = body.velocity.x > 0;

        if ((audioTimer -= Time.deltaTime) <= 0 && transform.position.y < -20) {
            audioTimer = Random.Range(15f,60f);
            PlayAudio(ambient[Random.Range(0, ambient.Length)]);
        }
        
        if (Mathf.Sign(lastPos.y) != Mathf.Sign(transform.position.y)) {
            if (Mathf.Abs(body.velocity.y) > 2) {
                PlayAudio(splash);
                GameObject.Instantiate(splashPrefab, transform.position, Quaternion.identity);
            }

            if (scoreOnHand > 0) {
                scoreDeposited += scoreOnHand;
                GameManager.INSTANCE.totalScore += scoreOnHand;
                PlayAudio(deposit);
                depositWarnings.SetActive(false);
                pauseWarnings.SetActive(true);
                scoreOnHand = 0;
            }
        }
        lastPos = transform.position;

        float y = transform.position.y/2f;
        if (y < -maxDepth) {
            if (Time.time%0.4f < 0.2f) {
                depthText.color = Color.red;
            } else {
                depthText.color = Color.white;
            }
            if ((depthTimer -= Time.deltaTime) <= 0) {
                
                health -= 5;
                cam.shake = 0.15f;
                cam.shakeMagnitude = 5/40f;
                depthTimer = 3f;

                if (health <= 0) {
                    Death();
                }
            }
        } else if (y < -maxDepth + 50) {
            depthText.color = Color.yellow;
        } else {
            depthText.color = Color.white;
        }
    }

    void FixedUpdate() {
        if (dead) {
            body.velocity = Vector3.zero;
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (!(h == 0 && v == 0)) {
            wasd.SetActive(false);
        }

        if (transform.position.y > 0) {
            body.gravityScale = 2f;
            return;
        }

        int xmod = 1, ymod = 1;
        if ((body.velocity.x < 0 && h > 0.2) || (body.velocity.x > 0 && h < -0.2)) {
            xmod = 2;
        }
        if ((v > 0.2) || (body.velocity.y > 0 && v < -0.2)) {
            ymod = 2;
        }

        body.AddForce(new Vector2(h * xSpeed * xmod, v * ySpeed * ymod));

        if (body.velocity.y > -sinkMaxSpeed) {
            body.gravityScale = sinkGravity;
        } else {
            body.gravityScale = 0;
        }

        transform.rotation = Quaternion.Euler(new Vector3(0,0,-body.velocity.x));
    }

    void Death() {
        GameObject.Instantiate(explodePrefab, transform.position, Quaternion.identity);
        PlayAudio(explosion);
        spriteRenderer.color = new Color(0,0,0,0);
        dead = true;
        spotlight.gameObject.SetActive(false);

        areaLight.GetComponent<LightDimmer>().enabled = false;
        Light2D area = areaLight.GetComponent<Light2D>();
        area.intensity = 0.5f;

        GameManager manager = GameManager.INSTANCE;

        int insurance = 0;
        if (manager.insuranceUpgrade) {
            insurance += (150 * manager.armorUpgrade);
            insurance += (150 * manager.engineUpgrade);
            insurance += (150 * (manager.finderUpgrade ? 1 : 0));
            insurance += (150 * manager.lightUpgrade);
            insurance += (150 * manager.depthUpgrade);
        }
        
        manager.totalScore += insurance;

        manager.armorUpgrade = 0;
        manager.engineUpgrade = 0;
        manager.lightUpgrade = 0;
        manager.finderUpgrade = false;
        manager.insuranceUpgrade = false;

        if (insurance != 0) {
            insuranceText.text = "You received $" + insurance + " back from insurance.";
        }

        infoText.text = "You earned: $" + scoreDeposited + "\nMap Seed: " + manager.seed;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        float totalForce = 0;
        for (int i = 0; i < collision.contactCount; i++) {
            totalForce += collision.GetContact(i).normalImpulse;
        }
        if (totalForce < 0.1) return;
        int damage = (int) ((totalForce * 4f) * Mathf.Max(1,-transform.position.y/100f));

        health -= damage;
        cam.shake = 0.15f;
        cam.shakeMagnitude = damage/40f;
        PlayAudio(impact);

        if (health <= 0) {
            Death();
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Collectable collectable = collider.gameObject.GetComponent<Collectable>();
        if (collectable != null) {
            scoreOnHand += collectable.value;
            PlayAudio(collect);
            GameObject.Destroy(collider.gameObject);
            mapManager.collectedCollectables.Add(collectable.ID);
        }
    }

    void PlayAudio(AudioClip clip) {
        source.PlayOneShot(clip);
    }

    public void Resume() {
        paused = !paused;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void BackToMenu() {
        GameManager manager = GameManager.INSTANCE;
        if (!dead && transform.position.y < -100) {
            int insurance = 0;
            if (manager.insuranceUpgrade) {
                insurance += (150 * manager.armorUpgrade);
                insurance += (150 * manager.engineUpgrade);
                insurance += (150 * (manager.finderUpgrade ? 1 : 0));
                insurance += (150 * manager.lightUpgrade);
                insurance += (150 * manager.depthUpgrade);
            }
            manager.totalScore += insurance;
            
            manager.armorUpgrade = 0;
            manager.engineUpgrade = 0;
            manager.lightUpgrade = 0;
            manager.finderUpgrade = false;
            manager.insuranceUpgrade = false;
        }
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("Menus");
    }
}
