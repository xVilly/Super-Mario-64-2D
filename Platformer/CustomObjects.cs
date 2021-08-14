using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;
using System.Diagnostics;

namespace Platformer
{
    public class CoinObject : EntityObject
    {
        public CoinObject(string _Name, Vector2 _Position) : base(_Name, _Position)
        {
            Size = new Vector2(20, 20);
            Hitbox = new Hitbox(20);
            Texture = SpriteManager.GetObjectTexture(0);
            Collision = true;
            Gravity = true;
        }
        public override void OnPlayerCollision()
        {
        }
    }
}
