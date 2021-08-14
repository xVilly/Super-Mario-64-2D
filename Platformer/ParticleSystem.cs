using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Platformer
{
    public struct ParticleTemplate
    {
        public List<Texture2D> textures;
        public Vector2 velocity;
        public float speed;
        public float angularVelocity;
        public Color color;
        public float minSize;
        public float maxSize;
        public int time;
        public int count;
        public bool gravity;
        public bool collision;
    }

    public static class ParticleTemplates
    {
        public static ParticleTemplate bonk = new ParticleTemplate() {
            textures = new List<Texture2D>() { SpriteManager.GetParticleTexture(1), SpriteManager.GetParticleTexture(2) },
            velocity = Vector2.Zero,
            speed = 2.0f,
            angularVelocity = 1.0f,
            color = Color.Yellow,
            minSize = 0.5f,
            maxSize = 1.0f,
            time = 10,
            count = 25,
            gravity = true,
            collision = false
        };

        public static ParticleTemplate runningDust = new ParticleTemplate()
        {
            textures = new List<Texture2D>() { SpriteManager.GetParticleTexture(0) },
            velocity = Vector2.Zero,
            speed = 1.5f,
            angularVelocity = 1.0f,
            color = Color.Gray,
            minSize = 0.9f,
            maxSize = 1.4f,
            time = 7,
            count = 5,
            gravity = true,
            collision = false
        };

    }
    public class Particle
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public int TTL { get; set; }
        public bool Gravity { get; set; }
        public bool Collision { get; set; }

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, int ttl, bool gravity, bool collision)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            TTL = ttl;
            Gravity = gravity;
            Collision = collision;
        }

        public void Update()
        {
            TTL--;
            Position += Velocity;
            Angle += AngularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            spriteBatch.Draw(Texture, Position, sourceRectangle, Color,
                Angle, origin, Size, SpriteEffects.None, 0f);
        }
    }

    public class ParticleController
    {
        public bool Active = true;
        public bool SpawnUpdate = false;
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        public ParticleTemplate CurrentTemplate;
        private List<Particle> particles;
        

        public ParticleController(Vector2 location)
        {
            EmitterLocation = location;
            this.particles = new List<Particle>();
            random = new Random();
            GameWorld.particleControllers.Add(this);
        }

        public void Update()
        {
            if (!Active)
                return;
            int total = CurrentTemplate.count;

            if (SpawnUpdate)
            {
                for (int i = 0; i < total; i++)
                {
                    particles.Add(GenerateNewParticle(CurrentTemplate));
                }
            }

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void SpawnParticles()
        {
            if (!Active)
                return;
            int total = CurrentTemplate.count;
            for (int i = 0; i < total; i++)
            {
                particles.Add(GenerateNewParticle(CurrentTemplate));
            }
        }

        private Particle GenerateNewParticle(ParticleTemplate template)
        {
            Texture2D texture = template.textures[random.Next(template.textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity;
            if (template.velocity != Vector2.Zero)
                velocity = template.velocity;
            else
                velocity = new Vector2(
                                    template.speed * (float)(random.NextDouble() * 2 - 1),
                                    template.speed * (float)(random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = template.angularVelocity * 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = template.color;
            float size = (float)Math.Max(template.minSize, random.NextDouble() * template.maxSize);
            int ttl = template.time + random.Next(template.time);

            return new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl, template.gravity, template.collision);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Active)
                return;
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
        }
    }
}
