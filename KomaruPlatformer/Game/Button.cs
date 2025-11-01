using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KomaruPlatformer;

public class Button : Sprite
{
    private readonly int buttonNumber;
    public int currentFrame { get; private set; } = 0;
    private readonly Game1 game;
    public string text { get; private set; }
    public Vector2 textPosition { get; private set; }
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

    public Button(Texture2D[] frames, Vector2 position, Vector2 scale,
    int buttonNumber, Game1 game, string text, int textXSpacing)
    : base(frames, position, scale)
    {
        this.buttonNumber = buttonNumber;
        this.game = game;
        this.text = text;

        textPosition = new Vector2
        (
            position.X + textXSpacing,
            position.Y + 50
        );
    }

    public void Update(int currentButton)
    {
        currentButton++;

        if (currentButton != buttonNumber)
        {
            currentFrame = 0;
        }
        else
        {
            currentFrame = 1;
        }
    }
}