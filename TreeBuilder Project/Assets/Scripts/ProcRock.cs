using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcRock : MonoBehaviour
{
    public float rockChance = .5f;
    public float rockRange = 10f;
    public float rockRandomRange = 10f;

    public float noiseRange = 1f;
    public float scaleRange = 2f;

    
    // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i < 360; i++)
        {
            if (Random.value > rockChance)
                continue;

            Vector3 arm = (Quaternion.Euler(0, i, 0))*Vector3.right * Random.Range(1f,1f+rockRandomRange);
            arm *= rockRange;
            
            GameObject rock = new GameObject();
            rock.name = "Rock";
            rock.transform.position = arm;

            MeshRenderer meshRenderer = rock.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load("Grey") as Material;

            IcoSphere.Create(rock,1);

            Vector3[] vertices = rock.GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < vertices.Length; j++)
            {
                //cheap and nasty "noise"
                vertices[j] += new Vector3(Random.Range(-noiseRange, noiseRange), Random.Range(-noiseRange, noiseRange), Random.Range(-noiseRange, noiseRange));
            }

            rock.GetComponent<MeshFilter>().mesh.vertices = vertices;

            //create sharp edges
            rock.GetComponent<MeshFilter>().mesh = UniqueVertices(rock.GetComponent<MeshFilter>().mesh);



            float s = Random.Range(-scaleRange, scaleRange);
            rock.transform.localScale += new Vector3(s, s, s);

        }


    }

    public static Mesh UniqueVertices(Mesh mesh)
    {

        //Process the triangles
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.name = "Unique Verts";

        return mesh;
    }

}
