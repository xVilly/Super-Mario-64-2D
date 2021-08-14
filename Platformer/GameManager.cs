using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Platformer.Helpers;
using System.Diagnostics;


namespace Platformer
{
    public enum GameState
    {
        Intro,
        Menu,
        World
    }
    public class GameManager : Game
    {
        public static Texture2D defaultTexture;
        public static SpriteFont defaultFont;

        public GraphicsDeviceManager _graphics;
        public SpriteBatch spriteBatch;
        public BasicEffect basicEffect;

        private static GameState State;

        public GameManager()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            GameWorld.Setup(this);

            ChangeGameState(GameState.Intro);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            defaultTexture = Content.Load<Texture2D>("Sprites/Entities/default");
            //defaultFont = Content.Load<SpriteFont>("Fonts/defaultFont");
            defaultFont = Content.Load<SpriteFont>("Fonts/mario");
            defaultFont.Spacing = 5;

            SpriteManager.LoadTextures(this);
            SoundManager.LoadSoundEffects(this);

            GameWorld.Initialize(this);

            ChangeGameState(GameState.World);
        }

        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (State)
            {
                case GameState.Intro:
                case GameState.Menu:
                case GameState.World:
                    GameWorld.Update(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (State)
            {
                case GameState.Intro:
                case GameState.Menu:
                case GameState.World:
                    GameWorld.Draw(gameTime, this);
                    break;
            }

            base.Draw(gameTime);
        }

        public void ChangeGameState(GameState _state)
        {
            State = _state;
            switch (_state)
            {
                case GameState.World:
                    GameWorld.OnSwitch(this);
                    break;
            }
        }

        public GameState GetGameState()
        {
            return State;
        }
    }
}
