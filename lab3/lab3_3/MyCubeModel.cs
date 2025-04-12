using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Lab2
{
    internal class MyCubeModel
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }

        public Coordinate<float> originalPosition { get; set; } = new Coordinate<float>(0, 0, 0);
        public Coordinate<float> currentPosition { get; set; } = new Coordinate<float>(0, 0, 0);
        public Coordinate<float> actualRotation { get; set; } = new Coordinate<float>(0, 0, 0);
        public Coordinate<float> goalRotation { get; set; } = new Coordinate<float>(0, 0, 0);
        public Coordinate<bool> needsRotation { get; set; } = new Coordinate<bool>(false, false, false);

        public List<string> rotationHistory = new List<string>();

        public bool IsPositiveRotation { get; set; } = true;

        private GL Gl;

        private MyCubeModel(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, Coordinate<float> position)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indeces;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
            this.originalPosition = new Coordinate<float>(position);
            this.currentPosition = new Coordinate<float>(position);
        }

        

        public static unsafe MyCubeModel CreateCubeWithFaceColors(GL Gl, float[] face1Color, float[] face2Color, float[] face3Color, float[] face4Color, float[] face5Color, float[] face6Color, Coordinate<float> position)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // counter clockwise is front facing
            float[] vertexArray = new float[] {
                -0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,

                -0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,

                -0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, 0.5f,

                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,

                0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,

                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, 0.5f,

            };

            List<float> colorsList = new List<float>();
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);

            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);

            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);

            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);

            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);

            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);


            float[] colorArray = colorsList.ToArray();

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                10, 11, 8,

                12, 14, 13,
                12, 15, 14,

                17, 16, 19,
                17, 19, 18,

                20, 22, 21,
                20, 23, 22
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)indexArray.Length;

            return new MyCubeModel(vao, vertices, colors, indices, indexArrayLength, Gl, position);
        }

        internal void ReleaseMyCubeModel()
        {
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }

        public void Reset()
        {
            actualRotation = new Coordinate<float>(0, 0, 0);
            goalRotation = new Coordinate<float>(0, 0, 0);
            needsRotation = new Coordinate<bool>(false, false, false);
            rotationHistory = new List<string>();
            IsPositiveRotation = true;
        }

        public bool IsInOriginalPosition()
        {
            return currentPosition.X == originalPosition.X && currentPosition.Y == originalPosition.Y && currentPosition.Z == originalPosition.Z;
        }

        public Matrix4X4<float> GetRotationMatrix()
        {
            var backRotationValue = -(float)Math.PI / 2;
            var frontRotationValue = (float)Math.PI / 2;

            var rotationMatrix = Matrix4X4<float>.Identity;


            // perform history rotations
            foreach (var rotation in rotationHistory)
            {
                switch (rotation)
                {
                    case "X":
                        rotationMatrix *= Matrix4X4.CreateRotationX(frontRotationValue);
                        break;
                    case "-X":
                        rotationMatrix *= Matrix4X4.CreateRotationX(backRotationValue);
                        break;
                    case "Y":
                        rotationMatrix *= Matrix4X4.CreateRotationY(frontRotationValue);
                        break;
                    case "-Y":
                        rotationMatrix *= Matrix4X4.CreateRotationY(backRotationValue);
                        break;
                    case "Z":
                        rotationMatrix *= Matrix4X4.CreateRotationZ(frontRotationValue);
                        break;
                    case "-Z":
                        rotationMatrix *= Matrix4X4.CreateRotationZ(backRotationValue);
                        break;
                }
            }

            // apply current rotation
            if (needsRotation.X)
                rotationMatrix *= Matrix4X4.CreateRotationX(actualRotation.X);
            else if (needsRotation.Y)
                rotationMatrix *= Matrix4X4.CreateRotationY(actualRotation.Y);
            else if (needsRotation.Z)
                rotationMatrix *= Matrix4X4.CreateRotationZ(actualRotation.Z);

            return rotationMatrix;

        }
    }
}
