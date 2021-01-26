using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2021.1.7
/// 
/// created by www.github.com/FuaAlfu
/// </summary>

//[RequireComponent(typeof (Rigidbody),]
public class FPScontroler : MonoBehaviour
{
    [Tooltip("insert our lovely camera here")]
    [SerializeField]
    GameObject cam;

    [SerializeField]
    AudioSource[] footSteps;

    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoA;
    public AudioSource healthA;
    public AudioSource triggerSound;
    public AudioSource lavaAudio;
    public AudioSource death;
    public AudioSource reloadClip;

    [SerializeField]
    Animator anim;

    [SerializeField]
    private float speed = 1.2f;

    [SerializeField]
    float Xsensitivity = 2 , Ysensitivity = 2;

    float x, z;

    float minimumX = -90;
    float maximumX = 90;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    bool playInWalking = false;
    bool previoslyGrounded = true;

    //holder
    Rigidbody rb;
    CapsuleCollider cupsuleCollider;
    Quaternion cameraRot;
    Quaternion characterRot;

    //invetory
    int ammo = 0;
    int maxAmmo = 50;
    int health = 0;
    int maxHealth = 100;
    int ammoClip = 0;
    int ammoClipMax = 10;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        cupsuleCollider = this.GetComponent<CapsuleCollider>();

        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;

        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            anim.SetBool("arm", !anim.GetBool("arm")); //if true , will become false and vice versa
        }

        //-----------------------(firing)--------------------------\
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    anim.SetBool("fire", !anim.GetBool("fire")); //if true , will become false and vice versa
        //}

        if(Input.GetMouseButtonDown(0))
        {
            if (ammoClip > 0)
            {
                //anim.SetBool("fire", true);
                anim.SetTrigger("fire");
                // ammo = Mathf.Clamp(ammo + -1, 0, maxAmmo); //decreece
                ammoClip--;
            }
            else if (anim.GetBool("arm"))
                triggerSound.Play();
            Debug.Log("Ammo left in clip " + ammoClip);
                // shoot.Play(); //to test
        }
        //else if(Input.GetMouseButtonUp(0))
        //    anim.SetBool("fire", false);

        //-------------------(reload animation)---------------------\
        if(Input.GetKeyDown(KeyCode.R) && anim.GetBool("arm"))
        {
            anim.SetTrigger("reloading");
            reloadClip.Play();
            int amountNeed = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeed < ammo ? amountNeed : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable;
            Debug.Log("ammo left: " + ammo);
            Debug.Log("ammo in clip: " + ammoClip);
        }
        //-------------------(walking animation)---------------------\
        //create absulote values
        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool("walking"))
            {
                anim.SetBool("walking", true);
                //footSteps[0].Play(); //for testing
               InvokeRepeating("PlayfootstepAudio",0,0.4f); //0 make it happen immeditly
            }
        }
        else if (anim.GetBool("walking"))
        {
            anim.SetBool("walking", false);
            //footSteps[0].Stop(); //for testing
            CancelInvoke("PlayfootstepAudio");
            playInWalking = false;  //fix walk and landing sound bug 
        }

        //---------------(jump)-----------------------------------\
        bool grounded = IsGrounded(); //hold current state
        if (Input.GetButton("Jump") && grounded)
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool("walking"))
            {
                CancelInvoke("PlayfootstepAudio");
                playInWalking = false;  //fix walk and landing sound bug 
            }
        }
        else if(!previoslyGrounded && grounded)
        {
            land.Play();
        }
        previoslyGrounded = grounded;
    }

    void PlayfootstepAudio()
    {
        //create randomise for our lovely array
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footSteps.Length); //start up from second position to the end..

        audioSource = footSteps[n];
        audioSource.Play();
        footSteps[n] = footSteps[0]; //random location will swamp them around..
        footSteps[0] = audioSource;

        playInWalking = true;
    }

    private void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X") * Ysensitivity;
        float xRot = Input.GetAxis("Mouse Y") * Xsensitivity;

        //Quaternion always multiaply
        cameraRot *= Quaternion.Euler(-xRot, 0, 0); // or cameraRot *= Quaternion.Euler(-xRot * Ysensitivity, 0, 0);
        characterRot *= Quaternion.Euler(0, yRot, 0);

        cameraRot = ClampRotationAroundXAxis(cameraRot);

        this.transform.rotation = characterRot;
        cam.transform.localRotation = cameraRot;
    
         x = Input.GetAxis("Horizontal") * speed;
         z = Input.GetAxis("Vertical") * speed;

        transform.position += cam.transform.forward * z + cam.transform.right * x; //old ::: new Vector3(x * speed, 0, z * speed);
        UpdateCursorLock();
    }

    private void OnCollisionEnter(Collision c)
    {
        if (IsGrounded())
        {
            //land.Play(); //no need anymore
            if (anim.GetBool("walking") && !playInWalking)
            {
                InvokeRepeating("PlayfootstepAudio", 0, 0.4f); //start playing footstep once again after landing
            }
        }

        if(c.gameObject.tag == "Ammo" && ammo < maxAmmo)
        {
            //ammo += 10; //stand
            ammo = Mathf.Clamp(ammo + 10,0,maxAmmo);
            Debug.Log("toch ammo" + ammo);
            ammoA.Play();
            Destroy(c.gameObject);
        }

        if (c.gameObject.tag == "medKit" && health < maxHealth)
        {
            health += 25;
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            Debug.Log("toch medkit"+ health);
            healthA.Play();
            Destroy(c.gameObject);
        }

        if(c.gameObject.CompareTag("Lava"))
        {
           // health--; //or health -= 50;
           health = Mathf.Clamp(health - 50, 0, maxHealth);
            lavaAudio.Play();
            Debug.Log("toch lava and your health become" + health);
            if (health <= 0)
                death.Play();
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        //check out lecture's doc for more explanation 
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minimumX, maximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if(Physics.SphereCast(transform.position,cupsuleCollider.radius,Vector3.down, out hitInfo,    //we could use Vector3.down or Vector3(0,-1,0) too
            (cupsuleCollider.height/2f) - cupsuleCollider.radius + 0.1f))
        {
            return true;
        }
        return false;
    }

    //screen bounds setup
    public void SetCursorLook(bool value)
    {
        //turn on & off
        lockCursor = value;
        if(!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)

            InternalLockUpdate();
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0))
            cursorIsLocked = true;

        if(cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if(!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
