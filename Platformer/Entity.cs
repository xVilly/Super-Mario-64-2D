using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

using Platformer.Helpers;

namespace Platformer
{
    public enum EntityState
    {
        IDLE,
        WALK,
        JUMP,
        FALL,
        DIVE,
        POUND,
        CROUCH,
        SLIDE,
        SIDEFLIP,
        ROLL,
        LONGJUMP,
        PUNCH,
        KICK,
        LEDGE,
        BONK,
        SLIDEKICK,
        RUNNING
    }

    public enum CollisionType
    {
        FLOOR,
        CEILING,
        FLOOR_SLOPE,
        WALL_LEFT,
        WALL_RIGHT
    }

    public class ActionWindow
    {
        public static List<ActionWindow> actionWindows = new List<ActionWindow>();

        public bool Active = false;
        public int FrameCounter = 0;
        public int WindowLength; // (Frames)

        public ActionWindow(int _windowLength)
        {
            WindowLength = Math.Max(_windowLength, 1);
            actionWindows.Add(this);
        }
        public void Start()
        {
            Active = true;
            FrameCounter = 0;
        }
        public void Update()
        {
            if (!Active)
                return;
            FrameCounter++;
            if (FrameCounter == WindowLength)
                Active = false;
        }
    }
    public class Entity
    {
        // Entity Constants
        public const double MIN_RUNNING_VEL = 3.5;
        public const double MAX_RUNNING_VEL = 4.5;
        public const double MAX_WALKING_VEL_CROUCH = 1.5;
        public const double MAX_HORIZONTAL_VEL = 50;
        public const double MAX_VERTICAL_VEL = 50;
        public const double JUMP_FORCE = 5;

        public const double AIR_FRICTION = 0.010;
        public const double GROUND_FRICTION = 0.05;
        public const double GRAVITY = 0.27;

        public const double POUND_GRAVITYMULT = 2.5;
        public const double POSTPOUND_SPEED = 3;

        public const double DIVE_GRAVITYMULT = 0.8;
        public const double DIVE_SPEED = 1.5f;
        public const double POSTDIVE_SPEED = 3;

        // Constants - action time windows
        public const int DirChangeWindow = 15; // (frames)
        public const int ActionExhaust = 300;

        // Movement assistance
        public const float AutoStepLimit = 3.5f;

        // Main variables
        public Vector2 position;
        public Vector2 velocity = new Vector2(0, 0);
        public Vector2 size;
        public Vector2 scale;
        public Vector2 startSize;
        public Vector2 crouchSize;
        public float gravityMultiplier = 1.0f;
        public float frictionMultiplier = 1.0f;
        public int spriteID;
        public bool direction = true;
        public Animator animator;
        private EntityState entityState;

        // Collision & Physics
        public bool freezePlayer = false;
        public bool horizontalInput = false;
        public bool inputDirection = true;
        public bool crouch = false;
        public bool blockDefaultMovementInput = false;
        public bool blockGravity = false;
        public bool blockFriction = false;
        public bool blockDir = false;
        public bool onGround = false;
        public bool _onGround = false;
        public bool onSlope = false;
        public MapObject collisionObject = null;
        public SlopeObject lastSlopeObject = null;

        // Action windows
        
        public ActionWindow jumpWindow = new ActionWindow(12);
        public ActionWindow landWindow = new ActionWindow(6);
        public ActionWindow wallJumpWindow = new ActionWindow(5);
        public ActionWindow poundWindow = new ActionWindow(24);
        public ActionWindow postPoundWindow = new ActionWindow(12);
        public ActionWindow postDiveWindow = new ActionWindow(90);
        public ActionWindow postDiveRollWindow = new ActionWindow(18);
        public ActionWindow punchOneWindow = new ActionWindow(24);
        public ActionWindow punchDoubleWindow = new ActionWindow(24);
        public ActionWindow kickWindow = new ActionWindow(30);
        public ActionWindow ledgeGrabWindow = new ActionWindow(30);
        public ActionWindow ledgeStandWindow = new ActionWindow(18);
        public ActionWindow postLedgeStandWindow = new ActionWindow(30);
        public ActionWindow bonkWindow = new ActionWindow(90);
        public ActionWindow slideKickWindow = new ActionWindow(30);
        public ActionWindow slideKickNoActionWindow = new ActionWindow(30);
        public ActionWindow runningWindow = new ActionWindow(60);


        // Actions variables

        private int dirChangeFrames = 0;
        public int jumpSequence = 0;

        private bool wallJump = false;
        private bool wallJumpDirection = false;
        private float wallJumpSpeed = 0;
        private int wallJumpFrame = 0;

        private bool pounding = false;
        private bool poundFreeze = false;
        private float poundSpeed = 0;
        private bool postPound = false;

        private bool diving = false;
        private float diveHitSpeed = 0;
        private bool postDive = false;
        private bool postDiveRoll = false;

        private bool punching = false;
        private bool punchingDouble = false;
        private bool schedulePunch = false;

        private bool kicking = false;
        private bool canKick = true;

        private int framesAfterDiveLand = 0;

        private MapObject ledgeGrabObject = null;
        private bool ledgeGrabDirection = true;
        private bool ledgeGrab = false;
        private bool ledgeStand = false;
        private bool postLedgeStand = false;

        private bool bonked = false;

        private bool slideKick = false;

        private bool runningStart = false;

        private bool wasPressedJump = false;
        private bool wasPressedCrouch = false;

        //(index) 0: all-actions, 1: jump, 2: crouch
        private long[] actionExhaust = {0, 0, 0, 0 };
        private int[] actionExhaustMs = {0, 0, 0, 0 };

        // Particle engines reserved for player
        ParticleEngine _particleBonk;
        ParticleEngine _particleRunDust;


    public Entity(Vector2 startPosition)
        {
            position = startPosition;
            size = new Vector2(32, 64);
            scale = new Vector2(1, 1);
            startSize = size;
            crouchSize = new Vector2(size.X, size.Y * 3 / 4);
            animator = new Animator(this, 50);
            animator.idle = new Animation(0, 4, 400);
            animator.walk = new Animation(4, 8, 50);
            animator.jump = new Animation(12, 1, 500);
            animator.jump2 = new Animation(13, 1, 500);
            animator.fall = new Animation(14, 1, 500);
            animator.fall2 = new Animation(15, 1, 500);
            animator.fall3 = new Animation(16, 1, 500);
            animator.triplejump = new Animation(17, 8, 50);
            animator.dive = new Animation(25, 1, 500);
            animator.pound = new Animation(26, 10, 40, false);
            animator.postpound = new Animation(36, 1, 500);
            animator.roll = new Animation(37, 4, 50);
            animator.dirchange = new Animation(41, 1, 500);
            animator.sideflip = new Animation(42, 7, 100);
            animator.crouch = new Animation(49, 1, 500);
            animator.crouchwalk = new Animation(50, 7, 150);
            animator.slide = new Animation(57, 1, 500);
            animator.slidekick = new Animation(58, 3, 75, false);
            animator.punch = new Animation(62, 4, 75, false);
            animator.punch2 = new Animation(66, 4, 75, false);
            animator.kick = new Animation(70, 2, 75, false);
            animator.ledge = new Animation(73, 1, 500);
            animator.ledgestand = new Animation(74, 1, 100, false);
            animator.running = new Animation(76, 7, 45);

            _particleBonk = new ParticleEngine(Vector2.Zero);
            _particleBonk.CurrentTemplate = ParticleTemplates.bonk;
            _particleRunDust = new ParticleEngine(Vector2.Zero);
            _particleRunDust.CurrentTemplate = ParticleTemplates.runningDust;

            animator.SwitchState(PlayerAnimations.IDLE);
            entityState = EntityState.IDLE;
        }

