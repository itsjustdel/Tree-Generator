using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour {
    public int gridSizeX;
    public int gridSizeZ;

    public int step = 5;
    // Use this for initialization
    void Start ()
    {
        StartCoroutine("Plant");
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
                tree.AddComponent<ProceduralTree>();
                tree.AddComponent<WindAnimation>();

                TreeInfo treeInfo = tree.AddComponent<TreeInfo>();
                //we can create tree types by changing the values in tree info

                //how many sides the trunk has - we need surprisingly few
                treeInfo.sides = 6;

                //how fat the tree is to start with 
                treeInfo.startingWidth = Random.Range(0.5f,1.5f);//Probably need to use a coroutine to cut up building process if we want to go higher
                //treeInfo.startingWidth = 2f;
                //////finish this

                treeInfo.radiusReduceAmount = Random.Range(0.05f, treeInfo.startingWidth*.5f);
                //treeInfo.endingWidth = treeInfo.radiusReduceAmount;//just leave small0.05f
                treeInfo.stepHeight = Random.Range(0.5f, 3f);

                //bend amount and density to do
                treeInfo.stepReduce = treeInfo.radiusReduceAmount*(Random.Range(0.1f,1.5f));//lower is more elegant

                //make leaf size relative to stretch?
                treeInfo.leafSize = Random.Range(treeInfo.stepHeight*.2f , treeInfo.stepHeight);
                //keep relatively thin
                treeInfo.leafThickness = treeInfo.leafSize / Random.Range(5 , 10);

                treeInfo.minimumBranchWidthForLeaf = treeInfo.startingWidth / Random.Range(1.5f,3f);

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

                yield return new WaitForEndOfFrame();
            }
        }

        yield break;
    }
}
