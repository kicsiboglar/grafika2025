using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using ImGuiNET;

namespace Lab3_3
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static GL Gl;

        private static uint program;

        private static ImGuiController controller;

        private static List<MyCubeModel> cubes = new List<MyCubeModel>(new MyCubeModel[27]);

        private static float targetRotation = 0.0f;
        private static float currentRotation = 0.0f;

        private const float RotationSpeed = MathF.PI / 3.0f;

        private static float Shininess = 50;

        private static float AmbientStrength = 0.5f;

        private static float DiffuseStrength = 0.5f;

        private static float SpecularStrength = 0.5f;

        private static float LightColorR = 1f;
        private static float LightColorG = 1f;
        private static float LightColorB = 1f;

        private static float LightPosX = 3.0f;
private static float LightPosY = 3.0f;
private static float LightPosZ = 3.0f;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";
        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";
        private const string AmbientStrengthVariableName = "ambientStrength";
        private const string DiffuseStrengthVariableName = "diffuseStrength";
        private const string SpecularStrengthVariableName = "specularStrength";

        private static readonly string VertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 vPos;
            layout (location = 1) in vec4 vCol;
            layout (location = 2) in vec3 vNorm;

            uniform mat4 uModel;
            uniform mat3 uNormal;

            uniform mat4 uView;
            uniform mat4 uProjection;

            out vec4 outCol;
            out vec3 outNormal;
            out vec3 outWorldPosition;
            
            void main()
            {
                outCol = vCol;
                gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
                outNormal = normalize(uNormal * vNorm);
                outWorldPosition = vec3(uModel * vec4(vPos.x, vPos.y, vPos.z, 1.0));
            }
            ";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float shininess;
uniform float ambientStrength;
uniform float diffuseStrength;
uniform float specularStrength;
out vec4 FragColor;

in vec4 outCol;
in vec3 outWorldPosition;
in vec3 outNormal;

