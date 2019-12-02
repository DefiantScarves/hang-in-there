using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;



public class fade_to_white : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DoFadein());

    }

    //Fade In
    IEnumerator DoFadein ()
    {
        Debug.Log("Fading in.");
        CanvasGroup fade = GetComponent<CanvasGroup>();
        fade.interactable = false;
        fade.alpha = 1;
        while (fade.alpha > 0){
            fade.alpha -= Time.deltaTime/2;
            yield return null;
        }
       
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Start"))
            StartCoroutine(DoFadeOut());
    }

    IEnumerator DoFadeOut(){
        StopCoroutine(DoFadein());
        CanvasGroup fade = GetComponent<CanvasGroup>();
        Debug.Log("Fading Out");
        fade.alpha = 0;
        while (fade.alpha < 1) {
            fade.alpha += Time.deltaTime;
            if (fade.alpha == 1)
                LoadLevel("HDRPVS");
            yield return null;


        }

    }

    public void LoadLevel (string name) {
        Debug.Log("Loading level: " + name);
        SceneManager.LoadScene("HDRPVS");


    }


}
