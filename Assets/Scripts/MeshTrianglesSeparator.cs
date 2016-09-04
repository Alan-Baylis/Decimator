using System.Collections.Generic;
using UnityEngine;

public class MeshTrianglesSeparator
{
    public static ModelData SeparateTrianglesInMesh(ModelData data)
    {
        Vector3[] meshVertices = data.Verts.ToArray();
        int[] meshTriangles = data.Tris.ToArray();
        
        Vector3[] outputMeshVertices = new Vector3[meshTriangles.Length];
        int[] outputMeshTriangles = new int[meshTriangles.Length];

        for (int i = 0; i != meshTriangles.Length; i++)
        {
            outputMeshVertices[i] = meshVertices[meshTriangles[i]];
            outputMeshTriangles[i] = i;
        }

        ModelData outputData = new ModelData(new List<Vector3>(outputMeshVertices), new List<int>(outputMeshTriangles));

        return outputData;
    }
}