void main()
{
    vec3 norm = normalize(outNormal);
    vec3 lightDir = normalize(lightPos - outWorldPosition);
    vec3 viewDir = normalize(viewPos - outWorldPosition);
    
    vec3 ambient = ambientStrength * lightColor;
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diffuseStrength * diff * lightColor;
    
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    float spec = pow(max(dot(norm, halfwayDir), 0.0), shininess);
    vec3 specular = specularStrength * spec * lightColor;
    
    vec3 result = (ambient + diffuse + specular) * outCol.rgb;
    FragColor = vec4(result, outCol.a);
}
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "3 labor feladat - 3";
            windowOptions.Size = new Vector2D<int>(500, 500);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            IInputContext inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();
            controller = new ImGuiController(Gl, window, inputContext);

            window.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };
            Gl.ClearColor(System.Drawing.Color.White);

            SetupRubikCube();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    cameraDescriptor.DecreaseZYAngle();
                    break;
                    ;
                case Key.Right:
                    cameraDescriptor.IncreaseZYAngle();
                    break;
                case Key.Down:
                    cameraDescriptor.IncreaseDistance();
                    break;
                case Key.Up:
                    cameraDescriptor.DecreaseDistance();
                    break;
                case Key.U:
                    cameraDescriptor.IncreaseZXAngle();
                    break;
                case Key.J:
                    cameraDescriptor.DecreaseZXAngle();
                    break;
                case Key.Space:
                    targetRotation += MathF.PI / 2.0f;
                    break;
                case Key.Backspace:
                    targetRotation -= MathF.PI / 2.0f;
                    break;

                case Key.W:
                    cameraDescriptor.MoveFocus(Vector3D<float>.UnitY, 0.1f);
                    break;
                case Key.D:
                    cameraDescriptor.MoveFocus(Vector3D<float>.UnitX, 0.1f);
                    break;
                case Key.S:
                    cameraDescriptor.MoveFocus(Vector3D<float>.UnitX * -1, 0.1f);
                    break;
                case Key.A:
                    cameraDescriptor.MoveFocus(Vector3D<float>.UnitY * -1, 0.1f);
                    break;

            }
        }

        private static void Window_Update(double deltaTime)
        {

            cubeArrangementModel.AdvanceTime(deltaTime, cubes);
            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightingVariables();

            DrawRubikCube();
            DrawInteractiveGui(deltaTime);

            controller.Render();
        }

        private static unsafe void SetLightingVariables()
        {
            SetUniformLocationVariables(LightColorVariableName, LightColorR, LightColorG, LightColorB);
            SetUniformLocationVariables(LightPositionVariableName, LightPosX, LightPosY, LightPosZ);
            SetUniformLocationVariables(ViewPosVariableName, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);

            SetUniformLocationVariable(ShininessVariableName, Shininess);
            SetUniformLocationVariable(AmbientStrengthVariableName, AmbientStrength);
            SetUniformLocationVariable(DiffuseStrengthVariableName, DiffuseStrength);
            SetUniformLocationVariable(SpecularStrengthVariableName, SpecularStrength);
        }

        private static unsafe void SetUniformLocationVariable(string variableName, float value)
        {
            int location = Gl.GetUniformLocation(program, variableName);
            if (location == -1)
            {
                throw new Exception($"{variableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, value);
            CheckError();
        }
        private static unsafe void SetUniformLocationVariables(string variableName, float value1, float value2, float value3)
        {
            int location = Gl.GetUniformLocation(program, variableName);
            if (location == -1)
            {
                throw new Exception($"{variableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, value1, value2, value3);
            CheckError();
        }

        private static unsafe void DrawRubikCube()
        {
            foreach (var cube in cubes)
            {
                RenderSmallCube(cube);
            }
        }

        private static void DrawInteractiveGui(double deltaTime)
        {
            ImGui.Begin("Lighting properties", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGui.SliderFloat("Ambient strength", ref AmbientStrength, 0, 1);
            ImGui.SliderFloat("Diffuse strength", ref DiffuseStrength, 0, 1);
            ImGui.SliderFloat("Specular strength", ref SpecularStrength, 0, 1);

            ImGui.SliderFloat("Light color R", ref LightColorR, 0, 1);
            ImGui.SliderFloat("Light color G", ref LightColorG, 0, 1);
            ImGui.SliderFloat("Light color B", ref LightColorB, 0, 1);

            ImGui.InputFloat("Light position X", ref LightPosX);
            ImGui.InputFloat("Light position Y", ref LightPosY);
            ImGui.InputFloat("Light position Z", ref LightPosZ);

            ImGui.Separator();
            ImGui.Text("Forgatási vezérlők");
            
            if (ImGui.Button("X tengely bal oldal +"))
                HandleSingleRotation("X", -1f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("X tengely bal oldal -"))
                HandleSingleRotation("X", -1f, false, deltaTime);
                
            if (ImGui.Button("X tengely középső +"))
                HandleSingleRotation("X", 0f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("X tengely középső -"))
                HandleSingleRotation("X", 0f, false, deltaTime);
                
            if (ImGui.Button("X tengely jobb oldal +"))
                HandleSingleRotation("X", 1f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("X tengely jobb oldal -"))
                HandleSingleRotation("X", 1f, false, deltaTime);
                
            if (ImGui.Button("Y tengely alsó +"))
                HandleSingleRotation("Y", -1f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("Y tengely alsó -"))
                HandleSingleRotation("Y", -1f, false, deltaTime);
                
            if (ImGui.Button("Y tengely középső +"))
                HandleSingleRotation("Y", 0f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("Y tengely középső -"))
                HandleSingleRotation("Y", 0f, false, deltaTime);
                
            if (ImGui.Button("Y tengely felső +"))
                HandleSingleRotation("Y", 1f, true, deltaTime);
            ImGui.SameLine();
            if (ImGui.Button("Y tengely felső -"))
                HandleSingleRotation("Y", 1f, false, deltaTime);

            ImGui.End();
        }

        private static unsafe void RenderSmallCube(MyCubeModel cube)
        {

    var scale = Matrix4X4.CreateScale(0.99f); 
    var translation = Matrix4X4.CreateTranslation(cube.originalPosition.X, 
                                                cube.originalPosition.Y, 
                                                cube.originalPosition.Z);
    var rotation = cube.GetRotationMatrix();
    
    var modelMatrix = scale * translation * rotation;
    SetModelMatrix(modelMatrix);

            Coordinate<float> roundedCoordinate = GetRoundErrorFixedCoordinate(modelMatrix.M41, modelMatrix.M42, modelMatrix.M43);

            cube.currentPosition = new Coordinate<float>(roundedCoordinate);
            
            SetMatrix(modelMatrix, ModelMatrixVariableName);
            
            Gl.BindVertexArray(cube.Vao);
            Gl.DrawElements(GLEnum.Triangles, cube.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
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

       private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            SetMatrix(modelMatrix, ModelMatrixVariableName);
            
            Matrix3X3<float> upper3x3 = new Matrix3X3<float>(
                modelMatrix.M11, modelMatrix.M12, modelMatrix.M13,
                modelMatrix.M21, modelMatrix.M22, modelMatrix.M23,
                modelMatrix.M31, modelMatrix.M32, modelMatrix.M33);
            

            Matrix4X4<float> temp4x4 = new Matrix4X4<float>(
                upper3x3.M11, upper3x3.M12, upper3x3.M13, 0,
                upper3x3.M21, upper3x3.M22, upper3x3.M23, 0,
                upper3x3.M31, upper3x3.M32, upper3x3.M33, 0,
                0, 0, 0, 1);
            
            if (Matrix4X4.Invert(temp4x4, out Matrix4X4<float> inverted4x4))
            {
                Matrix3X3<float> inverted3x3 = new Matrix3X3<float>(
                    inverted4x4.M11, inverted4x4.M12, inverted4x4.M13,
                    inverted4x4.M21, inverted4x4.M22, inverted4x4.M23,
                    inverted4x4.M31, inverted4x4.M32, inverted4x4.M33);
                
                Matrix3X3<float> normalMatrix = new Matrix3X3<float>(
                    inverted3x3.M11, inverted3x3.M21, inverted3x3.M31,
                    inverted3x3.M12, inverted3x3.M22, inverted3x3.M32,
                    inverted3x3.M13, inverted3x3.M23, inverted3x3.M33);
                
                int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
                if (location != -1)
                {
                    Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
                }
            }
        }

        private static unsafe void SetInverseMatrix(Matrix4X4<float> normalMatrixTranslation)
        {
            Matrix4X4<float> modelInverse;
            Matrix4X4.Invert<float>(normalMatrixTranslation, out modelInverse);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInverse));
            int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
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
                        cubes[index] = SetUpCubeObject(i, j, k);
                        cubes[index].currentPosition = new Coordinate<float>(i, j, k);
                        index++;
                    }
                }
            }
        }

private static unsafe MyCubeModel SetUpCubeObject(int x, int y, int z)
{
    // Start with all faces black
    float[] topColor = Colors.Black;
    float[] frontColor = Colors.Black;
    float[] leftColor = Colors.Black;
    float[] bottomColor = Colors.Black;
    float[] backColor = Colors.Black;
    float[] rightColor = Colors.Black;

    // Color all exterior faces (not just one per cube)
    if (y == 1) topColor = Colors.Yellow;    // Top face of whole cube
    if (y == -1) bottomColor = Colors.Purple; // Bottom face
    if (z == 1) frontColor = Colors.Green;   // Front face
    if (z == -1) backColor = Colors.Blue;    // Back face
    if (x == -1) leftColor = Colors.Orange;  // Left face
    if (x == 1) rightColor = Colors.Red;     // Right face

    return MyCubeModel.CreateCubeWithFaceColors(Gl, topColor, frontColor, leftColor, 
                                              bottomColor, backColor, rightColor,
                                              new Coordinate<float>(x,y,z));
}

        private static void HandleSingleRotation(string axes, float targetPos, bool positiveDirection, double deltaTime)
        {
            foreach (var cube in cubes)
            {
                if (cube.currentPosition[axes] == targetPos)
                {
                    cube.goalRotation[axes] = (float)(positiveDirection ? MathF.PI / 2 : -MathF.PI / 2);
                    cube.needsRotation[axes] = true;
                    cube.IsPositiveRotation = positiveDirection;
                }
            }
            cubeArrangementModel.AnimationEnabled = true;
            cubeArrangementModel.AdvanceTime(deltaTime,cubes);

        }

        private static void Window_Closing()
        {
            foreach (var cube in cubes)
            {
                cube.ReleaseMyCubeModel();
            }
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}