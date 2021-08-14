using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Platformer
{
    public class SoundManager
    {
        private static string[] soundEffects =
        {
        };

        public static List<SoundEffect> soundEffectsList = new List<SoundEffect>();

        public static void LoadSoundEffects(GameManager game)
        {
            foreach (string str in soundEffects)
            {
                soundEffectsList.Add(game.Content.Load<SoundEffect>("Sound/Effects/" + str));
            }
        }

        public static void PlaySoundEffect(int id)
        {
            var instance = soundEffectsList[id].CreateInstance();
            instance.Play();
        }

    }
}
