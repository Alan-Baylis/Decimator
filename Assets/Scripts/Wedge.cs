using UnityEngine;
using System.Collections.Generic;

namespace PolyReduction
{
    public class Wedge
    {
        public Vector3 m_position { get; set; } // location of this wedge

        private int m_iID; // place of wedge in original list
        public int ID
        {
            get
            {
                return m_iID;
            }

            set
            {
                m_iID = value;
            }
        }

        private List<Vertex> m_vertices;
        public List<Vertex> Vertices
        {
            get
            {
                return m_vertices;
            }
        }

        private List<Wedge> m_neighbors; // adjacent wedges
        public List<Wedge> Neighbors
        {
            get
            {
                return m_neighbors;
            }
        }

        private List<WedgeTriangle> m_adjacentTriangles; // adjacent triangles
        public List<WedgeTriangle> AdjacentTriangles
        {
            get
            {
                return m_adjacentTriangles;
            }
        }

        public float m_cost { get; set; } // cached cost of collapsing edge
        public Wedge m_collapse { get; set; } // candidate wedge for collapse     

        public Wedge(Vector3 v, int id)
        {
            m_position = v;
            m_iID = id;

            m_vertices = new List<Vertex>();
            m_neighbors = new List<Wedge>(3);
            m_adjacentTriangles = new List<WedgeTriangle>(3);
        }

        public void AddVertex(Vertex vertex)
        {
            m_vertices.Add(vertex);
        }

        public void RemoveVertex(Vertex vertex)
        {
            if (HasVertex(vertex))
                m_vertices.Remove(vertex);
        }

        public bool HasVertex(Vertex vertex)
        {
            return m_vertices.Contains(vertex);
        }

        public Vertex GetVertexForID(int id)
        {
            for (int i = 0; i != m_vertices.Count; i++)
            {
                if (m_vertices[i].ID == id)
                    return m_vertices[i];
            }

            return null;
        }

        /**
        * Recompute the list of adjacent triangles using the list of child vertices
        **/
        //public void InvalidateAdjacentTriangles()
        //{
        //    m_adjacentTriangles.Clear();

        //    for (int i = 0; i != m_vertices.Count; i++)
        //    {
        //        for (int j = 0; j != m_vertices[i].AdjacentTriangles.Count; j++)
        //        {
        //            if (!HasAdjacentTriangle(m_vertices[i].AdjacentTriangles[j]))
        //                m_adjacentTriangles.Add(m_vertices[i].AdjacentTriangles[j]);
        //        }
        //    }
        //}

        public void AddAdjacentTriangle(WedgeTriangle triangle)
        {
            if (!HasAdjacentTriangle(triangle))
                m_adjacentTriangles.Add(triangle);
        }

        public void RemoveAdjacentTriangle(WedgeTriangle triangle)
        {
            if (HasAdjacentTriangle(triangle))
                m_adjacentTriangles.Remove(triangle);
        }

        public bool HasAdjacentTriangle(WedgeTriangle triangle)
        {
            return m_adjacentTriangles.Contains(triangle);
        }

        public void AddNeighbor(Wedge neighbor)
        {
            if (!HasNeighbor(neighbor))
                m_neighbors.Add(neighbor);
        }

        public void RemoveNeighbor(Wedge neighbor)
        {
            if (HasNeighbor(neighbor))
                m_neighbors.Remove(neighbor);
        }

        public bool HasNeighbor(Wedge neighbor)
        {
            return m_neighbors.Contains(neighbor);
        }

        /**
        * Return the triangles shared by two wedges.
        **/
        public List<WedgeTriangle> GetSharedTrianglesWithWedge(Wedge wedge)
        {
            List<WedgeTriangle> sharedTriangles = new List<WedgeTriangle>();

            for (int i = 0; i < m_adjacentTriangles.Count; i++)
            {
                for (int j = 0; j != wedge.m_adjacentTriangles.Count; j++)
                {
                    if (m_adjacentTriangles[i] == wedge.m_adjacentTriangles[j])
                        sharedTriangles.Add(m_adjacentTriangles[i]);
                }
            }

            return sharedTriangles;
        }

        public void Delete()
        {
            //for each neighbor of this vertex remove it from the neighbors list
            for (int i = 0; i != m_neighbors.Count; i++)
            {
                m_neighbors[i].RemoveNeighbor(this);
            }
        }
    }
}