        public bool HasActionExhaust(int id)
        {
            return Maths.InWindow(actionExhaust[id], actionExhaustMs[id]);
        }

        public void AddActionExhaust(int id, int ms = ActionExhaust)
        {
            actionExhaust[id] = Maths.GetMs();
            actionExhaustMs[id] = ms;
        }

        public void ChangeState(EntityState state)
        {
            EntityState lastState = entityState;
            entityState = state;
            switch (state)
            {
                case EntityState.WALK:
                    animator.SwitchState(PlayerAnimations.WALK);
                    break;
                case EntityState.JUMP:
                    if (jumpSequence == 2)
                        animator.SwitchState(PlayerAnimations.DOUBLEJUMP);
                    else if (jumpSequence == 3)
                        animator.SwitchState(PlayerAnimations.TRIPLEJUMP);
                    else
                        animator.SwitchState(PlayerAnimations.JUMP);
                    break;
                case EntityState.FALL:
                    if (jumpSequence == 2 && Math.Abs(velocity.X) >= 2.5)
                        animator.SwitchState(PlayerAnimations.FALLDOUBLE);
                    else if (jumpSequence == -1)
                        animator.SwitchState(PlayerAnimations.FALLTRIPLE);
                    else
                        animator.SwitchState(PlayerAnimations.FALL);
                    break;
                case EntityState.CROUCH:
                    if (pounding)
                        animator.SwitchState(PlayerAnimations.POSTPOUND);
                    else if (onGround && Math.Abs(velocity.X) > 0)
                        animator.SwitchState(PlayerAnimations.CROUCHWALK);
                    else
                        animator.SwitchState(PlayerAnimations.CROUCH);
                    break;
                case EntityState.DIVE:
                    animator.SwitchState(PlayerAnimations.DIVE);
                    break;
                case EntityState.SLIDE:
                    if (lastState == EntityState.DIVE)
                        animator.SwitchState(PlayerAnimations.DIVE);
                    else
                    {
                        animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                    break;
                case EntityState.SIDEFLIP:
                    animator.SwitchState(PlayerAnimations.SIDEFLIP);
                    break;
                case EntityState.ROLL:
                    animator.SwitchState(PlayerAnimations.ROLL);
                    break;
                case EntityState.LONGJUMP:
                    animator.SwitchState(PlayerAnimations.LONGJUMP);
                    break;
                case EntityState.PUNCH:
                    animator.SwitchState(PlayerAnimations.PUNCH);
                    break;
                case EntityState.KICK:
                    animator.SwitchState(PlayerAnimations.KICK);
                    break;
                case EntityState.LEDGE:
                    animator.SwitchState(PlayerAnimations.LEDGE);
                    break;
                case EntityState.BONK:
                    animator.SwitchState(PlayerAnimations.SLIDE);
                    break;
                case EntityState.SLIDEKICK:
                    animator.SwitchState(PlayerAnimations.SLIDEKICK);
                    break;
                case EntityState.RUNNING:
                    animator.SwitchState(PlayerAnimations.RUNNING);
                    break;
                default:
                    animator.SwitchState(PlayerAnimations.IDLE);
                    break;
            }
        }
        public EntityState GetState() { return entityState; }

        // Movement point - bottom middle hitbox point (for movement physics)
        public Vector2 GetMovementPoint()
        {
            return new Vector2(position.X + (size.X / 2), position.Y + size.Y);
        }

        public Vector2 GetHeadPoint()
        {
            return new Vector2(position.X + (size.X / 2), position.Y);
        }

        public void Update()
        {
            animator.Update();
            #region Player - Camera Input
            if (Camera.GetMode() != CameraMode.LOCKED) {
                if (InputManager.CameraMode.OnPress())
                    Camera.ChangeMode(Camera.GetMode() == CameraMode.LAKITU ? CameraMode.MARIO : CameraMode.LAKITU);
                if (InputManager.CameraLeft.OnPress()){
                    if (Camera.cameraOffset == CameraOffset.NONE)
                        Camera.cameraOffset = CameraOffset.LEFT;
                    else if (Camera.cameraOffset == CameraOffset.RIGHT)
                        Camera.cameraOffset = CameraOffset.NONE;
                }
                if (InputManager.CameraRight.OnPress()){
                    if (Camera.cameraOffset == CameraOffset.NONE)
                        Camera.cameraOffset = CameraOffset.RIGHT;
                    else if (Camera.cameraOffset == CameraOffset.LEFT)
                        Camera.cameraOffset = CameraOffset.NONE;
                }
                if (InputManager.CameraUp.OnPress()){
                    if (Camera.cameraZoom == CameraZoom.NORMAL)
                        Camera.ChangeZoom(CameraZoom.IN);
                    else if (Camera.cameraZoom == CameraZoom.OUT)
                        Camera.ChangeZoom(CameraZoom.NORMAL);
                }
                if (InputManager.CameraDown.OnPress()){
                    if (Camera.cameraZoom == CameraZoom.NORMAL)
                        Camera.ChangeZoom(CameraZoom.OUT);
                    else if (Camera.cameraZoom == CameraZoom.IN)
                        Camera.ChangeZoom(CameraZoom.NORMAL);
                }

            }
            #endregion
            #region Player - Movement Input
            float stepValue = 0.5f;
            horizontalInput = false;
            if (blockDefaultMovementInput)
                stepValue = 0;
            if (InputManager.HorizontalInput != 0) {
                horizontalInput = true;
                inputDirection = InputManager.HorizontalInput > 0 ? true : false;
                if (crouch){
                    if (Math.Abs(velocity.X) < Entity.MAX_WALKING_VEL_CROUCH * Math.Abs(InputManager.HorizontalInput))
                        velocity += new Vector2(stepValue * 0.5f * InputManager.HorizontalInput, 0);
                } else if (!onGround) {
                    velocity += new Vector2(stepValue * 0.15f * InputManager.HorizontalInput, 0);
                } else {
                    if (entityState != EntityState.RUNNING){
                        if (Math.Abs(velocity.X) < Entity.MIN_RUNNING_VEL * Math.Abs(InputManager.HorizontalInput))
                            velocity += new Vector2(stepValue * InputManager.HorizontalInput, 0);
                    } else {
                        if (Math.Abs(velocity.X) < Entity.MAX_RUNNING_VEL * Math.Abs(InputManager.HorizontalInput))
                            velocity += new Vector2(stepValue * InputManager.HorizontalInput, 0);
                    }
                    if (Math.Abs(velocity.X) > 4.0f && dirChangeFrames == 0 && GetState() == EntityState.RUNNING && !Maths.sameSign(velocity.X, InputManager.HorizontalInput))
                        dirChangeFrames = 1;
                }
            }
            if (!runningStart && Math.Abs(velocity.X) >= Entity.MIN_RUNNING_VEL - 0.5f && entityState == EntityState.WALK && onGround && Maths.sameSign(InputManager.HorizontalInput, velocity.X)){
                runningStart = true;
                runningWindow.Start();
            } else if (runningStart && (Math.Abs(velocity.X) < Entity.MIN_RUNNING_VEL - 0.5f || entityState != EntityState.WALK || !onGround || !Maths.sameSign(InputManager.HorizontalInput, velocity.X))){
                runningStart = false;
                runningWindow.Active = false;
            } else if (runningStart && !runningWindow.Active &&
                Math.Abs(velocity.X) >= Entity.MIN_RUNNING_VEL - 0.5f && entityState == EntityState.WALK && onGround && Maths.sameSign(InputManager.HorizontalInput, velocity.X)) {
                runningStart = false;
                ChangeState(EntityState.RUNNING);
                velocity.X += velocity.X > 0 ? 1.0f : -1.0f;
            } else if (entityState == EntityState.RUNNING && Math.Abs(velocity.X) >= 4.0f) {
                _particleRunDust.EmitterLocation = Camera.ConvertPos(GetMovementPoint());
                _particleRunDust.SpawnParticles();
            }
            #endregion
            #region Player - All Movement Actions


            // Action: Dirchange (Player quick turn on ground)
            if (dirChangeFrames >= 1 && dirChangeFrames < Entity.DirChangeWindow)
            {
                dirChangeFrames++;
                if (GetState() != EntityState.SIDEFLIP)
                    animator.SwitchState(PlayerAnimations.DIRCHANGE);
                frictionMultiplier = 0.8f;
                blockDefaultMovementInput = true;
                blockDir = true;
                if (dirChangeFrames == 1)
                {
                    if (velocity.X > 0)
                        direction = false;
                    else
                        direction = true;
                }
            }
            else if (dirChangeFrames == Entity.DirChangeWindow)
            {
                frictionMultiplier = 1.0f;
                dirChangeFrames = 0;
                blockDefaultMovementInput = false;
                blockDir = false;
            }


            // Reset player jump sequence after missing land window
            if (jumpSequence > 0 && onGround && !landWindow.Active)
            {
                if (jumpSequence == 3)
                    jumpSequence = -1;
                else
                    jumpSequence = 0;
            }


            // Action: Jumping
            if (InputManager.JumpButton.OnPress() && onGround
                && !blockDefaultMovementInput && !HasActionExhaust(0) 
                && !diving && !postDive && !postDiveRoll && !pounding && !punching && !kicking && !postPound && !slideKick && !ledgeGrab && !ledgeStand){
                if (onSlope) {
                    SlopeObject slopeObject = collisionObject.GetSlopeObject();
                    if (slopeObject.incline >= 0.25) {
                        if (jumpSequence == 3) {
                            if (landWindow.Active && Math.Abs(velocity.X) >= 2.5) {
                                jumpSequence = -1;
                                landWindow.Active = false;
                                velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                                if (direction)
                                    velocity.X += 1.0f;
                                else
                                    velocity.X += 1.0f;
                                jumpWindow.Start();
                                ChangeState(EntityState.ROLL);
                            }
                            else {
                                jumpSequence = -1;
                                landWindow.Active = false;
                            }
                        }
                        else {
                            velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 0.5f));
                            jumpWindow.Start();
                            ChangeState(EntityState.JUMP);
                        }
                    }
                    else {
                        if (jumpSequence == 3){
                            if (landWindow.Active && Math.Abs(velocity.X) >= 2.5) {
                                jumpSequence = -1;
                                landWindow.Active = false;
                                velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                                if (direction)
                                    velocity.X += 1.0f;
                                else
                                    velocity.X += 1.0f;
                                jumpWindow.Start();
                                ChangeState(EntityState.ROLL);
                            }
                            else {
                                jumpSequence = -1;
                                landWindow.Active = false;
                            }
                        }
                        else {
                            velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 0.8f));
                            jumpWindow.Start();
                            ChangeState(EntityState.JUMP);
                        }
                    }
                } else {
                    if (jumpSequence == -1) {
                        jumpSequence = 0;
                    }
                        // DOUBLE-JUMP
                    if (jumpSequence == 1 && landWindow.Active) {
                        jumpSequence = 2;
                        landWindow.Active = false;
                        velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.35f));
                        jumpWindow.Start();
                        ChangeState(EntityState.JUMP);
                    }
                        // TRIPLE-JUMP
                    else if (jumpSequence == 3) {
                        if (landWindow.Active && Math.Abs(velocity.X) >= 2.5) {
                            landWindow.Active = false;
                            velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.85f));
                            if (direction)
                                velocity.X += 1.0f;
                            else
                                velocity.X += 1.0f;
                            jumpWindow.Start();
                            ChangeState(EntityState.JUMP);
                            jumpSequence = -1;
                        } else {
                           jumpSequence = -1;
                           landWindow.Active = false;
                        }
                    } else {
                        velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.0f));
                        jumpWindow.Start();
                        ChangeState(EntityState.JUMP);
                    }
                }
            }
            if (InputManager.JumpButton.IsPressed() && jumpWindow.Active && velocity.Y < 0)
            {
                if (!blockDefaultMovementInput)
                    velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE / 30.0f));
            }


            // Action: Sideflip
            if (InputManager.JumpButton.OnPress() && onGround && dirChangeFrames >= 1 && dirChangeFrames < Entity.DirChangeWindow
                && !HasActionExhaust(0)
                && !crouch && !diving && !postDive && !postDiveRoll && !pounding && !punching && !kicking && !postPound && !slideKick && !ledgeGrab && !ledgeStand) {
                frictionMultiplier = 1.0f;
                blockDefaultMovementInput = true;
                dirChangeFrames = 1;
                ChangeState(EntityState.SIDEFLIP);
                velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.75f));
                if (direction)
                    velocity.X = -3.0f;
                else
                    velocity.X = 3.0f;
            }
            

            // Ceiling check for crouch actions
            bool canStand = true;
            foreach (RectObject rect in Game1.mapRectangles)
            {
                if (Maths.PointInRectangle(new Vector2(GetHeadPoint().X, position.Y - (startSize.Y - crouchSize.Y)), new Rectangle(rect.position.ToPoint(), rect.size.ToPoint())))
                    canStand = false;
            }


            // Action: Crouch
            if (InputManager.CrouchButton.OnPress() && !crouch && onGround && !landWindow.Active
                && !blockDefaultMovementInput && !HasActionExhaust(2) && !HasActionExhaust(0)
                && !diving && !postDive && !postDiveRoll && !pounding && !punching && !kicking && !postPound && !slideKick && !ledgeGrab && !ledgeStand) {
                crouch = true;
                position.Y = position.Y + (startSize.Y - crouchSize.Y);
                size = crouchSize;
            }
            if (InputManager.CrouchButton.IsReleased() && crouch && onGround && canStand) {
                crouch = false;
                position.Y = position.Y - (startSize.Y - crouchSize.Y);
                size = startSize;
                AddActionExhaust(2);
                AddActionExhaust(0);
            }


            // Action: Wall-jump
            wallJumpFrame++;
            if (InputManager.JumpButton.OnPress() && wallJump)
            {
                int xDir = wallJumpDirection ? -1 : 1;
                wallJump = false;
                blockDefaultMovementInput = false;
                blockGravity = false;
                direction = !wallJumpDirection;
                if (GetState() != EntityState.JUMP)
                    ChangeState(EntityState.JUMP);
                float speedBonus = 1.0f;
                if ((InputManager.HorizontalInput > 0 && !wallJumpDirection) || (InputManager.HorizontalInput < 0 && wallJumpDirection))
                    speedBonus = 1.15f;

                if (wallJumpFrame == 1) {
                    velocity.X = speedBonus * wallJumpSpeed * 1.1f * xDir;
                    velocity.Y = -5;
                    Game1.DEBUG_WALLKICK = "1 FRAME (PERFECT)";
                } else if (wallJumpFrame == 2) {
                    velocity.X = speedBonus * 4.0f * xDir;
                    velocity.Y = -5;
                    Game1.DEBUG_WALLKICK = "2 FRAME";
                } else if (wallJumpFrame == 3) {
                    velocity.X = speedBonus * 3.0f * xDir;
                    velocity.Y = -4;
                    Game1.DEBUG_WALLKICK = "3 FRAME";
                } else {
                    velocity.X = speedBonus * 2.0f * xDir;
                    velocity.Y = -3;
                    Game1.DEBUG_WALLKICK = "4+ FRAME";
                }
            }
            if (!wallJumpWindow.Active && wallJump)
            {
                wallJump = false;
                blockDefaultMovementInput = false;
                blockGravity = false;
                if (Math.Abs(wallJumpSpeed) > 3.5f) {
                    ChangeState(EntityState.BONK);
                    bonked = true;
                    bonkWindow.Start();
                    blockDefaultMovementInput = true;
                    AddActionExhaust(0, 1500);
                    velocity.X = wallJumpDirection ? 3 : -3;
                    velocity.Y = -1;
                    _particleBonk.EmitterLocation = Camera.ConvertPos(GetMovementPoint());
                    _particleBonk.SpawnParticles();
                }
            }
            if (bonked && !bonkWindow.Active)
            {
                bonked = false;
                blockDefaultMovementInput = false;
                if (onGround)
                    ChangeState(EntityState.IDLE);
                else
                    ChangeState(EntityState.FALL);
            }


            // Action: Pound
            if (InputManager.CrouchButton.OnPress() && !onGround && !crouch 
                && !blockDefaultMovementInput && !HasActionExhaust(0)
                && !diving && !postDive && !postDiveRoll && !pounding && !punching && !kicking && !postPound && !slideKick && !ledgeGrab && !ledgeStand)
            {
                animator.SwitchState(PlayerAnimations.POUND);
                pounding = true;
                gravityMultiplier = 0f;
                blockDefaultMovementInput = true;
                velocity.X = 0f;
                velocity.Y = 0f;
                poundWindow.Start();
                poundFreeze = true;
                crouch = true;
                size = crouchSize;
            }
            if (pounding && poundFreeze && !poundWindow.Active)
            {
                ChangeState(EntityState.CROUCH);
                poundFreeze = false;
                gravityMultiplier = (float)Entity.POUND_GRAVITYMULT;
                blockDefaultMovementInput = false;
                jumpSequence = -1;
                landWindow.Active = false;
            }


            // Action: Punch
            if (InputManager.AttackButton.OnPress() && onGround && !punching && (Math.Abs(velocity.X) < 3 || !Maths.sameSign(InputManager.HorizontalInput, velocity.X))
                && !HasActionExhaust(0) && !blockDefaultMovementInput
                && !crouch && !diving && !postDive && !postDiveRoll && !pounding && !kicking && !postPound && !slideKick && !ledgeGrab && !ledgeStand)
            {
                punching = true;
                punchOneWindow.Start();
                ChangeState(EntityState.PUNCH);
                blockDefaultMovementInput = true;
                velocity.X += direction ? 1 : -1;
            }
            else if (InputManager.AttackButton.OnPress() && onGround && punching && !punchingDouble && punchOneWindow.Active)
            {
                schedulePunch = true;
            }
            if (punching && !punchOneWindow.Active)
            {
                if (!schedulePunch) {
                    punching = false;
                    blockDefaultMovementInput = false;
                    ChangeState(EntityState.IDLE);
                    AddActionExhaust(0, 100);
                } else if (!punchingDouble) {
                    punchingDouble = true;
                    animator.SwitchState(PlayerAnimations.DOUBLEPUNCH);
                    punchDoubleWindow.Start();
                    velocity.X += direction ? 1.5f : -1.5f;
                }
            }
            if (punching && punchingDouble && !punchDoubleWindow.Active)
            {
                punching = false;
                punchingDouble = false;
                schedulePunch = false;
                blockDefaultMovementInput = false;
                ChangeState(EntityState.IDLE);
                AddActionExhaust(0, 200);
            }


            // Action: Kick
            if (InputManager.AttackButton.OnPress() && !onGround && canKick && !kicking && (Math.Abs(velocity.X) < 3.5 || !Maths.sameSign(InputManager.HorizontalInput, velocity.X))
                && !blockDefaultMovementInput && !HasActionExhaust(0)
                && !crouch && !diving && !postDive && !postDiveRoll && !pounding && !punching && !postPound && !slideKick && !ledgeGrab && !ledgeStand)
            {
                kicking = true;
                canKick = false;
                kickWindow.Start();
                ChangeState(EntityState.KICK);
                AddActionExhaust(0, 500);
                velocity.Y = -3.0f;
                gravityMultiplier = 0.75f;
                if (Math.Abs(velocity.X) > 3)
                    velocity.X *= 1.15f;
                else
                    velocity.X *= 0.5f;
            }
            if (kicking && !kickWindow.Active)
            {
                kicking = false;
                gravityMultiplier = 1.0f;
                AddActionExhaust(0, 300);
                if (!onGround)
                    ChangeState(EntityState.FALL);
                else
                    ChangeState(EntityState.IDLE);
            }
            
            
            // Action: Dive
            if (InputManager.AttackButton.OnPress() && !diving && !crouch && Math.Abs(velocity.X) >= 3.5  && (Maths.sameSign(InputManager.HorizontalInput, velocity.X) || InputManager.HorizontalInput == 0) 
                && !blockDefaultMovementInput && !HasActionExhaust(0)
                && !kicking && !postDive && !postDiveRoll && !pounding && !punching && !postPound && !slideKick && !ledgeGrab && !ledgeStand)
            {
                AddActionExhaust(0, 100);
                crouch = true;
                position.Y = position.Y + (startSize.Y - crouchSize.Y);
                size = crouchSize;
                if (GetState() == EntityState.SIDEFLIP)
                    direction = velocity.X > 0;
                if (GetState() != EntityState.DIVE)
                    ChangeState(EntityState.DIVE);
                diving = true;
                blockDefaultMovementInput = true;
                jumpSequence = -1;
                if (onGround)
                    velocity.Y = (float)-Entity.JUMP_FORCE;
                else {
                    if (velocity.Y < 0)
                        velocity.Y *= 0.85f;
                    else if (velocity.Y >= 0)
                        velocity.Y *= 0.75f;
                }
                gravityMultiplier = (float)Entity.DIVE_GRAVITYMULT;
                if (velocity.X >= 0)
                    velocity.X += (float)Entity.DIVE_SPEED;
                else
                    velocity.X -= (float)Entity.DIVE_SPEED;
            } 
            else if (onGround && diving)
            {
                diving = false;
                gravityMultiplier = 1.0f;
                postDive = true;
                postDiveWindow.Start();
                velocity.Y -= 1.0f;
                velocity.X *= 1.25f;
                frictionMultiplier = 0.6f;
            }
            if (!postDiveWindow.Active && postDive)
            {
                frictionMultiplier = 1.0f;
                postDive = false;
                blockDefaultMovementInput = false;
                crouch = false;
                size = startSize;
                ChangeState(EntityState.IDLE);
                AddActionExhaust(0, 300);
                jumpSequence = -1;
            }
            else if (postDiveWindow.Active && postDive && Math.Abs(velocity.X) > 0.1f && Math.Abs(velocity.Y) > 0.1f)
                postDiveWindow.Start();
            if (postDive && onGround)
                framesAfterDiveLand++;


            // Action: Roll (From dive)
            if (postDive && !postDiveRoll && onGround && framesAfterDiveLand > 1 && (InputManager.AttackButton.OnPress() || InputManager.JumpButton.OnPress()))
            {
                frictionMultiplier = 1.0f;
                postDive = false;
                postDiveRoll = true;
                postDiveRollWindow.Start();
                ChangeState(EntityState.ROLL);
                bool perfectRollout = framesAfterDiveLand <= 10 && ((InputManager.AttackButton.OnPress() && InputManager.JumpButton.WasPressed()) || (InputManager.JumpButton.OnPress() && InputManager.AttackButton.WasPressed()));
                bool goodRollout = framesAfterDiveLand <= 10;
                float multiplier = 1.0f;
                if (perfectRollout)
                    multiplier = 2.0f;
                else if (goodRollout)
                    multiplier = 1.5f;
                velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 0.5f * multiplier));
                if (direction)
                    velocity.X += 1.5f * multiplier;
                else
                    velocity.X -= 1.5f * multiplier;
                framesAfterDiveLand = 0;
                AddActionExhaust(0, 200);
                jumpSequence = -1;
            }
            if (!postDiveRollWindow.Active && postDiveRoll)
            {
                postDiveRoll = false;
                blockDefaultMovementInput = false;
                crouch = false;
                size = startSize;
                jumpSequence = 1;
                landWindow.Start();
                AddActionExhaust(0, 200);
            }


            // Pound effects
            if (onGround && pounding)
            {
                pounding = false;
                gravityMultiplier = 1.0f;
                if (poundSpeed > Entity.POSTPOUND_SPEED){
                    postPoundWindow.Start();
                    blockDefaultMovementInput = true;
                    postPound = true;
                    if (poundSpeed > Entity.POSTPOUND_SPEED * 1.5)
                        velocity.Y = -2;
                    else if (poundSpeed > Entity.POSTPOUND_SPEED * 2.0)
                        velocity.Y = -3;
                    else
                        velocity.Y = -1;
                } else {
                    crouch = false;
                    size = startSize;
                    if (collisionObject.GetSlopeObject() == null && GetHeadPoint().Y < collisionObject.position.Y)
                        position.Y = collisionObject.position.Y - size.Y;
                    else
                        position.Y = position.Y - (startSize.Y - crouchSize.Y);
                }
            }
            if (!postPoundWindow.Active && postPound)
            {
                postPound = false;
                blockDefaultMovementInput = false;
                crouch = false;
                size = startSize;
                if (collisionObject.GetSlopeObject() == null && GetHeadPoint().Y < collisionObject.position.Y)
                    position.Y = collisionObject.position.Y - size.Y;
                else
                    position.Y = position.Y - (startSize.Y - crouchSize.Y);
                AddActionExhaust(0, 200);
            }
           

            // Action: Ledge stand
            if (ledgeGrab && !ledgeGrabWindow.Active && InputManager.JumpButton.OnPress())
            {
                animator.SwitchState(PlayerAnimations.LEDGESTAND);
                ledgeGrab = false;
                ledgeStand = true;
                ledgeStandWindow.Start();
            }
            if (ledgeStand && !ledgeStandWindow.Active)
            {
                ledgeStand = false;
                postLedgeStand = true;
                postLedgeStandWindow.Start();
                animator.SwitchState(PlayerAnimations.IDLE);
                position = new Vector2(ledgeGrabDirection ? ledgeGrabObject.position.X : ledgeGrabObject.position.X + ledgeGrabObject.size.X - size.X, ledgeGrabObject.position.Y - size.Y);
                freezePlayer = false;
                gravityMultiplier = 1.0f;
                blockDefaultMovementInput = true;
                AddActionExhaust(0, 500);
            }
            if (postLedgeStand && !postLedgeStandWindow.Active)
            {
                postLedgeStand = false;
                blockDefaultMovementInput = false;
            }


            // Action: Slidekick
            if (InputManager.AttackButton.OnPress() && crouch && onGround && !slideKick && Math.Abs(velocity.X) >= 2.0f
                && !blockDefaultMovementInput && !HasActionExhaust(0)
                && !diving && !kicking && !postDive && !postDiveRoll && !pounding && !punching && !postPound && !ledgeGrab && !ledgeStand)
            {
                slideKick = true;
                slideKickWindow.Start();
                slideKickNoActionWindow.Start();
                ChangeState(EntityState.SLIDEKICK);
                velocity.Y -= 2.0f;
                velocity.X += velocity.X > 0 ? 2.0f : -2.0f;
                velocity.X *= 1.15f;
                blockDefaultMovementInput = true;
                frictionMultiplier = 0.35f;
            }
            if (slideKick && slideKickWindow.Active && Math.Abs(velocity.X) >= 0.5)
                slideKickWindow.Start();
            if (slideKick && !slideKickWindow.Active)
            {
                slideKick = false;
                ChangeState(EntityState.IDLE);
                blockDefaultMovementInput = false;
                frictionMultiplier = 1.0f;
            }


            #endregion
            #region Player - Physics
            // ->Gravity
            if (velocity.Y < Entity.MAX_VERTICAL_VEL && !blockGravity)
                velocity.Y += (float)Entity.GRAVITY * gravityMultiplier;
            // ->Ground friction
            if (onGround && !blockFriction)
            {
                if (velocity.X > 0.2)
                    velocity.X -= (float)Entity.GROUND_FRICTION * collisionObject.Friction * frictionMultiplier;
                else if (velocity.X < -0.2)
                    velocity.X += (float)Entity.GROUND_FRICTION * collisionObject.Friction * frictionMultiplier;
                else
                    velocity.X = 0;
            }
            // ->Air friction
            else if (!onGround && !blockFriction)
            {
                if (velocity.X > 0.05)
                    velocity.X -= (float)Entity.AIR_FRICTION;
                else if (velocity.X < -0.05)
                    velocity.X += (float)Entity.AIR_FRICTION;
                else
                    velocity.X = 0;
            }

            // ->Slope movement (Slide Trigger, Upwards movement)
            if (onSlope)
            {
                frictionMultiplier = 0.2f;
                SlopeObject slopeObject = collisionObject.GetSlopeObject();
                if (GetState() != EntityState.SLIDE)
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
                    if (Math.Abs(velocity.X) > 2)
                    {
                        if (!slopeObject.direction && velocity.X < 0)
                            velocity.X += (float)slopeObject.incline * inclineExhaust;
                        else if (slopeObject.direction && velocity.X > 0)
                            velocity.X -= (float)slopeObject.incline * inclineExhaust;
                    }
                }
                if (GetState() != EntityState.SLIDE && ((slopeObject.direction && velocity.X <= 0.005) || (!slopeObject.direction && velocity.X >= -0.005)) && slopeObject.incline >= 0.5)
                {
                    direction = slopeObject.direction;
                    ChangeState(EntityState.SLIDE);
                    blockDefaultMovementInput = true;
                }
            }

            // ->Slide movement (On slope, first ground contact)
            if (GetState() == EntityState.SLIDE)
            {
                if (onSlope)
                {
                    SlopeObject slopeObject = collisionObject.GetSlopeObject();
                    float slideMultiplier = 1.5f;
                    if (horizontalInput && inputDirection != slopeObject.direction)
                    {
                        slideMultiplier = 2.0f;
                        animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                    else
                    {
                        animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                    if (slopeObject.direction)
                    {
                        velocity.X -= (float)slopeObject.incline * 0.1f * slideMultiplier;
                    }
                    else
                    {
                        velocity.X += (float)slopeObject.incline * 0.1f * slideMultiplier;
                    }
                }
                else
                {
                    if (lastSlopeObject != null && collisionObject != lastSlopeObject)
                    {
                        lastSlopeObject = null;
                        frictionMultiplier = 0.2f;
                        animator.SwitchState(PlayerAnimations.SLIDE);
                    }
                }
            }

            // ->Slide exit (jump, slope jump)
            if (GetState() == EntityState.SLIDE && InputManager.JumpButton.OnPress())
            {
                if (lastSlopeObject == null)
                {
                    // Jump normally, give control
                    velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.0f));
                    jumpWindow.Start();
                    ChangeState(EntityState.JUMP);
                    blockDefaultMovementInput = false;
                }
                else
                {
                    // Jump on slope is only possible when fast + not-high incline OR last 20px of any slope
                    if (((Math.Abs(velocity.X) >= 4.0f && lastSlopeObject.incline < 1.5f) || GetMovementPoint().Y > lastSlopeObject.position.Y + lastSlopeObject.size.Y - 20) && velocity.Y >= 0)
                    {
                        // Jump slightly higher, small X vel boost
                        velocity.X *= 1.1f;
                        velocity += new Vector2(0, (float)-(Entity.JUMP_FORCE * 1.2f));
                        jumpWindow.Start();
                        ChangeState(EntityState.JUMP);
                        blockDefaultMovementInput = false;
                    }
                }
            }

            // ->Friction type check
            if (GetState() != EntityState.SLIDE && !onSlope && frictionMultiplier == 0.2f)
                frictionMultiplier = 1.0f;



            if (velocity.X > Entity.MAX_HORIZONTAL_VEL)
                velocity.X = (float)Entity.MAX_HORIZONTAL_VEL;
            else if (velocity.X < -Entity.MAX_HORIZONTAL_VEL)
                velocity.X = (float)-Entity.MAX_HORIZONTAL_VEL;
            if (velocity.Y > Entity.MAX_HORIZONTAL_VEL)
                velocity.Y = (float)Entity.MAX_HORIZONTAL_VEL;
            else if (velocity.Y < -Entity.MAX_VERTICAL_VEL)
                velocity.Y = (float)-Entity.MAX_VERTICAL_VEL;
            #endregion

            _onGround = onGround;
            onGround = false;
            onSlope = false;

            CollisionCheck();

            #region Player - Movement Assistance
            #endregion

            #region Player - Entity state updates
            // change direction
            if (velocity.X < -0.2 && onGround && !blockDir)
            {
                direction = false;
            }
            else if (velocity.X > 0.2 && onGround && !blockDir)
            {
                direction = true;
            }


            // change entity state
            if (Math.Abs(velocity.X) > 0.5 && Math.Abs(velocity.X) < Entity.MIN_RUNNING_VEL + 0.5f && GetState() != EntityState.WALK && onGround && !blockDefaultMovementInput && !crouch)
            {
                ChangeState(EntityState.WALK);
            }
            else if (Math.Abs(velocity.X) > Entity.MIN_RUNNING_VEL + 0.5f && onGround && GetState() != EntityState.RUNNING && !blockDefaultMovementInput && !crouch)
            {
                ChangeState(EntityState.RUNNING);
            }
            else if (Math.Abs(velocity.X) <= 0.5 && onGround && GetState() != EntityState.IDLE && !blockDefaultMovementInput && !crouch)
            {
                ChangeState(EntityState.IDLE);
            }
            else if (velocity.Y > 2 && !onGround && !diving && !blockDefaultMovementInput && GetState() != EntityState.FALL && !crouch && GetState() == EntityState.SIDEFLIP)
            {
                direction = !direction;
                ChangeState(EntityState.FALL);
            }
            else if (velocity.Y > 2 && !onGround && !diving && !blockDefaultMovementInput && GetState() != EntityState.FALL && !crouch)
            {
                ChangeState(EntityState.FALL);
            }
            else if (onGround && !blockDefaultMovementInput && crouch && GetState() != EntityState.CROUCH)
            {
                ChangeState(EntityState.CROUCH);
            }
            

            wasPressedJump = Keyboard.GetState().IsKeyDown(Keys.Up);
            wasPressedCrouch = Keyboard.GetState().IsKeyDown(Keys.Down);
            #endregion
        }

        public void CollisionCheck()
        {
            // 1. Apply physics, save old position
            Vector2 oldPos = position;
            Vector2 oldPosF = GetMovementPoint();
            Vector2 oldPosH = GetHeadPoint();
            if (!freezePlayer)
                position += velocity;
            // 2. Loop through solid objects
            foreach (MapObject obj in Game1.mapObjects)
            {
                if (!obj.collision)
                    continue;

                // 3. Check player collision with rectangle (Feet and head)
                if (obj.type == SolidObject.Rectangle)
                {
                    Rectangle collisionRect = new Rectangle(obj.position.ToPoint(), obj.size.ToPoint());
                    bool _feetC = Maths.PointInRectangle(GetMovementPoint(), collisionRect);
                    bool _headC = Maths.PointInRectangle(GetHeadPoint(), collisionRect);
                    Rectangle playerBodyCollider = new Rectangle(GetHeadPoint().ToPoint(), (new Vector2(1, size.Y)).ToPoint());
                    if (playerBodyCollider.Intersects(collisionRect) || _feetC || _headC)
                    {
                        // Determine penetration depth (based on velocity vector and rectangle bounds)
                        Vector2 _pushBack = new Vector2(0, 0);
                        if (oldPosF.Y <= collisionRect.Top) // Fell on the rect
                        {
                            OnCollision(CollisionType.FLOOR, obj);
                            collisionObject = obj;
                            _pushBack.Y += collisionRect.Top - GetMovementPoint().Y; // push back up by distance from feet to top
                        }
                        else if (oldPosH.Y >= collisionRect.Bottom) // Hit rect with head (from under)
                        {
                            OnCollision(CollisionType.CEILING, obj);
                            _pushBack.Y += collisionRect.Bottom - GetHeadPoint().Y; // push back down by distance from head to bottom
                        }
                        else if (oldPosF.X <= collisionRect.Left) // Hit rect from left
                        {
                            OnCollision(CollisionType.WALL_RIGHT, obj);
                            _pushBack.X += collisionRect.Left - GetMovementPoint().X;
                        }
                        else if (oldPosF.X >= collisionRect.Right) // Hit rect from right
                        {
                            OnCollision(CollisionType.WALL_LEFT, obj);
                            _pushBack.X += collisionRect.Right - GetMovementPoint().X;
                        }
                        else // Feet or Head was already inside rect (no movement) ==> try to push in nearest direction out of rectangle
                        {

                            float _dB = collisionRect.Top - GetMovementPoint().Y;
                            float _dT = collisionRect.Bottom - GetHeadPoint().Y;
                            float _dR = collisionRect.Left - GetMovementPoint().X;
                            float _dL = collisionRect.Right - GetMovementPoint().X;
                            if ((Math.Abs(_dB) <= Math.Abs(_dR) && Math.Abs(_dB) <= Math.Abs(_dL)) || (Math.Abs(_dT) <= Math.Abs(_dR) && Math.Abs(_dT) <= Math.Abs(_dL)))
                            {
                                if (_feetC && !_headC)
                                    _pushBack.Y = _dB;
                                else if (!_feetC && _headC)
                                    _pushBack.Y = _dT;
                                else
                                {
                                    if (Math.Abs(_dB) <= Math.Abs(_dT))
                                        _pushBack.Y = _dB;
                                    else
                                        _pushBack.Y = _dT;
                                }
                            }
                            else
                            {
                                if (Math.Abs(_dR) <= Math.Abs(_dL))
                                    _pushBack.X = _dR;
                                else
                                    _pushBack.X = _dL;
                            }

                        }
                        if (!freezePlayer)
                            position += _pushBack;
                    }
                }
                else if (obj.type == SolidObject.Slope)
                {
                    SlopeObject slope = obj.GetSlopeObject();
                    bool _feetC = Maths.PointInTriangle(GetMovementPoint(), slope.GetVertices()[0], slope.GetVertices()[1], slope.GetVertices()[2]);
                    bool _headC = Maths.PointInTriangle(GetHeadPoint(), slope.GetVertices()[0], slope.GetVertices()[1], slope.GetVertices()[2]);
                    bool _bodyC = GetHeadPoint().Y < slope.position.Y + slope.size.Y && GetMovementPoint().Y > slope.position.Y + slope.size.Y && GetMovementPoint().X >= slope.position.X && GetMovementPoint().X < slope.position.X + slope.size.X;
                    if (_bodyC || _feetC || _headC)
                    {
                        Vector2 _pushBack = new Vector2(0, 0);
                        if (slope.direction)
                        {
                            if (oldPosF.Y < slope.position.Y + slope.size.Y && oldPosF.X <= slope.position.X + slope.size.X || oldPosF.Y <= slope.position.Y && oldPosF.X >= slope.position.X + slope.size.X)
                            { // Landed on slope
                                OnCollision(CollisionType.FLOOR_SLOPE, obj);
                                collisionObject = obj;
                                _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                            }
                            else if (oldPosH.Y >= slope.position.Y + slope.size.Y) // Hit slope from bottom (with head)
                            {
                                OnCollision(CollisionType.CEILING, obj);
                                _pushBack.Y = slope.position.Y + slope.size.Y - GetHeadPoint().Y;
                            }
                            else if (oldPosF.X >= slope.position.X + slope.size.X) // Hit slope from right side
                            {
                                OnCollision(CollisionType.WALL_LEFT, obj);
                                _pushBack.X = slope.position.X + slope.size.X - GetMovementPoint().X;
                            }
                            else if (oldPosF.X <= slope.position.X) // hit slope from left side
                            {
                                OnCollision(CollisionType.WALL_RIGHT, obj);
                                _pushBack.X = slope.position.X - GetMovementPoint().X;
                            }
                            else
                            {// Feet or Head was already inside slope (no movement) ==> try to push in nearest direction out of slope
                                float _dB = Vector2.Distance(Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()), GetMovementPoint());
                                float _dT = slope.position.Y + slope.size.Y - GetHeadPoint().Y;
                                float _dL = slope.position.X + slope.size.X - GetMovementPoint().X;
                                if (Math.Abs(_dB) <= Math.Abs(_dL) || Math.Abs(_dT) <= Math.Abs(_dL))
                                {
                                    if (_feetC && !_headC)
                                        _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                                    else if (!_feetC && _headC)
                                        _pushBack.Y = _dT;
                                    else
                                    {
                                        if (Math.Abs(_dB) <= Math.Abs(_dT))
                                            _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                                        else
                                            _pushBack.Y = _dT;
                                    }
                                }
                                else
                                {
                                    _pushBack.X = _dL;
                                }
                            }
                        }
                        else
                        {
                            if (oldPosF.Y < slope.position.Y + slope.size.Y && oldPosF.X > slope.position.X || oldPosF.Y <= slope.position.Y && oldPosF.X <= slope.position.X)
                            { // Landed on slope
                                OnCollision(CollisionType.FLOOR_SLOPE, obj);
                                collisionObject = obj;
                                _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                            }
                            else if (oldPosH.Y >= slope.position.Y + slope.size.Y) // Hit slope from bottom (with head)
                            {
                                OnCollision(CollisionType.CEILING, obj);
                                _pushBack.Y = slope.position.Y + slope.size.Y - GetHeadPoint().Y;
                            }
                            else if (oldPosF.X <= slope.position.X) // Hit slope from left side
                            {
                                OnCollision(CollisionType.WALL_RIGHT, obj);
                                _pushBack.X = slope.position.X - GetMovementPoint().X;
                            }
                            else if (oldPosF.X >= slope.position.X + slope.size.X) // hit slope from right side
                            {
                                OnCollision(CollisionType.WALL_LEFT, obj);
                                _pushBack.X = slope.position.X + slope.size.X - GetMovementPoint().X;
                            }
                            else
                            {// Feet or Head was already inside slope (no movement) ==> try to push in nearest direction out of slope
                                float _dB = Vector2.Distance(Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()), GetMovementPoint());
                                float _dT = slope.position.Y + slope.size.Y - GetHeadPoint().Y;
                                float _dR = slope.position.X - GetMovementPoint().X;
                                if (Math.Abs(_dB) <= Math.Abs(_dR) || Math.Abs(_dT) <= Math.Abs(_dR))
                                {
                                    if (_feetC && !_headC)
                                        _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                                    else if (!_feetC && _headC)
                                        _pushBack.Y = _dT;
                                    else
                                    {
                                        if (Math.Abs(_dB) <= Math.Abs(_dT))
                                            _pushBack = Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], GetMovementPoint()) - GetMovementPoint();
                                        else
                                            _pushBack.Y = _dT;
                                    }
                                }
                                else
                                {
                                    _pushBack.X = _dR;
                                }
                            }
                        }
                        if (!freezePlayer)
                            position += _pushBack;
                    }
                }
            }
        }

        public void OnCollision(CollisionType type, MapObject obj)
        {
            switch (type)
            {
                case CollisionType.WALL_RIGHT:
                    if (!_onGround)
                    {
                        // Ledge grab
                        if (obj.size.Y > 20 && obj.size.X > 20 && direction && GetHeadPoint().Y < obj.position.Y && GetMovementPoint().Y > obj.position.Y && Math.Abs(GetHeadPoint().Y - obj.position.Y) < 5 && Math.Abs(GetMovementPoint().X - obj.position.X) < 1 && Math.Abs(velocity.X) < 5 && InputManager.HorizontalInput > 0 && MapObject.GetObjectFromPos(new Vector2(obj.position.X + (size.X/2), obj.position.Y - size.Y)) == null && MapObject.GetObjectFromPos(new Vector2(obj.position.X + (size.X/2), obj.position.Y - 1)) == null && !diving && !postDive && !postDiveRoll && !kicking && !punching && !crouch)
                        {
                            ledgeGrabDirection = true;
                            ledgeGrab = true;
                            ledgeGrabWindow.Start();
                            ledgeGrabObject = obj;
                            ChangeState(EntityState.LEDGE);
                            blockDefaultMovementInput = true;
                            freezePlayer = true;
                            gravityMultiplier = 0.0f;
                            position.Y = obj.position.Y - 20;
                            position.X = obj.position.X - (size.X / 2);
                            velocity.Y = 0;
                            velocity.X = 0;
                        }
                        else if (Math.Abs(velocity.X) > 2 && GetMovementPoint().Y < obj.position.Y + obj.size.Y)
                        {
                            wallJumpSpeed = Math.Abs(velocity.X);
                            velocity.Y = -1;
                            velocity.X = 0;
                            blockDefaultMovementInput = true;
                            blockGravity = true;
                            wallJump = true;
                            wallJumpDirection = true;
                            wallJumpFrame = 0;
                            wallJumpWindow.Start();
                        }
                        else if (Math.Abs(velocity.X) > 3.5f && GetMovementPoint().Y >= obj.position.Y + obj.size.Y)
                        {
                                ChangeState(EntityState.BONK);
                                bonked = true;
                                bonkWindow.Start();
                                _particleBonk.EmitterLocation = Camera.ConvertPos(GetMovementPoint());
                                _particleBonk.SpawnParticles();
                                blockDefaultMovementInput = true;
                                AddActionExhaust(0, 1500);
                                velocity.X = -3;
                                velocity.Y = -1;
                        }
                        else
                            velocity.X = 0;
                    }
                    else
                    {
                        // Auto-step system
                        Vector2 futureHeadPoint = GetHeadPoint() + (obj.position - GetMovementPoint());
                        if (GetMovementPoint().Y > obj.position.Y && Math.Abs(GetMovementPoint().Y - obj.position.Y) <= AutoStepLimit && MapObject.GetObjectFromPos(futureHeadPoint, true) == null)
                        {
                            position += (obj.position - GetMovementPoint());
                            velocity.X *= 0.5f;
                        }
                        else
                        {
                            velocity.X = 0;
                        }
                    }
                    break;
                case CollisionType.WALL_LEFT:
                    if (!_onGround)
                    {
                        if (obj.size.Y > 20 && obj.size.X > 20 && !direction && GetHeadPoint().Y < obj.position.Y && GetMovementPoint().Y > obj.position.Y && Math.Abs(GetHeadPoint().Y - obj.position.Y) < 5 && Math.Abs(GetMovementPoint().X - (obj.position.X + obj.size.X)) < 1 && Math.Abs(velocity.X) < 5 && InputManager.HorizontalInput < 0 && MapObject.GetObjectFromPos(new Vector2(obj.position.X + obj.size.X - (size.X / 2), obj.position.Y - size.Y)) == null && MapObject.GetObjectFromPos(new Vector2(obj.position.X + obj.size.X - (size.X / 2), obj.position.Y - 1)) == null && !diving && !postDive && !postDiveRoll && !kicking && !punching && !crouch)
                        {
                            ledgeGrabDirection = false;
                            ledgeGrab = true;
                            ledgeGrabWindow.Start();
                            ledgeGrabObject = obj;
                            ChangeState(EntityState.LEDGE);
                            blockDefaultMovementInput = true;
                            freezePlayer = true;
                            gravityMultiplier = 0.0f;
                            position.Y = obj.position.Y - 20;
                            position.X = obj.position.X + obj.size.X - (size.X / 2);
                            velocity.Y = 0;
                            velocity.X = 0;
                        }
                        else if (Math.Abs(velocity.X) > 2 && GetMovementPoint().Y < obj.position.Y + obj.size.Y)
                        {
                            wallJumpSpeed = Math.Abs(velocity.X);
                            velocity.Y = -1;
                            velocity.X = 0;
                            blockDefaultMovementInput = true;
                            blockGravity = true;
                            wallJump = true;
                            wallJumpDirection = false;
                            wallJumpFrame = 0;
                            wallJumpWindow.Start();
                            _particleBonk.EmitterLocation = Camera.ConvertPos(GetMovementPoint());
                            _particleBonk.SpawnParticles();
                        }
                        else if (Math.Abs(velocity.X) > 3.5 && GetMovementPoint().Y >= obj.position.Y + obj.size.Y)
                        {
                            ChangeState(EntityState.BONK);
                            bonked = true;
                            bonkWindow.Start();
                            _particleBonk.EmitterLocation = Camera.ConvertPos(GetMovementPoint());
                            _particleBonk.SpawnParticles();
                            blockDefaultMovementInput = true;
                            AddActionExhaust(0, 1500);
                            velocity.X = 3;
                            velocity.Y = -1;
                        }
                        else
                            velocity.X = 0;
                    }
                    else
                    {
                        // Auto-step system
                        Vector2 futureHeadPoint = GetHeadPoint() + (new Vector2(obj.position.X + obj.size.X, obj.position.Y) - GetMovementPoint());
                        if (GetMovementPoint().Y > obj.position.Y && Math.Abs(GetMovementPoint().Y - obj.position.Y) <= AutoStepLimit && MapObject.GetObjectFromPos(futureHeadPoint, true) == null)
                        {
                            position += (new Vector2(obj.position.X + obj.size.X, obj.position.Y) - GetMovementPoint());
                            velocity.X *= 0.5f;
                        }
                        else
                        {
                            velocity.X = 0;
                        }
                    }
                    break;
                case CollisionType.CEILING:
                    velocity.Y = 0;
                    break;
                case CollisionType.FLOOR_SLOPE:
                    onGround = true;
                    velocity.Y = 0;
                    break;
                case CollisionType.FLOOR: // LAND
                default:
                    if (pounding)
                        poundSpeed = velocity.Y;
                    if (diving)
                        diveHitSpeed = velocity.X;
                    if (!_onGround)
                    {
                        if (jumpSequence == 0)
                        {
                            landWindow.Start();
                            jumpSequence = 1;
                        }
                        else if (jumpSequence == 2)
                        {
                            landWindow.Start();
                            jumpSequence = 3;
                        }
                    }
                    onGround = true;
                    canKick = true;
                    velocity.Y = 0;
                    break;
            }
        }


        public void Draw(SpriteBatch spriteBatch, Texture2D defaultTex)
        {
            if (Game1.ENTITY_HITBOX)
            {
                // czerwona otoczka hitboxa ( debug )
                spriteBatch.Draw(defaultTex, Camera.ConvertRect(new Rectangle((int)position.X, (int)position.Y, (int)size.X, 1)), Color.Red);
                spriteBatch.Draw(defaultTex, Camera.ConvertRect(new Rectangle((int)position.X, (int)position.Y, 1, (int)size.Y)), Color.Red);
                spriteBatch.Draw(defaultTex, Camera.ConvertRect(new Rectangle((int)position.X, (int)position.Y + (int)size.Y, (int)size.X, 1)), Color.Red);
                spriteBatch.Draw(defaultTex, Camera.ConvertRect(new Rectangle((int)position.X + (int)size.X, (int)position.Y, 1, (int)size.Y + 1)), Color.Red);
                // kropka w miejscu Movement Pointa
                spriteBatch.Draw(defaultTex, Camera.ConvertRect(new Rectangle((int)GetMovementPoint().X, (int)GetMovementPoint().Y, 1, 1)), Color.Yellow);
            }
            SpriteEffects effect = SpriteEffects.None;
            if (!direction)
                effect = SpriteEffects.FlipHorizontally;
            Texture2D t = SpriteManager.GetEntityTexture(spriteID);
            Vector2 textureSize = new Vector2(t.Width * scale.X, t.Height * scale.Y);
            spriteBatch.Draw(t, Camera.ConvertRect(new Rectangle((int)position.X - (int)((textureSize.X - size.X) / 2), (int)position.Y - (int)(textureSize.Y - size.Y), (int)textureSize.X, (int)textureSize.Y)), new Rectangle(0,0,t.Width,t.Height),Color.White,
                0f,new Vector2(0,0),effect,0f);
        }


    }
}
