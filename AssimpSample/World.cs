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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Threading;

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
        private float m_sceneDistance = 30.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private enum TextureObjects { Stairs = 0, Surface };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//resources//metal.jpg", "..//..//resources//ceramic_tiles.jpg" };

        private DispatcherTimer timer;
        private bool animationInProgress = false;
        private int iteration;
        private float human_rotateX = 0.0f;
        private float human_rotateY = 0.0f;
        private float human_rotateZ = 0.0f;
        private float human_coordinateX = 2.5f;
        private float human_coordinateY = 10.6f;
        private float human_coordinateZ = -1f;
        
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

        public bool AnimationInProgress
        {
            get { return animationInProgress; }
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
            this.m_textures = new uint[m_textureCount];
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

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);

                image.UnlockBits(imageData);
                image.Dispose();
            }
            //SetupLighting(gl);

            m_scene.LoadScene();
            m_scene.Initialize();
        }

        private void SetupLighting(OpenGL gl)
        {
            //gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            //gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_NORMALIZE);

            gl.ShadeModel(OpenGL.GL_SMOOTH);

            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);

            float[] light0pos = new float[] { 10.0f, -5f, 0.0f, 1.0f };
            float[] light0ambient = new float[] { MainWindow.ambientPointLightValue, MainWindow.ambientPointLightValue, MainWindow.ambientPointLightValue, 1.0f };
            float[] light0diffuse = new float[] { 1f, 1f, 0f, 1.0f }; //color
            float[] light0specular = new float[] { 1f, 1f, 0f, 1.0f }; 

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            float[] light1pos = new float[] { 0f, 100f, 0f, 1.0f };
            float[] light1ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            float[] light1diffuse = new float[] { 1f, 0f, 0f, 1.0f }; //color
            float[] light1specular = new float[] { 1f, 0f, 0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //gl.LookAt(5.0f, 7.0f, 0f, 5.0f, 7.0f, 1f, 0.0f, 1.0f, 0.0f);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50.0, (double)m_width / (double)m_height, 0.5, 20000.0);
            //gl.LookAt(-3f, 12.5f, 4f, -3f, 11f, 1f, 0.0f, 1.0f, 0.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            //gl.Rotate(0f, 180f, 0f);

            SetupLighting(gl);
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
            gl.Translate(human_coordinateX, human_coordinateY-1.5, human_coordinateZ);
            gl.Scale(1f * 0.03, MainWindow.humanHeight * 0.03, 1f * 0.03);
            gl.Rotate(human_rotateX, human_rotateY, human_rotateZ);
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
            gl.Disable(OpenGL.GL_AUTO_NORMAL);
            gl.PushMatrix();
            gl.Translate(0f, -1f, 0f);
            gl.Rotate(-90f, 0f, 0f);
            Cylinder cil = new Cylinder
            {
                Height = 10,
                BaseRadius = 2,
                TopRadius = 2
            };
            cil.NormalGeneration = Normals.Smooth;
            cil.CreateInContext(gl);
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.Enable(OpenGL.GL_AUTO_NORMAL);
        }

        private void DrawSurface(OpenGL gl)
        {
            gl.Disable(OpenGL.GL_AUTO_NORMAL);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Surface]);
            gl.PushMatrix();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0f, -1f, 0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Color(0f, 1f, 0f);
            gl.Vertex(10f, -1f, -16.85f);
            gl.TexCoord(0.0f, 10.0f);
            gl.Color(0f, 0f, 1f);
            gl.Vertex(-10f, -1f, -16.85f);
            gl.TexCoord(10.0f, 10.0f);
            gl.Color(1f, 1f, 0f);
            gl.Vertex(-10f, -1f, 17f);
            gl.TexCoord(10.0f, 0.0f);
            gl.Color(1f, 0f, 0f);
            gl.Vertex(10f, -1f, 17f);
            gl.End();
            gl.PopMatrix();
            gl.Enable(OpenGL.GL_AUTO_NORMAL);
        }

        private void DrawStairs(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Stairs]);
            Cube cube = new Cube();
            for (int i = 0; i < 20; i++)
            {
                gl.PushMatrix();
                gl.Color(0f, 0f, 1f);
                gl.Rotate(0f, i*20f, 0f);
                gl.Translate(2.9f, -0.7f + i * 0.5, 0f);
                gl.Scale(0.8f, 0.2f, 0.2f);
                cube.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                gl.PopMatrix();
            }
        }

        public void Animation()
        {
            animationInProgress = true;
            iteration = 0;
            human_coordinateX = 2.5f;
            human_coordinateY = 10.6f;
            human_coordinateZ = -1f;
            human_rotateY = 0f;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(MainWindow.animationSpeed);
            timer.Tick += new EventHandler(AnimateHuman);
            timer.Start();
        }

        public void AnimateHuman(object sender, EventArgs e)
        {
            if (iteration < 10)
            {
                To17thStep();
            }
            else if (iteration >= 10 && iteration < 20)
            {
                To14thStep();
            }
            else if (iteration >= 20 && iteration < 30)
            {
                To11thStep();
            }
            else if (iteration >= 30 && iteration < 40)
            {
                To8thStep();
            }
            else if (iteration >= 40 && iteration < 50)
            {
                To5thStep();
            }
            else if (iteration >= 50 && iteration < 60)
            {
                To2ndStep();
            }
            else if (iteration >= 60 && iteration < 70)
            {
                ToFloor();
            }
            else
            {
                timer.Stop();
                animationInProgress = false;
            }
        }

        private void To17thStep()
        {
            iteration++;
            human_coordinateX -= 0.05f;
            human_coordinateY -= 0.15f;
            human_coordinateZ += 0.3f;
            human_rotateY += -8f;
        }

        private void To14thStep()
        {
            iteration++;
            human_coordinateX -= 0.25f;
            human_coordinateY -= 0.15f;
            human_coordinateZ += 0.05f;
            human_rotateY += -3f;
        }

        private void To11thStep()
        {
            iteration++;
            human_coordinateX -= 0.22f;
            human_coordinateY -= 0.15f;
            human_coordinateZ -= 0.18f;
            human_rotateY += -5f;
        }

        private void To8thStep()
        {
            iteration++;
            human_coordinateX += 0.03f;
            human_coordinateY -= 0.15f;
            human_coordinateZ -= 0.2f;
            human_rotateY += -5f;
        }

        private void To5thStep()
        {
            iteration++;
            human_coordinateX += 0.29f;
            human_coordinateY -= 0.15f;
            human_coordinateZ -= 0.18f;
            human_rotateY += -7f;
        }

        private void To2ndStep()
        {
            iteration++;
            human_coordinateX += 0.23f;
            human_coordinateY -= 0.15f;
            human_coordinateZ += 0.23f;
            human_rotateY += -5f;
        }

        private void ToFloor()
        {
            iteration++;
            human_coordinateY -= 0.09f;
            human_coordinateZ += 0.2f;
            human_rotateY += -3f;
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
