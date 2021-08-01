using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Platformer.Helpers;
using System.Diagnostics;

namespace Platformer
{
    public class Game1 : Game
    {
        // Game settings (constants)
        public const bool MAP_HITBOX = false;
        public const bool ENTITY_HITBOX = true;

        public const bool DEBUG = true;
        public int DEBUG_DISPLAY = 0;
        public string DEBUG_WALLKICK = "--";

        // Map objects lists
        public static List<MapObject> mapObjects;
        public static List<SlopeObject> mapSlopes;
        public static List<RectObject> mapRectangles;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private BasicEffect basicEffect;
        private Texture2D defaultTexture;
        private SpriteFont defaultFont;
        private Entity player;

        bool wasPressedJump = false;
        bool wasPressedCrouch = false;

        public Game1()
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
            defaultTexture = Content.Load<Texture2D>("Sprites/Entities/default");
            defaultFont = Content.Load<SpriteFont>("Fonts/defaultFont");
            SpriteManager.LoadTextures(this);

            MapManager.Load("C:/Users/Michal/source/repos/LevelEditor/LevelEditor/bin/Debug/netcoreapp3.1/Castle Grounds.lev");
            
            

            player = new Entity(new Vector2(150,-600));
        }

        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            Camera.Update(elapsed);
            Camera.targetPosition = Maths.SmoothDamp(Camera.mapPosition, player.GetMovementPoint(), ref Camera.cameraVelocity, 2.0f, 50.0f, 1.0f);
            player.Update();
            // GUI Input
            if (Keyboard.GetState().IsKeyDown(Keys.F8))
            {
                if (DEBUG_DISPLAY != 1)
                    DEBUG_DISPLAY ++;
                else
                    DEBUG_DISPLAY = 0;
            }

            // Movement system (input)

