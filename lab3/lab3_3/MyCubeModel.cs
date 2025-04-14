using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Lab3_3
{
    internal class MyCubeModel
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Normals { get; }
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

        private MyCubeModel(uint vao, uint vertices, uint normals, uint colors, uint indeces, uint indexArrayLength, GL gl, Coordinate<float> position)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Normals = normals;
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
            // In CreateCubeWithFaceColors method:

// Define your vertex positions (just positions, no normals)
float[] vertexArray = new float[] {
    // top face (y+) - Consistent counter-clockwise winding when looking from above
    -0.5f, 0.5f, -0.5f,  // back-left
    -0.5f, 0.5f, 0.5f,   // front-left
    0.5f, 0.5f, 0.5f,    // front-right
    0.5f, 0.5f, -0.5f,   // back-right

    // bottom face (y-) - CCW when looking from below
    -0.5f, -0.5f, -0.5f, // back-left
    0.5f, -0.5f, -0.5f,  // back-right
    0.5f, -0.5f, 0.5f,   // front-right
    -0.5f, -0.5f, 0.5f,  // front-left

    // front face (z+) - CCW when looking from front
    -0.5f, -0.5f, 0.5f,  // bottom-left
    0.5f, -0.5f, 0.5f,   // bottom-right
    0.5f, 0.5f, 0.5f,    // top-right
    -0.5f, 0.5f, 0.5f,   // top-left

    // back face (z-) - CCW when looking from back
    -0.5f, -0.5f, -0.5f, // bottom-left
    -0.5f, 0.5f, -0.5f,  // top-left
    0.5f, 0.5f, -0.5f,   // top-right
    0.5f, -0.5f, -0.5f,  // bottom-right

    // left face (x-) - CCW when looking from left
    -0.5f, -0.5f, -0.5f, // back-bottom
    -0.5f, -0.5f, 0.5f,  // front-bottom
    -0.5f, 0.5f, 0.5f,   // front-top
    -0.5f, 0.5f, -0.5f,  // back-top

    // right face (x+) - CCW when looking from right
    0.5f, -0.5f, 0.5f,   // front-bottom
    0.5f, -0.5f, -0.5f,  // back-bottom
    0.5f, 0.5f, -0.5f,   // back-top
    0.5f, 0.5f, 0.5f,    // front-top
};

// Define normals that match the vertex order
float[] normalArray = new float[] {
    // top face normals
    0f, 1f, 0f,
    0f, 1f, 0f,
    0f, 1f, 0f,
    0f, 1f, 0f,

    // bottom face normals
    0f, -1f, 0f,
    0f, -1f, 0f,
    0f, -1f, 0f,
    0f, -1f, 0f,

    // front face normals
    0f, 0f, 1f,
    0f, 0f, 1f,
    0f, 0f, 1f,
    0f, 0f, 1f,

    // back face normals
    0f, 0f, -1f,
    0f, 0f, -1f,
    0f, 0f, -1f,
    0f, 0f, -1f,

    // left face normals
    -1f, 0f, 0f,
    -1f, 0f, 0f,
    -1f, 0f, 0f,
    -1f, 0f, 0f,

    // right face normals
    1f, 0f, 0f,
    1f, 0f, 0f,
    1f, 0f, 0f,
    1f, 0f, 0f,
};

// Ensure consistent triangle winding for all faces
uint[] indexArray = new uint[] {
    // Each face has 2 triangles
    // Top face
    0, 1, 2,  // First triangle
    0, 2, 3,  // Second triangle
    
    // Bottom face
    4, 5, 6,
    4, 6, 7,
    
    // Front face
    8, 9, 10,
    8, 10, 11,
    
    // Back face
    12, 13, 14,
    12, 14, 15,
    
    // Left face
    16, 17, 18,
    16, 18, 19,
    
    // Right face
    20, 21, 22,
    20, 22, 23
};

            float[] colorArray = new float[] {
    // Top face - Yellow
    face1Color[0], face1Color[1], face1Color[2], face1Color[3],
    face1Color[0], face1Color[1], face1Color[2], face1Color[3],
    face1Color[0], face1Color[1], face1Color[2], face1Color[3],
    face1Color[0], face1Color[1], face1Color[2], face1Color[3],
    
    // Bottom face - Purple
    face4Color[0], face4Color[1], face4Color[2], face4Color[3],
    face4Color[0], face4Color[1], face4Color[2], face4Color[3],
    face4Color[0], face4Color[1], face4Color[2], face4Color[3],
    face4Color[0], face4Color[1], face4Color[2], face4Color[3],
    
    // Front face - Green
    face2Color[0], face2Color[1], face2Color[2], face2Color[3],
    face2Color[0], face2Color[1], face2Color[2], face2Color[3],
    face2Color[0], face2Color[1], face2Color[2], face2Color[3],
    face2Color[0], face2Color[1], face2Color[2], face2Color[3],
    
    // Back face - Blue
    face5Color[0], face5Color[1], face5Color[2], face5Color[3],
    face5Color[0], face5Color[1], face5Color[2], face5Color[3],
    face5Color[0], face5Color[1], face5Color[2], face5Color[3],
    face5Color[0], face5Color[1], face5Color[2], face5Color[3],
    
    // Left face - Orange
    face3Color[0], face3Color[1], face3Color[2], face3Color[3],
    face3Color[0], face3Color[1], face3Color[2], face3Color[3],
    face3Color[0], face3Color[1], face3Color[2], face3Color[3],
    face3Color[0], face3Color[1], face3Color[2], face3Color[3],
    
    // Right face - Red
    face6Color[0], face6Color[1], face6Color[2], face6Color[3],
    face6Color[0], face6Color[1], face6Color[2], face6Color[3],
    face6Color[0], face6Color[1], face6Color[2], face6Color[3],
    face6Color[0], face6Color[1], face6Color[2], face6Color[3]
};


            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint normals = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, normals);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)normalArray.AsSpan(), GLEnum.StaticDraw);
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, null);

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

            return new MyCubeModel(vao, vertices,normals, colors, indices, indexArrayLength, Gl, position);
        }

        internal void ReleaseMyCubeModel()
        {
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Normals);
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
