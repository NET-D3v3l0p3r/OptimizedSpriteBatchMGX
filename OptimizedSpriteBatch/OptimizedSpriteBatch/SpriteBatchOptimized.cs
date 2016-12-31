//---1---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Drawing;

namespace OptimizedSpriteBatch
{
    public class SpriteBatchOptimized
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public int AbsoluteTextureSizeWidth { get; private set; }
        public int AbsoluteTextureSizeHeight { get; private set; }

        public float RatioX { get; private set; }
        public float RatioY { get; private set; }

        private Bitmap _internalTextureAtlas = null;
        private Texture2D _internalTextureAtlas2D = null;

        private int _maxLine = 1024;
        private int _x, _y;

        private Dictionary<Texture2D, Vector2> _textureAtlas = new Dictionary<Texture2D, Vector2>();
        private List<Sprite> _spriteBuffer = new List<Sprite>();
        private Dictionary<string, int> _vectorBuffer = new Dictionary<string, int>();

       
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private BasicEffect _effect;

        private int _toDraw;

        private Matrix _globalMatrix;

        private bool _beginCalled;
        private bool _needsFlush;
        private bool _clearCache;

        public SpriteBatchOptimized(GraphicsDevice _device, int _textureWidth, int _textureHeight, bool _clear = true)
        {
            GraphicsDevice = _device;

            AbsoluteTextureSizeWidth = _textureWidth;
            AbsoluteTextureSizeHeight = _textureHeight;

            _effect = new BasicEffect(GraphicsDevice);
            this._clearCache = _clear;
        }

        /// <summary>
        ///  ADD TETXURE TO TEXTURE ATLAS
        /// </summary>
        /// <param name="_position"></param>
        /// <param name="_manipulation"></param>
        /// <param name="_texture"></param>
        /// <returns></returns>
        public void AddTexture(Texture2D _texture)
        {
            if (_texture.Width != AbsoluteTextureSizeWidth || _texture.Height != _texture.Height)
                throw new Exception("Pushed texture width and height must be identicaly to absolute width and height!");

            if (_textureAtlas.ContainsKey(_texture))
                throw new Exception("Texture already pushed!");

            if (_internalTextureAtlas == null)
            {
                _internalTextureAtlas = new Bitmap(_texture.Width, _texture.Height);

                Graphics _g = Graphics.FromImage(_internalTextureAtlas);
                _g.DrawImage(ToolKit.FromTexture2D(_texture), new PointF(0, 0));

            }
            else
            {
                Bitmap __copy = _internalTextureAtlas;

                if (_x + _texture.Width > _maxLine)
                {
                    _internalTextureAtlas = new Bitmap(_texture.Width + _internalTextureAtlas.Width, _texture.Height + _internalTextureAtlas.Height);
                    _x = 0;
                    _y += AbsoluteTextureSizeHeight;
                }
                else
                {
                    _internalTextureAtlas = new Bitmap(_texture.Width + _internalTextureAtlas.Width, _internalTextureAtlas.Height);
                    _x += AbsoluteTextureSizeWidth;
                }

                Graphics _g = Graphics.FromImage(_internalTextureAtlas);
                _g.DrawImage(__copy, new PointF(0, 0));
                _g.DrawImage(ToolKit.FromTexture2D(_texture), new PointF(_x, _y));

                

            }
            RatioX = (float)AbsoluteTextureSizeWidth / (float)_internalTextureAtlas.Width;
            RatioY = (float)AbsoluteTextureSizeHeight / (float)_internalTextureAtlas.Height;

            _textureAtlas.Add(_texture, new Vector2(_x / AbsoluteTextureSizeWidth , _y  / AbsoluteTextureSizeHeight));
        }

        /// <summary>
        /// CALL THIS IN LOAD METHOD
        /// </summary>
        /// <param name="_position"></param>
        /// <param name="_manipulation"></param>
        /// <param name="_texture"></param>
        public void AddSprite(Texture2D _texture, string _name, Vector2 _initialPosition, Vector2 _origin, float _rotation)
        {
            _vectorBuffer.Add(_name, _spriteBuffer.Count * 4);
            _spriteBuffer.Add(new Sprite(this, -1 * (_initialPosition / new  Vector2(AbsoluteTextureSizeWidth, AbsoluteTextureSizeHeight)), _origin, _rotation, _textureAtlas[_texture]));
            _needsFlush = true;
        }

