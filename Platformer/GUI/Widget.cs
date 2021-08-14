using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer.GUI
{
    public abstract class Widget
    {
        private static int _ID = 0;

        private int ID;
        public int GetID { get { return ID; } }

        private string name;
        public string Name { get { return name; } }
        public void Rename(string _arg) { name = _arg; }

        protected Widget parent;
        public Widget Parent { get { return parent; } }

        protected List<Widget> children;
        public List<Widget> Children { get { return children; } }

        private Vector2 position;
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 LocalPosition { get { if (parent != null) return parent.Position - position; else return Vector2.Zero; } }

        private bool visible = true;
        public bool IsVisible { get { return visible; } }

        private bool enabled = true;
        public bool IsEnabled { get { return enabled; } }

        public Widget()
        {
            ID = _ID;
            _ID++;

            name = "widget" + ID;
            children = new List<Widget>();
            position = Vector2.Zero;

            GUIManager.ActiveWidgets.Add(this);
        }

        public virtual void Destroy()
        {
            GUIManager.ActiveWidgets.Remove(this);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (children.Count > 0)
                foreach (Widget c in children)
                    c.Draw(spriteBatch);
        }

        public virtual void SetParent(Widget _newParent)
        {
            if (parent != null)
                parent.children.Remove(this);
            parent = _newParent;
            if (_newParent == null)
                return;
            _newParent.children.Add(this);
        }

        public virtual void AddChild(Widget _child)
        {
            if (_child == null)
                return;
            _child.parent = this;
            children.Add(_child);
        }

        public virtual void RemoveChild(Widget _child)
        {
            if (_child == null)
                return;
            _child.parent = null;
            children.Remove(_child);
        }

        public virtual void SetVisible(bool _arg, bool _ignore_children = false)
        {
            visible = _arg;
            if (!_ignore_children){
                foreach (Widget child in children)
                    SetVisible(_arg);
            }
        }

        public virtual void SetEnabled(bool _arg, bool _ignore_children = false)
        {
            enabled = _arg;
            if (!_ignore_children)
            {
                foreach (Widget child in children)
                    SetEnabled(_arg);
            }
        }

        public void PushFront()
        {
            GUIManager.ActiveWidgets.Remove(this);
            GUIManager.ActiveWidgets.Add(this);
        }
        public void PushBack()
        {
            GUIManager.ActiveWidgets.Remove(this);
            GUIManager.ActiveWidgets.Insert(0, this);
        }
        public void PushFront(Widget _widget)
        {
            if (_widget == null)
                return;
            GUIManager.ActiveWidgets.Remove(this);
            GUIManager.ActiveWidgets.Insert(GUIManager.ActiveWidgets.IndexOf(_widget)+1, this);
        }
        public void PushBack(Widget _widget)
        {
            if (_widget == null)
                return;
            GUIManager.ActiveWidgets.Remove(this);
            GUIManager.ActiveWidgets.Insert(GUIManager.ActiveWidgets.IndexOf(_widget) - 1, this);
        }
    }
}
