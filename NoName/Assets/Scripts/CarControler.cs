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

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
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
        input = Input.GetAxisRaw("Horizontal");
        Steer(input);
        ApplyDownForce();
	}

    void Drive(float input)
    {
        float torque;
        if (input < 0)
        {
            torque = reverseTorque;
        }
        else
            AdjustTorque(out torque);
        switch (driveWheels)
        {
            case FunctWheels.FWD:
                wheelsCl[0].motorTorque = (input * torque)/2f;
                wheelsCl[1].motorTorque = (input * torque)/2f;
                break;
            case FunctWheels.BWD:
                wheelsCl[2].motorTorque = (input * torque )/2;
                wheelsCl[3].motorTorque = (input * torque)/2;
                break;
            case FunctWheels.AWD:
                for (int i = 0; i < 4; i++)
                {
                    wheelsCl[i].motorTorque = (input * torque)/4;
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
    void AdjustTorque(out float _torque)
    {
        _torque = maxTorque;

        WheelHit wheelHit;

        switch(driveWheels)
        {
            case FunctWheels.FWD:

                wheelsCl[0].GetGroundHit(out wheelHit);
                if (wheelHit.forwardSlip >= slipLimit)
                {
                    _torque -= (Mathf.Pow(wheelHit.forwardSlip * 10, 2) - slipLimit) * 10 + (maxTorque/20);
                }
                wheelsCl[1].GetGroundHit(out wheelHit);
                if (wheelHit.forwardSlip >= slipLimit)
                {
                    _torque -= (Mathf.Pow(wheelHit.forwardSlip * 10, 2) - slipLimit) * 10 + (maxTorque/20);
                }

                break;
            case FunctWheels.BWD:

                wheelsCl[2].GetGroundHit(out wheelHit);
                if (wheelHit.forwardSlip >= slipLimit)
                {
                    _torque -= (wheelHit.forwardSlip*4 - slipLimit) * 10;
                }
                wheelsCl[3].GetGroundHit(out wheelHit);
                if (wheelHit.forwardSlip >= slipLimit)
                {
                    _torque -= (wheelHit.forwardSlip*4 - slipLimit) * 10;
                }
                break;
            case FunctWheels.AWD:
                for (int i = 0; i < 4; i++)
                {
                    wheelsCl[i].GetGroundHit(out wheelHit);
                    if (wheelHit.forwardSlip >= slipLimit)
                    {
                        _torque -= (wheelHit.forwardSlip*2 - slipLimit) * 10;
                    }
                }
                break;
        }
        Debug.Log(_torque/2);
    }
}