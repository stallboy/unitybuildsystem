/*
 * I wrote a script that adds a menu option to export the current scene's NavMesh to an .obj file. Unity 4.6.
 */

using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Obj exporter component based on: http://wiki.unity3d.com/index.php?title=ObjFromMeshFilter

public class ObjFromNavMesh 
{
    public static void Export()
    {
        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

        Mesh mesh = new Mesh();
        mesh.name = "ExportedNavMesh";
        mesh.vertices = triangulatedNavMesh.vertices;
        mesh.triangles = triangulatedNavMesh.indices;
        string filename = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) +
                          " Exported NavMesh.obj";
        MeshToFile(mesh, filename);
        Debug.Log("NavMesh exported as '" + filename + "'");
        AssetDatabase.Refresh();
    }

    struct VecMark
    {
        public Vector3 vec;
        public int newidx;
    };

    public static void SplitAndExport()
    {
        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();
        const int indexCount = 63333;
        int count = triangulatedNavMesh.indices.Length/indexCount + 1;

        for (int i = 0; i < count; ++i)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ExportedNavMesh" + i.ToString();

            List<int> idx = new List<int>();
            for (int j = indexCount*i; j < indexCount*(i + 1); ++j)
            {
                if (j < triangulatedNavMesh.indices.Length)
                    idx.Add(triangulatedNavMesh.indices[j]);
            }
            // get the vertex list
            VecMark[] vec = new VecMark[triangulatedNavMesh.vertices.Length];
            for (int j = 0; j < triangulatedNavMesh.vertices.Length; ++j)
                vec[j].vec = triangulatedNavMesh.vertices[j];

            List<Vector3> vecs = new List<Vector3>();
            for (int j = 0; j < idx.Count; ++j)
            {
                int index = idx[j];
                vecs.Add(vec[index].vec);
                vec[index].newidx = vecs.Count - 1;
            }

            // reset the indices now
            for (int j = 0; j < idx.Count; ++j)
            {
                int oldidx = idx[j];
                idx[j] = vec[oldidx].newidx;
            }

            // now create the sub mesh
            mesh.vertices = vecs.ToArray();
            mesh.triangles = idx.ToArray();
            string filename = Application.dataPath + "/" +
                              Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + " Exported NavMesh" +
                              i.ToString() + ".obj";
            MeshToFile(mesh, filename);
            Debug.Log("NavMesh exported as '" + filename + "'");
            //AssetDatabase.Refresh();
        }
    }

    static string MeshToString(Mesh mesh)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mesh.name).Append("\n");
        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (int material = 0; material < mesh.subMeshCount; material++)
        {
            sb.Append("\n");
            //sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            //sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = mesh.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i + 2] + 1,
                    triangles[i + 1] + 1, triangles[i] + 1));
            }
        }
        return sb.ToString();
    }

    static void MeshToFile(Mesh mesh, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mesh));
        }
    }
}