// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 30f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CCW);
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50.0, (double)m_width / (double)m_height, 0.5, 20000.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            DrawHuman(gl);
            DrawCylinder(gl);
            DrawSurface(gl);
            DrawStairs(gl);

            gl.PopMatrix();
            DrawText(gl);

            gl.Flush();
        }

        private void DrawHuman(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(5f, 0.7f, 0f);
            m_scene.Draw();
            gl.PopMatrix();
        }

        private void DrawText(OpenGL gl)
        {
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);
            gl.PushMatrix();
            gl.DrawText(m_width - 350, 100, 1.0f, 0.0f, 0.0f, "Verdana italic", 10, "Predmet: Racunarska grafika");
            gl.DrawText(m_width - 350, 80, 1.0f, 0.0f, 0.0f, "Verdana italic", 10, "Sk.god: 2020/21");
            gl.DrawText(m_width - 350, 60, 1.0f, 0.0f, 0.0f, "Verdana italic", 10, "Ime: Rados");
            gl.DrawText(m_width - 350, 40, 1.0f, 0.0f, 0.0f, "Verdana italic", 10, "Prezime: Milicev");
            gl.DrawText(m_width - 350, 20, 1.0f, 0.0f, 0.0f, "Verdana italic", 10, "Sifra zad: 16.1");
            gl.PopMatrix();
            gl.Viewport(0, 0, m_width, m_height);
        }

        private void DrawCylinder(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0f, -1f, 0f);
            gl.Rotate(-90f, 0f, 0f);
            Cylinder cil = new Cylinder();
            cil.Height = 10;
            cil.BaseRadius = 2;
            cil.TopRadius = 2;
            cil.CreateInContext(gl);
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }

        private void DrawSurface(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Color(0f, 1f, 0f);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(10f, -1f, -16.85f);
            gl.Vertex(-10f, -1f, -16.85f);
            gl.Vertex(-10f, -1f, 17f);
            gl.Vertex(10f, -1f, 17f);
            gl.End();
            gl.PopMatrix();
        }

        private void DrawStairs(OpenGL gl)
        {
            Cube cube = new Cube();
            for (int i = 0; i < 20; i++)
            {
                gl.PushMatrix();
                gl.Color(0f, 0f, 1f);
                gl.Rotate(0f, i*20f, 0f);
                gl.Translate(2.9f/* - i * 0.5*/, -0.7f + i * 0.5, 0f/* - i * 0.4*/);
                gl.Scale(0.8f, 0.2f, 0.2f);
                cube.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                gl.PopMatrix();
            }
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50.0, (double)width / (double)height, 0.5, 20000.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
