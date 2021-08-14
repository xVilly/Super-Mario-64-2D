using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer.Helpers;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Platformer
{
    // .lev FILE STRUCTURE
    // SECTION 0: file signature
    // SECTION 1: META
    //  - level name, author, date, editor
    // SECTION 2: default spawn position
    //  - x, y
    // SECTION 3: solid map objects
    // 27, 37 = EOF

    public struct SolidObjectInfo
    {
        public SolidObjectType type;
        public Vector2 position;
        public Vector2 size;
        public Int16 textureId;
        public bool direction;
        public bool collision;
    }
    public static class MapManager
    {
        private static string DefaultDirectory = Directory.GetCurrentDirectory();
        private static byte[] FileSignature = { 2, 8, 16, 4, 9, 86, 22, 11, 4, 73, 11, 4, 7, 76, 26, 28, 1, 9, 76, 12, 15, 8, 7, 18, 89, 11, 15, 2, 2, 9, 12 };
        private static byte[] SubSeparator = { 13, 14, 5, 8, 12, 19, 22, 18, 25, 22 };
        private static byte[] DetailSeparator = { 13, 14, 18, 2, 2, 1, 19, 34, 7 };
        public static void Load(string path)
        {
            string FileDir = path;
            if (!File.Exists(FileDir))
            {
                Debug.WriteLine("[MapManager::Load] Level file not found.");
                return;
            }
            Level.Unload();
            byte[] fileData;
            try
            {
                fileData = File.ReadAllBytes(FileDir);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
            if (fileData.Length > 0)
            {
                // Prepare vars
                string level_name = "unknown";
                string author = "unknown";
                string date = "unknown";
                string editor = "unknown";
                Vector2 defaultPos = Vector2.Zero;
                // Check file signature
                if (fileData.Length < FileSignature.Length || !Maths.SubArray<byte>(fileData, 0, FileSignature.Length).SequenceEqual<byte>(FileSignature))
                {
                    Debug.WriteLine("[MapManager::Load] File signature not supported.");
                    return;
                }
                int currentSection = 0;
                int currentSubSection = 0;
                int currentLength = 0;
                int solidObjectOffset = 0;
                byte[] result;
                for (int i = FileSignature.Length; i < fileData.Length; i++)
                {
                    if (Maths.ArraySplitterFound(fileData, SubSeparator, i))
                    {
                        currentSection++;
                        currentSubSection = 0;
                        currentLength = 0;
                    }
                    else if (Maths.ArraySplitterFound(fileData, DetailSeparator, i))
                    {
                        result = Maths.SubArray<byte>(fileData, i - currentLength - 1, currentLength - 1);

                        if (currentSection == 1 && currentSubSection == 0)
                            level_name = System.Text.Encoding.ASCII.GetString(result);
                        else if (currentSection == 1 && currentSubSection == 1)
                            author = System.Text.Encoding.ASCII.GetString(result);
                        else if (currentSection == 1 && currentSubSection == 2)
                            date = System.Text.Encoding.ASCII.GetString(result);
                        else if (currentSection == 1 && currentSubSection == 3)
                            editor = System.Text.Encoding.ASCII.GetString(result);
                        else if (currentSection == 2 && currentSubSection == 0)
                            defaultPos.X = BitConverter.ToInt32(result);
                        else if (currentSection == 2 && currentSubSection == 1)
                            defaultPos.Y = BitConverter.ToInt32(result);
                        else if (currentSection == 3 && result.Length >= 22) // solid map objects
                        {
                            byte[] _type = Maths.SubArray<byte>(result, solidObjectOffset + 0, 2); // type: 2 byte
                            byte[] _posx = Maths.SubArray<byte>(result, solidObjectOffset + 2, 4); // posX: 4 bytes
                            byte[] _posy = Maths.SubArray<byte>(result, solidObjectOffset + 6, 4); // posY: 4 bytes
                            byte[] _sizex = Maths.SubArray<byte>(result, solidObjectOffset + 10, 4); // sizeX: 4 bytes
                            byte[] _sizey = Maths.SubArray<byte>(result, solidObjectOffset + 14, 4); // sizeY: 4 bytes
                            byte[] _texture = Maths.SubArray<byte>(result, solidObjectOffset + 18, 2); // texture: 2 bytes
                            byte[] _direction = Maths.SubArray<byte>(result, solidObjectOffset + 20, 1); // direction: 1 byte
                            byte[] _collision = Maths.SubArray<byte>(result, solidObjectOffset + 21, 1); // collision: 1 byte
                            // Create new solid object info
                            SolidObjectInfo info;
                            info.type = (SolidObjectType)BitConverter.ToInt16(_type);
                            info.position = new Vector2(BitConverter.ToInt32(_posx), BitConverter.ToInt32(_posy));
                            info.size = new Vector2(BitConverter.ToInt32(_sizex), BitConverter.ToInt32(_sizey));
                            info.textureId = BitConverter.ToInt16(_texture);
                            info.direction = BitConverter.ToBoolean(_direction);
                            info.collision = BitConverter.ToBoolean(_collision);
                            if (info.type == SolidObjectType.Rectangle)
                            {
                                RectObject rect = new RectObject(info.position, info.size);
                                rect.texture = SpriteManager.GetMapTexture(info.textureId);
                                rect.collision = info.collision;
                            }
                            else if (info.type == SolidObjectType.Slope)
                            {
                                SlopeObject slope = new SlopeObject(info.position, info.size, info.direction);
                                slope.texture = SpriteManager.GetMapTexture(info.textureId);
                                slope.collision = info.collision;
                            }
                        }

                        currentSubSection++;
                        currentLength = 0;
                    }
                    else
                    {
                        currentLength++;
                    }
                }
                // Apply everything
                if (!Level.IsLoaded())
                {
                    Level.Name = level_name;
                    Level.Author = author;
                    Level.Date = "unknown";
                    Level.Editor = editor;
                    Level.StartPoint = defaultPos;
                    Level.Load();
                }
            }
            else
            {
                Debug.WriteLine("[MapManager::Load] Level file corrupted.");
                return;
            }
        }
    }
}
