using UnityEngine;
using System.Collections.Generic;

namespace PolyReduction
{
    [ExecuteInEditMode]
    public class PolyReducer : MonoBehaviour
    {
        private List<Wedge> m_initialWedges;
        private List<Wedge> m_wedges;

        private int[] m_collapseMap; // to which neighbor each wedge collapses

        private List<WedgeTriangle> m_originalTriangles;

        private ModelData m_data;

        private Mesh m_mesh;

        //Set a value of a maximum vertex count to render
        //public int m_renderedVerticesCount;
        //private int m_prevRenderedVerticesCount;

        //Set a value of a maximum wedge count to render
        public int m_renderedWedgesCount;
        private int m_prevRenderedWedgesCount;

        //the number of wedges able to collapse
        private int m_minRendereWedgesCount;

        public void Start()
        {
            //PrepareModel();
        }

        /**
        * Prepare the data from the model attached to this component
        * Once this step is done, we can cut down the vertices on this model and create low-polygon models
        **/
        public void PrepareModel()
        {
            //Debug.Log("PrepareModel");

            int[] permutation;
            //m_data = GetMeshData();
            //WriteModelDataToFile(100.0f);
            m_data = GetDummyData();
            //m_data = MeshTrianglesSeparator.SeparateTrianglesInMesh(m_data);
            ProgressiveMesh(m_data, out m_collapseMap, out permutation);
            PermuteWedges(permutation);

            m_renderedWedgesCount = m_initialWedges.Count;
            m_prevRenderedWedgesCount = m_renderedWedgesCount;
            m_minRendereWedgesCount = 3; //we need at least 3 wedges to render a triangle

            //Debug.Log("model setup with %i:" + m_renderedWedgesCount + " vertices");
        }

        /**
        * Core function of the algorithm
        * **/
        private void ProgressiveMesh(ModelData data, out int[] map, out int[] permutation)
        {
            PrepareMeshData(data.Verts, data.Tris);

            ComputeAllEdgeCollapseCosts();

            //for (int i = 0; i != m_wedges.Count; i++)
            //{
            //    Wedge mn = m_wedges[i];
            //    if (mn.m_collapse != null)
            //        Debug.Log("wedge " + mn.ID + " costs " + mn.m_cost + " to collapse on " + mn.m_collapse.ID);
            //}

            map = new int[m_wedges.Count];
            permutation = new int[m_wedges.Count];

            // reduce the object down to nothing
            while (m_wedges.Count > 0)
            {
                // get the next vertex to collapse
                Wedge mn = MinimumCostEdge();
                // keep track of this vertex, i.e. the collapse ordering
                permutation[mn.ID] = m_wedges.Count - 1;
                // keep track of vertex to which we collapse to
                map[m_wedges.Count - 1] = (mn.m_collapse != null) ? mn.m_collapse.ID : -1;              
                // Collapse this edge
                Collapse(mn, mn.m_collapse);

                //if (mn.m_collapse != null)
                //    Debug.Log("Wedge " + mn.ID + " collapses on wedge " + mn.m_collapse.ID + " with cost " + mn.m_cost);

                //first time we encounter a wedge that cannot collapse, we store here the count of remaining wedges including the one we just try to collapse
                if (mn.m_collapse == null && m_minRendereWedgesCount <= 3) 
                    m_minRendereWedgesCount = m_wedges.Count + 1;
            }

            // reorder the map list based on the collapse ordering
            for (int i = 0; i < map.Length; i++)
            {
                //map[i] = (map[i] == -1) ? 0 : permutation[map[i]];
                if (map[i] >= 0)
                    map[i] = permutation[map[i]];
            }
        }

        /**
         * Transform the inital mesh data (verts and tris) to one which is more appropriate to our algorithm (with more info in it)
         * **/
        private void PrepareMeshData(List<Vector3> verts, List<int> tris)
        {
            m_wedges = new List<Wedge>();
            m_initialWedges = new List<Wedge>(); //as m_wedges will be consumed, store here a copy of this list

            m_originalTriangles = new List<WedgeTriangle>();

            BuildWedges(m_wedges, verts, tris);
            BuildWedges(m_initialWedges, verts, tris);
        }

