using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagateMessages : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger enter");
    }
    void OnTriggerExit(Collider other)
    {
        Debug.Log("trigger exit");
    }
    void OnTriggerStay(Collider other)
    {
        Debug.Log("trigger stay");
    }
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("collision enter");
    }
    void OnCollisionExit(Collision other)
    {
        Debug.Log("collision exit");
    }
    void OnCollisionStay(Collision other)
    {
        Debug.Log("collision stay");
    }
}
