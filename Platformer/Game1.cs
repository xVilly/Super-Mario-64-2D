using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Platformer.Helpers;
using System.Diagnostics;
using Penumbra;

namespace Platformer
{
    public class Game1 : Game
    {

        public static PenumbraComponent penumbra;
        // Game settings (constants)
        public const bool MAP_HITBOX = false;
        public const bool ENTITY_HITBOX = false;

        public const bool DEBUG = true;
        public static int DEBUG_DISPLAY = 0;
        public static string DEBUG_WALLKICK = "--";

        // Map objects lists
        public static List<MapObject> mapObjects;
        public static List<SlopeObject> mapSlopes;
        public static List<RectObject> mapRectangles;

        // Particle engines list
        public static List<ParticleEngine> particleEngines;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private BasicEffect basicEffect;
        private Texture2D defaultTexture;
        private SpriteFont defaultFont;
        private Entity player;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            penumbra = new PenumbraComponent(this);
            Components.Add(penumbra);
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            penumbra.AmbientColor = new Color(255, 255, 255, 150);
            PointLight sun = new PointLight();
            sun.Position = new Vector2(640, 0);
            sun.Scale = new Vector2(8000, 2500);
            sun.Intensity = 1f;
            sun.ShadowType = ShadowType.Occluded;
            penumbra.Lights.Add(sun);

            base.Initialize();
        }

        protected override void LoadContent()
        {

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            Camera.mapPosition = new Vector2(0, 0);
            Camera.cameraVelocity = new Vector2(0, 0);
            Camera.WindowSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
            mapObjects = new List<MapObject>();
            mapSlopes = new List<SlopeObject>();
            mapRectangles = new List<RectObject>();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            particleEngines = new List<ParticleEngine>();
            defaultTexture = Content.Load<Texture2D>("Sprites/Entities/default");
            defaultFont = Content.Load<SpriteFont>("Fonts/defaultFont");
            SpriteManager.LoadTextures(this);
            SoundManager.LoadSoundEffects(this);

            MapManager.Load("C:/Users/Michal/source/repos/LevelEditor/LevelEditor/bin/Debug/netcoreapp3.1/Castle Grounds.lev");

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>("Sprites/Particles/circle"));
            textures.Add(Content.Load<Texture2D>("Sprites/Particles/star"));
            textures.Add(Content.Load<Texture2D>("Sprites/Particles/diamond"));


            player = new Entity(new Vector2(150,-600));
            Camera.Setup(player);

