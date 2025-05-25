using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bratalian2
{
    public class Game1 : Game
    {
        private enum GameState
        {
            Exploring,
            Battle
        }
        private enum BattlePhase
        {
            EnemyIntroText,     // Fase 1: Mostrar "Você encontrou..."
            PlayerChooseText,   // Fase 2: Mostrar "Escolhe o teu Bratalian!"
            PlayerChoosing,     // Fase 3: Jogador navega e escolhe
            PlayerLeaving,       // Fase 4: Player sai de cena
            PlayerEntering,     // Fase 5: Bratalian do jogador entra em cena
            BattleActive,        // Fase 6: Ambos prontos para lutar
                VictoryText

        }
        BattlePhase battlePhase = BattlePhase.EnemyIntroText;
        

        private const int TileSize = 16;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private const float BratalianBattleY = 120f;
        private KeyboardState previousKeyboardState;


        // Texturas de mapa e fundo
        private Texture2D tilesetTex, borderTex, bushTex, battleBackgroundTex;

        // Player e mapa
        private Player player;
        private MapZone bigZone;

        // Parâmetros do mapa
        private int exteriorMargin = 12, interiorMargin = 2;
        private int zoneW = 38, zoneH = 22, pathW = 3, gap = 30, vgap = 24;

        // Câmera
        private Camera2D camera;

        // Estado de jogo
        private GameState currentState = GameState.Exploring;

        // Bratalians
        private List<Bratalian> bratalians = new List<Bratalian>();
        private Bratalian bratalianAtual;
        private Bratalian bratalianPlayer;

     
        private int playerHealth = 100;
        private int bratalianHealth = 100;
        private const int maxHealth = 100;
        private const float V = 300f;
        private Texture2D healthBarTexture;
        Vector2 battlePlayerPosition = new Vector2(100, 500); // posição inicial de costas
        Vector2 battlePlayerExitPosition = new Vector2(100, 700); // fora da tela embaixo
        Vector2 bratalianBattlePosition = new Vector2(100, 800); // entra de baixo
        Vector2 bratalianBattleTargetPosition = new Vector2(100, 350); // onde ele vai parar
        Vector2 bratalianEnemyPosition;
        private int ataqueSelecionadoIndex = 0;



        float bratalianPlayerScale = 3f; // aumenta tamanho do bratalian do jogador



        private Texture2D battleInterfaceTex;
        private Texture2D battlePlayerTex;
        List<Bratalian> equipeDoJogador = new List<Bratalian>();
        int bratalianSelecionadoIndex = 0;
        Vector2 battlePlayerFixedPosition = new Vector2(100, 350); // nova posição mais acima


        // Texto de encontro
        private SpriteFont font;
        private bool mostrarTextoDeEncontro = false;
        private string textoDoBratalian = "";
        private string textoAtaqueSelecionado = "";

        private float textoTimer = 0f;
        private float textoSpeed = 0.05f; 
        private int letrasVisiveis = 0;


        // Ginásios
        private Texture2D gymBlueTex, gymGreenTex, gymRedTex;
        private Vector2 gymBluePos, gymGreenPos, gymRedPos;
        private List<Rectangle> gymCollisionRects = new List<Rectangle>();

        // Árvores
        private Texture2D treeTex, treeBlueTex, bigTreeTex, bigTreeBlueTex;
        private List<Tree> trees = new List<Tree>();
        private List<Rectangle> treeCollisionRects = new List<Rectangle>();

        // Interação com “E”
        private Texture2D interactTex;
        private Tree promptTree;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            bigZone = MapGenerator.Generate(
                exteriorMargin, interiorMargin,
                zoneW, zoneH, pathW,
                gap, vgap);

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            camera = new Camera2D(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Texturas de mapa
            tilesetTex = Content.Load<Texture2D>("tileset");
            borderTex = Content.Load<Texture2D>("borda");
            bushTex = Content.Load<Texture2D>("bush");


            // Player
            var idleTex = Content.Load<Texture2D>("Char_002_Idle");
            var walkTex = Content.Load<Texture2D>("Char_002");
            player = new Player(idleTex, walkTex);
            int sx = exteriorMargin + 1 + interiorMargin + (zoneW - 2 * interiorMargin) / 2;
            int sy = exteriorMargin + 1 + interiorMargin + (zoneH - 2 * interiorMargin) / 2;
            player.Position = new Vector2(sx * TileSize, sy * TileSize);

            // Batalha 
            battleBackgroundTex = Content.Load<Texture2D>("battle_bck");
            font = Content.Load<SpriteFont>("Arial");
            battleInterfaceTex = Content.Load<Texture2D>("battle_interface");
            battlePlayerTex = Content.Load<Texture2D>("battle_player");
            healthBarTexture = new Texture2D(GraphicsDevice, 1, 1);
            healthBarTexture.SetData(new[] { Color.White });
            Texture2D inicialTex = Content.Load<Texture2D>("tralalero_tralala");
            Vector2 inicialPos = new Vector2(100, 180);

            


            // Ginásios
            gymBlueTex = Content.Load<Texture2D>("ginasioazul");
            gymGreenTex = Content.Load<Texture2D>("ginasioverde");
            gymRedTex = Content.Load<Texture2D>("ginasiovermelho");
            int z1x = exteriorMargin + 1, z1y = exteriorMargin + 1;
            int z2x = z1x + zoneW + gap;
            int z3y = z1y + zoneH + vgap;
            int z3x = z1x, z4x = z2x;
            Func<int, int, Vector2> center = (zx, zy) => new Vector2(
                (zx + interiorMargin + (zoneW - 2 * interiorMargin) / 2) * TileSize,
                (zy + interiorMargin + (zoneH - 2 * interiorMargin) / 2) * TileSize);
            gymBluePos = center(z2x, z1y);
            gymGreenPos = center(z3x, z3y);
            gymRedPos = center(z4x, z3y);
            gymCollisionRects.Add(new Rectangle(
                (int)gymBluePos.X - gymBlueTex.Width / 2,
                (int)gymBluePos.Y - gymBlueTex.Height,
                gymBlueTex.Width, gymBlueTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymGreenPos.X - gymGreenTex.Width / 2,
                (int)gymGreenPos.Y - gymGreenTex.Height,
                gymGreenTex.Width, gymGreenTex.Height));
            gymCollisionRects.Add(new Rectangle(
                (int)gymRedPos.X - gymRedTex.Width / 2,
                (int)gymRedPos.Y - gymRedTex.Height,
                gymRedTex.Width, gymRedTex.Height));

            // Árvores e ícone E
            treeTex = Content.Load<Texture2D>("minitree");
            treeBlueTex = Content.Load<Texture2D>("minitreeazul");
            bigTreeTex = Content.Load<Texture2D>("tree");
            bigTreeBlueTex = Content.Load<Texture2D>("treeazul");
            interactTex = Content.Load<Texture2D>("E");

            var rnd = new Random();
            const double treeChance = 0.05, blueRatio = 0.1;
            for (int x = 0; x < bigZone.Width; x++)
                for (int y = 0; y < bigZone.Height; y++)
                {
                    if (bigZone.Tiles[x, y] != TileType.Grass || rnd.NextDouble() >= treeChance)
                        continue;
                    if (trees.Any(t =>
                        Math.Abs((t.Position.X / TileSize) - x) <= 1 &&
                        Math.Abs((t.Position.Y / TileSize) - y) <= 1))
                        continue;

                    bool isBlue = rnd.NextDouble() < blueRatio;
                    bool isBig = rnd.NextDouble() < 0.5;
                    var tex = isBig
                        ? (isBlue ? bigTreeBlueTex : bigTreeTex)
                        : (isBlue ? treeBlueTex : treeTex);

                    var pos = new Vector2(x * TileSize, y * TileSize);
                    trees.Add(new Tree(tex, pos, isBlue));

                    // colisão de 1 tile de largura, ajustado verticalmente
                    int collW = TileSize;
                    int offX = (int)pos.X + (tex.Width - collW) / 2;
                    treeCollisionRects.Add(new Rectangle(
                        offX, (int)pos.Y - (tex.Height - TileSize),
                        collW, tex.Height));
                }
            // Bratalians
            var bratalianData = new[]
            {
                new {
                    Name = "Tralalero Tralala",
                    Type1 = "Dark", Type2 = "Water",
                    Attacks = new[] {
                        new Attack("Bite", 60, 100),
                        new Attack("Aqua Tail", 90, 90),
                        new Attack("Dive", 80, 100)
                    }
                },
                new {
                    Name = "Tung Tung Tung Sahur",
                    Type1 = "Fighting", Type2 = (string?)null,
                    Attacks = new[] {
                        new Attack("Force Palm", 60, 100),
                        new Attack("Revenge", 60, 100),
                        new Attack("Hammer", 100, 90)
                    }
                },
                new {
                    Name = "Lirili Larila",
                    Type1 = "Fairy", Type2 = "Flying",
                    Attacks = new[] {
                        new Attack("Draining Kiss", 50, 100),
                        new Attack("Air Slash", 75, 95),
                        new Attack("Dazzling Gleam", 80, 100)
                    }
                },
                new {
                    Name = "Cappuccino Assassino",
                    Type1 = "Dark", Type2 = "Poison",
                    Attacks = new[] {
                        new Attack("Poison Jab", 80, 100),
                        new Attack("Night Slash", 70, 100),
                        new Attack("Throat Chop", 80, 100)
                    }
                },
                new {
                    Name = "Brr Brr Patapim",
                    Type1 = "Grass", Type2 = "Fighting",
                    Attacks = new[] {
                        new Attack("Power Whip", 120, 85),
                        new Attack("Low Sweep", 65, 100),
                        new Attack("Leaf Blade", 90, 100)
                    }
                },
                new {
                    Name = "Bombardino Crocodilo",
                    Type1 = "Ground", Type2 = "Steel",
                    Attacks = new[] {
                        new Attack("Iron Tail", 100, 75),
                        new Attack("Earthquake", 100, 100),
                        new Attack("Metal Claw", 50, 95)
                    }
                },
                new {
                    Name = "La Vaca Saturno Saturnita",
                    Type1 = "Psychic", Type2 = "Fairy",
                    Attacks = new[] {
                        new Attack("Zen Headbutt", 80, 90),
                        new Attack("Moonblast", 95, 100),
                        new Attack("Psybeam", 65, 100)
                    }
                },
                new {
                    Name = "Frigo Cammello",
                    Type1 = "Ice", Type2 = "Ground",
                    Attacks = new[] {
                        new Attack("Icicle Crash", 85, 90),
                        new Attack("Stomping Tantrum", 75, 100),
                        new Attack("Avalanche", 60, 100)
                    }
                },
                new {
                    Name = "Boneca Ambalabu",
                    Type1 = "Dark", Type2 = "Grass",
                    Attacks = new[] {
                        new Attack("Bite", 60, 100),
                        new Attack("Seed Bomb", 80, 100),
                        new Attack("Foul Play", 95, 100)
                    }
                },
                new {
                    Name = "Bombardini Guzzini",
                    Type1 = "Flying", Type2 = "Steel",
                    Attacks = new[] {
                        new Attack("Drill Peck", 80, 100),
                        new Attack("Steel Wing", 70, 90),
                        new Attack("Air Slash", 75, 95)
                    }
                },
                new {
                    Name = "Trippi Troppi Troppa Trippa",
                    Type1 = "Water", Type2 = "Ground",
                    Attacks = new[] {
                        new Attack("Muddy Water", 90, 85),
                        new Attack("Waterfall", 80, 100),
                        new Attack("Bulldoze", 60, 100)
                    }
                },
                new {
                    Name = "Trulimero Trulicina",
                    Type1 = "Water", Type2 = "Psychic",
                    Attacks = new[] {
                        new Attack("Psychic Fangs", 85, 100),
                        new Attack("Surf", 90, 100),
                        new Attack("Zen Headbutt", 80, 90)
                    }
                },
            };

            foreach (var b in bratalianData)
            {
                var tex = Content.Load<Texture2D>(b.Name.ToLower().Replace(" ", "_"));
                Vector2 pos;
                do
                {
                    int bx = rnd.Next(exteriorMargin + interiorMargin + 1,
                                      bigZone.Width - exteriorMargin - interiorMargin - 1);
                    int by = rnd.Next(exteriorMargin + interiorMargin + 1,
                                      bigZone.Height - exteriorMargin - interiorMargin - 1);
                    pos = new Vector2(bx * TileSize, by * TileSize);
                } while (bigZone.Tiles[(int)(pos.X / TileSize), (int)(pos.Y / TileSize)] != TileType.Bush);

                var bratalian = new Bratalian(tex, pos, b.Name, new Vector2(0.2f, 0.2f), b.Type1, b.Type2);
                foreach (var atk in b.Attacks)
                    bratalian.Attacks.Add(atk);

                bratalians.Add(bratalian);
            }
            Bratalian inicial = bratalians[0];  
            equipeDoJogador.Add(inicial);

        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape)) Exit();
            if (currentState == GameState.Exploring)
            {
                var old = player.Position;
                player.Update(gameTime, ks, bigZone);

                // colisão tile-a-tile (fronteiras)
                int tx = ((int)player.Position.X + player.Width / 2) / TileSize;
                int ty = ((int)player.Position.Y + player.Height / 2) / TileSize;
                if (IsBlocked(bigZone, tx, ty))
                    player.Position = old;

                // colisão ginásios e árvores
                var pr = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Width, player.Height);
                foreach (var gr in gymCollisionRects)
                    if (pr.Intersects(gr)) { player.Position = old; break; }
                foreach (var tr in treeCollisionRects)
                    if (pr.Intersects(tr)) { player.Position = old; break; }

                // detecção de árvore azul interativa
                promptTree = trees.FirstOrDefault(t =>
                {
                    if (!t.IsInteractive) return false;
                    var r = t.Bounds; r.Inflate(4, 4);
                    return r.Intersects(pr);
                });

                // interação E
                if (promptTree != null && ks.IsKeyDown(Keys.E))
                {
                    if (promptTree.Texture == treeBlueTex) promptTree.Texture = treeTex;
                    else if (promptTree.Texture == bigTreeBlueTex) promptTree.Texture = bigTreeTex;
                    promptTree.IsInteractive = false;
                    promptTree = null;
                }

                // encontros
                foreach (var b in bratalians)
                {
                    if (b.IsDefeated) continue; // IGNORA derrotados

                    var br = new Rectangle((int)b.Position.X, (int)b.Position.Y, 24, 24);
                    if (pr.Intersects(br))
                    {
                        StartBattle(b);
                        break;
                    }
                }

            }
            else if (currentState == GameState.Battle)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                switch (battlePhase)
                {
                    case BattlePhase.EnemyIntroText:
                        var enemyTarget = new Vector2(GraphicsDevice.Viewport.Width - 550, BratalianBattleY);
                        var enemyDir = enemyTarget - bratalianEnemyPosition;
                        if (enemyDir.Length() > 2f)
                        {
                            enemyDir.Normalize();
                            bratalianEnemyPosition += enemyDir * 300f * dt;
                        }
                        else
                        {
                            bratalianEnemyPosition = enemyTarget; // fixa posição final
                        }
                        if (mostrarTextoDeEncontro && letrasVisiveis < textoDoBratalian.Length)
                        {
                            textoTimer += dt;
                            if (textoTimer >= textoSpeed)
                            {
                                letrasVisiveis++;
                                textoTimer = 0f;
                            }
                        }
                        else if (mostrarTextoDeEncontro && letrasVisiveis >= textoDoBratalian.Length)
                        {
                            // Passa para a próxima fase após o texto terminar
                            textoDoBratalian = "Escolhe o teu Bratalian!";
                            letrasVisiveis = 0;
                            textoTimer = 0f;
                            mostrarTextoDeEncontro = true;
                            battlePhase = BattlePhase.PlayerChooseText;
                        }
                        break;

                    case BattlePhase.PlayerChooseText:

                        textoTimer += dt;
                        if (letrasVisiveis < textoDoBratalian.Length)
                        {
                            if (textoTimer >= textoSpeed)
                            {
                                letrasVisiveis++;
                                textoTimer = 0f;
                            }
                        }
                        else
                        {
                            // Quando termina o texto, permite escolha
                            battlePhase = BattlePhase.PlayerChoosing;
                        }
                        break;
                    case BattlePhase.PlayerChoosing:

                        var keyboard = Keyboard.GetState();
                        if (keyboard.IsKeyDown(Keys.Down))
                            bratalianSelecionadoIndex = (bratalianSelecionadoIndex + 1) % equipeDoJogador.Count;
                        if (keyboard.IsKeyDown(Keys.Up))
                            bratalianSelecionadoIndex = (bratalianSelecionadoIndex - 1 + equipeDoJogador.Count) % equipeDoJogador.Count;

                        if (keyboard.IsKeyDown(Keys.Enter))
                        {
                            bratalianPlayer = equipeDoJogador[bratalianSelecionadoIndex];
                            bratalianBattlePosition = bratalianBattleTargetPosition;
                            ataqueSelecionadoIndex = 0;
                            battlePhase = BattlePhase.PlayerLeaving;

                        }
                        break;
                    case BattlePhase.PlayerLeaving:
                        {
                            var dir = battlePlayerExitPosition - battlePlayerPosition;
                            if (dir.Length() > 2f)
                            {
                                dir.Normalize();
                                battlePlayerPosition += dir * 600f * dt;
                            }
                            else
                            {
                                battlePlayerPosition = battlePlayerExitPosition;
                                battlePhase = BattlePhase.PlayerEntering;
                            }
                            break;
                        }


                    case BattlePhase.PlayerEntering:
                        {
                            float speed = 600f;
                            var direction = bratalianBattleTargetPosition - bratalianBattlePosition;
                            if (direction.Length() > 1f)
                            {
                                direction.Normalize();
                                bratalianBattlePosition += direction * speed * dt;
                            }
                            else
                            {
                                bratalianBattlePosition = bratalianBattleTargetPosition;
                                battlePhase = BattlePhase.BattleActive;
                            }
                            break;
                        }


                    case BattlePhase.BattleActive:
                        keyboard = Keyboard.GetState();

                        bool upPressed = keyboard.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up);
                        bool downPressed = keyboard.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down);

                        if (bratalianPlayer != null && bratalianPlayer.Attacks != null && bratalianPlayer.Attacks.Count > 0)
                        {
                            if (upPressed)
                            {
                                ataqueSelecionadoIndex--;
                                if (ataqueSelecionadoIndex < 0)
                                    ataqueSelecionadoIndex = bratalianPlayer.Attacks.Count - 1;
                            }
                            else if (downPressed)
                            {
                                ataqueSelecionadoIndex = (ataqueSelecionadoIndex + 1) % bratalianPlayer.Attacks.Count;
                            }
                        }

                        bool enterPressed = keyboard.IsKeyDown(Keys.Enter) && !previousKeyboardState.IsKeyDown(Keys.Enter);
                        if (enterPressed && bratalianPlayer.Attacks.Count > 0)
                        {
                            var ataqueEscolhido = bratalianPlayer.Attacks[ataqueSelecionadoIndex];
                            textoAtaqueSelecionado = $"Você escolheu: {ataqueEscolhido.Name}";

                            // Aplica dano no inimigo
                            bratalianHealth -= ataqueEscolhido.Power;

                            if (bratalianHealth <= 0)
                            {
                                textoDoBratalian = $"Voce venceu! Ganhou {bratalianAtual.Name}!";
                                letrasVisiveis = 0;
                                textoTimer = 0f;
                                mostrarTextoDeEncontro = true;
                                battlePhase = BattlePhase.VictoryText;
                            }




                            Console.WriteLine(textoAtaqueSelecionado);
                        }

                        previousKeyboardState = keyboard;

                        break;
                    case BattlePhase.VictoryText:
                        textoTimer += dt;
                        if (letrasVisiveis < textoDoBratalian.Length)
                        {
                            if (textoTimer >= textoSpeed)
                            {
                                letrasVisiveis++;
                                textoTimer = 0f;
                            }
                        }
                        else
                        {
                            // Quando termina o texto, adiciona o Bratalian ao jogador e sai da batalha
                            if (bratalianAtual != null && !equipeDoJogador.Contains(bratalianAtual))
                            {
                                equipeDoJogador.Add(bratalianAtual);
                            }

                            if (bratalianAtual != null && bratalians.Contains(bratalianAtual))
                            {
                                bratalians.Remove(bratalianAtual);
                            }

                            bratalianAtual = null;
                            mostrarTextoDeEncontro = false;
                            currentState = GameState.Exploring;
                        }
                        break;



                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // === Batch 1: Mundo ===
            var view = camera.GetViewMatrix(player.Position);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: view);

            DrawBackground();
            DrawMapZone(bigZone);

            foreach (var t in trees) t.Draw(spriteBatch);
            player.Draw(spriteBatch);

            spriteBatch.Draw(gymBlueTex, gymBluePos - new Vector2(gymBlueTex.Width / 2, gymBlueTex.Height), Color.White);
            spriteBatch.Draw(gymGreenTex, gymGreenPos - new Vector2(gymGreenTex.Width / 2, gymGreenTex.Height), Color.White);
            spriteBatch.Draw(gymRedTex, gymRedPos - new Vector2(gymRedTex.Width / 2, gymRedTex.Height), Color.White);

            if (mostrarTextoDeEncontro)
            {
                var ts = font.MeasureString(textoDoBratalian);
                var tp = player.Position - new Vector2(ts.X / 2, ts.Y + 10);
                spriteBatch.DrawString(font, textoDoBratalian, tp, Color.White);
            }

            spriteBatch.End();

            // === Batch 2: UI “E” ===
            if (promptTree != null)
            {
                spriteBatch.Begin();
                var b = promptTree.Bounds;
                var wp = new Vector2(b.Center.X, b.Top);
                var sp = Vector2.Transform(wp, view);
                spriteBatch.Draw(interactTex,
                    sp - new Vector2(interactTex.Width / 2, interactTex.Height + 4),
                    Color.White);
                spriteBatch.End();
            }

            // === Batch 3: Batalha ===
            if (currentState == GameState.Battle)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);

                // Fundo da batalha
                spriteBatch.Draw(battleBackgroundTex, GraphicsDevice.Viewport.Bounds, Color.White);

                

                // Player ainda não saiu
                if (battlePhase != BattlePhase.PlayerEntering && bratalianPlayer == null)
                {
                    spriteBatch.Draw(battlePlayerTex, battlePlayerPosition, null, Color.White, 0f, Vector2.Zero, 20f, SpriteEffects.None, 0f);
                }

                // Inimigo visível sempre
                if (bratalianAtual != null)
                {
                    spriteBatch.Draw(bratalianAtual.Texture, bratalianEnemyPosition, null,
                                     Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                }

                // Player visível se já entrou e foi escolhido
                if (battlePhase != BattlePhase.PlayerChoosing && bratalianPlayer != null)
                {
                    spriteBatch.Draw(bratalianPlayer.Texture, bratalianBattlePosition, null,
                                     Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                }

                // Interface por cima
                spriteBatch.Draw(battleInterfaceTex, GraphicsDevice.Viewport.Bounds, Color.White * 0.5f);
                // Texto de introdução do inimigo
                if (battlePhase == BattlePhase.EnemyIntroText)
                {
                    Rectangle caixaTexto = new Rectangle(100, 550, 1066, 100);
                    string textoParcial = textoDoBratalian.Substring(0, letrasVisiveis);
                    string textoFormatado = QuebrarTexto(textoParcial, font, caixaTexto.Width);
                    Vector2 posicaoTexto = new Vector2(caixaTexto.X + 10, caixaTexto.Y + 20);
                    spriteBatch.DrawString(font, textoFormatado, posicaoTexto, Color.Black);
                }
                // Escolha do Bratalian
                if (battlePhase == BattlePhase.PlayerChoosing)
                {
                    spriteBatch.DrawString(font, "Escolhe o teu Bratalian!", new Vector2(100, 50), Color.White);
                    for (int i = 0; i < equipeDoJogador.Count; i++)
                    {
                        string nome = equipeDoJogador[i].Name;
                        Color cor = (i == bratalianSelecionadoIndex) ? Color.Yellow : Color.White;
                        spriteBatch.DrawString(font, nome, new Vector2(120, 100 + i * 30), cor);
                    }
                }

                // Fase ativa: mostrar vida, ataques, nomes
                // Fase ativa: mostrar vida, ataques, nomes
                if (battlePhase == BattlePhase.BattleActive)
                {
                    // Só desenha se os dois ainda têm vida
                    if (playerHealth > 0 && bratalianHealth > 0)
                    {
                        int playerHealthBarX = 130;
                        int playerHealthBarY = 130;
                        int bratalianHealthBarX = GraphicsDevice.Viewport.Width - 370;
                        int bratalianHealthBarY = 130;
                        int healthBarWidth = 200;
                        int healthBarHeight = 20;

                        Rectangle playerHealthBarBack = new Rectangle(playerHealthBarX, playerHealthBarY, healthBarWidth, healthBarHeight);
                        Rectangle bratalianHealthBarBack = new Rectangle(bratalianHealthBarX, bratalianHealthBarY, healthBarWidth, healthBarHeight);

                        Rectangle playerHealthBarFront = new Rectangle(
                            playerHealthBarX,
                            playerHealthBarY,
                            (int)(healthBarWidth * (playerHealth / (float)maxHealth)),
                            healthBarHeight
                        );

                        int bratalianCurrentWidth = (int)(healthBarWidth * (bratalianHealth / (float)maxHealth));
                        Rectangle bratalianHealthBarFront = new Rectangle(
                            bratalianHealthBarX + (healthBarWidth - bratalianCurrentWidth),
                            bratalianHealthBarY,
                            bratalianCurrentWidth,
                            healthBarHeight
                        );

                        Vector2 posicaoNomebratilianPlayer = new Vector2(playerHealthBarX, playerHealthBarY - 70);
                        Vector2 posicaoNomebratilianEnemy = new Vector2(bratalianHealthBarX - 120, bratalianHealthBarY - 70);

                        spriteBatch.DrawString(font, bratalianPlayer.Name, posicaoNomebratilianPlayer, Color.Black);
                        spriteBatch.DrawString(font, bratalianAtual.Name, posicaoNomebratilianEnemy, Color.Black);

                        spriteBatch.Draw(healthBarTexture, playerHealthBarBack, Color.DarkGray);
                        spriteBatch.Draw(healthBarTexture, bratalianHealthBarBack, Color.DarkGray);
                        spriteBatch.Draw(healthBarTexture, playerHealthBarFront, Color.LimeGreen);
                        spriteBatch.Draw(healthBarTexture, bratalianHealthBarFront, Color.Red);

                        // Lista de ataques
                        for (int i = 0; i < bratalianPlayer.Attacks.Count; i++)
                        {
                            string nomeAtaque = bratalianPlayer.Attacks[i].Name;
                            Vector2 posicao = new Vector2(110, 570 + i * 30); // Linhas separadas
                            Color cor = (i == ataqueSelecionadoIndex) ? Color.Yellow : Color.White;
                            spriteBatch.DrawString(font, nomeAtaque, posicao, cor);
                        }
                    }
                }
                if (battlePhase == BattlePhase.VictoryText)
                {
                    Rectangle caixaTexto = new Rectangle(100, 550, 1066, 100);
                    string textoParcial = textoDoBratalian.Substring(0, letrasVisiveis);
                    string textoFormatado = QuebrarTexto(textoParcial, font, caixaTexto.Width);
                    Vector2 posicaoTexto = new Vector2(caixaTexto.X + 10, caixaTexto.Y + 20);
                    spriteBatch.DrawString(font, textoFormatado, posicaoTexto, Color.Black);
                }

                spriteBatch.End(); // Fim do batch da batalha
            }

            base.Draw(gameTime);
        }


        private void DrawBackground()
        {
            var vp = GraphicsDevice.Viewport;
            int cols = (int)(vp.Width / (TileSize * camera.Zoom)) + 3;
            int rows = (int)(vp.Height / (TileSize * camera.Zoom)) + 3;
            var inv = Matrix.Invert(camera.GetViewMatrix(player.Position));
            var tl = Vector2.Transform(Vector2.Zero, inv);
            int sx = (int)(tl.X / TileSize) - 1, sy = (int)(tl.Y / TileSize) - 1;
            var grass = new Rectangle(0, 0, TileSize, TileSize);

            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                {
                    var pos = new Vector2((sx + x) * TileSize, (sy + y) * TileSize);
                    spriteBatch.Draw(tilesetTex, pos, grass, Color.White);
                    if (new Random((sx + x) * 73856093 ^ (sy + y) * 19349663).NextDouble() < 0.05)
                        spriteBatch.Draw(bushTex, pos, Color.White);
                }
        }

        private void DrawMapZone(MapZone zone)
        {
            var sb = spriteBatch; int TS = TileSize;
            var g0 = new Rectangle(0, 0, TS, TS);
            var g1 = new Rectangle(16, 0, TS, TS);
            var tl0 = new Rectangle(0, 16, TS, TS);
            var tl1 = new Rectangle(16, 16, TS, TS);
            var tl2 = new Rectangle(32, 16, TS, TS);
            var bl0 = new Rectangle(0, 48, TS, TS);
            var bl1 = new Rectangle(16, 48, TS, TS);
            var bl2 = new Rectangle(32, 48, TS, TS);
            var l0 = new Rectangle(0, 32, TS, TS);
            var r0 = new Rectangle(32, 32, TS, TS);
            var B0 = new Rectangle(0, 0, TS, TS);
            var B1 = new Rectangle(16, 0, TS, TS);
            var B2 = new Rectangle(32, 0, TS, TS);
            var L0 = new Rectangle(0, 16, TS, TS);
            var R0 = new Rectangle(32, 16, TS, TS);
            var B3 = new Rectangle(0, 32, TS, TS);
            var B4 = new Rectangle(16, 32, TS, TS);
            var B5 = new Rectangle(32, 32, TS, TS);

            for (int x = 0; x < zone.Width; x++)
                for (int y = 0; y < zone.Height; y++)
                {
                    var t = zone.Tiles[x, y];
                    var pos = new Vector2(x * TS, y * TS);
                    if (t == TileType.Bush) { sb.Draw(bushTex, pos, Color.White); continue; }
                    if (t == TileType.GroundEdgeLeft || t == TileType.GroundEdgeRight ||
                       t == TileType.GroundEdgeBottomLeft || t == TileType.GroundEdgeBottom ||
                       t == TileType.GroundEdgeBottomRight)
                        sb.Draw(tilesetTex, pos, g0, Color.White);

                    Texture2D tex = tilesetTex; Rectangle src = g0;
                    switch (t)
                    {
                        case TileType.Grass: src = g0; break;
                        case TileType.Ground: src = g1; break;
                        case TileType.GroundEdgeTopLeft: src = tl0; break;
                        case TileType.GroundEdgeTop: src = tl1; break;
                        case TileType.GroundEdgeTopRight: src = tl2; break;
                        case TileType.GroundEdgeBottomLeft: src = bl0; break;
                        case TileType.GroundEdgeBottom: src = bl1; break;
                        case TileType.GroundEdgeBottomRight: src = bl2; break;
                        case TileType.GroundEdgeLeft: src = l0; break;
                        case TileType.GroundEdgeRight: src = r0; break;
                        case TileType.BorderTopLeft: tex = borderTex; src = B0; break;
                        case TileType.BorderTop: tex = borderTex; src = B1; break;
                        case TileType.BorderTopRight: tex = borderTex; src = B2; break;
                        case TileType.BorderLeft: tex = borderTex; src = L0; break;
                        case TileType.BorderRight: tex = borderTex; src = R0; break;
                        case TileType.BorderBottomLeft: tex = borderTex; src = B3; break;
                        case TileType.BorderBottom: tex = borderTex; src = B4; break;
                        case TileType.BorderBottomRight: tex = borderTex; src = B5; break;
                    }
                    sb.Draw(tex, pos, src, Color.White);
                }
        }

        public static bool IsBlocked(MapZone zone, int x, int y)
        {
            if (x < 0 || x >= zone.Width || y < 0 || y >= zone.Height) return true;
            switch (zone.Tiles[x, y])
            {
                case TileType.BorderTop:
                case TileType.BorderBottom:
                case TileType.BorderLeft:
                case TileType.BorderRight:
                case TileType.BorderTopLeft:
                case TileType.BorderTopRight:
                case TileType.BorderBottomLeft:
                case TileType.BorderBottomRight:
                    return true;
                default: return false;
            }
        }

        private void StartBattle(Bratalian b)
        {
            mostrarTextoDeEncontro = true;
            textoDoBratalian = $"Voce encontrou {b.Name}! Prepare-se para a batalha!";
            textoTimer = 0f;
            letrasVisiveis = 0;

            bratalianAtual = b;
            bratalianEnemyPosition = new Vector2(GraphicsDevice.Viewport.Width + 100, BratalianBattleY);

            bratalianPlayer = null;
            bratalianSelecionadoIndex = 0;
            ataqueSelecionadoIndex = 0;

            playerHealth = maxHealth;
            bratalianHealth = maxHealth;

            currentState = GameState.Battle;
            battlePhase = BattlePhase.EnemyIntroText;

            battlePlayerPosition = battlePlayerFixedPosition;
        }



        public string QuebrarTexto(string texto, SpriteFont fonte, int larguraMaxima)
        {
            string[] palavras = texto.Split(' ');
            string linhaAtual = "";
            string resultado = "";

            foreach (var palavra in palavras)
            {
                string testeLinha = string.IsNullOrEmpty(linhaAtual) ? palavra : linhaAtual + " " + palavra;
                float largura = fonte.MeasureString(testeLinha).X;

                if (largura > larguraMaxima)
                {
                    resultado += linhaAtual + "\n";
                    linhaAtual = palavra;
                }
                else
                {
                    linhaAtual = testeLinha;
                }
            }

            resultado += linhaAtual;
            return resultado;
        }

    }
}
