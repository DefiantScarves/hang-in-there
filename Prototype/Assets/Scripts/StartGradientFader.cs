using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StartGradientFader : MonoBehaviour {

    public CanvasGroup start_gradient;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float pingPong = Mathf.PingPong(Time.time * 0.5f, 0.9f);
        start_gradient.alpha = (pingPong);
        
    }
}
