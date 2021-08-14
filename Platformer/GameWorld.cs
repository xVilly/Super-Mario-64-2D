using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Platformer.Helpers;
using System.Diagnostics;
using Penumbra;
using Platformer.GUI;

namespace Platformer
{
    public static class GameWorld
    {
        public static PenumbraComponent penumbra;

        public const bool MAP_HITBOX = false;
        public const bool ENTITY_HITBOX = false;

        public const bool DEBUG = true;
        public static int DEBUG_DISPLAY = 0;
        public static string DEBUG_WALLKICK = "--";

        public static List<SolidObject> solidObjects;
        public static List<SlopeObject> mapSlopes;
        public static List<RectObject> mapRectangles;
        public static List<EntityObject> entityObjects;
        public static List<ParticleController> particleControllers;

        public static Player Player;

        public static void Setup(GameManager Manager)
        {
            penumbra = new PenumbraComponent(Manager);
            Manager.Components.Add(penumbra);
        }

        public static void Initialize(GameManager Manager)
        {
            penumbra.AmbientColor = new Color(255, 255, 255, 150);
            PointLight sun = new PointLight();
            sun.Position = new Vector2(640, 0);
            sun.Scale = new Vector2(8000, 2500);
            sun.Intensity = 1f;
            sun.ShadowType = ShadowType.Occluded;
            penumbra.Lights.Add(sun);

            Camera.mapPosition = new Vector2(0, 0);
            Camera.cameraVelocity = new Vector2(0, 0);
            Camera.WindowSize = new Vector2(Manager.Window.ClientBounds.Width, Manager.Window.ClientBounds.Height);
            solidObjects = new List<SolidObject>();
            mapSlopes = new List<SlopeObject>();
            mapRectangles = new List<RectObject>();
            entityObjects = new List<EntityObject>();
            
            particleControllers = new List<ParticleController>();
            MapManager.Load("C:/Users/Michal/source/repos/LevelEditor/LevelEditor/bin/Debug/netcoreapp3.1/Castle Grounds.lev");

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Manager.Content.Load<Texture2D>("Sprites/Particles/circle"));
            textures.Add(Manager.Content.Load<Texture2D>("Sprites/Particles/star"));
            textures.Add(Manager.Content.Load<Texture2D>("Sprites/Particles/diamond"));

            foreach (SolidObject solidObject in solidObjects)
            {
                solidObject.Initialize(Manager.GraphicsDevice);
            }
        }

        public static void OnSwitch(GameManager Manager)
        {
            // Setup GUI for this scene
            CreateGui(Manager);

            // Setup Camera and player
            Player = new Player(new Vector2(150, -600));
            Camera.Setup(Player);
        }

        public static void CreateGui(GameManager Manager)
        {
            ImageWidget x = new ImageWidget();
            // Camera image
            ImageWidget cam = new ImageWidget(SpriteManager.GetGUITexture(0), 48, 48);
            cam.Position = new Vector2(Manager.Window.ClientBounds.Width - 145, Manager.Window.ClientBounds.Height - 75);
            cam.SetParent(x);
            // Lakitu/Mario/Lock image
            ImageWidget mode = new ImageWidget(SpriteManager.GetGUITexture(4), 48, 48);
            mode.Position = new Vector2(cam.Right + 10, cam.Top);
            mode.SetParent(x);
            mode.Rename("GUI_GAMEWORLD_CAMERAMODE");
            // C-up image
            ImageWidget cup = new ImageWidget(SpriteManager.GetGUITexture(1), 48, 48);
            cup.Position = new Vector2(cam.Left + 2, cam.Top - 36);
            cup.SetParent(x);
            cup.Rename("GUI_GAMEWORLD_CUP");
            cup.SetVisible(false);
            // C-down image
            ImageWidget cdown = new ImageWidget(SpriteManager.GetGUITexture(2), 48, 48);
            cdown.Position = new Vector2(cam.Left + 2, cam.Bottom - 22);
            cdown.SetParent(x);
            cdown.Rename("GUI_GAMEWORLD_CDOWN");
            cdown.SetVisible(false);
        }

        public static void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            InputManager.Update();
            Camera.Update(elapsed);
            foreach (ActionWindow actionWindow in ActionWindow.actionWindows)
            {
                actionWindow.Update();
            }
            foreach (EntityObject entityObject in entityObjects)
            {
                entityObject.Update();
            }
            Player.Update();
            // GUI Input
            if (Keyboard.GetState().IsKeyDown(Keys.F8))
            {
                if (DEBUG_DISPLAY != 1)
                    DEBUG_DISPLAY++;
                else
                    DEBUG_DISPLAY = 0;
            }

            foreach (ParticleController controller in particleControllers)
            {
                controller.Update();
            }
        }

        public static void Draw(GameTime gameTime, GameManager Manager)
        {
            penumbra.BeginDraw();
            Manager.GraphicsDevice.Clear(Color.Blue);
            // Background Layer (using spritebatch)
            Manager.spriteBatch.Begin();
            if (Level.IsLoaded())
                Level.SkyBox.Draw(Manager.spriteBatch);
            Manager.spriteBatch.End();
            // Solid objects layer (using shader), then objects, entities and particles (spritebatch)
            Manager.spriteBatch.Begin();
            Manager.basicEffect.TextureEnabled = true;
            Manager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            int drewObjects = 0;
            foreach (SolidObject solidObject in solidObjects)
            {
                if (solidObject.IsInView())
                {
                    drewObjects++;
                    solidObject.Draw(Manager.spriteBatch, Manager.GraphicsDevice, Manager.basicEffect);
                }
            }
            foreach (EntityObject entityObject in entityObjects)
            {
                entityObject.Draw(Manager.spriteBatch);
            }
            Player.Draw(Manager.spriteBatch, GameManager.defaultTexture);

            foreach (ParticleController controller in particleControllers)
            {
                controller.Draw(Manager.spriteBatch);
            }
            GUIManager.DrawWidgets(Manager.spriteBatch);

            if (DEBUG && DEBUG_DISPLAY == 1)
            {
                Manager.spriteBatch.DrawString(GameManager.defaultFont, "XVEL: " + Math.Round(Player.velocity.X, 3), new Vector2(10, 10), Color.Red);
                Manager.spriteBatch.DrawString(GameManager.defaultFont, "YVEL: " + Math.Round(Player.velocity.Y, 3), new Vector2(10, 30), Color.Red);
                Manager.spriteBatch.DrawString(GameManager.defaultFont, "POS: x" + Math.Round(Player.position.X, 2) + " y" + Math.Round(Player.position.Y, 2), new Vector2(10, 50), Color.Red);
                Manager.spriteBatch.DrawString(GameManager.defaultFont, "WALLKICK: " + DEBUG_WALLKICK, new Vector2(10, 70), Color.Red);
                Manager.spriteBatch.DrawString(GameManager.defaultFont, "JUMP SEQ: " + Player.jumpSequence, new Vector2(10, 90), Color.Red);
            }



            Manager.spriteBatch.End();
        }
    }
}
