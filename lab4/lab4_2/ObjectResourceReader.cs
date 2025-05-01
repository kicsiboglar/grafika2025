using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using System.Globalization;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.IO;


namespace lab4_2
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateTeapotWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;

            readColladaData("cube.dae", out objVertices, out objFaces);

            List<float[]> objNormals = new List<float[]>();
            for (int i = 0; i < objVertices.Count; ++i)
            {
                objNormals.Add(new float[] { 0, 0, 1 });
            }

            List<float> glVertices = new List<float>();
            List<float> glNormals = new List<float>();
            List<uint> glIndices = new List<uint>();

            MapColladaDataToGlData(objVertices, objFaces, objNormals, out glVertices, out glNormals, out glIndices);
            
            return CreateOpenGlObject(Gl, vao, glVertices, glNormals, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glNormals, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            for (int i = 0; i < glVertices.Count; i++)
            {
                glVertices[i] *= 0.02f;
            }

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData<float>(GLEnum.ArrayBuffer, glVertices.ToArray(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);
            Gl.EnableVertexAttribArray(2);

            uint normals = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, normals);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glNormals.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData<uint>(GLEnum.ElementArrayBuffer, glIndices.ToArray(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, normals, indices, indexArrayLength, Gl);
        }

        public static void readColladaData(string resourceName, out List<float[]> vertices, out List<int[]> faces)
        {
            string fullResourceName = "lab4_2.Resources." + resourceName;
            using (var objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream(fullResourceName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(objStream);

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace("c", "http://www.collada.org/2005/11/COLLADASchema");


                vertices = new List<float[]>();
                faces = new List<int[]>();

                XmlNodeList geometryNodes = xmlDoc.SelectNodes("//c:geometry/c:mesh", nsMgr);

                foreach (XmlNode geometryNode in geometryNodes)
                {
                    foreach (XmlNode sourceNode in geometryNode.SelectNodes("c:source", nsMgr))
                    {
                        XmlNode floatArrayNode = sourceNode.SelectSingleNode("c:float_array", nsMgr);

                        float[] data = Array.ConvertAll(floatArrayNode.InnerText.Trim().Split(' '), float.Parse);
                        XmlNode accessorNode = sourceNode.SelectSingleNode("c:technique_common/c:accessor", nsMgr);

                        if (accessorNode.Attributes["stride"].Value == "3")
                            for (int i = 0; i < data.Length; i += 3)
                                vertices.Add(new float[] { data[i], data[i + 1], data[i + 2] });
                    }

                    XmlNode trianglesNode = geometryNode.SelectSingleNode("c:triangles", nsMgr);
                    string indicesText = trianglesNode.SelectSingleNode("c:p", nsMgr).InnerText;
                    int[] indices = Array.ConvertAll(indicesText.Trim().Split(' '), int.Parse);

                    for (int i = 0; i < indices.Length; i += 3)
                        faces.Add(new int[] { indices[i], indices[i + 1], indices[i + 2] });
                }
            }
        }

        private static void MapColladaDataToGlData(List<float[]> vertices, List<int[]> faces, List<float[]> normals, out List<float> glVertexData, out List<float> glNormalData, out List<uint> glFaceData)
        {
            var vertexIndexMap = new Dictionary<int, uint>();
            uint index = 0;

            glVertexData = new List<float>();
            glNormalData = new List<float>();
            glFaceData = new List<uint>();

            foreach (var face in faces)
            {
                for (int i = 0; i < face.Length; i++)
                {
                    if (!vertexIndexMap.ContainsKey(face[i]))
                    {
                        var vertex = vertices[face[i]];
                        var normal = normals.Count > face[i] ? normals[face[i]] : new float[] { 0, 0, 1 };

                        glVertexData.AddRange(vertex);
                        glVertexData.AddRange(normal);

                        glNormalData.AddRange(normal);

                        vertexIndexMap[face[i]] = index++;
                    }

                    glFaceData.Add(vertexIndexMap[face[i]]);
                }
            }
        } 
    }
}