        public void Edit(Texture2D _texture, string _name, Vector2 _position, Vector2 _origin, float _rotation)
        {
            __internalUpdate(_name, _position, _origin, _rotation, _texture);

        }

        private void __internalUpdate(string _name, Vector2 _newPosition, Vector2 _origin, float _rotation, Texture2D _texture)
        {
            int __index = _vectorBuffer[_name];
            Sprite __sprite = new Sprite(this, -1 * (_newPosition / new Vector2(AbsoluteTextureSizeWidth, AbsoluteTextureSizeHeight)), _origin, _rotation, _textureAtlas[_texture]);
            _vertexBuffer.SetData<VertexPositionTexture>(VertexPositionTexture.VertexDeclaration.VertexStride * __index, __sprite.Vertices, 0, 4, VertexPositionTexture.VertexDeclaration.VertexStride);
        }
      
        public void Begin(Matrix _manipulation)
        {

            if (!_beginCalled)
                _beginCalled = true;
            else throw new Exception("Call end()!");
            if (_needsFlush)
            {
                _needsFlush = false;
                __internalAllocateBuffers();
            }
            _globalMatrix = _manipulation;
        }

        public void End()
        {
            if (_beginCalled)
                _beginCalled = false;
            else throw new Exception("Call begin()!");

            __internalDraw(_globalMatrix);
        }

        /// <summary>
        /// CALL THIS IN LOAD METHOD
        /// </summary>
        private void __internalAllocateBuffers()
        {
            int __offset = 0;
            int __vertexOffset = 0;

            VertexPositionTexture[] __vertices;
            int[] __indices;

            __vertices = new VertexPositionTexture[_spriteBuffer.Count * 4];
            __indices = new int[_spriteBuffer.Count * 6];

            for (int i = 0; i < _spriteBuffer.Count; i++)
            {

                for (int j = 0; j < 4; j++)
                {
                    if (_spriteBuffer[i] != null)
                        __vertices[j + __offset] = _spriteBuffer[i].Vertices[j];
                }
                __offset += 4;
            }
            __offset = 0;
            for (int i = 0; i < _spriteBuffer.Count; i++)
            {

                for (int j = 0; j < 6; j++)
                {
                    if (_spriteBuffer[i] != null)
                        __indices[j + __offset] = _spriteBuffer[i].Indices[j] + __vertexOffset;
                }
                __offset += 6;
                __vertexOffset += 4;
            }


            _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), __vertices.Length, BufferUsage.None);
            _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, __indices.Length, BufferUsage.WriteOnly);

            _toDraw = _spriteBuffer.Count;

            _vertexBuffer.SetData(__vertices);
            _indexBuffer.SetData(__indices);

            _internalTextureAtlas2D = GetTextureAtlas();

            if (_clearCache)
                _spriteBuffer.Clear();

            __vertices = new VertexPositionTexture[0];
            __indices = new int[0];

            GC.Collect();
        }
        private void __internalDraw(Matrix _manipulation)
        {

            if (_vertexBuffer == null)
                return;

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _effect.Texture = _internalTextureAtlas2D;
            _effect.TextureEnabled = true;

            Vector2 __center;
            __center.X = GraphicsDevice.Viewport.Width * 0.5f;
            __center.Y = GraphicsDevice.Viewport.Height * 0.5f;

            _effect.View = Matrix.CreateLookAt(new Vector3(__center, 0), new Vector3(__center, 1), new Vector3(0, 1, 0)); ;
            _effect.World =( Matrix.CreateScale(AbsoluteTextureSizeWidth, AbsoluteTextureSizeHeight, 1) * Matrix.CreateTranslation(GraphicsDevice.Viewport.Width - (AbsoluteTextureSizeWidth), GraphicsDevice.Viewport.Height - (AbsoluteTextureSizeHeight), 0)) * _manipulation;
            _effect.Projection = Matrix.CreateOrthographic(__center.X * 2, __center.Y * 2, -0.5f, 1000);  
            _effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _toDraw * 2);
        }

        public void ClearCache()
        {
            _spriteBuffer.Clear();
            _vectorBuffer.Clear();

            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();

            GC.Collect();
        }


        public Texture2D GetTextureAtlas()
        {
            return ToolKit.FromBitmap(GraphicsDevice, _internalTextureAtlas);
        }
        
    }
}
