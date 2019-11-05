using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public RandomSpawner spawner;
    public float highestY = 0f;
    public float zoom = 1f;
    public float bumpStop = 1f;
    public bool makeNewTarget;
   public Vector3 start;
    public Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void NewTarget()
    {

        if (spawner.trees.Count == 0)
            return;

        start = transform.position;

        //find highest bounding box of tree
        highestY = 0;
        for (int i = 0; i < spawner.trees.Count; i++)
        {
            SkinnedMeshRenderer[] renderers = spawner.trees[i].GetComponentsInChildren<SkinnedMeshRenderer>();
            Debug.Log(renderers.Length);

            for (int j = 0; j < renderers.Length; j++)
            {
                if (renderers[j].bounds.extents.y + renderers[i].transform.position.y > highestY)
                {
                    highestY = renderers[j].bounds.extents.y + renderers[i].transform.position.y;
                }
            }
        }
       

        float tempHighest = highestY;
        if (tempHighest < bumpStop)
            tempHighest = bumpStop;

        target = spawner.trees[0].transform.position - transform.forward * tempHighest * zoom;

        makeNewTarget = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (makeNewTarget)
            NewTarget();

        Vector3 lookTarget = spawner.trees[0].transform.position;
        //lookTarget.y += highestY * .5f;
        // transform.LookAt(lookTarget);

        // transform.position = Vector3.Lerp(start, target, Time.deltaTime*0.1f);
        //
        transform.position =Vector3.Lerp(transform.position,  target + Vector3.up * highestY,Time.deltaTime);
        
    }
    


}
