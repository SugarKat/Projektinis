using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : MonoBehaviour {

    private CarControler car;

    private void Start()
    {
        car = GetComponent<CarControler>();
    }

    private void FixedUpdate()
    {
        float vertical, horizontal,space;
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        space = Input.GetAxis("Jump");

        car.Move(horizontal,vertical,vertical,space);
    }

}
