using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Drawing;
using System.IO;

namespace OptimizedSpriteBatch
{
    public static class ToolKit
    {

        public static Bitmap FromTexture2D(Texture2D _original)
        {
            MemoryStream ms = new MemoryStream();
            _original.SaveAsPng(ms, _original.Width, _original.Height);  
            return (Bitmap)Bitmap.FromStream(ms);
        }

        public static Texture2D FromBitmap(GraphicsDevice _device, Bitmap _original)
        {
            MemoryStream ms = new MemoryStream();
            _original.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return Texture2D.FromStream(_device, ms);
            
        }

        public static Texture2D ResizeTexture2D(GraphicsDevice _device, Texture2D _original, int _w, int _h)
        {
            Bitmap _surface = new Bitmap(_w, _h);

            Graphics _g = Graphics.FromImage(_surface);
            _g.DrawImage(FromTexture2D(_original), new RectangleF(0, 0, _surface.Width, _surface.Height));
            _g.Dispose();
            return FromBitmap(_device, _surface);
        }

        public static bool ContainsFaster<TKey, TValue>(this Dictionary<TKey, TValue> _dictionary, TKey _key)
        {
            return _dictionary.TryGetValue(_key, out var _outValue);
        }
    }
}
