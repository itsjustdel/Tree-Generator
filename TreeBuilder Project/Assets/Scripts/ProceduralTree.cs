using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTree : MonoBehaviour {

    

    public TreeInfo treeInfo;

    //using so we can control how tree builds. Recurring functions interject in a branches building. This list will help us finish
    //our current branch before starting another one. This is only for aesthetics.
    private List<BuildInfo> buildInfos = new List<BuildInfo>();

    private List<GameObject> branches = new List<GameObject>();


    public int animationStepBranch = 1;
    public int animationStepLeaves = 1;


    public GameObject groundPlane;

    int frameWait = 0;
    int materialIndexGreen = 0;

    GlobalVariables gV;
    Material leavesMat;
    Material barkMat;
    Material flowerMat;

    float animationSpeed;

    public bool buildFinished = false;
    void Start()
    {
        gV = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalVariables>();
        animationSpeed = GameObject.FindGameObjectWithTag("Code").GetComponent<RandomSpawner>().animationSpeed;

        ChooseMaterials();

        //Randomise values used to define tree shape
        //--to do
        //class which has all the info we need to make unique tree - given to game object when spawning
        treeInfo = GetComponent<TreeInfo>();

        //sides needs to be even
        if (treeInfo.sides % 2 != 0)
            treeInfo.sides += 1;

        //tree is building around vector.zero, move transform to zero, then back again after build.
        Vector3 originalPosition = transform.position;
        transform.position = Vector3.zero;

        //function which controls making the tree
        BuildTree();

        //move back
        transform.position = originalPosition;


    }

    void ChooseMaterials()
    {
        leavesMat = gV.materialsGreen[Random.Range(0, gV.materialsGreen.Length)];
        barkMat= Resources.Load("Brown0") as Material;
        flowerMat = GetComponent<ColourPicker>().matsAndShades[0].material; ;// gV.materialsFlower[Random.Range(0, gV.materialsFlower.Length)];

    }
    private void Update()
    {
        
        if (buildFinished == true)
        {
            // enabled = false;
            GetComponent<WindAnimation>().enabled = true;
        }

        
    }

    IEnumerator BuildTreeInfos()
    {

        while (buildInfos.Count > 0)
        {
            //build branch then remove from list
            GameObject branch = BuildBranch(buildInfos[0]);
            //keep a track of what we have built
            if (branch != null)
                branches.Add(branch);

          
            //remove, we have built this
            buildInfos.RemoveAt(0);

            //wait an amount of time then build another branch
            yield return new WaitForSeconds(animationSpeed);               
            

        }

        //once we get here, fly the flag
        buildFinished = true;

        //now combine meshes for performance
       
        CombineSkinnedMeshRenderers(branches, barkMat);
        List<GameObject> leaves = new List<GameObject>();
        List<GameObject> flowers = new List<GameObject>();
        for (int i = 0; i < branches.Count; i++)
        {
            leaves.Add(branches[i].transform.GetChild(0).gameObject);
            flowers.Add(branches[i].transform.GetChild(1).gameObject);
        }
        CombineSkinnedMeshRenderers(leaves, leavesMat);

       
        CombineSkinnedMeshRenderers(flowers, flowerMat);


    }

    void BuildTree()
    {

        //start the recurring function which will build the tree
        BuildInfo buildInfo = new BuildInfo();
        buildInfo.startRotation = Quaternion.identity;
        buildInfo.currentWidth = treeInfo.startingWidth;
        buildInfo.currentHeight = treeInfo.stepHeight;
        buildInfo.splittingBranch = true;
        buildInfo.branchingPivot = gameObject;
        //start recurring function

        buildInfos.Add(buildInfo);
        //GameObject branch = BuildBranch(buildInfo);
        //branch.name = "First Branch";//
        StartCoroutine("BuildTreeInfos");
        return;
        /*
        //keep a track of what we have built
        if (branch != null)
            branches.Add(branch);

        brown = Resources.Load("Brown0") as Material;
        CombineSkinnedMeshRenderers(branches, brown);
        List<GameObject> leaves = new List<GameObject>();
        List<GameObject> flowers = new List<GameObject>();
        for (int i = 0; i < branches.Count; i++)
        {
            leaves.Add(branches[i].transform.GetChild(0).gameObject);
            flowers.Add(branches[i].transform.GetChild(1).gameObject);
        }
        CombineSkinnedMeshRenderers(leaves, green);

        yellow = Resources.Load("Yellow0") as Material;
        CombineSkinnedMeshRenderers(flowers, yellow);
        */

    }

    void CombineSkinnedMeshRenderers(List<GameObject> parents, Material material)
    {

        //now we have built all branches, let's combine the renderers to reduce draw calls to the gpu (renders faster bascially)

        GameObject combinedSkinnedParent = NewBatchObject();
        Mesh mesh = combinedSkinnedParent.GetComponent<MeshFilter>().mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Transform> bones = new List<Transform>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        int verticeLimit = 500;//needs tested
        int verticeCountBeforeAdding = 0;
        int boneCount = 0;
        for (int i = 0; i < parents.Count; i++)
        {

            parents[i].GetComponent<SkinnedMeshRenderer>().enabled = false;
            // parents[i].name = i.ToString();
            //parents on parent
            Vector3[] verticesFromBranch = parents[i].GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < verticesFromBranch.Length; j++)
            {
                vertices.Add(verticesFromBranch[j]);
            }

            int[] trianglesFromBranch = parents[i].GetComponent<MeshFilter>().mesh.triangles;
            for (int j = 0; j < trianglesFromBranch.Length; j++)
            {
                int thisTri = trianglesFromBranch[j];
                //Debug.Log(thisTri + verticeCountBeforeAdding);
                triangles.Add(thisTri + verticeCountBeforeAdding);
            }

            //add bones
            Transform[] thisBones = parents[i].GetComponent<SkinnedMeshRenderer>().bones;
            for (int j = 0; j < thisBones.Length; j++)
            {
                bones.Add(thisBones[j]);
            }
            //add bone weights
            BoneWeight[] thisBoneWeights = parents[i].GetComponent<MeshFilter>().mesh.boneWeights;
            for (int j = 0; j < thisBoneWeights.Length; j++)
            {
                //we need to alter the index in the boneweight
                thisBoneWeights[j].boneIndex0 += boneCount;
                boneWeights.Add(thisBoneWeights[j]);
            }


            if (vertices.Count > verticeLimit)
            {
                //asign and reset

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().sharedMesh = combinedSkinnedParent.GetComponent<MeshFilter>().mesh;
                combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().bones = bones.ToArray();
                mesh.boneWeights = boneWeights.ToArray();
                mesh.bindposes = BindPoses(bones.ToArray());
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().sharedMaterial = material;

                combinedSkinnedParent = NewBatchObject();
                mesh = combinedSkinnedParent.GetComponent<MeshFilter>().mesh;
                vertices = new List<Vector3>();
                triangles = new List<int>();
                bones = new List<Transform>();
                boneWeights = new List<BoneWeight>();

                verticeCountBeforeAdding = 0;
                boneCount = 0;

                continue;
            }

            //add to a counter so triangle loop can add on top of what we have already entered
            //keep track for bones too
            verticeCountBeforeAdding += verticesFromBranch.Length;
            boneCount += thisBones.Length;


        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().sharedMesh = combinedSkinnedParent.GetComponent<MeshFilter>().mesh;
        combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().bones = bones.ToArray();
        mesh.boneWeights = boneWeights.ToArray();
        mesh.bindposes = BindPoses(bones.ToArray());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        combinedSkinnedParent.GetComponent<SkinnedMeshRenderer>().sharedMaterial = material;
    }


    private int CompareYPos(GameObject a, GameObject b)
    {
        return System.Math.Sign(a.transform.position.y - b.transform.position.y);
    }

    GameObject NewBatchObject()
    {
        GameObject batchObject = new GameObject();
        batchObject.transform.parent = transform;
        batchObject.name = "Batched";
        batchObject.AddComponent<MeshFilter>();
        batchObject.AddComponent<SkinnedMeshRenderer>();

        return batchObject;
    }

    GameObject BuildBranch(BuildInfo buildInfo)
    {
        //recursive function can be passed parameters which will immediately exit the build loop, ew can just sip and return now so we don't get any empty objects
        if (buildInfo.currentWidth < treeInfo.endingWidth)
            return null;

        //for each branch we will make a game object with its own mesh renderer and mesh filter
        GameObject branch = new GameObject();
        branch.transform.position = transform.position;
        branch.transform.parent = transform;
        branch.name = "Branch";

        //components Unity needs to render objects
        MeshFilter meshFilter = branch.AddComponent<MeshFilter>();
        SkinnedMeshRenderer meshRenderer = branch.AddComponent<SkinnedMeshRenderer>();
        //mesh data - branches
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<GameObject> bones = new List<GameObject>();

        //leaves
        GameObject leaves = new GameObject();
        leaves.transform.position = transform.position;
        leaves.transform.parent = branch.transform;
        leaves.name = "Leaves";
        MeshFilter meshFilterLeaves = leaves.AddComponent<MeshFilter>();
        SkinnedMeshRenderer meshRendererLeaves = leaves.AddComponent<SkinnedMeshRenderer>();
        //mesh data - leaves

        //keep track of when the branch gets thin enough to make leaves
        List<GameObject> pivotsForLeaves = new List<GameObject>();

        Mesh leavesMesh = new Mesh();
        List<GameObject> leafBones = new List<GameObject>();
        List<Vector3> leafVertices = new List<Vector3>();
        List<int> leafTriangles = new List<int>();
        List<BoneWeight> leavesWeights = new List<BoneWeight>();

        //flowers
        GameObject flowers = new GameObject();
        flowers.transform.position = transform.position;
        flowers.transform.parent = branch.transform;
        flowers.name = "Flowers";
        MeshFilter meshFilterFlowers = flowers.AddComponent<MeshFilter>();
        SkinnedMeshRenderer meshRendererFlowers = flowers.AddComponent<SkinnedMeshRenderer>();
        //mesh data - leaves
        Mesh flowersMesh = new Mesh();
        List<GameObject> flowersBones = new List<GameObject>();
        List<Vector3> flowersVertices = new List<Vector3>();
        List<int> flowersTriangles = new List<int>();
        List<BoneWeight> flowersWeights = new List<BoneWeight>();

        //for loop working out where the branch builds to, also decides if other branches are to split off and start building too 
        CreateBranchBones(out bones, out pivotsForLeaves, out vertices, out triangles, buildInfo.currentWidth, buildInfo.currentHeight, buildInfo.splittingBranch, buildInfo.branchingPivot, buildInfo.startRotation, bones, pivotsForLeaves, vertices, triangles);


        //now set weights so we can animate tree with a skinned mesh renderer
        List<BoneWeight> weights = SetWeightsBranch(bones, vertices);
        //skinned renderer wants a list of Transforms, /transform/ the gameobject list
        Transform[] bonesTransforms = new Transform[bones.Count];
        for (int i = 0; i < bones.Count; i++)
        {
            bonesTransforms[i] = bones[i].transform;
        }

        AddMeshInfo(meshFilter, meshRenderer, vertices, triangles, bones, weights, "Branch");

        //we can add leaves to the branches
        for (int i = 0; i < pivotsForLeaves.Count; i++)
        {
            //add new leaves to leaf vertices and bones list
            leafBones = LeavesCombined(out leafVertices, out leafTriangles, out leavesWeights, pivotsForLeaves[i], leafVertices, leafTriangles, leafBones, leavesWeights);
        }

        AddMeshInfo(meshFilterLeaves, meshRendererLeaves, leafVertices, leafTriangles, leafBones, leavesWeights, "Leaves");

        //we can add flowers to the leaves
        for (int i = 0; i < leafBones.Count; i++)
        {
            //make some flowers?
            float flowerProbabilty = .5f;//global
            if (Random.value < flowerProbabilty)
            {

                flowersBones = FlowersCombined(out flowersVertices, out flowersTriangles, out flowersWeights, leafBones[i], flowersVertices, flowersTriangles, flowersBones, flowersWeights);
            }
        }

        AddMeshInfo(meshFilterFlowers, meshRendererFlowers, flowersVertices, flowersTriangles, flowersBones, flowersWeights, "Flowers");

        for (int i = 0; i < leafBones.Count; i++)
        {
            //   GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //  c.transform.position = leafBones[i].transform.position;
            //  c.transform.localScale *= 0.2f;
        }

        return branch;


    }

    void CreateBranchBones(out List<GameObject> bonesToReturn, out List<GameObject> pivotsForLeavesToReturn, out List<Vector3> verticesToReturn, out List<int> trianglesToReturn, float currentWidth, float currentHeight, bool splittingBranch, GameObject branchingPivot, Quaternion startRotation, List<GameObject> bonesPassed, List<GameObject> pivotsForLeavesPassed, List<Vector3> verticesPassed, List<int> trianglesPassed)
    {
        List<GameObject> bones = new List<GameObject>(bonesPassed);
        List<GameObject> pivotsForLeaves = new List<GameObject>(pivotsForLeavesPassed);
        List<Vector3> vertices = new List<Vector3>(verticesPassed);
        List<int> triangles = new List<int>(trianglesPassed);

        GameObject previousPivot = null;
        Quaternion lastRotation = startRotation;
        for (float i = currentWidth, j = currentHeight; i > treeInfo.endingWidth; i -= treeInfo.radiusReduceAmount, j -= treeInfo.stepReduce)
        {
            if (splittingBranch)
            {
                //    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //   c.transform.position = branchingPivot.transform.position;
                //   c.transform.name = "Branchin Pivot";
            }

            //create a ring of points with given width and add to mesh points lists
            // next centre point - we will pivot away from last point

            Vector3 centre = branchingPivot.transform.position;
            if (!splittingBranch)
                centre = previousPivot.transform.position;

            //we need this radius size if we are starting a branch, so work out where we are heading to
            // float nextRadiusSize = i - radiusReduceAmount;
            List<Vector3> thisRing = CreateRing(i - treeInfo.radiusReduceAmount, Vector3.zero);

            //we will use Unity's gameobject and transform class to rotate these points to face the last centre
            //We could surely do this without using Unity's built in classes but this way lets me see exactly the rotations and positions in the editor
            //it also makes it much easier letting Unity do the rotational math
            GameObject pivotObject = new GameObject();
            pivotObject.transform.parent = branchingPivot.transform;
            //Make tidy by parenting to this gameobject 
            pivotObject.transform.position = branchingPivot.transform.position;
            if (!splittingBranch)
            {
                //use the previous pivot's rotation to create a forward direction
                pivotObject.transform.position = previousPivot.transform.position + previousPivot.transform.up * j;
                pivotObject.transform.parent = previousPivot.transform;
            }

            List<GameObject> rotatedChildren = new List<GameObject>();//optimisie, use this as look up table
            //make child game objects to be rotated
            for (int a = 0; a < thisRing.Count; a++)
            {
                GameObject armPoint = new GameObject();
                armPoint.transform.parent = pivotObject.transform;
                armPoint.transform.localPosition = thisRing[a];

                rotatedChildren.Add(armPoint);
                //armPoint.transform.parent = pivotObject.transform;
            }

            //set limits for random range, defines how the tree bends
            //work out how far are we towards the end, we will use this value to make the tree bend the higher it goes

            float fullWidth = treeInfo.startingWidth - treeInfo.endingWidth;
            float p = 100f - ((i / fullWidth) * 100);

            float maxRange = 5f;
            //make angle more extreme if we are making a new branch, don't do on first ring
            if (splittingBranch && branchingPivot.transform.position != Vector3.zero)
                maxRange = 90f;

            //set values for rotation of new pivot
            float x = Random.Range(-maxRange, maxRange) * treeInfo.bendAmount * p * 0.01f;//var?
            float y = Random.Range(-0, 0) * treeInfo.bendAmount * p * 0.01f;
            float z = Random.Range(-maxRange, maxRange) * treeInfo.bendAmount * p * 0.01f;

            pivotObject.transform.rotation = lastRotation * Quaternion.Euler(x, y, z);
            pivotObject.name = "Pivot Object";
            pivotObject.tag = "Branch";
            if (splittingBranch)
                pivotObject.name = "Splitting  Object";
            //now read new child transform positions
            List<Vector3> rotatedPositions = new List<Vector3>();
            for (int a = 0; a < pivotObject.transform.childCount; a++)
            {
                //get rotated child transform and add an upwards direction to it defined by pivot object's rotation
                Vector3 ringPoint = pivotObject.transform.GetChild(a).transform.position;
                rotatedPositions.Add(ringPoint);
            }

            //add new positions to vertices
            vertices.AddRange(rotatedPositions);

            //add triangles while we are here - I like to add triangles as I go when building procedural meshes instead of writing an all 
            //encompassing for loop once we have created all the vertices
            //add one to sides since whenj creating ring, I added a closing point for the loop/ring - just makes this loop nicer, otherwise I would ahve had to add another few lines to close the gap between the last and first point
            if (!splittingBranch)
            {
                int sidesForLoop = treeInfo.sides + 1;
                for (int a = 0; a < sidesForLoop - 1; a++)
                {
                    //attach to previous ring
                    triangles.Add(vertices.Count - sidesForLoop * 2 + a);
                    triangles.Add(vertices.Count - sidesForLoop * 2 + 1 + a);
                    //this triangle is from current ring
                    triangles.Add(vertices.Count - sidesForLoop + a);
                    //now do inverted trianges

                    //attach to previous ring
                    triangles.Add(vertices.Count - sidesForLoop * 2 + a + 1);
                    //these triangles are from current ring
                    triangles.Add(vertices.Count - sidesForLoop + a + 1);
                    triangles.Add(vertices.Count - sidesForLoop + a);

                }
            }

            //now we have built the wood section, let's add some leaves       
            //check if branches are thin enough to build leaves yer, stops leaves building on trunk
            //also, don't build on first split - give some space to make a branch first
            if (i < treeInfo.minimumBranchWidthForLeaf && !splittingBranch)
            {
                //add to list to build later
                pivotsForLeaves.Add(pivotObject);
            }



            //split this branch?
            bool split = false;

            if (Random.value < treeInfo.branchDensity)
                split = true;

            if (split)
            {
                //pass parameters to recurring function
                for (int a = 0; a < 1; a++)
                {
                    //make enxt branch smaller- if it is growing out another branch, it won't be bigger than it
                    float nextBranchWidth = i - treeInfo.radiusReduceAmount * 2;
                    //do the same for length
                    float nextBranchLength = j - treeInfo.stepReduce * 2;
                    pivotObject.name = "passed pivot";

                    BuildInfo buildInfo = new BuildInfo();
                    buildInfo.startRotation = pivotObject.transform.rotation;
                    buildInfo.currentWidth = nextBranchWidth;
                    buildInfo.currentHeight = nextBranchLength;
                    buildInfo.splittingBranch = true;
                    buildInfo.branchingPivot = pivotObject;

                    buildInfos.Add(buildInfo);
                    //GameObject nextBranch = BuildBranch(buildInfo);
                    /*
                    //keep a track of what we have built - using?
                    if (nextBranch != null)
                    {
                        //object which holds renderer
                        nextBranch.transform.parent = transform;//unsure
                        branches.Add(nextBranch);
                    }
                    */
                }
            }

            //set flags for next loop cycle

            //update for next loop

            lastRotation = pivotObject.transform.rotation;

            //tidy up game objects - we don't need these points ( optimisation, instantiate game objects and use look up table, then re -use objects)

            for (int a = 0; a < rotatedChildren.Count; a++)
            {
                Destroy(rotatedChildren[a]);
            }

            //add pivot to bones list
            bones.Add(pivotObject);

            previousPivot = pivotObject;

            //reset flag so we don't keep splitting
            splittingBranch = false;

        }

        bonesToReturn = bones;
        pivotsForLeavesToReturn = pivotsForLeaves;
        verticesToReturn = vertices;
        trianglesToReturn = triangles;
    }

    void AddMeshInfo(MeshFilter meshFilter, SkinnedMeshRenderer meshRenderer, List<Vector3> vertices, List<int> triangles, List<GameObject> bones, List<BoneWeight> weights, string type)
    {
        Mesh mesh = new Mesh();
        //flowers
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        meshFilter.mesh = mesh;

        //give colour/ material
        if (type == "Branch")
            meshRenderer.sharedMaterial = barkMat;// Resources.Load() as Material;
        else if (type == "Leaves")
            meshRenderer.sharedMaterial = leavesMat;
        else if (type == "Flowers")
            meshRenderer.sharedMaterial = flowerMat;

        //now set weights so we can animate tree with a skinned mesh renderer

        //skinned renderer wants a list of Transforms, /transform/ the gameobject list
        Transform[] bonesTransforms = new Transform[bones.Count];
        for (int i = 0; i < bones.Count; i++)
        {
            bonesTransforms[i] = bones[i].transform;
        }

        mesh.bindposes = BindPoses(bonesTransforms);
        mesh.boneWeights = weights.ToArray(); ;
        meshRenderer.bones = bonesTransforms;
        // meshRenderer.rootBone = branchingPivot.transform;//not sure about root bone
        meshRenderer.sharedMesh = mesh;
        //use Unity magic to work out normals and bounds
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    List<BoneWeight> SetWeightsBranch(List<GameObject> bones, List<Vector3> vertices)
    {

        List<BoneWeight> weights = new List<BoneWeight>();

        //for each bone/pivot object
        for (int i = 0; i < bones.Count; i++)
        {
            //for each ring
            for (int j = 0; j < treeInfo.sides + 1; j++)
            {

                //add vertices to bone

                // int ringNumber = i;
                //int thisVertice = j;
                int currentVertice = i * (treeInfo.sides + 1) + j;
                //add this to current bone = i
                //weights[currentVertice].boneIndex0 = i;
                //weights[currentVertice].weight0 = 1;

                BoneWeight bW = new BoneWeight();
                bW.boneIndex0 = i;
                bW.weight0 = 1;
                weights.Add(bW);

                if (i == 0)
                {
                    //   GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //    c.transform.localScale *= 0.2f;
                    //    c.name = currentVertice.ToString();
                    //   c.transform.position = vertices[currentVertice];
                }
            }
        }

        return weights;
    }

    Matrix4x4[] BindPoses(Transform[] bones)
    {
        Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
        for (int i = 0; i < bindPoses.Length; i++)
        {
            bindPoses[i] = bones[i].worldToLocalMatrix * transform.localToWorldMatrix;

        }
        return bindPoses;
    }


    void LeavesAndFloweresIndividual(GameObject pivotObject)
    {
        //every time a new pivot is created ( a segment of branch basically, create some leaves for it
        //create a parent, we will child some basic leaves to it then, rotate leaves parent object 
        GameObject leavesParent = new GameObject();
        leavesParent.name = "Leaves Parent";
        leavesParent.tag = "Leaves Parent";
        int leavesAmount = Random.Range(4, 4);


        //now move parent with leaves attached to pivot position
        leavesParent.transform.position = pivotObject.transform.position + transform.position;


        //control which type of rotation we want for leaves
        //0 - no rotation
        //1 - rotate only on y axis - will keep leaves parallel to ground
        //2 = full random rotation
        //3 match pivot object's rotation - gives a semi organised look to the leaves layout without beign too perfect looking

        int rotationType = 1;
        //choose which primitive shape to use for leaves - we could add a prefab here, or create more intricate shapes procedurally
        PrimitiveType leafShape = PrimitiveType.Cube;

        if (treeInfo.useSphereForLeaf)
            leafShape = PrimitiveType.Sphere;

        PrimitiveType flowerShape = PrimitiveType.Cube;
        if (treeInfo.useSphereForFlower)
            flowerShape = PrimitiveType.Sphere;

        //temp var in case we want to randomsie it
        float leafSize = treeInfo.leafSize;
        float leafThickness = treeInfo.leafThickness;

        for (int i = 0; i < 360; i += 360 / leavesAmount)
        {
            //create a cpntaining object which will be used for rotatin, We need this at a nuniform scale, other wise things get wonky if you keep childing
            //non uniform objects to it...
            GameObject holder = new GameObject();
            holder.transform.position = leavesParent.transform.position;
            holder.transform.name = "Leaf Holder";
            holder.transform.parent = leavesParent.transform;
            holder.transform.tag = "Leaf Holder";

            if (treeInfo.randomiseLeafSize)
            {
                //change for eavery leaf built if this flag is set
                leafSize = Random.Range(treeInfo.stepHeight * .2f, treeInfo.stepHeight);//same values as spawner script (link vars)

            }
            if (treeInfo.randomiseLeafHeight)
            {
                leafThickness = treeInfo.leafSize / Random.Range(5, 10);
            }
            //use Unity procedural primitive 
            GameObject leaf = GameObject.CreatePrimitive(leafShape);
            //we don't need the collider, it will slow down performance
            Destroy(leaf.GetComponent<Collider>());
            leaf.name = "Leaf";
            leaf.transform.localScale = new Vector3(leafSize, leafThickness, leafSize);
            //create an arm and spin it using the index in the for loop as the angle
            //using quaternions here as it is one easy line to use
            leaf.transform.position = leavesParent.transform.position + Quaternion.Euler(0, i, 0) * (Vector3.right * (leafSize * .5f));
            //change rotation
            if (rotationType == 0)
            {
                //don't do anything
            }
            else if (rotationType == 1)
            {
                //rotate only around y axis
                //add a little wiggle room for the leaf so it isn't always flat 
                float wiggleAmount = 10f;
                leaf.transform.rotation = Quaternion.Euler(Random.Range(-wiggleAmount, wiggleAmount), Random.Range(0, 360 / leavesAmount), Random.Range(-wiggleAmount, wiggleAmount));
            }
            else if (rotationType == 2)
            {
                //random
                leaf.transform.rotation = Random.rotationUniform;
            }

            //add colour
            leaf.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green0") as Material;
            //attach to parent object
            leaf.transform.parent = holder.transform;
            //roate parent 
            if (rotationType == 3)
                leaf.transform.rotation = pivotObject.transform.rotation;

            //add flowers
            bool addFlowers = true;
            if (addFlowers)
            {
                float flowerProbabilty = .5f;//global
                if (Random.value > flowerProbabilty)
                {

                    GameObject flower = GameObject.CreatePrimitive(flowerShape);//global
                    Destroy(flower.GetComponent<Collider>());
                    flower.name = "Flower";
                    //flower.transform.parent = leaf.transform;//or branhc?

                    float flowerSize = Random.Range(0.1f, .5f);
                    flower.transform.localScale = new Vector3(flowerSize, flowerSize, flowerSize);
                    flower.transform.parent = holder.transform;

                    flower.transform.position = pivotObject.transform.position + transform.position + Random.rotation * Vector3.right * leafSize;
                    flower.transform.localScale = new Vector3(flowerSize, flowerSize, flowerSize);
                    flower.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow0") as Material;
                }


            }
        }


        //attach leaf to it's branch
        leavesParent.transform.parent = pivotObject.transform;

        bool combineLeavesAndFlowers = true; //global - use with rotation seetings, if on we can't rirtae leaves individually and keep nice frames
        if (combineLeavesAndFlowers)
        {
            CombineMeshes(leavesParent.transform);
        }

    }

    List<GameObject> LeavesCombined(out List<Vector3> verticesToReturn, out List<int> trianglesToReturn, out List<BoneWeight> boneWeightsToReturn, GameObject pivotObject, List<Vector3> verticesPassed, List<int> trianglesPassed, List<GameObject> bonesPassed, List<BoneWeight> boneWeightsPassed)
    {
        //method receives a list of bones and positions, creates more bones and position and adds them to the received list, then returns the lists

        List<GameObject> bones = new List<GameObject>(bonesPassed);
        List<BoneWeight> tempBoneWeights = new List<BoneWeight>(boneWeightsPassed);
        List<Vector3> tempVertices = new List<Vector3>(verticesPassed);
        List<int> tempTriangles = new List<int>(trianglesPassed);
        //every time a new pivot is created ( a segment of branch basically, create some leaves for it
        //create a parent, we will child some basic leaves to it then, rotate leaves parent object 
        GameObject leavesParent = new GameObject();
        leavesParent.name = "Leaves Parent";
        leavesParent.tag = "Leaves Parent";
        int leavesAmount = Random.Range(4, 4);


        //now move parent with leaves attached to pivot position
        leavesParent.transform.position = pivotObject.transform.position + transform.position;


        //control which type of rotation we want for leaves
        //0 - no rotation
        //1 - rotate only on y axis - will keep leaves parallel to ground
        //2 = full random rotation
        //3 match pivot object's rotation - gives a semi organised look to the leaves layout without beign too perfect looking

        int rotationType = 1;
        //choose which primitive shape to use for leaves - we could add a prefab here, or create more intricate shapes procedurally
        PrimitiveType leafShape = PrimitiveType.Cube;

        if (treeInfo.useSphereForLeaf)
            leafShape = PrimitiveType.Sphere;

        //temp var in case we want to randomsie it
        float leafSize = treeInfo.leafSize;
        float leafThickness = treeInfo.leafThickness;


        //make a primitive so we can steal it's vertice data - this could be any shape..
        GameObject leafPrimitive = GameObject.CreatePrimitive(leafShape);//opto can make one in Start and refer to it

        for (int i = 0, j = 0; i < 360; i += 360 / leavesAmount, j++)
        {
            //change size and shape every leaf?
            if (treeInfo.randomiseLeafSize)
            {
                //change for eavery leaf built if this flag is set
                leafSize = Random.Range(treeInfo.stepHeight * .2f, treeInfo.stepHeight);//same values as spawner script (link vars)

            }
            if (treeInfo.randomiseLeafHeight)
            {
                leafThickness = treeInfo.leafSize / Random.Range(5, 10);
            }

            //use Unity procedural primitive 
            GameObject leafBone = new GameObject();// GameObject.CreatePrimitive(leafShape);
            //we don't need the collider, it will slow down performance
            //Destroy(leaf.GetComponent<Collider>());
            leafBone.name = "Leaf";
            leafBone.tag = "Individual Leaf";
            //add this object to the bones list
            bones.Add(leafBone);

            //create an arm and spin it using the index (i) in the for loop as the angle
            //using quaternions here as it is one easy line to use
            leafBone.transform.position = leavesParent.transform.position + Quaternion.Euler(0, i, 0) * (Vector3.right * (leafSize * .5f));
            //change rotation
            if (rotationType == 0)
            {
                //don't do anything
            }
            else if (rotationType == 1)
            {
                //rotate only around y axis
                //add a little wiggle room for the leaf so it isn't always flat 
                float wiggleAmount = 10f;
                leafBone.transform.rotation = Quaternion.Euler(Random.Range(-wiggleAmount, wiggleAmount), Random.Range(0, 360 / leavesAmount), Random.Range(-wiggleAmount, wiggleAmount));
            }
            else if (rotationType == 2)
            {
                //random
                leafBone.transform.rotation = Random.rotationUniform;
            }

            //scale the mesh
            Vector3[] leafVertices = leafPrimitive.GetComponent<MeshFilter>().mesh.vertices;
            //addd vertices to the list which is being returned by this function
            for (int a = 0; a < leafVertices.Length; a++)
            {
                leafVertices[a] = new Vector3(leafVertices[a].x * leafSize, leafVertices[a].y * leafThickness, leafVertices[a].z * leafSize);


                //add local vertice position to global leaf position, combined with rotation to give the ultimate position
                tempVertices.Add((leafBone.transform.rotation * leafVertices[a]) + leafBone.transform.position);


                //add bone weight here too
                BoneWeight bW = new BoneWeight();
                //point to a bone
                bW.boneIndex0 = bones.Count - 1;
                bW.weight0 = 1f;
                //vertice index
                tempBoneWeights.Add(bW);

            }
            //add triangles too
            int[] leafTriangles = leafPrimitive.GetComponent<MeshFilter>().mesh.triangles;
            for (int a = 0; a < leafTriangles.Length; a++)
            {
                //total leaves, plus this loop's leaves + this leaf
                int currentTriangle = leafTriangles[a] + leafVertices.Length * j + verticesPassed.Count;
                tempTriangles.Add(currentTriangle);
            }


            //attach to parent object
            leafBone.transform.parent = leavesParent.transform;
            //roate parent 
            if (rotationType == 3)
                leafBone.transform.rotation = pivotObject.transform.rotation;


        }

        //get rid of the primitive we stole mesh data from
        Destroy(leafPrimitive);


        //attach leaf to it's branch
        leavesParent.transform.parent = pivotObject.transform;

        //out
        verticesToReturn = tempVertices;
        trianglesToReturn = tempTriangles;
        boneWeightsToReturn = tempBoneWeights;
        //return
        return bones;

    }

    List<GameObject> FlowersCombined(out List<Vector3> verticesToReturn, out List<int> trianglesToReturn, out List<BoneWeight> boneWeightsToReturn, GameObject pivotObject, List<Vector3> verticesPassed, List<int> trianglesPassed, List<GameObject> bonesPassed, List<BoneWeight> boneWeightsPassed)
    {
        //method receives a list of bones and positions, creates more bones and position and adds them to the received list, then returns the lists

        List<GameObject> bones = new List<GameObject>(bonesPassed);
        List<BoneWeight> tempBoneWeights = new List<BoneWeight>(boneWeightsPassed);
        List<Vector3> tempVertices = new List<Vector3>(verticesPassed);
        List<int> tempTriangles = new List<int>(trianglesPassed);


        PrimitiveType flowerShape = PrimitiveType.Cube;

        if (treeInfo.useSphereForFlower)
            flowerShape = PrimitiveType.Sphere;

        //make a primitive so we can steal it's vertice data - this could be any shape..
        GameObject flowerPrimitive = GameObject.CreatePrimitive(flowerShape);

        //use Unity procedural primitive 
        GameObject flowerBone = new GameObject();// GameObject.CreatePrimitive(leafShape);
        //we don't need the collider, it will slow down performance
        //Destroy(leaf.GetComponent<Collider>());
        flowerBone.name = "Flower";
        flowerBone.tag = "Flower";
        //add this object to the bones list
        bones.Add(flowerBone);

        //create an arm and spin it using the index (i) in the for loop as the angle
        //using quaternions here as it is one easy line to use

        float flowerSize = Random.Range(0.1f, .5f);

        flowerBone.transform.parent = pivotObject.transform;
        flowerBone.transform.position = pivotObject.transform.position + transform.position + Random.rotation * Vector3.right * flowerSize * 2;
        //rotation?
        //random
        flowerBone.transform.rotation = Random.rotationUniform;

        //scale vertices
        Vector3[] flowerVertices = flowerPrimitive.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < flowerVertices.Length; i++)
        {
            flowerVertices[i] = new Vector3(flowerVertices[i].x * flowerSize, flowerVertices[i].y * flowerSize, flowerVertices[i].z * flowerSize);


            //add local vertice position to global flowe position, combined with rotation to give the ultimate position
            tempVertices.Add((flowerBone.transform.rotation * flowerVertices[i]) + flowerBone.transform.position);

            //add bone weight here too
            BoneWeight bW = new BoneWeight();
            //point to a bone
            bW.boneIndex0 = bones.Count - 1;
            bW.weight0 = 1f;
            //vertice index
            tempBoneWeights.Add(bW);
        }

        //add triangles too
        int[] leafTriangles = flowerPrimitive.GetComponent<MeshFilter>().mesh.triangles;
        for (int a = 0; a < leafTriangles.Length; a++)
        {
            //total leaves, plus this loop's leaves + this leaf
            int currentTriangle = leafTriangles[a] + verticesPassed.Count;
            tempTriangles.Add(currentTriangle);
        }


        //attach to parent object
        flowerBone.transform.parent = pivotObject.transform;

        //get rid of the primitive we stole mesh data from
        Destroy(flowerPrimitive);


        //out
        verticesToReturn = tempVertices;
        trianglesToReturn = tempTriangles;
        boneWeightsToReturn = tempBoneWeights;
        //return
        return bones;

    }

    List<Vector3> CreateRing(float width, Vector3 centre)
    {
        //create a ring of points 
        List<Vector3> ringPoints = new List<Vector3>();
        //we will do this be rotating around a central point using quaternions

        //start at 0 degrees and finish at 360, a full circle
        //we will step by 360 divided by how many sides we wish to have
        float step = 360 / treeInfo.sides;

        //Create an arm and then rotate it using the for loop
        Vector3 arm = Vector3.right * width;
        for (float i = 0; i < 360; i += step)
        {
            Vector3 position = Quaternion.Euler(0, i, 0) * arm;
            position += centre;


            ringPoints.Add(position);
        }
        ringPoints.Add(ringPoints[0]);

        return ringPoints;
    }

    static void CombineMeshes(Transform transform)
    {

        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
        MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            foreach (var material in meshRenderer.sharedMaterials)
                if (material != null && !combines.ContainsKey(material))
                    combines.Add(material, new List<CombineInstance>());

            meshRenderer.enabled = false;
        }

        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();
        foreach (var filter in meshFilters)
        {
            if (filter.sharedMesh == null)
                continue;



            CombineInstance ci = new CombineInstance();
            ci.mesh = filter.sharedMesh;

            ci.transform = myTransform * filter.transform.localToWorldMatrix;
            if (filter.GetComponent<MeshRenderer>() != null)
                combines[filter.GetComponent<Renderer>().sharedMaterial].Add(ci);

        }

        foreach (Material m in combines.Keys)
        {
            var go = new GameObject("Combined Mesh");

            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh.CombineMeshes(combines[m].ToArray(), true, true);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.material = m;


        }
    }

    public class BuildInfo
    { 
        public Quaternion startRotation;
        public float currentWidth;
        public float currentHeight;
        public bool splittingBranch;
        public GameObject branchingPivot;

    }

}


