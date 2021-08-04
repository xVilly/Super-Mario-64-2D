using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer
{
    public enum PlayerAnimations
    {
        IDLE,
        WALK,
        JUMP,
        DOUBLEJUMP,
        TRIPLEJUMP,
        FALL,
        FALLDOUBLE,
        FALLTRIPLE,
        POUND,
        POSTPOUND,
        CROUCH,
        CROUCHWALK,
        DIVE,
        SLIDE,
        DIRCHANGE,
        SIDEFLIP,
        ROLL,
        LONGJUMP,
        SLIDEKICK,
        PUNCH,
        DOUBLEPUNCH,
        KICK,
        LEDGE,
        LEDGESTAND,
        RUNNING
    }
    public class Animation
    {
        public int startIndex;
        public int frameCount;
        public int customSpeed;
        public bool loop;
        public Animation(int _startIndex, int _frameCount, int _customSpeed = 0, bool _loop = true)
        {
            startIndex = _startIndex;
            frameCount = _frameCount;
            customSpeed = _customSpeed;
            loop = _loop;
        }
    }
    public class Animator
    {
        public int defaultSpeed;
        private PlayerAnimations state;
        public Entity actor;
        public Animation idle;
        public Animation walk;
        public Animation jump;
        public Animation jump2;
        public Animation triplejump;
        public Animation fall;
        public Animation fall2;
        public Animation fall3;
        public Animation pound;
        public Animation postpound;
        public Animation crouch;
        public Animation crouchwalk;
        public Animation dive;
        public Animation slide;
        public Animation dirchange;
        public Animation sideflip;
        public Animation roll;
        public Animation longjump;
        public Animation slidekick;
        public Animation punch;
        public Animation punch2;
        public Animation kick;
        public Animation ledge;
        public Animation ledgestand;
        public Animation running;

        private Animation current;
        private int speed;
        private long lastUpdate = 0;
        private int currentFrame = 0;
        private bool stop = false;
        public Animator(Entity _actor, int _speed)
        {
            actor = _actor;
            speed = _speed;
            defaultSpeed = _speed;
        }

        public void SwitchState(PlayerAnimations _state)
        {
            state = _state;
            currentFrame = 0;
            lastUpdate = 0;
            stop = false;
            switch (state)
            {
                case PlayerAnimations.IDLE:
                    current = idle;
                    break;
                case PlayerAnimations.WALK:
                    current = walk;
                    break;
                case PlayerAnimations.JUMP:
                    current = jump;
                    break;
                case PlayerAnimations.DOUBLEJUMP:
                    current = jump2;
                    break;
                case PlayerAnimations.TRIPLEJUMP:
                    current = triplejump;
                    break;
                case PlayerAnimations.FALL:
                    current = fall;
                    break;
                case PlayerAnimations.FALLDOUBLE:
                    current = fall2;
                    break;
                case PlayerAnimations.FALLTRIPLE:
                    current = fall3;
                    break;
                case PlayerAnimations.POUND:
                    current = pound;
                    break;
                case PlayerAnimations.POSTPOUND:
                    current = postpound;
                    break;
                case PlayerAnimations.CROUCH:
                    current = crouch;
                    break;
                case PlayerAnimations.CROUCHWALK:
                    current = crouchwalk;
                    break;
                case PlayerAnimations.DIVE:
                    current = dive;
                    break;
                case PlayerAnimations.SLIDE:
                    current = slide;
                    break;
                case PlayerAnimations.DIRCHANGE:
                    current = dirchange;
                    break;
                case PlayerAnimations.SIDEFLIP:
                    current = sideflip;
                    break;
                case PlayerAnimations.ROLL:
                    current = roll;
                    break;
                case PlayerAnimations.LONGJUMP:
                    current = longjump;
                    break;
                case PlayerAnimations.SLIDEKICK:
                    current = slidekick;
                    break;
                case PlayerAnimations.PUNCH:
                    current = punch;
                    break;
                case PlayerAnimations.DOUBLEPUNCH:
                    current = punch2;
                    break;
                case PlayerAnimations.KICK:
                    current = kick;
                    break;
                case PlayerAnimations.LEDGE:
                    current = ledge;
                    break;
                case PlayerAnimations.LEDGESTAND:
                    current = ledgestand;
                    break;
                case PlayerAnimations.RUNNING:
                    current = running;
                    break;
                default:
                    return;
            }
            if (current.customSpeed > 0)
                speed = current.customSpeed;
        }

        public PlayerAnimations GetState() { return state; }

        public void Update()
        {
            if (lastUpdate + speed <= DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)
            {
                lastUpdate = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                if (currentFrame >= current.frameCount)
                {
                    if (current.loop)
                        currentFrame = 0;
                    else
                        stop = true;
                }

                actor.spriteID = current.startIndex + currentFrame;

                if (!stop)
                    currentFrame++;
            }
        }
    }
}