        private void BuildWedges(List<Wedge> wedges, List<Vector3> verts, List<int> tris)
        {
            Vertex[] vertices = new Vertex[verts.Count];

            //First sort vertices into wedges
            for (int i = 0; i != verts.Count; i++)
            {
                Vertex vertex = new Vertex(verts[i], i);
                vertices[i] = vertex;

                Wedge wedge = WedgeForPosition(wedges, verts[i]);
                if (wedge != null)
                {
                    wedge.AddVertex(vertex);
                }
                else
                {
                    wedge = new Wedge(verts[i], wedges.Count);
                    wedge.AddVertex(vertex);
                    wedges.Add(wedge);
                }
            }

            //Build neighbourly relations between vertices and wedges using the triangles of the model
            BuildNeighbourlyRelations(wedges, vertices, tris);

            //Determine which triangles are adjacent to each wedge
            //for (int i = 0; i != m_wedges.Count; i++)
            //{
            //    m_wedges[i].InvalidateAdjacentTriangles();
            //}
        }        

        /**
        * Build neighbourly relations between vertices and wedges using the triangles of the model
        **/
        private void BuildNeighbourlyRelations(List<Wedge> wedges, Vertex[] vertices, List<int> tris)
        {
            for (int i = 0; i != tris.Count; i += 3)
            {
                Vertex v0 = vertices[tris[i]];
                Vertex v1 = vertices[tris[i + 1]];
                Vertex v2 = vertices[tris[i + 2]];

                Triangle triangle = new Triangle(v0, v1, v2);
                //m_triangles.Add(triangle);

                //Set this triangle as an adjacent triangle for every vertex
                //v0.AddAdjacentTriangle(triangle);
                //v1.AddAdjacentTriangle(triangle);
                //v2.AddAdjacentTriangle(triangle);

                //for each triangle vertex, set the 2 opposite points as neighbors
                //v0.AddNeighbor(v1);
                //v0.AddNeighbor(v2);
                //v1.AddNeighbor(v0);
                //v1.AddNeighbor(v2);
                //v2.AddNeighbor(v0);
                //v2.AddNeighbor(v1);                

                Wedge w0 = GetWedgeHoldingVertex(wedges, v0);
                Wedge w1 = GetWedgeHoldingVertex(wedges, v1);
                Wedge w2 = GetWedgeHoldingVertex(wedges, v2);
                
                w0.AddNeighbor(w1);
                w0.AddNeighbor(w2);
                w1.AddNeighbor(w0);
                w1.AddNeighbor(w2);
                w2.AddNeighbor(w0);
                w2.AddNeighbor(w1);

                WedgeTriangle wedgeTriangle = new WedgeTriangle(w0, w1, w2);
                wedgeTriangle.m_mappedVertexTriangle = triangle; //map here the vertex triangle to the wedge triangle

                if (wedges == m_initialWedges) //populate the m_originalTriangles using the m_initialWedges list only
                    m_originalTriangles.Add(wedgeTriangle);

                w0.AddAdjacentTriangle(wedgeTriangle);
                w1.AddAdjacentTriangle(wedgeTriangle);
                w2.AddAdjacentTriangle(wedgeTriangle);
            }
        }

        /**
        * Return the wedge holding the vertex 'vertex' in its list
        **/
        private Wedge GetWedgeHoldingVertex(List<Wedge> wedges, Vertex vertex)
        {
            for (int i = 0; i != wedges.Count; i++)
            {
                if (wedges[i].HasVertex(vertex))
                    return wedges[i];
            }

            return null;
        }

