using Assets.Scripts.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public Transform camera;
    public Rigidbody rb;

    public float camRotationSpeed = 5f;
    public float cameraMinimumY = -60f;
    public float cameraMaximumY = 75f;
    public float rotationSmoothSpeed = 10f;

    public float walkSpeed;
    public float runSpeed;
    public float maxSpeed; 
    public float jumpPower = 10f; 

    public float extraGravity = 45;

    float bodyRotationX;
    float camRotationY;
    Vector3 directionIntentX;
    Vector3 directionIntentY;
    float speed;
    public bool isGrounded;

    bool isWalking = false;
    bool isRunning = false;
    [SerializeField] bool isJumping = false;

    [SerializeField] public Transform groundCheckTransform = null;
    [SerializeField] private LayerMask playerMask;

    [SerializeField] FootstepGenerator soundGenerator;
    [SerializeField] float footStepTimer;

    private bool jumpKeyWasPressed = false;
    public bool isMoving = false;

    private GameObject spellCasterObject;
    private SpellCast spellCast;

    public float movementThreshold = 0.1f;
    private bool isFootstepCooldownActive = false;

    public float maxStamina = 100f;
    public float playerStamina = 100f;
    public float staminaDepletionRate = 10f; // How fast stamina depletes while running
    public float staminaRegenRate = 5f; // How fast stamina regenerates
    public float minStaminaToRun = 1f;
    public float minStaminaToRunAgain = 10f;
    private bool isRunnable;

    public Slider staminaSlider;
    public CanvasGroup staminaCanvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        /*rb = GetComponent<Rigidbody>();

        mainCam = Camera.main;*/

        soundGenerator = GetComponent<FootstepGenerator>();  

        //spellCasterObject = GameObject.Find("SpellCaster");
        //spellCast = spellCasterObject.GetComponent<SpellCast>();

        LockCursor();

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = playerStamina;
        }
    }

    // Update is called once per frame
    void Update()
    {      
        InputHandler();

        LookRotation();

        HandleStamina();

        UpdateStaminaUI();
    }

    void FixedUpdate()
    {
        Movement();

        //MoveCheck();

        Jump();

        //ChangeCursorState();

        CheckGrounded();

        CheckMoving();
    }

    void InputHandler()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyWasPressed = true;
        }*/

        /*if (Input.GetKeyDown(KeyCode.V))
        {
            spellCast.CastSpell();
        }*/
    }

    void OnSpellCastEnable()
    {
        spellCast.onSpellCast.AddListener(HandleSpellCast);
    }

    void OnSpellCastDisable()
    {
        spellCast.onSpellCast.RemoveListener(HandleSpellCast);
    }

    void HandleSpellCast()
    {
        // Handle any additional logic related to spell casting here
    }

    void Movement()
    {
        //Hướng đi match với camera
        directionIntentX = camera.right;
        directionIntentX.y = 0;
        directionIntentX.Normalize();

        directionIntentY = camera.forward;
        directionIntentY.y = 0;
        directionIntentY.Normalize();

        //di chuyển
        rb.linearVelocity = directionIntentY * Input.GetAxis("Vertical") * speed + directionIntentX * Input.GetAxis("Horizontal") * speed + Vector3.up * rb.linearVelocity.y;
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        if (isMoving)
        {
            PlayFootSound();
        }

        if (playerStamina >= minStaminaToRunAgain)
        {
            isRunnable = true;
        }

        //control speed based on movement state
        if (Input.GetKey(KeyCode.LeftShift) && playerStamina >= minStaminaToRun && isRunnable == true) 
        {
            speed = runSpeed;
            isRunning = true;
        } 
        else 
        { 
            speed = walkSpeed; 
            isRunning = false;
            isRunnable = false;
        }
    }

    void HandleStamina()
    {
        // Running depletes stamina
        if (isRunning && playerStamina > 0)
        {
            playerStamina -= staminaDepletionRate * Time.deltaTime;
            playerStamina = Mathf.Clamp(playerStamina, 0, maxStamina); // Ensure stamina doesn't go below 0
        }
        else
        {
            // Regenerate stamina when not running
            playerStamina += staminaRegenRate * Time.deltaTime;
            playerStamina = Mathf.Clamp(playerStamina, 0, maxStamina); // Ensure stamina doesn't exceed max
        }

        // Disable running if stamina is too low
        if (playerStamina < minStaminaToRun)
        {
            isRunning = false;
        }
    }

    void UpdateStaminaUI()
    {
        // Update the slider's value
        if (staminaSlider != null)
        {
            staminaSlider.value = playerStamina;
        }

        // Control visibility based on stamina
        if (staminaCanvasGroup != null)
        {
            if (playerStamina >= maxStamina)
            {
                staminaCanvasGroup.alpha = 0; // Hide the slider
            }
            else
            {
                staminaCanvasGroup.alpha = 1; // Show the slider
            }
        }
    }

    void Jump()
    {
        if (Physics.OverlapSphere(groundCheckTransform.position, 0.01f, playerMask).Length == 0)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
        }

        if (jumpKeyWasPressed)
        {
            if (!isGrounded)
            {
                jumpKeyWasPressed = false;
                return;
            }
            else
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                jumpKeyWasPressed = false;
                isGrounded = true;
            }
        }
    }

    void CheckGrounded()
    {
        //isGrounded = Physics.OverlapSphere(groundCheckTransform.position, 0.1f, playerMask).Length > 0;

        if (jumpKeyWasPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            jumpKeyWasPressed = false;
        }
    }

    void CheckMoving()
    {
        if(rb.linearVelocity.magnitude > movementThreshold)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void PlayFootSound()
    {
        if (isMoving && isGrounded && !isFootstepCooldownActive)
        {
            if (isRunning)
            {
                StartCoroutine(PlayStepSound(0.3f));
            }
            else
            {
                StartCoroutine(PlayStepSound(1f)); // Start sound with a cooldown of 1 seconds
            }           
        }
    }

    IEnumerator PlayStepSound(float cooldown)
    {
        isFootstepCooldownActive = true; // Activate the cooldown

        var clipCount = soundGenerator.footStepSounds.Count;
        var randomIndex = Random.Range(0, clipCount);
        soundGenerator.audioSource.clip = soundGenerator.footStepSounds[randomIndex];

        soundGenerator.audioSource.Play(); // Play the footstep sound

        yield return new WaitForSeconds(cooldown); // Wait for the cooldown duration

        isFootstepCooldownActive = false; // Reset the cooldown
    }

    void LookRotation()
    {
        bodyRotationX += Input.GetAxis("Mouse X") * camRotationSpeed;
        camRotationY += Input.GetAxis("Mouse Y") * camRotationSpeed;

        //Giới hạn góc độ camera có thể quay
        camRotationY = Mathf.Clamp(camRotationY, cameraMinimumY, cameraMaximumY);

        //Xoay người
        Quaternion camTargetRotation = Quaternion.Euler(-camRotationY, 0, 0);
        Quaternion bodyTargetRotation = Quaternion.Euler(0, bodyRotationX, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, bodyTargetRotation, Time.deltaTime * rotationSmoothSpeed);

        camera.localRotation = Quaternion.Lerp(camera.localRotation, camTargetRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ChangeCursorState()
    {
        if(Input.GetKey(KeyCode.L)) 
        { 
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }

    //void MoveCheck()
    //{
    //    //Vector3 p1 = transform.position;
    //    //yield return new WaitForSeconds(0.5f);
    //    //Vector3 p2 = transform.position;

    //    //if (p1 != p2)
    //    //{
    //    //    isMoving = true;
    //    //}
    //    //else
    //    //{
    //    //    isMoving = false;
    //    //}
    //    bool isMoving = IsPlayerMoving();

    //    if (isMoving && !soundGenerator.audioSource.isPlaying)
    //    {
    //        soundGenerator.audioSource.Play(); // Start playing if moving and not already playing
    //    }
    //    else if (!isMoving && soundGenerator.audioSource.isPlaying)
    //    {
    //        soundGenerator.audioSource.Stop(); // Stop playing if not moving
    //    }
    //}

    //private bool IsPlayerMoving()
    //{
    //    if (rb != null)
    //    {
    //        // Check Rigidbody velocity
    //        return rb.linearVelocity.magnitude > movementThreshold;
    //    }
    //    else if (cc != null)
    //    {
    //        // Check CharacterController velocity
    //        return this.linear.magnitude > movementThreshold;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No Rigidbody or CharacterController attached!");
    //        return false;
    //    }
    //}
}
