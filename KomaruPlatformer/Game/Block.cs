using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KomaruPlatformer;

public class Block : Sprite
{
    public Rectangle rectangle
    {
        get
        {
            return new Rectangle
            (
                (int)position.X, (int)position.Y,
                (int)scale.X, (int)scale.Y
            );
        }
    }

    public Rectangle intersectionCollider
    {
        get
        {
            return new Rectangle
            (
                (int)position.X, (int)position.Y + 10,
                (int)scale.X, (int)scale.Y - 5
            );
        }
    }

    public Rectangle groundCollider
    {
        get
        {
            return new Rectangle
            (
                (int)position.X, (int)position.Y,
                (int)scale.X, 5
            );
        }
    }

    public Block(Texture2D texture, Vector2 position, Vector2 scale)
    : base(texture, position, scale)
    {
        
    }
}