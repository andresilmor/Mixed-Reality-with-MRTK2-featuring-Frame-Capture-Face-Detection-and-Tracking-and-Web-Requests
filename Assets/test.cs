using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, 1 << 6))
        {
            Debug.Log(hit.collider.gameObject.tag);
        } // TODO: Check -1
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
