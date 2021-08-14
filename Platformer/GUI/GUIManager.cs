using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer.GUI
{
    public static class GUIManager
    {
        public static List<Widget> ActiveWidgets = new List<Widget>();

        public static void Update()
        {
        }

        public static void DrawWidgets(SpriteBatch spriteBatch)
        {
            foreach(Widget widget in ActiveWidgets)
            {
                if (widget.Parent == null)
                    continue;
                widget.Draw(spriteBatch);
            }
        }

        public static Widget GetWidgetByID(int _id)
        {
            return ActiveWidgets.Find(x => x.GetID == _id);
        }
        public static Widget GetWidgetByName(string _name)
        {
            return ActiveWidgets.Find(x => x.Name == _name);
        }
    }
}