        /**
        * Return the data contained inside the mesh as one single object
        **/
        private ModelData GetMeshData()
        {
            m_mesh = GetComponent<MeshFilter>().sharedMesh;

            if (m_mesh == null)
                throw new System.Exception("A mesh has to be added to this object MeshFilter component in order to perform a polygon reduction on it");

            List<Vector3> verts = new List<Vector3>(m_mesh.vertices);
            List<int> tris = new List<int>(m_mesh.triangles);

            return new ModelData(verts, tris);
        }

        /**
        * Render this model using exactly m_maxVertices
        **/
        public void RenderModel()
        {
            Vector3[] cutVertices;
            int[] cutTriangles;
            CutMeshPolygons(out cutVertices, out cutTriangles, m_renderedWedgesCount);
            RefreshModel(cutVertices, cutTriangles);
        }

        /**
        * Return a list of vertices and indices where player has specified a maximum count of vertices for the initial model
        **/
        public void CutMeshPolygons(out Vector3[] cutVertices, out int[] cutTriangles, int maxWedges)
        {
            //no work to do here
            if (maxWedges >= m_initialWedges.Count)
            {
                cutVertices = m_data.Verts.ToArray();
                cutTriangles = m_data.Tris.ToArray();
                return;
            }

            Vertex[] vertices = new Vertex[m_data.Verts.Count];

            //build the triangle list
            List<int> triangles = new List<int>();
            List<int> dismissedVerticesIDs = new List<int>();
            for (int i = 0; i != m_originalTriangles.Count; i++)
            {
                WedgeTriangle triangle = m_originalTriangles[i];

                int p0 = triangle.Wedges[0].ID;
                int p1 = triangle.Wedges[1].ID;
                int p2 = triangle.Wedges[2].ID;

                //int p0 = m_data.Tris[i];
                //int p1 = m_data.Tris[i + 1];
                //int p2 = m_data.Tris[i + 2];

                int collapsedP0 = Map(p0, maxWedges);
                int collapsedP1 = Map(p1, maxWedges);
                int collapsedP2 = Map(p2, maxWedges);

                if (collapsedP0 == -1)
                    collapsedP0 = p0;
                if (collapsedP1 == -1)
                    collapsedP1 = p1;
                if (collapsedP2 == -1)
                    collapsedP2 = p2;

                //one-dimensional (flat) triangle, dismiss it
                if (collapsedP0 == collapsedP1 || collapsedP1 == collapsedP2 || collapsedP2 == collapsedP0)
                    continue;

                Triangle vertexTriangle = triangle.m_mappedVertexTriangle;
                Vertex v0 = vertexTriangle.Vertices[0];
                Vertex v1 = vertexTriangle.Vertices[1];
                Vertex v2 = vertexTriangle.Vertices[2];

                if (vertices[v0.ID] == null)
                {
                    //v0.m_position = WedgeForID(m_initialWedges, collapsedP0).m_position;
                    v0.m_position = m_initialWedges[collapsedP0].m_position;
                    vertices[v0.ID] = v0;
                }
                if (vertices[v1.ID] == null)
                {
                    //v1.m_position = WedgeForID(m_initialWedges, collapsedP1).m_position;
                    v1.m_position = m_initialWedges[collapsedP1].m_position;
                    vertices[v1.ID] = v1;
                }
                if (vertices[v2.ID] == null)
                {
                    //v2.m_position = WedgeForID(m_initialWedges, collapsedP2).m_position;
                    v2.m_position = m_initialWedges[collapsedP2].m_position;
                    vertices[v2.ID] = v2;
                }

                triangles.Add(v0.ID);
                triangles.Add(v1.ID);
                triangles.Add(v2.ID);
            }

            //traverse vertices array and find holes in it and extract positions
            int numVertices = 0;
            for (int i = 0; i != vertices.Length; i++)
            {
                if (vertices[i] == null)
                    dismissedVerticesIDs.Add(i);
                else
                    numVertices++;
            }

            cutVertices = new Vector3[numVertices];
            for (int i = 0; i != vertices.Length; i++)
            {
                if (vertices[i] != null)
                {
                    int shiftedID = GetShiftedID(dismissedVerticesIDs, i);
                    cutVertices[shiftedID] = vertices[i].m_position;
                }                
            }

            //do the same thing for triangles
            for (int i = 0; i != triangles.Count; i++)
            {
                triangles[i] = GetShiftedID(dismissedVerticesIDs, triangles[i]);
            }

            cutTriangles = triangles.ToArray();
        }
        
