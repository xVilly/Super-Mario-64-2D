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
    // - Level System
    // Levels are map regions loaded directly to RAM from game files (one level loaded at a time)
    // This class holds data of current loaded level
    public static class Level
    {
        private static bool loaded;

        // Level Name - must be same as filename
        public static string Name;

        public static string Author;

        public static string Date;

        public static string Editor;

        public static Vector2 StartPoint;

        public static Skybox SkyBox;

        public static bool IsLoaded()
        {
            return loaded;
        }
        public static void Load()
        {
            loaded = true;
            SkyBox = new Skybox(Color.CornflowerBlue, 0);
        }
        public static void Unload()
        {
            loaded = false;
            GameWorld.solidObjects.Clear();
            GameWorld.mapSlopes.Clear();
            GameWorld.mapRectangles.Clear();
        }
    }
}
