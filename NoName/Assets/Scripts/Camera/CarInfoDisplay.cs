using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarInfoDisplay : MonoBehaviour {

    public Text speed;
    public Slider torque;

    public void UpdateInfo(float _torque, float _speed)
    {
        speed.text = "Speed: " + _speed.ToString("00");
        torque.value = _torque;
    }
}
