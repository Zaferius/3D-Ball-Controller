using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    public float moveForce; //Ball's acceleration 
    public float jumpForce; //Ball's jumping power 
    public float maxSpeed;  //Ball's max speed 
   
    [Range(0.999f, 0.990f)]
    public float breakingDuration; //Ball's breaking duration(left to right >> Soft brake to hard brake)
    
    private FloatingJoystick floatingJoystick; //Joystick ref)
    private Rigidbody rb; //Ball's RigidBody

    [Header("Condition")]
    public float speed; //Ball's current speed
    private bool braking; //Ball's breaking check bool
    private bool jumping; //Ball's jumping check bool

    [Header("Indicator")] 
    public Transform indicator; //Ball's direction indicator object

    [Header("Jump")]
    private int tapCount; //Screen tap count
    private bool jumpTimerActive; //Countdown for second tap(jumping) check bool
    private float jumpTimer; //Countdown for second tap(jumping)

    private void Start()
    {
        floatingJoystick = GameObject.FindGameObjectWithTag("FloatingJoystick").GetComponent<FloatingJoystick>();
        rb = GetComponent<Rigidbody>();
        
        //VERY IMPORTANT CODE LINE
        rb.maxAngularVelocity = 50; // In order to simulate the physics of the ball at the maximum level, we must increase the maximum number of rolls (rb.maxAngularVelocity).
                                    // Otherwise, no matter how fast it accelerates, after a while, the number of rolls will not be able to keep up with its speed.
    }

    private void Update()
    {
       HandleSpeed();

        if (Input.GetMouseButton(0))
        {
           HandleMovement();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            CheckTap();
        }

        if (Input.GetMouseButtonUp(0))
        {
            braking = true;
        }

        if (braking)
        {
            Brake();
        }

        if (jumpTimerActive)
        {
            JumpTimer();
        }
        
    }

    private void FixedUpdate()
    {
        HandleIndicator();
    }
    private void Brake()
    {
        //We wrote a "brake" code to prevent the ball from going by itself when we pulled our hand away.
        //A code that reduces the speed of the ball every second (rb.velocity *= breakingDuration;)
        //and we added a specifier so that when its velocity goes down to 0, it doesn't go below 0 and gain negative velocity.
        rb.velocity *= breakingDuration;
        if (rb.velocity.magnitude < 0.025f)
        {
            rb.velocity = Vector3.zero;
        }
    }
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
        rb.AddForce(indicator.forward * jumpForce / 2,ForceMode.Impulse);
        jumping = true;
       
        tapCount = 0;
        jumpTimerActive = false;

        indicator.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    private void Impact()
    {
        //When the ball touches anywhere, we open our arrow that shows our direction and that it no longer bounces.
        jumping = false;
        indicator.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
    }
    private void CheckTap()
    {
        if (!jumping)
        {
            tapCount++;
            jumpTimerActive = true;
            
            if (tapCount == 2 && jumpTimerActive)
            {
                Jump();
            }
        }
    }
    private void JumpTimer()
    {
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0)
        {
            jumpTimer = 0.75f;
            tapCount = 0;
            jumpTimerActive = false;
        }
    }
    private void HandleMovement()
    {
        //We set "false" the "Brake" boolean so that the brake command does not work while the movement is provided.
        
        //By multiplying the vertical value of our joystick with "Vector3.up",
        //we get a variable value by multiplying the horizontal value of our joystick with "Vector3.right".
        //This allows us to find our direction when applying force to the Ball.
        braking = false;
        Vector3 direction = Vector3.forward * floatingJoystick.Vertical + Vector3.right * floatingJoystick.Horizontal;
        rb.AddForce(direction * moveForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }
    private void HandleIndicator()
    {
        Vector3 prevLocation = indicator.position;
        indicator.position = Vector3.Lerp(indicator.position, transform.position, 50 * Time.deltaTime);
        Vector3 newDir = (indicator.position - prevLocation).normalized;
        newDir.y = 0;
        indicator.rotation = Quaternion.LookRotation(newDir);
    }
    private void HandleSpeed()
    {
        //"rb.velocity.magnitude;" to calculate the velocity of the ball. we are using.
        speed = rb.velocity.magnitude;
        if (speed >= maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Impact();
    }

}
