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
        RenderTexture rs;
        RenderWindow window;
        Font font;
        RayCaster caster;

        Vector2i screen = new Vector2i(200,150);

        float fov = 100;

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

        public void StartSFMLProgram()
        {
            #region Inicialization      
            TileInfo[,] Map = new TileInfo[_m.GetLength(0), _m.GetLength(1)];

            for (int y = 0; y < _m.GetLength(1); y++)
                for (int x = 0; x < _m.GetLength(0); x++)
                    Map[x, y] = _m[y,x] == 1 ? new TileInfo {
                        Solid=true,
                        DownAtlas = new Vector2i(0,0),
                        UpAtlas = new Vector2i(0, 0),
                        LeftAtlas = new Vector2i(1, 0),
                        RightAtlas = new Vector2i(1, 0)
                    } : new TileInfo { Solid=false};

            window = new RenderWindow(new VideoMode(800, 600), "SFML window");
            window.SetVisible(true);
            window.Closed += new EventHandler(OnClosed);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
            rs = new RenderTexture((uint)screen.X, (uint)screen.Y);

            caster = new RayCaster
            {
                CellSize = 16,
                Map = Map
            };

            Renderer ren = new Renderer(caster,rs, fov);
            ren.Textures.Add(new Texture("Texture.png"));
            #endregion

            Vector2f player = new Vector2f(caster.CellSize * 6 + 8, caster.CellSize * 5 + 8);
            float angle = 45;
            Vector2f M;
            while (window.IsOpen)
            {
                angle -= Keyboard.IsKeyPressed(Keyboard.Key.Left) ? 2 : 0;
                angle += Keyboard.IsKeyPressed(Keyboard.Key.Right) ? 2 : 0;

                M = new Vector2f(0,0);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                    M += new Vector2f(CosD(angle) * 2, SinD(angle) * 2);

                if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                    M -= new Vector2f(CosD(angle) * 2, SinD(angle) * 2);

                if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                    M += new Vector2f(CosD(angle + 90) * 2, SinD(angle + 90) * 2);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                    M += new Vector2f(CosD(angle - 90) * 2, SinD(angle - 90) * 2);


                RayResult R = caster.RayCast(player, 0);
                if (R.Magnitude < Math.Abs(M.X) + 10 && Math.Sign(M.X) == 1)
                    M.X = 0;

                R = caster.RayCast(player, 180);
                if (R.Magnitude < Math.Abs(M.X) + 10 && Math.Sign(M.X) == -1)
                    M.X = 0;

                R = caster.RayCast(player, 90);
                if (R.Magnitude < Math.Abs(M.Y) + 10 && Math.Sign(M.Y) == 1)
                    M.Y = 0;

                R = caster.RayCast(player, 270);
                if (R.Magnitude < Math.Abs(M.Y) + 10 && Math.Sign(M.Y) == -1)
                    M.Y = 0;

                player += M;


                window.DispatchEvents();
                rs.Clear(Color.Black);

                ren.Render(player, angle);

                window.Draw(new Vertex[] {
                    new Vertex
                    {
                        Position = new Vector2f(0,0),
                        TexCoords = new Vector2f(0,screen.Y-1),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(0,window.Size.Y-1),
                        TexCoords = new Vector2f(0,0),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(window.Size.X-1,window.Size.Y-1),
                        TexCoords = new Vector2f(screen.X-1,0),
                        Color = Color.White
                    },
                    new Vertex
                    {
                        Position = new Vector2f(window.Size.X-1,0),
                        TexCoords = new Vector2f(screen.X-1,screen.Y-1),
                        Color = Color.White
                    },
                },0,4,PrimitiveType.Quads,new RenderStates(rs.Texture));
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