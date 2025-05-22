using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;

namespace lab4_1
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateTeapotWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;
            List<float[]> objNormals;
            List<int[]> normalIndices;

            ReadObjDataForTeapot(out objVertices, out objFaces, out objNormals, out normalIndices);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objNormals, objFaces, normalIndices, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData<float>(GLEnum.ArrayBuffer, glVertices.ToArray(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);
            Gl.EnableVertexAttribArray(2);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData<float>(GLEnum.ArrayBuffer, glColors.ToArray(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData<uint>(GLEnum.ElementArrayBuffer, glIndices.ToArray(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<float[]> objNormals, List<int[]> objFaces, List<int[]> normalIndices, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            for (int faceIdx = 0; faceIdx < objFaces.Count; ++faceIdx)
            {
                int[] face = objFaces[faceIdx];
                int[] normals = normalIndices[faceIdx];

                for (int i = 0; i < 3; ++i)
                {
                    float[] vertex = objVertices[face[i] - 1];
                    float[] normal = objNormals[normals[i] - 1];

                    string key = $"{vertex[0]} {vertex[1]} {vertex[2]} {normal[0]} {normal[1]} {normal[2]}";
                    if (!glVertexIndices.ContainsKey(key))
                    {
                        glVertices.AddRange(vertex);
                        glVertices.AddRange(normal);
                        glColors.AddRange(faceColor);
                        glVertexIndices[key] = glVertexIndices.Count;
                    }

                    glIndices.Add((uint)glVertexIndices[key]);
                }
            }
        }

        private static unsafe void ReadObjDataForTeapot(out List<float[]> objVertices, out List<int[]> objFaces, out List<float[]> objNormals, out List<int[]> normalIndices)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            objNormals = new List<float[]>();
            normalIndices = new List<int[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("lab4_1.Resources.cube.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    string line = objReader.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    switch (parts[0])
                    {
                        case "v":
                            objVertices.Add(parts.Skip(1).Take(3).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray());
                            break;
                        case "vn":
                            objNormals.Add(parts.Skip(1).Take(3).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray());
                            break;
                        case "f":
                            int[] vertexIndices = new int[3];
                            int[] normalIdx = new int[3];

                            for (int i = 0; i < 3; ++i)
                            {
                                string[] indices = parts[i + 1].Split("//", StringSplitOptions.RemoveEmptyEntries);
                                vertexIndices[i] = int.Parse(indices[0], CultureInfo.InvariantCulture);
                                normalIdx[i] = int.Parse(indices[1], CultureInfo.InvariantCulture);
                            }

                            objFaces.Add(vertexIndices);
                            normalIndices.Add(normalIdx);
                            break;
                    }
                }
            }
        }
    }
}
