using UnityEngine;
using System.Collections.Generic;

namespace PolyReduction
{
    public class Vertex
    {
        public Vector3 m_position { get; set; } // location of this point

        private int m_iID; // place of vertex in original list
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

        //private List<Triangle> m_adjacentTriangles; // adjacent triangles
        //public List<Triangle> AdjacentTriangles
        //{
        //    get
        //    {
        //        return m_adjacentTriangles;
        //    }
        //}

        public Vertex(Vector3 v, int _id)
        {
            m_position = v;
            m_iID = _id;
            
            //m_adjacentTriangles = new List<Triangle>(3);
        }

        //public void AddAdjacentTriangle(Triangle triangle)
        //{
        //    if (!HasAdjacentTriangle(triangle))
        //        m_adjacentTriangles.Add(triangle);
        //}

        //public void RemoveAdjacentTriangle(Triangle triangle)
        //{
        //    if (HasAdjacentTriangle(triangle))
        //        m_adjacentTriangles.Remove(triangle);
        //}

        //public bool HasAdjacentTriangle(Triangle triangle)
        //{
        //    return m_adjacentTriangles.Contains(triangle);
        //}

        /**
        * Do this vertex and another one share one or more adjacent triangles
        */
        //public List<Triangle> GetSharedTriangles(Vertex vertex)
        //{
        //    List<Triangle> sharedTriangles = new List<Triangle>();
        //    for (int i = 0; i < m_adjacentTriangles.Count; i++)
        //    {
        //        if (m_adjacentTriangles[i].HasVertex(vertex))
        //            sharedTriangles.Add(m_adjacentTriangles[i]);
        //    }

        //    return sharedTriangles;
        //}

        //public bool ShareTriangleWithVertex(Vertex vertex)
        //{
        //    for (int i = 0; i < m_adjacentTriangles.Count; i++)
        //    {
        //        if (m_adjacentTriangles[i].HasVertex(vertex))
        //            return true;
        //    }

        //    return false;
        //}

        /**
        * Tell if this vertex can collapse on one of the vertices of the parameter 'wedge'
        * Return the Vertex on which this vertex can collapse on
        **/
        //public Vertex FindVertexToCollapseOn(Wedge wedge)
        //{
        //    if (wedge == null)
        //        return null;

        //    for (int i = 0; i != wedge.Vertices.Count; i++)
        //    {
        //        if (wedge.Vertices[i].ShareTriangleWithVertex(this))
        //            return wedge.Vertices[i];
        //    }

        //    return null;
        //}

        /**
        * Call this to perform the action of vertex collapsing on a wedge
        * Vertex can either collapse on another vertex or on itself (i.e it did not find another vertex to collapse on)
        * In the first case, copy adjacent triangles to new vertex
        * In the second case, simply update the position of the vertex and move it to the new wedge vertex list
        **/
        //public void CollapseOnWedgeVertex(Wedge collapseWedge, Vertex collapseVertex)
        //{
        //    if (collapseWedge == null)
        //        return;

        //    //move the vertex
        //    this.m_position = collapseWedge.m_position;

        //    //copy adjacent triangles to collapseVertex and recompute normal
        //    for (int i = 0; i != m_adjacentTriangles.Count; i++)
        //    {
        //        Triangle triangle = m_adjacentTriangles[i];

        //        triangle.ReplaceVertex(this, collapseVertex);
        //    }
        //}

        //public void Delete()
        //{
        //    if (m_adjacentTriangles.Count > 0)
        //        throw new System.Exception("Vertex still references one or more adjacent triangles");

        //    //for each neighbor of this vertex remove it from the neighbors list
        //    //for (int i = 0; i != m_neighbors.Count;i++)
        //    //{
        //    //    m_neighbors[i].m_neighbors.Remove(this);
        //    //}
        //}
    }




    /**
    * Use this class to keep information on a vertex that collapsed on another
    **/
    public class CollapsedVertex
    {
        public int m_initialIndex;
        public int m_collapsedIndex;

        public CollapsedVertex()
        {
            m_initialIndex = -1;
            m_collapsedIndex = -1;
        }

        public override string ToString()
        {
            return "vertex " + m_initialIndex + " collapsed on vertex " + m_collapsedIndex;
        }
    }

    public class DisplacedVertex
    {
        public int m_index;
        public Vector3 m_targetPosition; //the position where to displace this vertex
    }
}