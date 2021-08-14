using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;

namespace Platformer
{
    public struct Circle
    {
        public Circle(int x, int y, int radius) : this()
        {
            Center = new Point(x, y);
            Radius = radius;
        }

        public Point Center { get; private set; }
        public int Radius { get; private set; }

    }
    public enum HitboxType
    {
        RECTANGLE,
        CIRCLE
    }
    public struct Hitbox
    {
        private Vector2 Position;

        public HitboxType Type;
        public Vector2 Size;
        public float Radius;
        public Hitbox(Vector2 _Size) : this()
        {
            Type = HitboxType.RECTANGLE;
            Size = _Size;
            Position = Vector2.Zero;
        }
        public Hitbox(float _Radius) : this()
        {
            Type = HitboxType.CIRCLE;
            Radius = _Radius;
            Position = Vector2.Zero;
        }
        public void Update(Vector2 _Position)
        {
            Position = _Position;
        }

        public bool Collision(Hitbox _Body)
        {
            if (this.Type == HitboxType.RECTANGLE && _Body.Type == HitboxType.RECTANGLE)
                return Maths.FOverlap(this.Position.X, this.Position.Y, this.Size.X, this.Size.Y, _Body.Position.X, _Body.Position.Y, _Body.Size.X, _Body.Size.Y);
            else if (this.Type == HitboxType.RECTANGLE && _Body.Type == HitboxType.CIRCLE)
                return Maths.FOverlap(_Body.Position.X, _Body.Position.Y, _Body.Radius, this.Position.X, this.Position.Y, this.Size.X, this.Size.Y);
            else if (this.Type == HitboxType.CIRCLE && _Body.Type == HitboxType.RECTANGLE)
                return Maths.FOverlap(this.Position.X, this.Position.Y, this.Radius, _Body.Position.X, _Body.Position.Y, _Body.Size.X, _Body.Size.Y);
            else if (this.Type == HitboxType.CIRCLE && _Body.Type == HitboxType.CIRCLE)
                return Maths.FOverlap(this.Position.X, this.Position.Y, this.Radius, _Body.Position.X, _Body.Position.Y, _Body.Radius);
            return false;
        }
    }
    public abstract class EntityObject
    {
        private const float MAX_VERTICAL_VEL = 50.0f;
        private const float GRAVITY = 0.27f;

        public string Name;
        public Vector2 Position;
        public Vector2 Size = Vector2.Zero;
        public Texture2D Texture = null;
        public Hitbox Hitbox = new Hitbox(Vector2.Zero);
        public Vector2 Velocity;
        public bool Collision = false;
        public bool Gravity = false;
        public Vector2 CollisionOffset;

        private Rectangle drawnRectangle;
        private Vector2 prevPosition;
        private SolidObject collisionObject;
        private Vector2 collisionPoint { get { return Position + CollisionOffset; } }
        private bool onGround = false;
        private bool overlappingPlayer = false;

        public EntityObject(string _Name, Vector2 _Position)
        {
            Name = _Name;
            Position = _Position;

            Velocity = Vector2.Zero;
            drawnRectangle = new Rectangle(_Position.ToPoint(), Size.ToPoint());
            CollisionOffset = new Vector2(Size.X / 2, Size.Y);
            prevPosition = collisionPoint;

            GameWorld.entityObjects.Add(this);
        }