        /**
        * Map a wedge index using the collapse map
        **/
        private int Map(int a, int mx)
        {
            while (a >= mx)
            {
                a = m_collapseMap[a];
            }
            return a;
        }

        /**
        * Shift the parameter 'id' according to the array of 'vertices' that collapsed
        **/
        private int GetShiftedID(List<int> dismissedVerticesIDs, int id)
        {
            int shift = 0;
            for (int i = 0; i != dismissedVerticesIDs.Count; i++)
            {
                if (id > dismissedVerticesIDs[i])
                    shift++;
                else
                    break;
            }
            return id - shift;
        }

        /**
        * Return the ID of the vertex on which the vertex of ID 'id' collapses
        **/
        private int GetCollapseIDForID(List<CollapsedVertex> vertices, int id)
        {
            for (int i = 0; i != vertices.Count; i++)
            {
                if (vertices[i].m_initialIndex == id)
                    return vertices[i].m_collapsedIndex;
            }

            //this vertex did not collapse
            return id;
        }       

        /**
        * Return the wedge at position 'position' if it exists
        **/
        private Wedge WedgeForPosition(List<Wedge> wedges, Vector3 position)
        {
            for (int i = 0; i != wedges.Count; i++)
            {
                float sqrDistance = (wedges[i].m_position - position).sqrMagnitude;
                if (sqrDistance < 1E-07)
                    return wedges[i];
            }

            return null;
        }

        /**
        * Return the wedge with ID 'id'
        **/
        private Wedge WedgeForID(List<Wedge> wedges, int id)
        {
            for (int i = 0; i != wedges.Count; i++)
            {
                if (wedges[i].ID == id)
                    return wedges[i];
            }

            return null;
        }

        /**
        * Reorder the wedges according to the permutation array
        **/
        private void PermuteWedges(int[] permutation)
        {
            if (permutation.Length != m_initialWedges.Count)
                throw new System.Exception("permutation list and initial wedges are not of the same size");

            // rearrange the wedge array 
            List<Wedge> tmpArray = new List<Wedge>(m_initialWedges);

            for (int i = 0; i != m_initialWedges.Count; i++)
            {
                m_initialWedges[permutation[i]] = tmpArray[i];
            }

            for (int i = 0; i < m_initialWedges.Count; i++)
            {
                m_initialWedges[permutation[i]].ID = permutation[i];
            }

            //int[] 
            //for (int i = 0; i != m_originalTriangles.Count; i++)
            //{
            //    WedgeTriangle triangle = m_originalTriangles[i];
            //    int w0ID = triangle.Wedges[0].ID;
            //    int w1ID = triangle.Wedges[1].ID;
            //    int w2ID = triangle.Wedges[2].ID;
            //    triangle.PermuteWedgesIDs(permutation);
            //}

            //use the collapse map to attribute one collapse wedge or a null one to each wedge
            //for (int i = 0; i < m_initialWedges.Count; i++)
            //{
            //    m_initialWedges[i].m_collapse = (m_collapseMap[i] == -1) ? null : m_initialWedges[m_collapseMap[i]];
            //}


            int index = 0;
            for (int i = 0; i != m_collapseMap.Length; i++)
            {
                if (m_collapseMap[i] == -1)
                    index++;
                else
                    break;
            }
            Debug.Log(index + " vertices cannot collapse");
        }

