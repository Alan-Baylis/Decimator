using System.Collections.Generic;
using UnityEngine;

public class ModelData
{
    private List<Vector3> m_verts;
    public List<Vector3> Verts
    {
        get
        {
            return m_verts;
        }
    }

    private List<int> m_tris;
    public List<int> Tris
    {
        get
        {
            return m_tris;
        }
    }

    public ModelData(List<Vector3> verts, List<int> tris)
    {
        m_verts = verts;
        m_tris = tris;
    }
}
