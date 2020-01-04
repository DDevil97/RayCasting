using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static SFMLTest.RayCaster;

namespace SFMLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MySFMLProgram app = new MySFMLProgram();
            app.StartSFMLProgram();
        }


    }

    class MySFMLProgram
    {
        RenderWindow window;
        Font font;
        RayCaster caster;

        Vector2i screen = new Vector2i(100,75);
        Vector2f pixel;

        float fov = 90;
        float distanceProjectionPlane;

        int[,] _m = {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,0,0,0,0,1,1,0,1,1,1,0,1,1},
        {1,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1},
        {1,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1},
        {1,0,1,0,0,1,1,1,1,0,1,0,0,0,0,1},
        {1,0,1,0,0,1,0,0,0,0,1,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };


        public void Render(Vector2f player, float angle)
        {
            List<Vertex> vertices = new List<Vertex>();

            for (int x = 0; x < screen.X;x++)
            {
                float rayAngle = AtanD((x - screen.X / 2.0f) / distanceProjectionPlane);
                RayResult ray = caster.RayCast(player, angle + rayAngle);
                float lineHeightHalf = ((caster.CellSize / (ray.Magnitude * CosD(rayAngle))) * distanceProjectionPlane)/2;
                Color color = Color.Black;
                switch (ray.Side)
                {
                    case Side.Down:
                    case Side.Up:
                        color = new Color(255, 255, 255, 255);
                        break;
                    case Side.Left:
                    case Side.Right:
                        color = new Color(128, 128, 128, 255);
                        break;
                }

                vertices.Add(new Vertex
                    {
                        Position = new Vector2f(x * pixel.X, Round(screen.Y / 2 - lineHeightHalf) * pixel.Y),
                        Color = color
                    });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f((x + 1) * pixel.X, Round(screen.Y / 2 - lineHeightHalf) * pixel.Y),
                    Color = color
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f((x + 1) * pixel.X, Round(screen.Y / 2 + lineHeightHalf) * pixel.Y),
                    Color = color
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector2f(x * pixel.X, Round(screen.Y / 2 + lineHeightHalf) * pixel.Y),
                    Color = color
                });
            }

            window.Draw(vertices.ToArray(), 0, (uint)vertices.Count, PrimitiveType.Quads);
        }

        public void StartSFMLProgram()
        {
            #region Inicialization
            bool[,] Map = new bool[_m.GetLength(0), _m.GetLength(1)];

            for (int x = 0; x < _m.GetLength(0); x++)
                for (int y = 0; y < _m.GetLength(1); y++)
                    Map[x, y] = _m[x, y] == 0 ? false : true;

            window = new RenderWindow(new VideoMode(800, 600), "SFML window");
            window.SetVisible(true);
            window.Closed += new EventHandler(OnClosed);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);

            pixel = new Vector2f(window.Size.X / screen.X,window.Size.Y / screen.Y);
            distanceProjectionPlane = (screen.X / 2) / TanD(fov / 2);
            caster = new RayCaster
            {
                CellSize = 16,
                Map = Map
            };
            #endregion

            Vector2f player = new Vector2f(80,80);
            float angle = 0;

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.Black);

                Render(player, angle++);

                window.Display();
                Thread.Sleep(10);
            }
        }

        #region Event listeners
        void OnClosed(object sender, EventArgs e)
        {
            window.Close();
        }

        void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape)
                window.Close();
        } 
        #endregion
    }
}