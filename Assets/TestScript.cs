using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public GridLayout gridLayout;
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Debug.Log(gridLayout.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        }
    }
}
