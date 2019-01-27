using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal enum FunctWheels
{
    FWD,BWD,AWD
}

public class CarControler : MonoBehaviour
{

    [SerializeField]
    private GameObject[] wheelsObj;
    [SerializeField]
    private WheelCollider[] wheelsCl;
    [SerializeField]
    private FunctWheels driveWheels = FunctWheels.BWD;
    [SerializeField]
    private float maxSpeedLimit = 210f;
    [SerializeField]
    private float slipLimit = .3f;
    Rigidbody rb;

    public Vector3 centerOfMass;
    public float maxTorque = 100f;
    public float reverseTorque = 200;
    public float brakeTorque = 500f;
    public float steeringAngle = 45f;
    public Text txtSpeed;

    float currentTorque;
    float oldRot;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        currentTorque = maxTorque;
	}
	
	void FixedUpdate ()
    {
        for (int i = 0; i < 4; i++)
        {
            MeshUpdate(wheelsCl[i], wheelsObj[i]);
        }
        float input = Input.GetAxis("Vertical");

        Drive(input);
        CapSpeed();

        input = Input.GetAxis("Jump");
        HandBrake(input);
        input = Input.GetAxis("Horizontal");
        input = Mathf.Clamp(input, -1, 1);
        Steer(input);
        SteerHelper();
        AdjustTorque();
        ApplyDownForce();
	}

    void Drive(float input)
    {
        if (input < 0)
        {
            //currentTorque = reverseTorque;
        }
        else
        switch (driveWheels)
        {
            case FunctWheels.FWD:
                wheelsCl[0].motorTorque = (input * currentTorque)/2f;
                wheelsCl[1].motorTorque = (input * currentTorque)/2f;
                break;
            case FunctWheels.BWD:
                wheelsCl[2].motorTorque = (input * currentTorque)/2;
                wheelsCl[3].motorTorque = (input * currentTorque)/2;
                break;
            case FunctWheels.AWD:
                for (int i = 0; i < 4; i++)
                {
                    wheelsCl[i].motorTorque = (input * currentTorque)/4;
                }
                break;
        }

    }
    void ApplyDownForce ()
    {
        Vector3 downForce;
        downForce = -transform.up * rb.velocity.magnitude * 200f;
        rb.AddForce(downForce);
    }
    void HandBrake(float input)
    {
        wheelsCl[2].brakeTorque = input * float.MaxValue;
        wheelsCl[3].brakeTorque = input * float.MaxValue;
    }
    void Steer(float input)
    {
        wheelsCl[0].steerAngle = input * steeringAngle;
        wheelsCl[1].steerAngle = input * steeringAngle;
    }
    void CapSpeed()
    {
        float speed = rb.velocity.magnitude*3.6f;
        if (speed > maxSpeedLimit)
            rb.velocity = maxSpeedLimit/3.6f * rb.velocity.normalized;
        txtSpeed.text = "Speed: " + speed.ToString("00"); 
    }
    void MeshUpdate(WheelCollider wl,GameObject mesh)
    {
        Vector3 pos;
        Quaternion quat;
        wl.GetWorldPose(out pos, out quat);
        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
    void AdjustTorque()
    {
        WheelHit wheelHit;

        switch(driveWheels)
        {
            case FunctWheels.FWD:

                wheelsCl[0].GetGroundHit(out wheelHit);
                Torque(wheelHit.forwardSlip);
        
                wheelsCl[1].GetGroundHit(out wheelHit);
                Torque(wheelHit.forwardSlip);

                break;
            case FunctWheels.BWD:

                wheelsCl[2].GetGroundHit(out wheelHit);
                Torque(wheelHit.forwardSlip);

                wheelsCl[3].GetGroundHit(out wheelHit);
                Torque(wheelHit.forwardSlip);

                break;
            case FunctWheels.AWD:
                for (int i = 0; i < 4; i++)
                {
                    wheelsCl[i].GetGroundHit(out wheelHit);
                    Torque(wheelHit.forwardSlip);
                }
                break;
        }
        Debug.Log(currentTorque);
        
    }
    private void Torque(float forwardSlip)
    {
        if (forwardSlip >= slipLimit && currentTorque >= 0)
        {
             currentTorque -= 10;
        }
        else
        {
            currentTorque += 10;
            if (currentTorque > maxTorque)
            {
                currentTorque = maxTorque;
            }
        }
    }
    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            wheelsCl[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(oldRot - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - oldRot) * .75f;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }
        oldRot = transform.eulerAngles.y;
    }
}