        /**
      * Compute the cost to collapse a specific edge defined by vertices u and v
      * **/
        private float ComputeEdgeCollapseCost(Wedge u, Wedge v)
        {
            // if we collapse edge uv by moving u to v then how 
            // much different will the model change, i.e. how much "error".
            // Texture, vertex normal, and border vertex code was removed
            // to keep this demo as simple as possible.
            // The method of determining cost was designed in order 
            // to exploit small and coplanar regions for
            // effective polygon reduction.
            // Is is possible to add some checks here to see if "folds"
            // would be generated.  i.e. normal of a remaining face gets
            // flipped.  I never seemed to run into this problem and
            // therefore never added code to detect this case.
            int i;
            float edgelength = (v.m_position - u.m_position).magnitude;
            float curvature = 0;

            // find the "sides" triangles that are on the edge uv
            List<WedgeTriangle> sides = u.GetSharedTrianglesWithWedge(v);

            if (sides.Count < 2) //wedge u cannot be collapsed on v because this edge is non manifold (i.e does not have 2 adjacent triangle)
                return -1; //return a negative/invalid cost

            // use the triangle facing most away from the sides 
            // to determine our curvature term
            for (i = 0; i < u.AdjacentTriangles.Count; i++)
            {
                Vector3 n1 = u.AdjacentTriangles[i].m_normal;
                float mincurv = 1; // curve for face i and closer side to it
                for (int j = 0; j < sides.Count; j++)
                {
                    // use dot product of face normals
                    Vector3 n2 = sides[j].m_normal;
                    float dotprod;
                    if (n1 == n2)
                        dotprod = 1;
                    else
                        dotprod = n1.x * n2.x + n1.y * n2.y + n1.z * n2.z;
                    mincurv = Mathf.Min(mincurv, (1 - dotprod) / 2.0f);
                }
                curvature = Mathf.Max(curvature, mincurv);
            }

            

            //Debug.Log("cost between " + u.ID + " and " + v.ID + " is " + edgelength * curvature);

            // the more coplanar the lower the curvature term   
            return edgelength * curvature;
        }

        /**
       * Compute the cost to collapse a specific edge to one of its neighbors (the one with the least cost is chosen)
       * **/
        private void ComputeEdgeCostAtWedge(Wedge w)
        {
            // compute the edge collapse cost for all edges that start
            // from vertex v.  Since we are only interested in reducing
            // the object by selecting the min cost edge at each step, we
            // only cache the cost of the least cost edge at this vertex
            // (in member variable collapse) as well as the value of the 
            // cost (in member variable objdist).
            if (w.Neighbors.Count == 0)
            {
                // v doesn't have neighbors so it costs nothing to collapse
                w.m_collapse = null;
                w.m_cost = 0;
                return;
            }
            w.m_cost = 1000000;
            w.m_collapse = null;
            // search all neighboring edges for "least cost" edge
            for (int i = 0; i < w.Neighbors.Count; i++)
            {
                float dist;
                dist = ComputeEdgeCollapseCost(w, w.Neighbors[i]);
                if (dist < 0) //non-manifold edge
                {
                    w.m_collapse = null;
                    w.m_cost = 0;
                }
                else
                {
                    if (dist < w.m_cost)
                    {
                        w.m_collapse = w.Neighbors[i];  // candidate for edge collapse
                        w.m_cost = dist;             // cost of the collapse
                    }
                }
            }
        }

        /**
       * Compute the cost to collapse an edge for every edge in the mesh
       * **/
        private void ComputeAllEdgeCollapseCosts()
        {
            // For all the edges, compute the difference it would make
            // to the model if it was collapsed.  The least of these
            // per vertex is cached in each vertex object.
            for (int i = 0; i < m_wedges.Count; i++)
            {
                ComputeEdgeCostAtWedge(m_wedges[i]);
            }            
        }

