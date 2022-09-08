using System.Collections;
using System.Collections.Generic;
using Pancake.SOA;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    public StringVariable stringVariable;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(stringVariable.Value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
