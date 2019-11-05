using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour {

    //how far the trees will move
	public float windStrength = 1f;
    //how quickly the trees will move
    public float windNoiseStep = 1f;

    public float timer = 0f;

    public Material[] materialsGreen;
    public Material[] materialsFlower;


    private void Update()
    {
        timer += Time.deltaTime*windNoiseStep;
    }
}
