using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBoidPosition : MonoBehaviour {

    private void OnTriggerExit(Collider other)
    {
        Vector3 currentPosition = other.gameObject.transform.position;
        if(Mathf.Abs(currentPosition.x) > transform.localScale.x / 2)
        {
            currentPosition.x = -currentPosition.x;
        }
        if (Mathf.Abs(currentPosition.z) > transform.localScale.z / 2)
        {
            currentPosition.z = -currentPosition.z;
        }
        other.gameObject.transform.position = currentPosition;
    }

}
