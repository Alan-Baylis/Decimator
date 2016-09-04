using System.Collections.Generic;
using UnityEngine;

namespace PolyReduction
{
    public class Triangle
    {
        private Vertex[] m_vertices; // the 3 points that make this tri
        public Vertex[] Vertices
        {
            get
            {
                return m_vertices;
            }
        }

        public Vector3 m_normal { get; set; } // orthogonal unit vector

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            m_vertices = new Vertex[3];
            m_vertices[0] = v0;
            m_vertices[1] = v1;
            m_vertices[2] = v2;

            ComputeNormal();
        }

        public void ComputeNormal()
        {
            Vector3 v0 = m_vertices[0].m_position;
            Vector3 v1 = m_vertices[1].m_position;
            Vector3 v2 = m_vertices[2].m_position;

            Vector3 u = v1 - v0;
            Vector3 v = v2 - v0;

            Vector3 crossProduct = new Vector3(u.y * v.z - u.z * v.y,
                                               u.z * v.x - u.x * v.z,
                                               u.x * v.y - u.y * v.x);

            crossProduct.Normalize();

            m_normal = crossProduct;
        }
        
        public bool HasVertex(Vertex v)
        {
            return v == m_vertices[0] || v == m_vertices[1] || v == m_vertices[2];
        }
    }

    public class WedgeTriangle
    {
        private Wedge[] m_wedges; // the 3 points that make this tri
        public Wedge[] Wedges
        {
            get
            {
                return m_wedges;
            }
        }

        public Triangle m_mappedVertexTriangle { get; set; }

        public Vector3 m_normal { get; set; } // orthogonal unit vector

        public WedgeTriangle(Wedge w0, Wedge w1, Wedge w2)
        {
            m_wedges = new Wedge[3];
            m_wedges[0] = w0;
            m_wedges[1] = w1;
            m_wedges[2] = w2;

            ComputeNormal();
        }

        public void ComputeNormal()
        {
            Vector3 v0 = m_wedges[0].m_position;
            Vector3 v1 = m_wedges[1].m_position;
            Vector3 v2 = m_wedges[2].m_position;

            Vector3 u = v1 - v0;
            Vector3 v = v2 - v0;

            Vector3 crossProduct = new Vector3(u.y * v.z - u.z * v.y,
                                               u.z * v.x - u.x * v.z,
                                               u.x * v.y - u.y * v.x);

            crossProduct.Normalize();

            m_normal = crossProduct;
        }

        public void ReplaceIDs(int[] newIDs)
        {
            m_wedges[0].ID = newIDs[0];
            m_wedges[1].ID = newIDs[1];
            m_wedges[2].ID = newIDs[2];
        }

        public void ReplaceWedge(Wedge vOld, Wedge vNew)
        {
            for (int i = 0; i != 3; i++)
            {
                if (vOld == m_wedges[i])
                    m_wedges[i] = vNew;
            }

            vNew.AddAdjacentTriangle(this);
            ComputeNormal();
        }       

        public bool HasWedge(Wedge w)
        {
            return m_wedges[0] == w || m_wedges[1] == w || m_wedges[2] == w;
        }

        public void Delete()
        {
            for (int i = 0; i < 3; i++)
            {
                Wedge wedge = m_wedges[i];
                if (wedge != null)
                    wedge.RemoveAdjacentTriangle(this);
            }
        }
    }
}
