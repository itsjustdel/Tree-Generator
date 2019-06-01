using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour {

    ///holds info for tree building
    /////public floats will be viewable in unity's inspector
    public int sides = 6;
    //starting width of trunk
    public float startingWidth = 1f;
    //stop when the branches get as thin as minimum radius
    public float endingWidth = 0.05f;
    //how much to reduce each step
    public float radiusReduceAmount = 0.1f;

    //how long each space between rings will be
    public float stepHeight = 2f;
    //how much it reduces by each step
    public float stepReduce = 0.05f;

    //the higher this is, the more floppy the tree will be
    public float bendAmount = 5f;

    [Range(0f, 1f)]
    public float branchDensity = 0.4f;

    public float minimumBranchHeight = .5f;

    //leaf options
    public float minimumBranchWidthForLeaf = 0.5f;

    public float leafSize;
    public float leafThickness;

    public bool randomiseLeafSize = false;
    public bool randomiseLeafHeight = false;

    public bool useSphereForLeaf = false;
    public bool useSphereForFlower = false;


}
