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
using static SFMLTest.Helpers;
using SFMLTest.Tile;

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
        float angle = 0;
        Vector2i screen = new Vector2i(300, 250);

        float fov = 80;

        int[,] _m = {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,2,0,0,0,0,2,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

        public void StartSFMLProgram()
        {
            #region Initialization      
            BaseTile[,] Map = new BaseTile[_m.GetLength(0), _m.GetLength(1)];

            for (int y = 0; y < _m.GetLength(1); y++)
                for (int x = 0; x < _m.GetLength(0); x++)
                    switch (_m[y, x])
                    {
                        case 1:
                            Map[x, y] = new BaseTile
                            {
                                Solid = true,
                                DownAtlas = new Vector2i(1, 0),
                                UpAtlas = new Vector2i(1, 0),
                                LeftAtlas = new Vector2i(1, 0),
                                RightAtlas = new Vector2i(1, 0)
                            };
                            break;

                        case 2:
                            Map[x, y] = new CircleTile
                            {
                                Solid = true,
                                DownAtlas = new Vector2i(1, 0),
                                UpAtlas = new Vector2i(1, 0),
                                LeftAtlas = new Vector2i(1, 0),
                                RightAtlas = new Vector2i(1, 0),
                                CeilAtlas = new Vector2i(0, 0),
                                FloorAtlas = new Vector2i(2, 0)
                            };
                            break;
                        case 5:
                            Map[x, y] = new BaseTile
                            {
                                Solid = false,
                                DownAtlas = new Vector2i(0, 0),
                                UpAtlas = new Vector2i(0, 0),
                                LeftAtlas = new Vector2i(1, 0),
                                RightAtlas = new Vector2i(1, 0),
                                CeilAtlas = new Vector2i(0, 0),
                                FloorAtlas = new Vector2i(5, 0)
                            };
                            break;

                        default:
                            Map[x, y] = new BaseTile
                            {
                                Solid = false,
                                DownAtlas = new Vector2i(0, 0),
                                UpAtlas = new Vector2i(0, 0),
                                LeftAtlas = new Vector2i(1, 0),
                                RightAtlas = new Vector2i(1, 0),
                                CeilAtlas = new Vector2i(0, 0),
                                FloorAtlas = new Vector2i(2, 0)
                            };
                            break;
                    }


            window = new RenderWindow(new VideoMode(800,600), "SFML window", Styles.Default);
            window.SetVisible(true);
            window.Closed += new EventHandler(OnClosed);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
            window.MouseMoved += Window_MouseMoved;
            rs = new RenderTexture((uint)screen.X, (uint)screen.Y);

            caster = new RayCaster
            {
                CellSize = 32,
                Map = Map
            };

            Renderer ren = new Renderer(caster, window, rs, fov);
            ren.Textures.Add(new Texture("Texture.png"));
            ren.MapAtlasInUse = 0;
            #endregion

            Vector2f player = new Vector2f(caster.CellSize * 6 + 8, caster.CellSize * 5 + 8);
            Vector2f sp1 = player + new Vector2f(70, 15);
            Vector2f sp2 = player + new Vector2f(50, 70);

            Vector2f sp3 = new Vector2f(caster.CellSize * 6 + 8, caster.CellSize * 5 + 8) + new Vector2f(30, -30);
            Vector2f scen = sp1 + new Vector2f(60, 60);

            Vector2f M;

            font = new Font("Perfect DOS VGA 437 Win.ttf");
            Text t = new Text("Fps: ", font, 16);
            int fps = 0;
            int fpsCounter = 0;
            int ticks = Environment.TickCount;

            var lamps = new List<Light>
                {
                    new Light {
                        Position = new Vector2f(sp1.X,sp1.Y),
                        Color = new Color(255,255,255)

                    },
                    new Light {
                        Position = new Vector2f(sp2.X,sp2.Y),
                        //Color = new Color(255,255,255)
                        Color = new Color(128,128,0)
                    }
                };

            ren.LightMapScaler = 4f;
            ren.GenerateLightMap(lamps);

            List<Sprite> sprites = new List<Sprite>();
            Random r = new Random();

            for (int i = 0; i < 10; i++)
                sprites.Add(new Sprite
                {
                    Atlas = new Vector2i(4, 0),
                    Position = new Vector2f((float)r.NextDouble() * caster.CellSize * Map.GetLength(0), (float)r.NextDouble() * caster.CellSize * Map.GetLength(1))
                });

            sprites.Add(new Sprite
            {
                Atlas = new Vector2i(3, 0),
                Position = sp2
            });

            Sprite p = new Sprite
            {
                Atlas = new Vector2i(3, 0),
                Position = sp1
            };
            sprites.Add(p);

            while (window.IsOpen)
            {
                if (Environment.TickCount - ticks >= 1000)
                {
                    fps = fpsCounter;
                    fpsCounter = 0;
                    ticks = Environment.TickCount;
                }

                angle -= (window.Size.X / 2 - Mouse.GetPosition().X) / 4f;



                angle -= Keyboard.IsKeyPressed(Keyboard.Key.Left) ? 2 : 0;
                angle += Keyboard.IsKeyPressed(Keyboard.Key.Right) ? 2 : 0;

                M = new Vector2f(0, 0);

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



                Mouse.SetPosition(new Vector2i((int)window.Size.X / 2, (int)window.Size.Y / 2));

                window.DispatchEvents();
                rs.Clear(Color.Black);


                //lamps[0].Position = player;
                p.Position = lamps[0].Position;
                ren.Render(player, angle, sprites, lamps);
                t.DisplayedString = $"Fps: {fps}";
                rs.Draw(t);
                ren.ShowBuffer();
                Thread.Sleep(0);

                fpsCounter++;
            }
        }

        //private void RenderMiniMap()

        private void Window_MouseMoved(object sender, MouseMoveEventArgs e)
        {
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