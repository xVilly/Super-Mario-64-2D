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

        // Author
        public static string Author;

        // Date
        public static string Date;

        // Editor
        public static string Editor;

        // Player Default Spawn Position
        public static Vector2 StartPoint;

        public static bool IsLoaded()
        {
            return loaded;
        }
        public static void Load()
        {
            loaded = true;
        }
        public static void Unload()
        {
            loaded = false;
            Game1.mapObjects.Clear();
            Game1.mapSlopes.Clear();
            Game1.mapRectangles.Clear();
        }
    }
}
