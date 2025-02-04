using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsControl : MonoBehaviour
{
    public Rigidbody rb;

    private Vector3 oldGravity = Physics.gravity;   
    private Vector3 reversedGravity = -2 * Physics.gravity;
    private bool isReversedGravity = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerTimeControl();

        PlayerGravityControl();
    }

    void PlayerTimeControl()
    {
        Time.timeScale = Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.G))
        {
            Time.timeScale = 0.5f;
            print("Time slowed");
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    void PlayerGravityControl()
    {

        if (Input.GetKey(KeyCode.H))
        {
            rb.AddForce(-2 * Physics.gravity, ForceMode.Acceleration);
            isReversedGravity = true;
        }
        else
        {
            if(isReversedGravity)
            {
                rb.AddForce(Physics.gravity, ForceMode.Acceleration);
                isReversedGravity = false;
            }
        }
    }
}
