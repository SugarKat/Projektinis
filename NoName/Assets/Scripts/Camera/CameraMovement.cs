using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    Transform target;
    public Vector3 cameraOffset;
    public Vector3 targetOffset;
    public Transform _camera;
    public bool staticFol = false;
    Vector3 posi;

	void Start ()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        _camera.localPosition = cameraOffset;
	}
	
	void Update ()
    {
        if (staticFol)
            StaticFollow();
        else
            DynamicFollow();
	}

    Vector3 GetLookDir(Transform _target)//Pagal judejimo krypti randa i kuria puse turi ziuret kamera
    {
        Vector3 dir = Vector3.zero;
        dir = _target.position + _target.GetComponent<Rigidbody>().velocity;
        dir.y = transform.position.y;
        posi = dir;
        //Debug.Log(dir.y);
        return dir;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(posi,Vector3.one);
    }
    void StaticFollow()//Kamera tiesiog seka masina
    {
        Quaternion quat = target.rotation;
        quat.x = 0f;
        quat.z = 0f;
        transform.position = target.position;
        transform.rotation = quat;
    }
    void DynamicFollow()
    {
        transform.position = target.position + targetOffset;
        Vector3 dir = GetLookDir(target);
        Quaternion rot = Quaternion.identity;
        if (target.GetComponent<Rigidbody>().velocity.magnitude > 0.5f)
        {
            transform.LookAt(dir);
        }
        else
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 2f);
        //transform.rotation = target.rotation;
    }
}