            foreach (MapObject solidObject in mapObjects)
            {
                solidObject.Initialize(GraphicsDevice);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            InputManager.Update();
            Camera.Update(elapsed);
            foreach (ActionWindow actionWindow in ActionWindow.actionWindows)
            {
                actionWindow.Update();
            }
            player.Update();
            // GUI Input
            if (Keyboard.GetState().IsKeyDown(Keys.F8))
            {
                if (DEBUG_DISPLAY != 1)
                    DEBUG_DISPLAY ++;
                else
                    DEBUG_DISPLAY = 0;
            }

            // ---------------------------------COLLISION PHYSICS----------------------------------------
            /*
            if (player.onSlope)
            {
                SlopeObject slopeObject = player.collisionObject.GetSlopeObject();
                player.lastSlopeObject = slopeObject;
                if (player.velocity.X < 0 && !slopeObject.direction)
                {
                    canmove = false;
                    // calc next X and Y
                    float projectedX = player.position.X + moveVector.X;
                    float x = projectedX + (player.size.X / 2);
                    float X = slopeObject.position.X;
                    float Y = slopeObject.position.Y;
                    float W = slopeObject.size.X;
                    float H = slopeObject.size.Y;
                    float hitY = Y + H - (((x - X) / ((X + W) - X) * (0 - H)) + H);
                    float projectedY = hitY - player.size.Y;
                    player.position = new Vector2(projectedX, projectedY);
                }
                else if (player.velocity.X > 0 && slopeObject.direction)
                {
                    canmove = false;
                    // calc next X and Y
                    float projectedX = player.position.X + moveVector.X;
                    float x = projectedX + (player.size.X / 2);
                    float X = slopeObject.position.X;
                    float Y = slopeObject.position.Y;
                    float W = slopeObject.size.X;
                    float H = slopeObject.size.Y;
                    float hitY = Y + H - (((x - X) / ((X + W) - X) * (H - 0)) + 0);
                    float projectedY = hitY - player.size.Y;
                    player.position = new Vector2(projectedX, projectedY);
                }
                
            } // slope collision result (movement on slope)

            // slope slide correction
            if (player.GetState() == EntityState.SLIDE && player.lastSlopeObject != null)
            {
                SlopeObject slopeObject = player.lastSlopeObject;
                if (player.GetMovementPoint().X > slopeObject.position.X && player.GetMovementPoint().X < slopeObject.position.X+slopeObject.size.X && player.GetMovementPoint().Y < slopeObject.position.Y+slopeObject.size.Y - 10)
                {
                    if (player.velocity.X > 0 && !slopeObject.direction)
                    {
                        canmove = false;
                        // calc next X and Y
                        float projectedX = player.position.X + moveVector.X;
                        float x = projectedX + (player.size.X / 2);
                        float X = slopeObject.position.X;
                        float Y = slopeObject.position.Y;
                        float W = slopeObject.size.X;
                        float H = slopeObject.size.Y;
                        float hitY = Y + H - (((x - X) / ((X + W) - X) * (0 - H)) + H);
                        float projectedY = hitY - player.size.Y;
                        player.position = new Vector2(projectedX, projectedY);
                    }
                    else if (player.velocity.X < 0 && slopeObject.direction)
                    {
                        canmove = false;
                        // calc next X and Y
                        float projectedX = player.position.X + moveVector.X;
                        float x = projectedX + (player.size.X / 2);
                        float X = slopeObject.position.X;
                        float Y = slopeObject.position.Y;
                        float W = slopeObject.size.X;
                        float H = slopeObject.size.Y;
                        float hitY = Y + H - (((x - X) / ((X + W) - X) * (H - 0)) + 0);
                        float projectedY = hitY - player.size.Y;
                        player.position = new Vector2(projectedX, projectedY);
                    }
                }
            }
            */
            foreach (ParticleEngine engine in particleEngines)
            {
                engine.Update();
            }
            


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            penumbra.BeginDraw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DateTime d1 = DateTime.Now;
            basicEffect.TextureEnabled = true;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            int drewObjects = 0;
            foreach (MapObject solidObject in mapObjects)
            {
                if (solidObject.IsInView())
                {
                    drewObjects++;
                    solidObject.Draw(spriteBatch, GraphicsDevice, basicEffect);
                }
            }

            Debug.WriteLine("After Object Draw: " + (DateTime.Now - d1).TotalMilliseconds.ToString());
            player.Draw(spriteBatch, defaultTexture);

            if (DEBUG && DEBUG_DISPLAY == 1)
            {
                spriteBatch.DrawString(defaultFont, "XVEL: " + Math.Round(player.velocity.X, 3), new Vector2(10, 10), Color.Red);
                spriteBatch.DrawString(defaultFont, "YVEL: " + Math.Round(player.velocity.Y, 3), new Vector2(10, 30), Color.Red);
                spriteBatch.DrawString(defaultFont, "POS: x" + Math.Round(player.position.X, 2) + " y" + Math.Round(player.position.Y, 2), new Vector2(10, 50), Color.Red);
                spriteBatch.DrawString(defaultFont, "WALLKICK: " + DEBUG_WALLKICK, new Vector2(10, 70), Color.Red);
                spriteBatch.DrawString(defaultFont, "JUMP SEQ: " + player.jumpSequence, new Vector2(10, 90), Color.Red);
            }

            foreach (ParticleEngine engine in particleEngines)
            {
                engine.Draw(spriteBatch);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }

        

        private bool CollisionOverflow(MapObject obj, Vector2 pos, Vector2 nextPos)
        {
            SlopeObject slope = obj.GetSlopeObject();
            RectObject rect = obj.GetRectObject();
            if(slope != null)
            {
                if (pos.X >= slope.position.X && pos.X < slope.position.X + slope.size.X && pos.Y > slope.position.Y + slope.size.Y && nextPos.Y < slope.position.Y + slope.size.Y)
                    return true;
                if (slope.direction)
                {
                    if (pos.Y >= slope.position.Y && pos.Y < slope.position.Y + slope.size.Y && pos.X >= slope.position.X + slope.size.X && nextPos.X < slope.position.X + slope.size.X)
                        return true;
                    if (pos.Y < slope.position.Y + slope.size.Y && pos.X < slope.position.X + slope.size.X && nextPos.X >= slope.position.X && nextPos.Y >= slope.position.Y && (nextPos.Y >= slope.position.Y + slope.size.Y || nextPos.X > slope.position.X + slope.size.X))
                        return true;
                }
                else
                {
                    if (pos.Y >= slope.position.Y && pos.Y < slope.position.Y + slope.size.Y && pos.X < slope.position.X && nextPos.X >= slope.position.X)
                        return true;
                    if (pos.Y < slope.position.Y + slope.size.Y && pos.X >= slope.position.X && nextPos.X < slope.position.X + slope.size.X && nextPos.Y >= slope.position.Y && (nextPos.Y >= slope.position.Y + slope.size.Y || nextPos.X <= slope.position.X))
                        return true;
                }
            }
            else if (rect != null)
            {
                if (pos.X >= rect.position.X && pos.X < rect.position.X + rect.size.X && pos.Y < rect.position.Y && nextPos.Y > rect.position.Y)
                    return true;
                if (pos.X >= rect.position.X && pos.X < rect.position.X + rect.size.X && pos.Y >= rect.position.Y + rect.size.Y && nextPos.Y < rect.position.Y + rect.size.Y)
                    return true;

                if (pos.Y >= rect.position.Y && pos.Y < rect.position.Y + rect.size.Y && pos.X < rect.position.X && nextPos.X > rect.position.X)
                    return true;
                if (pos.Y >= rect.position.Y && pos.Y < rect.position.Y + rect.size.Y && pos.X >= rect.position.X + rect.size.X && nextPos.X < rect.position.X + rect.size.X)
                    return true;
            }
            return false;
        }
    }
}