            // HORIZONTAL MOVEMENT
            float stepValue = 0.5f;
            player.horizontalInput = false;
            if (!player.onGround)
                stepValue = 0.075f;
            else if (player.onSlope)
                stepValue = 0.5f - (float)Math.Max(0f, player.collisionObject.GetSlopeObject().incline * 0.55f);
            if (player.blockDefaultMovementInput)
                stepValue = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (!player.crouch)
                {
                    if (player.velocity.X > -Entity.MAX_WALKING_VEL)
                        player.velocity += new Vector2(-stepValue, 0);

                    if (player.velocity.X > 3.0f && player.dirChangeFrames == 0 && player.GetState() == EntityState.WALK)
                        player.dirChangeFrames = 1;
                }
                else
                {
                    if (player.velocity.X > -Entity.MAX_WALKING_VEL_CROUCH)
                        player.velocity += new Vector2(-stepValue, 0);
                }
                player.horizontalInput = true;
                player.inputDirection = false;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (!player.crouch)
                {
                    if (player.velocity.X < Entity.MAX_WALKING_VEL)
                        player.velocity += new Vector2(stepValue, 0);

                    if (player.velocity.X < -3.0f && player.dirChangeFrames == 0 && player.GetState() == EntityState.WALK)
                        player.dirChangeFrames = 1;
                }
                else
                {
                    if (player.velocity.X < Entity.MAX_WALKING_VEL_CROUCH)
                        player.velocity += new Vector2(stepValue, 0);
                }
                player.horizontalInput = true;
                player.inputDirection = true;
            }

            // DIRCHANGE
            if (player.dirChangeFrames >= 1 && player.dirChangeFrames < Entity.DirChangeWindow)
            {
                player.dirChangeFrames++;
                player.frictionType = FrictionType.DIRCHANGE;
                player.blockDefaultMovementInput = true;
                player.blockDir = true;
                if (player.dirChangeFrames == 1)
                {
                    if (player.velocity.X > 0)
                        player.direction = false;
                    else
                        player.direction = true;
                }
            }
            else if (player.dirChangeFrames == Entity.DirChangeWindow)
            {
                player.frictionType = FrictionType.NORMAL;
                player.dirChangeFrames = 0;
                player.blockDefaultMovementInput = false;
                player.blockDir = false;
            }

            if (player.jumpSequence > 0 && player.onGround && AfterWindow(player.landWindow, Entity.LandWindow))
            {
                if (player.jumpSequence == 3)
                    player.jumpSequence = -1;
                else
                    player.jumpSequence = 0;
                player.jumpWindow = 0;
            }

            // JUMPING
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump && player.onGround)
            {
                if (!player.blockDefaultMovementInput)
                {
                    if (player.onSlope)
                    {
                        SlopeObject slopeObject = player.collisionObject.GetSlopeObject();
                        if (slopeObject.incline >= 0.25)
                        {
                            if (player.jumpSequence == 3)
                            {
                                if (InWindow(player.landWindow, Entity.LandWindow) && Math.Abs(player.velocity.X) >= 2.5)
                                {
                                    player.jumpSequence = -1;
                                    player.landWindow = 0;
                                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                                    if (player.direction)
                                        player.velocity.X += 1.0f;
                                    else
                                        player.velocity.X += 1.0f;
                                    player.jumpWindow = GetMs();
                                    player.ChangeState(EntityState.ROLL);
                                }
                                else
                                {
                                    player.jumpSequence = -1;
                                    player.landWindow = 0;
                                }
                            }
                            else
                            {
                                player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 0.5f));
                                player.jumpWindow = GetMs();
                                player.ChangeState(EntityState.JUMP);
                            }
                        }
                        else
                        {
                            if (player.jumpSequence == 3)
                            {
                                if (InWindow(player.landWindow, Entity.LandWindow) && Math.Abs(player.velocity.X) >= 2.5)
                                {
                                    player.jumpSequence = -1;
                                    player.landWindow = 0;
                                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                                    if (player.direction)
                                        player.velocity.X += 1.0f;
                                    else
                                        player.velocity.X += 1.0f;
                                    player.jumpWindow = GetMs();
                                    player.ChangeState(EntityState.ROLL);
                                }
                                else
                                {
                                    player.jumpSequence = -1;
                                    player.landWindow = 0;
                                }
                            }
                            else
                            {
                                player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 0.8f));
                                player.jumpWindow = GetMs();
                                player.ChangeState(EntityState.JUMP);
                            }
                        }
                    }
                    else
                    {
                        if (player.jumpSequence == -1)
                        {
                            player.jumpSequence = 0;
                        }
                        // DOUBLE-JUMP
                        if (player.jumpSequence == 1 && InWindow(player.landWindow, Entity.LandWindow))
                        {
                            player.jumpSequence = 2;
                            player.landWindow = 0;
                            player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.35f));
                            player.jumpWindow = GetMs();
                            player.ChangeState(EntityState.JUMP);
                        }
                        // TRIPLE-JUMP
                        else if (player.jumpSequence == 3)
                        {
                            if (InWindow(player.landWindow, Entity.LandWindow) && Math.Abs(player.velocity.X) >= 2.5)
                            {
                                player.jumpSequence = -1;
                                player.landWindow = 0;
                                player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                                if (player.direction)
                                    player.velocity.X += 1.0f;
                                else
                                    player.velocity.X += 1.0f;
                                player.jumpWindow = GetMs();
                                player.ChangeState(EntityState.ROLL);
                            }
                            else
                            {
                                player.jumpSequence = -1;
                                player.landWindow = 0;
                            }
                        }
                        else
                        {
                            player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.0f));
                            player.jumpWindow = GetMs();
                            player.ChangeState(EntityState.JUMP);
                        }
                    }
                }
            }
            if (wasPressedJump && !Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                player.jumpWindow = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && InWindow(player.jumpWindow, Entity.JumpWindow) && player.velocity.Y < 0)
            {
                if (!player.blockDefaultMovementInput)
                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE / 30.0f));
            }

            // JUMP WHEN DIRCHANGE (SIDEFLIP)
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump && player.onGround && player.dirChangeFrames >= 1 && player.dirChangeFrames < Entity.DirChangeWindow)
            {
                player.blockDefaultMovementInput = true;
                player.dirChangeFrames = 1;
                player.ChangeState(EntityState.SIDEFLIP);
                player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.75f));
                if (player.direction)
                    player.velocity.X = -3.0f;
                else
                    player.velocity.X = 3.0f;
            }
            // CROUCH ACTIONS CEILING CHECK
            bool canStand = true;
            foreach (RectObject rect in mapRectangles)
            {
                if (Maths.PointInRectangle(new Vector2(player.GetHeadPoint().X, player.position.Y - (player.startSize.Y - player.crouchSize.Y)), new Rectangle(rect.position.ToPoint(), rect.size.ToPoint())))
                {
                    canStand = false;
                }
            }

            // CROUCHING
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !wasPressedCrouch && !player.crouch && player.onGround)
            {
                // crouch
                if (!player.blockDefaultMovementInput && !InWindow(player.landWindow, Entity.LandWindow))
                {
                    player.crouch = true;
                    player.position.Y = player.position.Y + (player.startSize.Y - player.crouchSize.Y);
                    player.size = player.crouchSize;
                }
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Down) && wasPressedCrouch && player.crouch && player.onGround)
            {
                // stand
                if (canStand)
                {
                    player.crouch = false;
                    player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                    player.size = player.startSize;
                }
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Down) && player.crouch && player.onGround && canStand)
            {
                player.crouch = false;
                player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                player.size = player.startSize;
            }

            player.wallJumpFrame++;
            // JUMP DURING WALL JUMP WINDOW
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump && player.wallJump)
            {
                int xDir = 1;
                if (player.wallJumpDirection)
                    xDir = -1;
                player.wallJump = false;
                player.blockDefaultMovementInput = false;
                player.blockGravity = false;
                player.direction = !player.wallJumpDirection;
                if (player.GetState() != EntityState.JUMP)
                    player.ChangeState(EntityState.JUMP);
                float speedBoost = 2f * xDir;
                if ((Keyboard.GetState().IsKeyDown(Keys.Right) && !player.wallJumpDirection) || (Keyboard.GetState().IsKeyDown(Keys.Left) && player.wallJumpDirection))
                {
                    speedBoost = -player.wallJumpSpeed * 0.75f;
                }
                // 1 frame perfect jump
                if (player.wallJumpFrame == 1)
                {
                    player.velocity.X = speedBoost + 3 * xDir;
                    player.velocity.Y = -5;
                    DEBUG_WALLKICK = "1 FRAME (PERFECT)";
                }
                // < 2 frame
                else if (player.wallJumpFrame == 2)
                {
                    player.velocity.X = speedBoost + 1 * xDir;
                    player.velocity.Y = -5;
                    DEBUG_WALLKICK = "2 FRAME";
                }
                // < 3 frame
                else if (player.wallJumpFrame == 3)
                {
                    player.velocity.X = speedBoost - 1 * xDir;
                    player.velocity.Y = -4;
                    DEBUG_WALLKICK = "3 FRAME";
                }
                // 4+ frames
                else
                {
                    player.velocity.X = 1.5f * xDir;
                    player.velocity.Y = -3;
                    DEBUG_WALLKICK = "4+ FRAME";
                }
            }

            // AFTER WALL JUMP WINDOW
            if (AfterWindow(player.wallJumpWindow, Entity.WallJumpWindow) && player.wallJump)
            {
                player.wallJump = false;
                player.blockDefaultMovementInput = false;
                player.blockGravity = false;

                if (player.wallJumpDirection)
                {
                    player.velocity.X = -1;
                }
                else
                {
                    player.velocity.X = 1;
                }
            }

            // JUMPING WHILE CROUCHING
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump && player.onGround && player.crouch)
            {
                if (!player.blockDefaultMovementInput && canStand)
                {
                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE / 5.0f));
                    player.jumpWindow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + 100;
                    //stand
                    player.crouch = false;
                    player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                    player.size = player.startSize;
                }
            }

            // CROUCH IN AIR (low horizontal speed) - POUND
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !wasPressedCrouch && !player.onGround && !player.crouch && player.velocity.X < 2 && player.velocity.X > -2 && !player.pounding && !player.diving)
            {
                if (!player.blockDefaultMovementInput)
                {
                    player.ChangeState(EntityState.ROLL);
                    player.pounding = true;
                    player.gravityMultiplier = 0f;
                    player.blockDefaultMovementInput = true;
                    player.velocity.X = 0f;
                    player.velocity.Y = 0f;
                    // pound freeze window
                    player.poundWindow = GetMs();
                    player.poundFreeze = true;
                    //crouch
                    player.crouch = true;
                    player.size = player.crouchSize;
                }
            }

            // AFTER POUND FREEZE
            if (player.pounding && player.poundFreeze && AfterWindow(player.poundWindow, Entity.PoundWindow))
            {
                player.ChangeState(EntityState.CROUCH);
                player.poundFreeze = false;
                player.gravityMultiplier = (float)Entity.POUND_GRAVITYMULT;
                player.blockDefaultMovementInput = false;

                player.jumpSequence = -1;
                player.landWindow = 0;
            }

            // CROUCH IN AIR (high horizontal speed) - DIVE
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !wasPressedCrouch && !player.onGround && !player.crouch && (player.velocity.X >= 2 || player.velocity.X <= -2) && !player.pounding && !player.diving)
            {
                if (!player.blockDefaultMovementInput)
                {
                    //crouch (dive)
                    player.crouch = true;
                    player.position.Y = player.position.Y + (player.startSize.Y - player.crouchSize.Y);
                    player.size = player.crouchSize;

                    if (player.GetState() == EntityState.SIDEFLIP)
                    {
                        player.direction = player.velocity.X > 0;
                    }

                    if (player.GetState() != EntityState.DIVE)
                        player.ChangeState(EntityState.DIVE);
                    player.diving = true;
                    player.blockDefaultMovementInput = true;
                    if (player.velocity.Y < 0)
                        player.velocity.Y *= 0.85f;
                    else if (player.velocity.Y >= 0)
                        player.velocity.Y *= 0.75f;
                    player.gravityMultiplier = (float)Entity.DIVE_GRAVITYMULT;
                    if (player.velocity.X >= 0)
                        player.velocity.X += (float)Entity.DIVE_SPEED;
                    else
                        player.velocity.X -= (float)Entity.DIVE_SPEED;
                }
            }

            // LAND AFTER DIVE
            if(player.onGround && player.diving)
            {
                player.diving = false;
                player.gravityMultiplier = 1.0f;
                // post-dive effect
                if (Math.Abs(player.diveHitSpeed) > Entity.POSTDIVE_SPEED)
                {
                    player.postDive = true;
                    player.postDiveWindow = GetMs();
                    // bounce
                    player.velocity.Y -= 1.5f;
                    if (player.diveHitSpeed > 0)
                    {
                        if (player.diveHitSpeed > Entity.POSTDIVE_SPEED * 1.5)
                            player.velocity.X += 1.5f;
                        else if (player.diveHitSpeed > Entity.POSTDIVE_SPEED * 2.0)
                            player.velocity.X += 2.0f;
                        else
                            player.velocity.X += 1.0f;
                    }
                    else
                    {
                        if (player.diveHitSpeed > Entity.POSTDIVE_SPEED * 1.5)
                            player.velocity.X -= 1.5f;
                        else if (player.diveHitSpeed > Entity.POSTDIVE_SPEED * 2.0)
                            player.velocity.X -= 2.0f;
                        else
                            player.velocity.X -= 1.0f;
                    }
                }
                else
                {
                    // stand
                    player.crouch = false;
                    player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                    player.size = player.startSize;
                    player.blockDefaultMovementInput = false;
                }
            }

            // POSTDIVE
            if (AfterWindow(player.postDiveWindow, Entity.PostDiveWindow) && player.postDive)
            {
                player.postDive = false;
                player.blockDefaultMovementInput = false;
                // stand
                player.crouch = false;
                //player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                player.size = player.startSize;
            }

            // POSTDIVE -> ROLL (CROUCH BUTTON)
            if (!AfterWindow(player.postDiveWindow, Entity.PostDiveWindow) && player.postDive && Keyboard.GetState().IsKeyDown(Keys.Down) && !wasPressedCrouch)
            {
                player.postDive = false;
                // roll
                player.postDiveRoll = true;
                player.postDiveRollWindow = GetMs();
                player.ChangeState(EntityState.ROLL);
                // 1 frame for quick jump
                if(Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump)
                {
                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.2f));
                    if (player.direction)
                        player.velocity.X += 1.5f;
                    else
                        player.velocity.X -= 1.5f;
                }
            }

            // POSTDIVEROLL
            if (AfterWindow(player.postDiveRollWindow, Entity.PostDiveRollWindow) && player.postDiveRoll)
            {
                player.postDiveRoll = false;
                player.blockDefaultMovementInput = false;
                // stand
                player.crouch = false;
                player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                player.size = player.startSize;
                // start double-jump window
                player.jumpSequence = 1;
                player.landWindow = GetMs();
            }

            // LAND AFTER POUND
            if (player.onGround && player.pounding)
            {
                player.pounding = false;
                player.gravityMultiplier = 1.0f;
                // Post-pound effect if fast enough
                if (player.poundSpeed > Entity.POSTPOUND_SPEED)
                {
                    player.postPoundWindow = GetMs();
                    player.blockDefaultMovementInput = true;
                    player.postPound = true;
                    // small bounce
                    if (player.poundSpeed > Entity.POSTPOUND_SPEED * 1.5)
                        player.velocity.Y = -2;
                    else if (player.poundSpeed > Entity.POSTPOUND_SPEED * 2.0)
                        player.velocity.Y = -3;
                    else
                        player.velocity.Y = -1;
                }
                else
                {
                    // stand
                    player.crouch = false;
                    player.size = player.startSize;
                    if (player.collisionObject.GetSlopeObject() == null && player.GetHeadPoint().Y < player.collisionObject.position.Y)
                        player.position.Y = player.collisionObject.position.Y - player.size.Y;
                    else
                        player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
                }
            }

            // POSTPOUND
            if (AfterWindow(player.postPoundWindow, Entity.PostPoundWindow) && player.postPound)
            {
                player.postPound = false;
                player.blockDefaultMovementInput = false;
                // stand
                player.crouch = false;
                player.size = player.startSize;
                if (player.collisionObject.GetSlopeObject() == null && player.GetHeadPoint().Y < player.collisionObject.position.Y)
                    player.position.Y = player.collisionObject.position.Y - player.size.Y;
                else
                    player.position.Y = player.position.Y - (player.startSize.Y - player.crouchSize.Y);
            }

            // ----- PHYSICS ------
            // ->Gravity
            if (player.velocity.Y < Entity.MAX_VERTICAL_VEL && !player.blockGravity)
                player.velocity.Y += (float)Entity.GRAVITY * player.gravityMultiplier;
            // ->Ground friction
            if (player.onGround && !player.blockFriction)
            {
                switch (player.frictionType)
                {
                    case FrictionType.SLOPESLIDE:
                        if (player.velocity.X > 0.2)
                            player.velocity.X -= (float)Entity.GROUND_FRICTION_SLOPESLIDE * player.collisionObject.Friction;
                        else if (player.velocity.X < -0.2)
                            player.velocity.X += (float)Entity.GROUND_FRICTION_SLOPESLIDE * player.collisionObject.Friction;
                        else
                            player.velocity.X = 0;
                        break;
                    case FrictionType.SLIDE:
                        if (player.velocity.X > 0.2)
                            player.velocity.X -= (float)Entity.GROUND_FRICTION_SLIDE * player.collisionObject.Friction;
                        else if (player.velocity.X < -0.2)
                            player.velocity.X += (float)Entity.GROUND_FRICTION_SLIDE * player.collisionObject.Friction;
                        else
                            player.velocity.X = 0;
                        break;
                    case FrictionType.DIRCHANGE:
                        if (player.velocity.X > 0.2)
                            player.velocity.X -= (float)Entity.GROUND_FRICTION_DIRCHANGE * player.collisionObject.Friction;
                        else if (player.velocity.X < -0.2)
                            player.velocity.X += (float)Entity.GROUND_FRICTION_DIRCHANGE * player.collisionObject.Friction;
                        else
                            player.velocity.X = 0;
                        break;
                    case FrictionType.NORMAL:
                    default:
                        if (player.velocity.X > 0.2)
                            player.velocity.X -= (float)Entity.GROUND_FRICTION_NORMAL * player.collisionObject.Friction;
                        else if (player.velocity.X < -0.2)
                            player.velocity.X += (float)Entity.GROUND_FRICTION_NORMAL * player.collisionObject.Friction;
                        else
                            player.velocity.X = 0;
                        break;
                }
            }
            // ->Air friction
            else if (!player.onGround && !player.blockFriction)
            {
                if (player.velocity.X > 0.05)
                    player.velocity.X -= (float)Entity.AIR_FRICTION;
                else if (player.velocity.X < -0.05)
                    player.velocity.X += (float)Entity.AIR_FRICTION;
                else
                    player.velocity.X = 0;
            }

            bool blockFriction = player.blockFriction;

            // ->Slope movement (Slide Trigger, Upwards movement)
            if (player.onSlope)
            {
                player.frictionType = FrictionType.SLOPESLIDE;
                SlopeObject slopeObject = player.collisionObject.GetSlopeObject();
                if (player.GetState() != EntityState.SLIDE)
                {
                    float inclineExhaust = 0.0f;
                    if (slopeObject.incline >= 1.5)
                    {
                        inclineExhaust = 0.35f;
                    }
                    else if (slopeObject.incline >= 1.0)
                    {
                        inclineExhaust = 0.3f;
                    }
                    else if (slopeObject.incline >= 0.5)
                    {
                        inclineExhaust = 0.25f;
                    }
                    else if (slopeObject.incline >= 0.25)
                    {
                        inclineExhaust = 0.2f;
                    }
                    if (Math.Abs(player.velocity.X) > 2)
                    {
                        if (!slopeObject.direction && player.velocity.X < 0)
                            player.velocity.X += (float)slopeObject.incline * inclineExhaust;
                        else if (slopeObject.direction && player.velocity.X > 0)
                            player.velocity.X -= (float)slopeObject.incline * inclineExhaust;
                    }
                }
                if (player.GetState() != EntityState.SLIDE && ((slopeObject.direction && player.velocity.X <= 0.005) || (!slopeObject.direction && player.velocity.X >= -0.005)) && slopeObject.incline >= 0.5)
                {
                    player.direction = slopeObject.direction;
                    player.ChangeState(EntityState.SLIDE);
                    player.blockDefaultMovementInput = true;
                }
            }

            // ->Slide movement (On slope, first ground contact)
            if (player.GetState() == EntityState.SLIDE)
            {
                if (player.onSlope)
                {
                    SlopeObject slopeObject = player.collisionObject.GetSlopeObject();
                    float slideMultiplier = 1.5f;
                    if (player.horizontalInput && player.inputDirection != slopeObject.direction)
                    {
                        slideMultiplier = 2.0f;
                        player.animator.SwitchState(PlayerAnimations.FASTSLIDE);
                    }
                    else
                    {
                        player.animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                    if (slopeObject.direction)
                    {
                        player.velocity.X -= (float)slopeObject.incline * 0.1f * slideMultiplier;
                    }
                    else
                    {
                        player.velocity.X += (float)slopeObject.incline * 0.1f * slideMultiplier;
                    }
                }
                else
                {
                    if (player.lastSlopeObject != null && player.collisionObject != player.lastSlopeObject)
                    {
                        player.lastSlopeObject = null;
                        player.frictionType = FrictionType.SLIDE;
                        player.animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                }
            }

            // ->Slide exit (jump, slope jump)
            if (player.GetState() == EntityState.SLIDE && Keyboard.GetState().IsKeyDown(Keys.Up) && !wasPressedJump)
            {
                if (player.lastSlopeObject == null)
                {
                    // Jump normally, give control
                    player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.0f));
                    player.jumpWindow = GetMs();
                    player.ChangeState(EntityState.JUMP);
                    player.blockDefaultMovementInput = false;
                }
                else
                {
                    // Jump on slope is only possible when fast + not-high incline OR last 20px of any slope
                    if (((Math.Abs(player.velocity.X) >= 4.0f && player.lastSlopeObject.incline < 1.5f) || player.GetMovementPoint().Y > player.lastSlopeObject.position.Y + player.lastSlopeObject.size.Y - 20) && player.velocity.Y >= 0)
                    {
                        // Jump slightly higher, small X vel boost
                        player.velocity.X *= 1.1f;
                        player.velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.2f));
                        player.jumpWindow = GetMs();
                        player.ChangeState(EntityState.JUMP);
                        player.blockDefaultMovementInput = false;
                    }
                }
            }

            // ->Friction type check
            if (player.GetState() != EntityState.SLIDE && !player.onSlope)
                player.frictionType = FrictionType.NORMAL;



            if (player.velocity.X > Entity.MAX_HORIZONTAL_VEL)
                player.velocity.X = (float)Entity.MAX_HORIZONTAL_VEL;
            else if (player.velocity.X < -Entity.MAX_HORIZONTAL_VEL)
                player.velocity.X = (float)-Entity.MAX_HORIZONTAL_VEL;
            if (player.velocity.Y > Entity.MAX_HORIZONTAL_VEL)
                player.velocity.Y = (float)Entity.MAX_HORIZONTAL_VEL;
            else if (player.velocity.Y < -Entity.MAX_VERTICAL_VEL)
                player.velocity.Y = (float)-Entity.MAX_VERTICAL_VEL;

            Vector2 moveVector = new Vector2(0, 0);
            moveVector += player.velocity;
            // COLLISION - NEW

            



            // --------------------------------- COLLISION CHECKS----------------------------------------
            bool canmove = true;
            bool _onGround = player.onGround;
            player.onGround = false;
            player.onSlope = false;
            foreach (SlopeObject obj in mapSlopes)
            {
                if (Maths.PointInTriangle(player.GetMovementPoint() + moveVector, obj.GetTrianglePoints()[0], obj.GetTrianglePoints()[1], obj.GetTrianglePoints()[2]) || CollisionOverflow(obj, player.GetMovementPoint(), player.GetMovementPoint() + moveVector))
                {
                    canmove = false;
                    bool leftright = false;
                    if (!obj.direction)
                    {
                        if (player.GetMovementPoint().X < obj.position.X && player.velocity.X > 0)
                        {
                            if (!_onGround)
                                player.velocity.X = -player.velocity.X * 0.2f;
                            else
                            {
                                moveVector.X = 0;
                                player.velocity.X = 0;
                            }
                            player.position.X = obj.position.X - player.size.X /2 - 1;
                            leftright = true;
                        }
                    }
                    else
                    {
                        if (player.GetMovementPoint().X > obj.position.X + obj.size.X && player.velocity.X < 0)
                        {
                            if (!_onGround)
                                player.velocity.X = -player.velocity.X * 0.2f;
                            else
                            {
                                moveVector.X = 0;
                                player.velocity.X = 0;
                            }
                            player.position.X = obj.position.X + obj.size.X - player.size.X / 2 + 1;
                            leftright = true;
                        }
                    }

                    if (!leftright)
                    {
                        if (player.position.Y < obj.position.Y + obj.size.Y && player.velocity.Y > 0)
                        {
                            player.velocity.Y = 0;
                            player.onSlope = true;
                            player.onGround = true;
                            player.collisionObject = obj;
                        }
                        else if (player.position.Y + player.size.Y > obj.position.Y && player.velocity.Y < 0)
                        {
                            player.velocity.Y = -player.velocity.Y;
                        }
                    }
                }

                if (Maths.PointInTriangle(player.GetHeadPoint() + moveVector, obj.GetTrianglePoints()[0], obj.GetTrianglePoints()[1], obj.GetTrianglePoints()[2]) || CollisionOverflow(obj, player.GetHeadPoint(), player.GetHeadPoint() + moveVector))
                {
                    canmove = false;

                    if (player.GetHeadPoint().Y >= obj.position.Y + obj.size.Y && player.velocity.Y < 0)
                    {
                        player.velocity.Y = 0;
                        player.position.Y = obj.position.Y + obj.size.Y + 1;
                        moveVector.Y = Math.Max(moveVector.Y, 0);
                        canmove = true;
                    }
                    else if (player.GetHeadPoint().X <= obj.position.X && player.velocity.X > 0 && !obj.direction)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X - player.size.X / 2 - 1;
                    }
                    else if (player.GetHeadPoint().X >= obj.position.X + obj.size.X && player.velocity.X < 0 && obj.direction)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X + obj.size.X - player.size.X / 2;
                    }
                }
            } // collsion check with all slopes

            foreach (RectObject obj in mapRectangles)
            {
                // ground & walls collision

                if (Maths.PointInRectangle(player.GetMovementPoint() + moveVector, new Rectangle(obj.position.ToPoint(), obj.size.ToPoint())) || CollisionOverflow(obj, player.GetMovementPoint(), player.GetMovementPoint() + moveVector))
                {
                    canmove = false;
                    if(player.GetMovementPoint().Y <= obj.position.Y && player.velocity.Y > 0)
                    {
                        if (player.pounding)
                            player.poundSpeed = player.velocity.Y;
                        if (player.diving)
                            player.diveHitSpeed = player.velocity.X;
                        player.collisionObject = obj;
                        player.velocity.Y = 0;
                        if (!_onGround)
                        {
                            if (player.jumpSequence == 0)
                            {
                                player.landWindow = GetMs();
                                player.jumpSequence = 1;
                            }
                            else if (player.jumpSequence == 2)
                            {
                                player.landWindow = GetMs();
                                player.jumpSequence = 3;
                            }
                        }
                        player.onGround = true;
                        player.position.Y = obj.position.Y - player.size.Y;
                        moveVector.Y = Math.Min(moveVector.Y, 0);
                        canmove = true;
                    }
                    else if(player.GetMovementPoint().X <= obj.position.X && player.velocity.X > 0)
                    {
                        if (!_onGround)
                        {
                            if (player.velocity.X > 2)
                            {
                                player.wallJumpSpeed = player.velocity.X;
                                player.velocity.Y = 1;
                                player.velocity.X = 0;
                                player.blockDefaultMovementInput = true;
                                player.blockGravity = true;
                                player.wallJump = true;
                                player.wallJumpDirection = true;
                                player.wallJumpFrame = 0;
                                player.wallJumpWindow = GetMs();
                            }
                            player.velocity.X = -player.velocity.X * 0.2f;
                        }
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X - player.size.X / 2 - 1;
                    }
                    else if (player.GetMovementPoint().X >= obj.position.X + obj.size.X && player.velocity.X < 0)
                    {
                        if (!_onGround)
                        {
                            if (player.velocity.X < -2)
                            {
                                player.wallJumpSpeed = player.velocity.X;
                                player.velocity.Y = 1;
                                player.velocity.X = 0;
                                player.blockDefaultMovementInput = true;
                                player.blockGravity = true;
                                player.wallJump = true;
                                player.wallJumpDirection = false;
                                player.wallJumpFrame = 0;
                                player.wallJumpWindow = GetMs();
                            }
                            player.velocity.X = -player.velocity.X * 0.2f;
                        }
                        else
                        {

                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X + obj.size.X - player.size.X / 2;
                    }
                    
                }

                if (Maths.PointInRectangle(player.GetHeadPoint() + moveVector, new Rectangle(obj.position.ToPoint(), obj.size.ToPoint())) || CollisionOverflow(obj, player.GetHeadPoint(), player.GetHeadPoint() + moveVector))
                {
                    canmove = false;

                    if (player.GetHeadPoint().Y >= obj.position.Y + obj.size.Y && player.velocity.Y < 0)
                    {
                        player.velocity.Y = 0;
                        player.position.Y = obj.position.Y + obj.size.Y + 1;
                        moveVector.Y = Math.Max(moveVector.Y, 0);
                        canmove = true;
                    }
                    else if (player.GetHeadPoint().X <= obj.position.X && player.velocity.X > 0)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X - player.size.X / 2 - 1;
                    }
                    else if (player.GetHeadPoint().X >= obj.position.X + obj.size.X && player.velocity.X < 0)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X + obj.size.X - player.size.X/2;
                    }
                }
                if (obj.size.Y < player.size.Y && 
                    player.GetHeadPoint().Y + moveVector.Y <= obj.position.Y && player.GetMovementPoint().Y + moveVector.Y >= obj.position.Y + obj.size.Y &&
                    player.GetHeadPoint().X + moveVector.X >= obj.position.X && player.GetHeadPoint().X + moveVector.X <= obj.position.X + obj.size.X)
                {
                    canmove = false;
                    if (player.GetHeadPoint().X <= obj.position.X && player.velocity.X > 0)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X - player.size.X / 2 - 1;
                    }
                    else if (player.GetHeadPoint().X >= obj.position.X + obj.size.X && player.velocity.X < 0)
                    {
                        if (!_onGround)
                            player.velocity.X = -player.velocity.X * 0.2f;
                        else
                        {
                            player.velocity.X = 0;
                        }
                        moveVector.X = 0;
                        canmove = true;
                        player.position.X = obj.position.X + obj.size.X - player.size.X / 2 + 1;
                    }
                    else if(player.GetHeadPoint().X >= obj.position.X + obj.size.X && player.velocity.X == 0)
                    {
                        moveVector.X = 0;
                        canmove = true;
                    }
                }
            }
            */
            // ------------------------------------------------------------------------------------------

            // ---------------------------------COLLISION PHYSICS----------------------------------------
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

            if (canmove) // apply movement vector to position after collisions check
                player.position += moveVector;

            // change direction
            if (player.velocity.X < -0.2 && player.onGround && !player.blockDir)
            {
                player.direction = false;
            }
            else if (player.velocity.X > 0.2 && player.onGround && !player.blockDir)
            {
                player.direction = true;
            }


            // change entity state
            if (Math.Abs(player.velocity.X) > 0.5 && player.onGround && player.GetState() != EntityState.WALK && !player.blockDefaultMovementInput && !player.crouch)
            {
                player.ChangeState(EntityState.WALK);
            }
            else if (Math.Abs(player.velocity.X) <= 0.5 && player.onGround && player.GetState() != EntityState.IDLE && !player.blockDefaultMovementInput && !player.crouch)
            {
                player.ChangeState(EntityState.IDLE);
            }
            else if (player.velocity.Y > 2 && !player.onGround && !player.diving && !player.blockDefaultMovementInput && player.GetState() != EntityState.FALL && !player.crouch && player.GetState() == EntityState.SIDEFLIP)
            {
                player.direction = !player.direction;
                player.ChangeState(EntityState.FALL);
            }
            else if (player.velocity.Y > 2 && !player.onGround && !player.diving && !player.blockDefaultMovementInput && player.GetState() != EntityState.FALL && !player.crouch)
            {
                player.ChangeState(EntityState.FALL);
            }
            else if (player.onGround && !player.blockDefaultMovementInput && player.crouch && player.GetState() != EntityState.CROUCH)
            {
                player.ChangeState(EntityState.CROUCH);
            }

                wasPressedJump = Keyboard.GetState().IsKeyDown(Keys.Up);
            wasPressedCrouch = Keyboard.GetState().IsKeyDown(Keys.Down);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach(MapObject x in mapObjects)
            {
                x.Draw(spriteBatch, GraphicsDevice, basicEffect);
            }

            player.Draw(spriteBatch, defaultTexture);

            if (DEBUG && DEBUG_DISPLAY == 1)
            {
                spriteBatch.DrawString(defaultFont, "XVEL: " + Math.Round(player.velocity.X, 3), new Vector2(10, 10), Color.Red);
                spriteBatch.DrawString(defaultFont, "YVEL: " + Math.Round(player.velocity.Y, 3), new Vector2(10, 30), Color.Red);
                spriteBatch.DrawString(defaultFont, "POS: x" + Math.Round(player.position.X, 2) + " y" + Math.Round(player.position.Y, 2), new Vector2(10, 50), Color.Red);
                spriteBatch.DrawString(defaultFont, "WALLKICK: " + DEBUG_WALLKICK, new Vector2(10, 70), Color.Red);
                spriteBatch.DrawString(defaultFont, "JUMP SEQ: " + player.jumpSequence, new Vector2(10, 90), Color.Red);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private long GetMs()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private bool InWindow(long windowStart, int ms)
        {
            return GetMs() >= windowStart && GetMs() <= windowStart + ms;
        }

        private bool AfterWindow(long windowStart, int ms)
        {
            return GetMs() > windowStart + ms;
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
