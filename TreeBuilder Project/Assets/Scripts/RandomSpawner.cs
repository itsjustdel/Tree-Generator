﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomSpawner : MonoBehaviour {
    public int gridSizeX;
    public int gridSizeZ;

    public int step = 5;

    public float animationSpeed = 0.1f;
    public bool newTree = false;
    public bool newFamily = true;

    public List<GameObject> trees = new List<GameObject>();

    public TreeInfo treeInfo;
    public ColourPicker colourPicker;

    public Material leavesMat;

    public Button newSpeciesButton;
    public Button newTreeButton;

    GlobalVariables gV;
    // Use this for initialization
    void Start()
    {
        //buttons
        Button btn0 = newSpeciesButton.GetComponent<Button>();
        btn0.onClick.AddListener(NewSpeciesClick);
        Button btn1 = newTreeButton.GetComponent<Button>();
        btn1.onClick.AddListener(NewTreeClick);

        gV = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalVariables>();

        treeInfo = gameObject.AddComponent<TreeInfo>();
        colourPicker = gameObject.AddComponent<ColourPicker>();
        //need to force new family on first build
        newFamily = true;

        StartCoroutine("Plant");
    }

    void NewSpeciesClick()
    {
        newFamily = true;
        newTree = true;
        Debug.Log("You have clicked the button! Species");
    }

    void NewTreeClick()
    {
        newFamily = false;
        newTree = true;
        Debug.Log("You have clicked the button Tree !");
    }

    private void Update()
    {
        if(newTree)
        {
            //get rid of old trees
            for (int i = 0; i < trees.Count; i++)
            {
                
                Destroy(trees[i]);
            }

            trees.Clear();

            //plant new guys
            StartCoroutine("Plant");


            //tell camera to make new target for smooth lerping
          //  Camera.main.GetComponent<CameraControl>().makeNewTarget = true;

            //reset flag
            newTree = false;


        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }


    }

    IEnumerator Plant()
    {
        for (int i = 0; i < gridSizeX; i += step)
        {
            for (int j = 0; j < gridSizeZ; j += step)
            {
                GameObject tree = new GameObject();
                tree.transform.position = new Vector3(i + Random.Range(-step/2,step/2), 0, j + Random.Range(-step / 2, step / 2));
                tree.name = "Tree";
                //choose colour now
                
                tree.AddComponent<ProceduralTree>();
                tree.AddComponent<WindAnimation>();

                //create new values for tree to based around?
                if (newFamily)
                {
                    //choose some colours
                    colourPicker.ChooseRandom();
                    //leaves
                    leavesMat = gV.materialsGreen[Random.Range(0, gV.materialsGreen.Length)];
                    //TreeInfo treeInfo = tree.AddComponent<TreeInfo>(); //adding on start to this gameobject (spawner)
                    //we can create tree types by changing the values in tree info

                    //how many sides the trunk has - we need surprisingly few
                    treeInfo.sides = 6;

                    //how fat the tree is to start with 
                    treeInfo.startingWidth = Random.Range(0.5f, 1.5f);//Probably need to use a coroutine to cut up building process if we want to go higher
                                                                      //treeInfo.startingWidth = 2f;
                                                                      //////finish this

                    treeInfo.radiusReduceAmount = Random.Range(0.05f, 0.1f);// treeInfo.startingWidth*.5f);
                                                                            //treeInfo.endingWidth = treeInfo.radiusReduceAmount;//just leave small0.05f
                    treeInfo.stepHeight = Random.Range(0.5f, 3f);

                    //bend amount and density to do
                    treeInfo.stepReduce = treeInfo.radiusReduceAmount * (Random.Range(0.1f, 1.5f));//lower is more elegant

                    //make leaf size relative to stretch?
                    treeInfo.leafSize = Random.Range(treeInfo.stepHeight * .2f, treeInfo.stepHeight);
                    //keep relatively thin
                    treeInfo.leafThickness = treeInfo.leafSize / Random.Range(5, 10);

                    treeInfo.minimumBranchWidthForLeaf = treeInfo.startingWidth / Random.Range(1.5f, 3f);

                    //which type of leaf, cube or sphere, (could add more shapes)
                    //at false by default - chance to change it
                    if (Random.value > 0.5f)
                        treeInfo.useSphereForLeaf = true;

                    if (Random.value > 0.5f)
                        treeInfo.useSphereForFlower = true;

                    if (Random.value > 0.5f)
                        treeInfo.randomiseLeafSize = true;

                    if (Random.value > 0.5f)
                        treeInfo.randomiseLeafHeight = true;
                }

                trees.Add(tree);

                yield return new WaitForEndOfFrame();
            }

            //tell camera to make new target for smooth lerping
            Camera.main.GetComponent<CameraControl>().makeNewTarget = true;
            
        }

        yield break;
    }
}
