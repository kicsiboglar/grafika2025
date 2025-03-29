using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Lab2;

namespace GrafikaLab2
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static List<MyCubeModel> Cubes = new List<MyCubeModel>(new MyCubeModel[27]);

        private static Key[] rotationKeys = new Key[] { Key.Q, Key.W, Key.A, Key.S, Key.Z, Key.X, Key.U, Key.J, Key.I, Key.K, Key.O, Key.L };


        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Lab2";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            windowOptions.PreferredDepthBufferBits = 24;
            
            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            foreach (var Cube in Cubes)
            {
                Cube.ReleaseMyCubeModel();
            }
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl.ClearColor(System.Drawing.Color.White);
            SetupRubikCube();
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);


            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Up:
                    camera.IncreaseZXAngle();
                    break;
                
                case Key.P:
                    camera.IncreaseDistance();
                    break;
                case Key.M:
                    camera.DecreaseDistance();
                    break;
            }
            HandleRubikCubeRotation(key);
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime,Cubes);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            DrawModelObject();
        }

        private static Coordinate<float> GetRoundErrorFixedCoordinate(float x, float y, float z)
        {
            return new Coordinate<float>(GetRoundedNumber(x), GetRoundedNumber(y), GetRoundedNumber(z));
        }

        private static float GetRoundedNumber(float number)
        {
            List<float> targets = new List<float> { -1.0f, 0.0f, 1.0f };
            float closest = targets.OrderBy(t => Math.Abs(t - number)).First();
            return closest;
        }

        private static unsafe void RenderSmallCube(MyCubeModel Cube)
        {
            var scaleForMatrix = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
            var translationMatrix = Matrix4X4.CreateTranslation(Cube.originalPosition.X, Cube.originalPosition.Y, Cube.originalPosition.Z);
            var rotationMatrix = Cube.GetRotationMatrix();

            var modelMatrix = scaleForMatrix * translationMatrix * rotationMatrix;

            Coordinate<float> roundedCoordinate = GetRoundErrorFixedCoordinate(modelMatrix.M41, modelMatrix.M42, modelMatrix.M43);

            Cube.currentPosition = new Coordinate<float>(roundedCoordinate);
            
            SetMatrix(modelMatrix, ModelMatrixVariableName);
            
            Gl.BindVertexArray(Cube.Vao);
            Gl.DrawElements(GLEnum.Triangles, Cube.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetupRubikCube()
        {
            int index = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        Cubes[index] = SetUpCubeObject(i, j, k);
                        index++;
                    }
                }
            }
        }

        private static unsafe MyCubeModel SetUpCubeObject(int x, int y, int z)
        {
            float[] topColor = Colors.Black;
            float[] frontColor = Colors.Black;
            float[] leftColor = Colors.Black;
            float[] bottomColor = Colors.Black;
            float[] backColor = Colors.Black;
            float[] rightColor = Colors.Black;

            if (x == 1) rightColor = Colors.Red;
            if (x == -1) leftColor = Colors.Orange;
            if (y == 1) topColor = Colors.White;
            if (y == -1) bottomColor = Colors.Yellow;
            if (z == 1) frontColor = Colors.Green;
            if (z == -1) backColor = Colors.Blue;

            return MyCubeModel.CreateCubeWithFaceColors(Gl, topColor, frontColor, leftColor, bottomColor, backColor, rightColor, new Coordinate<float>(x, y, z));
        }

        private static unsafe void DrawModelObject()
        {
            foreach (var Cube in Cubes)
            {
                RenderSmallCube(Cube);
            }

            if (IsSolved())
            {
                // cubeArrangementModel.AnimationEnabled = false;
                cubeArrangementModel.SolvingAnimationEnabled = true;
                // Console.WriteLine("Rubik's cube is solved!!!");
            }
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        private static bool IsSolved()
        {
            foreach (var Cube in Cubes)
            {
                if (!Cube.IsInOriginalPosition())
                {
                    return false;
                }
            }

            foreach (var Cube in Cubes)
            {
                if (Cube.rotationHistory.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task WaitForAnimation()
        {
            while (cubeArrangementModel.AnimationEnabled)
            {
                await Task.Delay(100);
            }
        }

        private static async Task MixRubikCube(int numberOfMoves)
        {

            Random random = new Random();
            for (int i = 0; i < numberOfMoves; i++)
            {
                int randomIndex = random.Next(0, rotationKeys.Length);
                HandleRubikCubeRotation(rotationKeys[randomIndex]);
                await WaitForAnimation();
            }

        }

        private static void HandleSingleRotation(string axes, float targetPos, bool positiveDirection)
        {
            foreach (var cube in Cubes)
            {
                if (cube.currentPosition[axes] == targetPos)
                {
                    cube.goalRotation[axes] = (float)(positiveDirection ? MathF.PI / 2 : -MathF.PI / 2);
                    cube.needsRotation[axes] = true;
                    cube.IsPositiveRotation = positiveDirection;
                }
            }
            cubeArrangementModel.AnimationEnabled = true;
        }

        private static async void HandleRubikCubeRotation(Key key)
        {
            if (cubeArrangementModel.AnimationEnabled
            || (cubeArrangementModel.SolvingAnimationEnabled && key != Key.Escape))
            {
                return;
            }
            switch (key)
            {
                // Cube rotations around X axis
                case Key.Q:
                    HandleSingleRotation("X", -1f, true);
                    break;
                case Key.W:
                    HandleSingleRotation("X", -1f, false);
                    break;
                case Key.A:
                    HandleSingleRotation("X", 0f, true);
                    break;
                case Key.S:
                    HandleSingleRotation("X", 0f, false);
                    break;
                case Key.Z:
                    HandleSingleRotation("X", 1f, true);
                    break;
                case Key.X:
                    HandleSingleRotation("X", 1f, false);
                    break;
                // Cube rotations around Y axis
                case Key.U:
                    HandleSingleRotation("Y", -1f, true);
                    break;
                case Key.J:
                    HandleSingleRotation("Y", -1f, false);
                    break;
                case Key.I:
                    HandleSingleRotation("Y", 0f, true);
                    break;
                case Key.K:
                    HandleSingleRotation("Y", 0f, false);
                    break;
                case Key.O:
                    HandleSingleRotation("Y", 1f, true);
                    break;
                case Key.L:
                    HandleSingleRotation("Y", 1f, false);
                    break;

                // Cube mix animation
                case Key.Space:
                    cubeArrangementModel.RotationSpeed = 6.0f;
                    await MixRubikCube(30);
                    cubeArrangementModel.RotationSpeed = 3.0f;
                    break;

                // Cube reset
                case Key.Escape:
                    if (cubeArrangementModel.SolvingAnimationEnabled)
                    {
                        cubeArrangementModel.SolvingAnimationEnabled = false;
                        cubeArrangementModel.CenterCubeScale = 0.95f;
                    }
                    ResetRubikCube();
                    break;
            }
        }   

        private static void ResetRubikCube()
        {
            foreach (var Cube in Cubes)
            {
                Cube.Reset();
            }
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}