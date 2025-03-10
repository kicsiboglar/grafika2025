using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Square;
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;


        private static float[] vertexArray = [

            0.0f, 0.0f, 0.0f,
            0.0f, 0.65f, 0.0f,
            -0.8f, 0.4f, 0.0f,

            0.0f, 0.0f, 0.0f,
            0.0f, 0.65f, 0.0f,
            0.8f, 0.4f, 0.0f,

            0.0f, 0.0f, 0.0f,
            0.8f, 0.4f, 0.0f,
            0.7f, -0.35f, 0.0f,

            0.0f, 0.0f, 0.0f,
            0.7f, -0.35f, 0.0f,
            0.0f, -0.8f, 0.0f,

            0.0f, 0.0f, 0.0f,
            -0.8f, 0.4f, 0.0f,
            -0.7f, -0.35f, 0.0f,

            0.0f, 0.0f, 0.0f,
            -0.7f, -0.35f, 0.0f,
            0.0f, -0.8f, 0.0f,
        ];

        private static float[] colorArray = [
            // red
            1.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,

            1.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f,

            // green
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,

            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,

            // blue
            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,

            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,            
        ];


        private static uint[] indexArray = [
            0, 1, 2,    // top - left
            3, 4, 5,    // top - right
            6, 7, 8,    // right - upper
            9, 10, 11,  // right - lower
            12, 13, 14, // left - upper
            15, 16, 17, // left - lower
        ];


        private static float[] lineVertexArray = [
            -0.8f, 0.4f, 0.0f,
            0.8f, 0.4f, 0.0f,

            0.8f, 0.4f, 0.0f,
            0.7f, -0.35f, 0.0f,

            0.7f, -0.35f, 0.0f,
            0.0f, -0.8f, 0.0f,

            0.0f, -0.8f, 0.0f,
            -0.7f, -0.35f, 0.0f,

            -0.7f, -0.35f, 0.0f,
            -0.8f, 0.4f, 0.0f,
        ];

        private static float[] lineColorArray = [
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f, 
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
            0.0f,  0.0f,  0.0f,  0.0f,
        ];

        private static uint[] lineIndexArray = [
            0, 1,
            2, 3,
            4, 5,
            6, 7,
            8, 9,
        ];
        private static readonly string VertexShaderSource = @" 
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Lab1-3";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load; //libc++abi: terminating due to uncaught exception of type PAL_SEHException
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);// ha attach utas van nem fut le

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);//ha attach elott vagy detach utan van ures a canvas
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            //uint->int: CS0266
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);


            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices); //nem rajzol ki semmit nelkule
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw); // nem fut le, ad errort d enem ad error kodot es nem ir ki error messaget
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);//0 3 -> 1 4: nem jelenik meg semmi a canvasen
            Gl.EnableVertexAttribArray(0);//0->1: nem jelenik meg semmi a canvasen // ha hianyzik sem jelenik meg semmi a canvasen
            CheckGLError();

            uint colors = Gl.GenBuffer(); 
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors); //nem rajzol ki semmit nelkule
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw); // nem fut le, ad errort d enem ad error kodot es nem ir ki error messaget
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);// 1 4 -> 0 3: nem jelenik meg semmi a canvasen
            Gl.EnableVertexAttribArray(1); //1->0: nem ad errort de fekete haromszog jelenik meg //ha hianyzik akkor is fekete haromszogek
            CheckGLError();

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            CheckGLError();

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            CheckGLError();

            Gl.UseProgram(program);
            
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            CheckGLError();

            // draw lines
            uint lineVertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            CheckGLError();

            uint lineColors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineColors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineColorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            CheckGLError();

            uint lineIndices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, lineIndices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)lineIndexArray.AsSpan(), GLEnum.StaticDraw);
            CheckGLError();
            
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.UseProgram(program);
            Gl.DrawElements(GLEnum.Lines, (uint)lineIndexArray.Length, GLEnum.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            CheckGLError();
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);



            CheckGLError();
        }

        private static void CheckGLError()
        {
            GLEnum error = Gl.GetError();
            while (error != GLEnum.NoError)
            {
                Console.WriteLine($"GL Error: {error}");
                error = Gl.GetError();
            }
        }
    }
