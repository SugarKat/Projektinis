using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    CarInfoDisplay crInfo;

    public float CurrentSpeed { get { return rb.velocity.magnitude * 3.6f; } }

    public Vector3 centerOfMass;
    public float maxTorque = 100f;
    public float reverseTorque = 200;
    public float brakeTorque = 500f;
    public float steeringAngle = 45f;
    public float downForce = 200f;

    float currentTorque;
    float oldRot;

	void Start ()
    {
        crInfo = GetComponent<CarInfoDisplay>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        currentTorque = 0;
	}

   

    public void Move (float steer,float accel,float brake,float handbrake)
    {
        for (int i = 0; i < 4; i++)
        {
            MeshUpdate(wheelsCl[i], wheelsObj[i]);
        }
        steer = Mathf.Clamp(steer, -1, 1);
        accel = Mathf.Clamp(accel, 0, 1);
        brake = -1*Mathf.Clamp(brake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        crInfo.UpdateInfo(currentTorque/maxTorque, CurrentSpeed);

        Steer(steer);
        Drive(accel,brake);
        CapSpeed();

        HandBrake(handbrake);

        SteerHelper();
        AdjustTorque(accel);
        ApplyDownForce();
	}

    void Drive(float accel,float footbrake)
    {
        switch (driveWheels)
        {
            case FunctWheels.FWD:
                wheelsCl[0].motorTorque = (accel * currentTorque)/2f;
                wheelsCl[1].motorTorque = (accel * currentTorque)/2f;
                break;
            case FunctWheels.BWD:
                wheelsCl[2].motorTorque = (accel * currentTorque)/2;
                wheelsCl[3].motorTorque = (accel * currentTorque)/2;
                break;
            case FunctWheels.AWD:
                for (int i = 0; i < 4; i++)
                {
                    wheelsCl[i].motorTorque = (accel * currentTorque)/4;
                }
                break;
        }
        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, rb.velocity) < 50f)
            {
                wheelsCl[i].brakeTorque = brakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                wheelsCl[i].brakeTorque = 0f;
                wheelsCl[i].motorTorque = -reverseTorque * footbrake;
            }
        }
    }
    void ApplyDownForce ()
    {
        rb.AddForce(-transform.up * rb.velocity.magnitude * 200f);
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
    }
    void MeshUpdate(WheelCollider wl,GameObject mesh)
    {
        Vector3 pos;
        Quaternion quat;
        wl.GetWorldPose(out pos, out quat);
        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
    void AdjustTorque(float _accel)
    {
        if (_accel <= .1f)
        {
            currentTorque -= 50;
            if (currentTorque < 0)
                currentTorque = 0;

        }
        else if (CurrentSpeed < 20f && currentTorque <= 2800f)
        {
            currentTorque += 120f;
        }
        else
        {
            WheelHit wheelHit;

            switch (driveWheels)
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
        }
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
            var turnadjust = (transform.eulerAngles.y - oldRot) * .5f;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }
        oldRot = transform.eulerAngles.y;
    }
}
