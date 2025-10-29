using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KomaruPlatformer;

// Base class for game objects
public class Sprite
{
    public Texture2D texture { get; protected set; }
    public Texture2D[] frames { get; protected set; }
    public Vector2 position { get; protected set; }
    public Vector2 scale { get; protected set; }

    public Sprite(Texture2D texture, Vector2 position, Vector2 scale)
    {
        this.texture = texture;
        this.position = position;
        this.scale = scale;
    }

    public Sprite(Texture2D[] frames, Vector2 position, Vector2 scale)
    {
        this.frames = frames;
        this.position = position;
        this.scale = scale;
    }
}