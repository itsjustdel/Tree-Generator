using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindAnimation : MonoBehaviour {

    //bends a procedural tree
    
    public float speed = 24;
    public float flux = 10f;
    public float scale = 1f;//global wind speed?
    public float leafParentScale = 3f;//global wind speed?
    public float leafIndividualScale = 5f;//global wind speed?
    public float s;
    public float windStrength = 1f;
    private float leafScale;
    private float leafSpeed;
    public float leafFlux = 1f;
    Transform[] childTransforms;
    List<Quaternion> originalLocalRotationsBranches = new List<Quaternion>();
    List<Quaternion> originalLocalRotationsLeavesParents = new List<Quaternion>();
    List<Quaternion> originalLocalRotationsIndividual = new List<Quaternion>();
    
    

    List<Transform> leafParents = new List<Transform>();
    List<Transform> individualLeaves = new List<Transform>();//includes leaf and flower
    List<Transform> branches = new List<Transform>();


    public bool rotateBranches = true;
    public bool rotateLeafParents = false;
    public bool rotateLeavesIndividually = true;

    private GlobalVariables gV;

    private float timer = 0f;

    void Awake()
    {
        enabled = false;
    }
    // Use this for initialization
    void Start()
    {

        

        gV = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalVariables>();

        childTransforms = GetComponentsInChildren<Transform>();

        

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == transform)
                continue;


            if (childTransforms[i].CompareTag("Branch"))
                branches.Add(childTransforms[i]);

            else if (childTransforms[i].CompareTag("Leaves Parent"))
                leafParents.Add(childTransforms[i]);

            else if (childTransforms[i].CompareTag("Individual Leaf"))
                individualLeaves.Add(childTransforms[i]);

           
        }

        //save original rotations
        for (int i = 0; i < branches.Count; i++)
        {
            originalLocalRotationsBranches.Add(branches[i].transform.rotation);
        }
        for (int i = 0; i < leafParents.Count; i++)
        {
            originalLocalRotationsLeavesParents.Add(leafParents[i].transform.rotation);
        }

        for (int i = 0; i < individualLeaves.Count; i++)
        {
            originalLocalRotationsIndividual.Add(individualLeaves[i].transform.rotation);
        }
       
    }
	
	// Update is called once per frame
	void Update ()
    {
      

      

        ///make ree more bendy the higher it is /***** is this working? being overwritten at the moment bytr good idea?
        s = s / childTransforms.Length;



        //the idea behind this wind algorith is palce the tree inside a 3d nois space and drag the noise over it using time.time as the variable
        //this will manipulate the tree
     

        if (rotateBranches)
        {
            //branches
            for (int i = 0; i < branches.Count; i++)
            {

                //enter the bone's position in to 3d noise
                float perlin3d = Perlin.Noise(branches[i].position.x + gV.timer, branches[i].position.y + gV.timer, branches[i].position.z + gV.timer);
                s = perlin3d * scale * gV.windStrength;


                branches[i].rotation = originalLocalRotationsBranches[i] * Quaternion.Euler(s, 0, 0);
            }
           
        }
        //leafParents 
        if (rotateLeafParents)
        {
            for (int i = 0; i < leafParents.Count; i++)
            {

                //enter the bone's position in to 3d noise
                float perlin3d = Perlin.Noise(leafParents[i].position.x + gV.timer, leafParents[i].position.y+ gV.timer, leafParents[i].position.z + gV.timer);
                s = perlin3d * leafParentScale * gV.windStrength;


                leafParents[i].rotation = originalLocalRotationsLeavesParents[i] * Quaternion.Euler(s, 0, 0);
            }
        }
        if(rotateLeavesIndividually)
        {
            for (int i = 0; i < individualLeaves.Count; i++)
            {

                //enter the bone's position in to 3d noise
                float perlin3d = Perlin.Noise(individualLeaves[i].position.x + gV.timer, individualLeaves[i].position.y + gV.timer, individualLeaves[i].position.z + gV.timer);
                s = perlin3d * leafIndividualScale * gV.windStrength;


                individualLeaves[i].rotation = originalLocalRotationsIndividual[i] * Quaternion.Euler(s, 0, 0);
            }
        }



    }

    float Oscillate(float time, float speed, float scale)
    {
        return Mathf.Cos(time * speed / Mathf.PI) * scale;
    }
}