        /**
        * Collapse the wedge u onto the wedge v
        * **/
        private void Collapse(Wedge u, Wedge v)
        {
            //Collapse the edge uv by moving wedge u onto v
            // Actually remove tris on uv, then update tris that
            // have u to have v, and then remove u.
            if (v == null)
            {
                // u is a vertex all by itself so just delete it
                u.Delete();
                m_wedges.Remove(u);
                return;
            }
            
            // delete triangles on edge uv:
            List<WedgeTriangle> uvSharedTriangles = u.GetSharedTrianglesWithWedge(v);
            for (int i = 0; i != uvSharedTriangles.Count; i++)
            {
                uvSharedTriangles[i].Delete();
            }

            // update remaining triangles to have v instead of u
            for (int i = 0; i != u.AdjacentTriangles.Count; i++)
            {
                u.AdjacentTriangles[i].ReplaceWedge(u, v);
            }

            //add u neighbors to v and add v as a new neighbor of u neighbors
            for (int i = 0; i != u.Neighbors.Count; i++)
            {
                if (u.Neighbors[i] != v)
                {
                    v.AddNeighbor(u.Neighbors[i]);
                    u.Neighbors[i].AddNeighbor(v);
                }
            }

            u.Delete();
            m_wedges.Remove(u);

            // recompute the edge collapse costs for neighboring vertices
            for (int i = 0; i < u.Neighbors.Count; i++)
            {
                ComputeEdgeCostAtWedge(u.Neighbors[i]);
            }
        }

        /**
        * Return the vertex with the 'least cost' to collapse
        * **/
        private Wedge MinimumCostEdge()
        {
            // Find the edge that when collapsed will affect model the least.
            // This funtion actually returns a Vertex, the second vertex
            // of the edge (collapse candidate) is stored in the vertex data.
            // Serious optimization opportunity here: this function currently
            // does a sequential search through an unsorted list :-(
            // Our algorithm could be O(n*lg(n)) instead of O(n*n)
            Wedge mn = null;
            float minCost = float.MaxValue;
            for (int i = 0; i < m_wedges.Count; i++)
            {
                if (m_wedges[i].m_collapse == null)
                    continue;

                if (m_wedges[i].m_cost == 0) //zero cost, take the first wedge we encounter
                    return m_wedges[i];

                if (m_wedges[i].m_cost < minCost)
                {
                    mn = m_wedges[i];
                    minCost = m_wedges[i].m_cost;
                }
            }

            if (mn == null) //we spent all wedges that collapse, only wedges with null collapse remain so return the first one
                mn = m_wedges[0];

            return mn;
        }

        /**
         * Build some data to test our algorithm
         * **/
        private ModelData GetDummyData()
        {
            //FILE SAMPLES
            float[,] dummyVertices = RabbitData.rabbit_vertices;
            int[,] dummyTriangles = RabbitData.rabbit_triangles;
            //float[,] dummyVertices = PlaneData.plane_vertices;
            //int[,] dummyTriangles = PlaneData.plane_triangles;



            //SAMPLE 1
            //float[,] dummyVertices = {
            //                            { 1, 0, 3 },
            //                            { 0, -1, -1},
            //                            { 2, 0.5f, 0},
            //                            { 2.5f, -1, -1.5f},
            //                            { 4, 0, 1},
            //                            { 3.5f, 2, 1}
            //                         };

            //int[,] dummyTriangles = {
            //                            { 0, 2, 1},
            //                            { 1, 2, 3},
            //                            { 2, 5, 3},
            //                            { 3, 5, 4}
            //                         };

            //SAMPLE 2
            //float[,] dummyVertices = {
            //                            { -5, 2, 0 },
            //                            { -5, -3, 0},
            //                            { 0, 5, 0},
            //                            { 3, 5, 0},
            //                            { 5, -3, 1},
            //                            { 1, -5, 1},
            //                            { -1, 0, 1},
            //                            { 2, 0, 1}
            //                         };

            //int[,] dummyTriangles = {
            //                            { 0, 6, 1},
            //                            { 1, 6, 5},
            //                            { 6, 7, 5},
            //                            { 5, 7, 4},
            //                            { 4, 7, 3},
            //                            { 7, 2, 3},
            //                            { 6, 2, 7},
            //                            { 6, 0, 2}
            //                         };

            List<Vector3> verts = new List<Vector3>(dummyVertices.GetLength(0));
            for (int i = 0; i != dummyVertices.GetLength(0); i++)
            {
                float x = dummyVertices[i, 0];
                float y = dummyVertices[i, 1];
                float z = dummyVertices[i, 2];

                verts.Add(new Vector3(x, y, z));
            }

            List<int> tris = new List<int>(dummyTriangles.Length);
            for (int i = 0; i != dummyTriangles.GetLength(0); i++)
            {
                tris.Add(dummyTriangles[i, 0]);
                tris.Add(dummyTriangles[i, 1]);
                tris.Add(dummyTriangles[i, 2]);
            }

            m_mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = m_mesh;
            m_mesh.vertices = verts.ToArray();
            m_mesh.triangles = tris.ToArray();

            ModelData dummyData = new ModelData(verts, tris);
            return dummyData;
        }