        public virtual void Update()
        {
            CollisionOffset.X = Size.X / 2;
            CollisionOffset.Y = Size.Y;

            if (Gravity){
                if (Velocity.Y < EntityObject.MAX_VERTICAL_VEL)
                    Velocity.Y += EntityObject.GRAVITY;
            }

            prevPosition = collisionPoint;
            Position += Velocity;

            if (Collision) {
                onGround = false;
                foreach (SolidObject obj in GameWorld.solidObjects) {
                    if (!obj.collision)
                        continue;
                    // TODO: Ignore collision check for solids further away than object velocity
                    float _pushBackX = 0, _pushBackY = 0;
                    if (obj.type == SolidObjectType.Rectangle){
                        if (Maths.FPointInRectangle(collisionPoint.X, collisionPoint.Y, obj.position.X, obj.position.Y, obj.size.X, obj.size.Y)){
                            if (prevPosition.Y <= obj.position.Y){
                                OnCollision(CollisionType.FLOOR, obj);
                                collisionObject = obj;
                                _pushBackX += obj.position.Y - collisionPoint.Y;
                            } else if (prevPosition.Y >= obj.position.Y + obj.size.Y) {
                                OnCollision(CollisionType.CEILING, obj);
                                _pushBackX += obj.position.Y + obj.size.Y - collisionPoint.Y;
                            } else if (prevPosition.X <= obj.position.X) {
                                OnCollision(CollisionType.WALL_RIGHT, obj);
                                _pushBackX += obj.position.X - collisionPoint.X;
                            } else if (prevPosition.X >= obj.position.X + obj.size.X) {
                                OnCollision(CollisionType.WALL_LEFT, obj);
                                _pushBackX += obj.position.X + obj.size.X - collisionPoint.X;
                            } else {
                                float _dB = obj.position.Y - collisionPoint.Y;
                                float _dT = obj.position.Y + obj.size.Y - collisionPoint.Y;
                                float _dR = obj.position.X - collisionPoint.X;
                                float _dL = obj.position.X + obj.size.X - collisionPoint.X;
                                if ((Math.Abs(_dB) <= Math.Abs(_dR) && Math.Abs(_dB) <= Math.Abs(_dL)) || (Math.Abs(_dT) <= Math.Abs(_dR) && Math.Abs(_dT) <= Math.Abs(_dL))){
                                    if (Math.Abs(_dB) <= Math.Abs(_dT))
                                        _pushBackY = _dB;
                                    else
                                        _pushBackY = _dT;
                                } else {
                                    if (Math.Abs(_dR) <= Math.Abs(_dL))
                                        _pushBackX = _dR;
                                    else
                                        _pushBackX = _dL;
                                }
                            }
                            Position.X += _pushBackX;
                            Position.Y += _pushBackY;
                        }
                    } else if (obj.type == SolidObjectType.Slope) {
                        SlopeObject slope = obj.GetSlopeObject();
                        if (Maths.PointInTriangle(collisionPoint, slope.GetVertices()[0], slope.GetVertices()[1], slope.GetVertices()[2])) {
                            if (slope.direction) {
                                if (prevPosition.Y < slope.position.Y + slope.size.Y && prevPosition.X <= slope.position.X + slope.size.X || prevPosition.Y <= slope.position.Y && prevPosition.X >= slope.position.X + slope.size.X) {
                                    OnCollision(CollisionType.FLOOR_SLOPE, obj);
                                    collisionObject = obj;
                                    _pushBackX = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).X;
                                    _pushBackY = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).Y;
                                } else if (prevPosition.Y >= slope.position.Y + slope.size.Y) {
                                    OnCollision(CollisionType.CEILING, obj);
                                    _pushBackY = slope.position.Y + slope.size.Y - collisionPoint.Y;
                                } else if (prevPosition.X >= slope.position.X + slope.size.X) {
                                    OnCollision(CollisionType.WALL_LEFT, obj);
                                    _pushBackX = slope.position.X + slope.size.X - collisionPoint.X;
                                } else if (prevPosition.X <= slope.position.X) {
                                    OnCollision(CollisionType.WALL_RIGHT, obj);
                                    _pushBackX = slope.position.X - collisionPoint.X;
                                } else {
                                    float _dB = Vector2.Distance(Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint), collisionPoint);
                                    float _dT = slope.position.Y + slope.size.Y - collisionPoint.Y;
                                    float _dL = slope.position.X + slope.size.X - collisionPoint.X;
                                    if (Math.Abs(_dB) <= Math.Abs(_dL) || Math.Abs(_dT) <= Math.Abs(_dL)){
                                        if (Math.Abs(_dB) <= Math.Abs(_dT)){
                                            _pushBackX = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).X;
                                            _pushBackY = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).Y;
                                        } else
                                            _pushBackY = _dT;
                                    } else
                                        _pushBackX = _dL;
                                }
                            } else {
                                if (prevPosition.Y < slope.position.Y + slope.size.Y && prevPosition.X > slope.position.X || prevPosition.Y <= slope.position.Y && prevPosition.X <= slope.position.X) {
                                    OnCollision(CollisionType.FLOOR_SLOPE, obj);
                                    collisionObject = obj;
                                    _pushBackX = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).X;
                                    _pushBackY = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).Y;
                                } else if (prevPosition.Y >= slope.position.Y + slope.size.Y) {
                                    OnCollision(CollisionType.CEILING, obj);
                                    _pushBackY = slope.position.Y + slope.size.Y - collisionPoint.Y;
                                } else if (prevPosition.X <= slope.position.X) {
                                    OnCollision(CollisionType.WALL_RIGHT, obj);
                                    _pushBackX = slope.position.X - collisionPoint.X;
                                } else if (prevPosition.X >= slope.position.X + slope.size.X) {
                                    OnCollision(CollisionType.WALL_LEFT, obj);
                                    _pushBackX = slope.position.X + slope.size.X - collisionPoint.X;
                                } else {
                                    float _dB = Vector2.Distance(Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint), collisionPoint);
                                    float _dT = slope.position.Y + slope.size.Y - collisionPoint.Y;
                                    float _dR = slope.position.X - collisionPoint.X;
                                    if (Math.Abs(_dB) <= Math.Abs(_dR) || Math.Abs(_dT) <= Math.Abs(_dR)) {
                                        if (Math.Abs(_dB) <= Math.Abs(_dT)) {
                                            _pushBackX = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).X;
                                            _pushBackY = (Maths.GetClosestPointOnLineSegment(slope.GetVertices()[0], slope.GetVertices()[2], collisionPoint) - collisionPoint).Y;
                                        } else
                                            _pushBackY = _dT;
                                    } else
                                        _pushBackX = _dR;
                                }
                            }
                            Position.X += _pushBackX;
                            Position.Y += _pushBackY;
                        }
                    }
                }
            }
            
            drawnRectangle.X = (int)Camera.ConvertPos(Position).X;
            drawnRectangle.Y = (int)Camera.ConvertPos(Position).Y;
            drawnRectangle.Width = (int)Camera.ConvertSize(Size).X;
            drawnRectangle.Height = (int)Camera.ConvertSize(Size).Y;
            overlappingPlayer = false;
            if (this.Hitbox.Collision(GameWorld.Player.Hitbox))
                overlappingPlayer = true;
            if (overlappingPlayer)
                OnPlayerCollision();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, drawnRectangle, Color.White);
        }

        public abstract void OnPlayerCollision();

        public virtual void OnCollision(CollisionType collisionType, SolidObject obj)
        {
            switch (collisionType)
            {
                case CollisionType.CEILING:
                    Velocity.Y = -Velocity.Y;
                    break;
                case CollisionType.WALL_LEFT:
                case CollisionType.WALL_RIGHT:
                    Velocity.X = -Velocity.X;
                    break;
                case CollisionType.FLOOR_SLOPE:
                case CollisionType.FLOOR:
                default:
                    if (Velocity.Y > 0.2f)
                        Velocity.Y *= 0.75f;
                    else
                        Velocity.Y = 0f;
                    onGround = true;
                    break;
            }
        }
    }
}
