using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OptimizedSpriteBatch
{
    public class Sprite
    {
        public VertexPositionTexture[] Vertices = new VertexPositionTexture[4];
        public short[] Indices = new short[6];

        public Sprite(SpriteBatchOptimized batch, Vector2 position, Vector2 origin, float rotation, Vector2 texCoord)
        {
            texCoord = new Vector2(texCoord.X * batch.RatioX, texCoord.Y * batch.RatioY);

            Vector3 __right = new Vector3(1, 0, 0);
            Vector3 __top = new Vector3(0, 1, 0);
            Vector3 __topRight = new Vector3(1, 1, 0);

            __right = Vector3.Transform(__right, Matrix.CreateRotationZ(rotation));
            __top = Vector3.Transform(__top, Matrix.CreateRotationZ(rotation));
            __topRight = Vector3.Transform(__topRight, Matrix.CreateRotationZ(rotation));


            Vertices[0] = new VertexPositionTexture(new Vector3(position.X, position.Y, 0), new Vector2(0 + texCoord.X, batch.RatioY + texCoord.Y));
            Vertices[1] = new VertexPositionTexture(new Vector3(position.X, position.Y, 0) + __right, new Vector2(batch.RatioX + texCoord.X, batch.RatioY + texCoord.Y));
            Vertices[2] = new VertexPositionTexture(new Vector3(position.X, position.Y, 0) + __topRight, new Vector2(batch.RatioX + texCoord.X, 0 + texCoord.Y));
            Vertices[3] = new VertexPositionTexture(new Vector3(position.X, position.Y, 0) + __top, new Vector2(0 + texCoord.X, 0 + texCoord.Y));


            Indices[0] = (int)(0);
            Indices[1] = (int)(1);
            Indices[2] = (int)(2);

            Indices[3] = (int)(2);
            Indices[4] = (int)(3);
            Indices[5] = (int)(0);

        }
    }
}