        /**
        * Reassign vertices and triangles to the model
        **/
        private void RefreshModel(Vector3[] vertices, int[] triangles)
        {
            m_mesh = new Mesh();
            this.GetComponent<MeshFilter>().sharedMesh = m_mesh;
            //m_mesh.Clear();

            m_mesh.vertices = vertices;
            m_mesh.triangles = triangles;
        }


        public void Update()
        {
            if (m_data != null)
            {
                if (m_renderedWedgesCount < m_minRendereWedgesCount)
                    m_renderedWedgesCount = m_minRendereWedgesCount;
                if (m_renderedWedgesCount > m_initialWedges.Count)
                    m_renderedWedgesCount = m_initialWedges.Count;

                if (m_renderedWedgesCount != m_prevRenderedWedgesCount)
                {
                    //Debug.Log("RenderModel:" + m_prevRenderedWedgesCount + " - " + m_renderedWedgesCount);
                    RenderModel();

                    m_prevRenderedWedgesCount = m_renderedWedgesCount;
                }
            }
        }

        private void WriteModelDataToFile(float scale)
        {
            string strData = "";

            //c# file immutable code
            strData += "using UnityEngine;\n\n";
            strData += "namespace PolyReduction\n";
            strData += "{\n";
            strData += "\tpublic class PlaneData\n";
            strData += "\t{\n";

            //vertices
            strData += "\t\tpublic static float[,] plane_vertices =\n";
            strData += "\t\t{\n";
            for (int i = 0; i != m_data.Verts.Count; i++)
            {
                Vector3 vertex = m_data.Verts[i];
                string strVertex = "\t\t\t{" + (vertex.x * scale) + "f," + (vertex.y * scale) + "f," + (vertex.z * scale) + "f}";
                strData += strVertex;
                if (i < m_data.Verts.Count - 1)
                    strData += ",";
                strData += "\n";
            }

            strData += "\t\t};\n\n";

            //triangles
            strData += "\t\tpublic static int[,] plane_triangles =\n";
            strData += "\t\t{\n";
            for (int i = 0; i != m_data.Tris.Count; i += 3)
            {
                string strTri = "\t\t\t{" + m_data.Tris[i] + "," + m_data.Tris[i + 1] + "," + m_data.Tris[i + 2] + "}";
                strData += strTri;
                if (i < m_data.Tris.Count - 1)
                    strData += ",";
                strData += "\n";
            }


            strData += "\t\t};\n";

            //end of c# file
            strData += "\t}\n}";

            // Write the string to a file.
            string pathToScriptsFolder = "C:\\Unity_workspace\\PolyReduction\\Assets\\Scripts";
            System.IO.StreamWriter file = new System.IO.StreamWriter(pathToScriptsFolder + "\\PlaneData.cs");
            file.WriteLine(strData);

            file.Close();
        }
    }
}
