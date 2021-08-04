using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Platformer
{
    public static class SpriteManager
    {
        private static string[] EntitySprites =
        {
            "Player/idle-1",
            "Player/idle-2",
            "Player/idle-3",
            "Player/idle-4",

            "Player/walk-1",
            "Player/walk-2",
            "Player/walk-3",
            "Player/walk-4",
            "Player/walk-5",
            "Player/walk-6",
            "Player/walk-7",
            "Player/walk-8",

            "Player/jump",
            "Player/jump2",

            "Player/land",
            "Player/land2",
            "Player/land3",

            "Player/tj1",
            "Player/tj2",
            "Player/tj3",
            "Player/tj4",
            "Player/tj5",
            "Player/tj6",
            "Player/tj7",
            "Player/tj8",

            "Player/dive",

            "Player/pound-1",
            "Player/pound-2",
            "Player/pound-3",
            "Player/pound-4",
            "Player/pound-5",
            "Player/pound-6",
            "Player/pound-7",
            "Player/pound-8",
            "Player/pound-9",
            "Player/pound-10",
            "Player/postpound",

            "Player/roll-1",
            "Player/roll-2",
            "Player/roll-3",
            "Player/roll-4",

            "Player/dirchange",
            "Player/sideflip-1",
            "Player/sideflip-2",
            "Player/sideflip-3",
            "Player/sideflip-4",
            "Player/sideflip-5",
            "Player/sideflip-6",
            "Player/sideflip-7",

            "Player/crouch",
            "Player/crouchwalk-1",
            "Player/crouchwalk-2",
            "Player/crouchwalk-3",
            "Player/crouchwalk-4",
            "Player/crouchwalk-5",
            "Player/crouchwalk-6",
            "Player/crouchwalk-7",

            "Player/slide",
            "Player/slidekick-1",
            "Player/slidekick-2",
            "Player/slidekick-3",
            "Player/slidekick-4",

            "Player/punch-1",
            "Player/punch-2",
            "Player/punch-3",
            "Player/punch-4",
            "Player/punch-5",
            "Player/punch-6",
            "Player/punch-7",
            "Player/punch-8",

            "Player/kick-1",
            "Player/kick-2",
            "Player/kick-3",

            "Player/ledge",
            "Player/ledge-1",
            "Player/ledge-2",

            "Player/run-1",
            "Player/run-2",
            "Player/run-3",
            "Player/run-4",
            "Player/run-5",
            "Player/run-6",
            "Player/run-7",

        };

        // Hardcoded textures
        private static string[] MapTextures =
        {
            "grass",
            "sand"
        };

        private static string[] ParticleTextures =
        {
            "circle",
            "diamond",
            "star",
        };

        private static List<Texture2D> LoadedEntityTextures = new List<Texture2D>();

        private static List<Texture2D> LoadedMapTextures = new List<Texture2D>();

        private static List<Texture2D> LoadedParticleTextures = new List<Texture2D>();

        public static void LoadTextures(Game1 game)
        {
            foreach(string str in EntitySprites)
            {
                LoadedEntityTextures.Add(game.Content.Load<Texture2D>("Sprites/Entities/" + str));
            }
            foreach(string str in MapTextures)
            {
                LoadedMapTextures.Add(game.Content.Load<Texture2D>("Sprites/Map/" + str));
            }
            foreach(string str in ParticleTextures)
            {
                LoadedParticleTextures.Add(game.Content.Load<Texture2D>("Sprites/Particles/" + str));
            }
        }
        
        public static Texture2D GetEntityTexture(int id)
        {
            if (id >= LoadedEntityTextures.Count)
                return null;
            return LoadedEntityTextures[id];
        }

        public static Texture2D GetMapTexture(int id)
        {
            if (id >= LoadedMapTextures.Count)
                return null;
            return LoadedMapTextures[id];
        }

        public static Texture2D GetParticleTexture(int id)
        {
            if (id >= LoadedParticleTextures.Count)
                return null;
            return LoadedParticleTextures[id];
        }
    }
}
