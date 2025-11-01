using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KomaruPlatformer;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Game
    private const string GAME_VERSION = "v0.0.2";
    private Scenes currentScene = Scenes.Menu;
    private MapLoader mapLoader;

    // Menu scene
    private Button[] menuButtons = new Button[3];
    private int currentButton = 0;
    private bool pressedWLastFrame = false;
    private bool pressedSLastFrame = false;

    // Game scene
    private Player player;
    private List<Block> blocksList = new List<Block>();
    public Block winBlock { get; private set; }

    // About scene
    private Texture2D monoGameTexture;

    // Text
    private SpriteFont arial;
    private SpriteFont bigArial;
    private bool learnedTutorial = false;
    private bool underMap = false;
    public bool congratulations = false;

    // FPS Counting
    private int fps = 60;
    private int tmpFps = 0;
    private const float FPS_UPDATE_TIME = 1f;
    private float timeToUpdateFps = FPS_UPDATE_TIME;

    // Sounds
    private Song song;
    private SoundEffectInstance congratulationsSoundInstance;
    private bool playedCongrSound = false;

    // Camera imitation
    public int mapHorizontalOffset = 0;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        Window.Title = $"Komaru Platformer - {fps} FPS";
        _graphics.PreferredBackBufferHeight = 650;
        _graphics.PreferredBackBufferWidth = 1200;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        // Getting button frames
        var buttonFrames = new Texture2D[2];
        for (int i = 0; i < buttonFrames.Length; i++)
        {
            buttonFrames[i] = Content.Load<Texture2D>($"UI/Button{i}");
        }

        // Creating variables for buttons
        int buttonPositionX = 450;
        var buttonScale = new Vector2(300, 150);
        int buttonYSpace = 150;
        int currentYSpace = 0;

        // Creating buttons
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var buttonText = string.Empty;
            int textXSpacing = 0;

            switch (i)
            {
                case 0:
                    buttonText = "Play";
                    textXSpacing = 85;
                    break;
                case 1:
                    buttonText = "About";
                    textXSpacing = 60;
                    break;
                case 2:
                    buttonText = "Quit";
                    textXSpacing = 85;
                    break;
            }

            menuButtons[i] = new Button
            (
                frames: buttonFrames,
                position: new Vector2(buttonPositionX, 150 + currentYSpace),
                scale: buttonScale,
                buttonNumber: i + 1,
                game: this, text: buttonText,
                textXSpacing: textXSpacing
            );

            currentYSpace += buttonYSpace;
        }

        // Adding blocks
        var blocksTexturesArray = new Texture2D[6];

        for (int i = 0; i < blocksTexturesArray.Length; i++)
        {
            blocksTexturesArray[i] = Content.Load<Texture2D>($"Blocks/Block{i}");
        }

        // Loading map
        mapLoader = new MapLoader
        (
            mapPath: "Content/map.txt",
            textures: blocksTexturesArray,
            blockSize: new Vector2(50, 50)
        );

        blocksList = mapLoader.GetBlocks();
        winBlock = mapLoader.ReturnWinBlock;

        // Getting player frames
        var playerFrames = new Texture2D[3];
        for (int i = 0; i < playerFrames.Length; i++)
        {
            playerFrames[i] = Content.Load<Texture2D>($"Komaru/Komaru{i}");
        }

        // Getting player sounds
        var playerSounds = new SoundEffect[2];
        playerSounds[0] = Content.Load<SoundEffect>("Sounds/Step");
        playerSounds[1] = Content.Load<SoundEffect>("Sounds/Jump");

        // Creating a player
        player = new Player
        (
            frames: playerFrames,
            position: new Vector2
            (_graphics.PreferredBackBufferWidth / 2 - 50, 100),
            scale: new Vector2(100, 100),
            speed: 10, jumpForce: 15,
            sounds: playerSounds,
            blocksList: blocksList,
            game: this
        );

        // Loading the fonts
        arial = Content.Load<SpriteFont>("Fonts/Arial");
        bigArial = Content.Load<SpriteFont>("Fonts/BigArial");

        // Loading sounds
        song = Content.Load<Song>("Sounds/Song");
        congratulationsSoundInstance = Content.Load<SoundEffect>
        ("Sounds/Congratulations").CreateInstance();
        congratulationsSoundInstance.Volume = 0.5f;

        // Starting song playing
        MediaPlayer.Volume = 0.25f;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);

        // Loading MonoGame texture
        monoGameTexture = Content.Load<Texture2D>("UI/MonoGame");
    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here

        // Getting keyboard state
        var keyboardState = Keyboard.GetState();

        // Game Scene
        if (currentScene == Scenes.Game)
        {
            // Checking for exit
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                currentScene = Scenes.Menu;
            }

            // Checking for learned tutorial
            if (keyboardState.IsKeyDown(Keys.A) ||
                keyboardState.IsKeyDown(Keys.D) ||
                keyboardState.IsKeyDown(Keys.Space))
            {
                learnedTutorial = true;
            }

            // Updating blocks list
            blocksList = mapLoader.GetBlocks();
            winBlock = mapLoader.ReturnWinBlock;
            player.blocksList = blocksList;

            player.Update(gameTime);
            mapLoader.mapHorizontalOffset = mapHorizontalOffset;

            // Checking for player under the map
            if (player.position.Y > _graphics.PreferredBackBufferHeight)
            {
                congratulations = false;
                underMap = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                underMap = false;
                congratulations = false;
                playedCongrSound = false;
            }

            // Checking for congratulations and plating sound
            if (congratulations && !playedCongrSound)
            {
                playedCongrSound = true;
                congratulationsSoundInstance.Play();
            }
        }
        else if (currentScene == Scenes.Menu)
        {
            // Updating buttons
            foreach (Button button in menuButtons)
            {
                button.Update(currentButton);
            }

            // Getting W & S keys for buttons switching
            if (keyboardState.IsKeyDown(Keys.W) && !pressedWLastFrame)
            {
                pressedWLastFrame = true;
                currentButton--;
                if (currentButton < 0) currentButton = 2;
            }
            else if (keyboardState.IsKeyUp(Keys.W))
            {
                pressedWLastFrame = false;
            }

            if (keyboardState.IsKeyDown(Keys.S) && !pressedSLastFrame)
            {
                pressedSLastFrame = true;
                currentButton++;
                if (currentButton > 2) currentButton = 0;
            }
            else if (keyboardState.IsKeyUp(Keys.S))
            {
                pressedSLastFrame = false;
            }

            // On clicking
            if (keyboardState.IsKeyDown(Keys.E))
            {
                switch (currentButton)
                {
                    case 0:
                        currentScene = Scenes.Game;
                        break;
                    case 1:
                        currentScene = Scenes.About;
                        break;
                    case 2:
                        Exit();
                        break;
                }
            }
        }
        else if (currentScene == Scenes.About)
        {
            // Checking for exit
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                currentScene = Scenes.Menu;
            }
        }

        // Counting FPS
        if (timeToUpdateFps <= 0)
        {
            fps = tmpFps;
            tmpFps = 0;
            timeToUpdateFps = FPS_UPDATE_TIME;
            Window.Title = $"Komaru Platformer - {fps} FPS";
        }
        else
        {
            timeToUpdateFps -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            tmpFps++;
        }
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Drawing game scene
        if (currentScene == Scenes.Game)
        {
            mapLoader.DrawMap(_spriteBatch);

            _spriteBatch.Begin();

            // Drawing player
            _spriteBatch.Draw
            (
                player.frames[player.currentFrame], player.rectangle,
                null, Color.White, 0f, Vector2.Zero,
                player.flip, 0f
            );

            // Drawing text
            if (!learnedTutorial)
            {
                _spriteBatch.DrawString
                (
                    arial, "Use A & D for move and SPACE for jump",
                    new Vector2(250, 20), Color.White
                );
            }

            if (underMap)
            {
                _spriteBatch.DrawString
                (
                    arial, "Press R for restart",
                    new Vector2(420, 20), Color.White
                );
            }

            if (congratulations)
            {
                _spriteBatch.DrawString
                (
                    arial, "Congratulations!",
                    new Vector2(450, 20), Color.White
                );
            }

            _spriteBatch.End();
        }
        // Drawing menu scene
        else if (currentScene == Scenes.Menu)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.DrawString
            (
                bigArial, "Komaru Platformer",
                new Vector2(300, 20), Color.White
            );

            for (int i = 0; i < menuButtons.Length; i++)
            {
                var button = menuButtons[i];

                _spriteBatch.Draw
                (
                    button.frames[button.currentFrame],
                    button.rectangle, Color.White
                );

                _spriteBatch.DrawString
                (
                    bigArial, button.text,
                    button.textPosition, Color.White
                );
            }

            _spriteBatch.DrawString
            (
                arial, "use W & S to navigate and E to use button",
                new Vector2(180, 620), Color.White
            );

            _spriteBatch.End();
        }
        // Drawing about scene
        else if (currentScene == Scenes.About)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.DrawString
            (
                arial, "Press ESC to exit this menu",
                new Vector2(10, 10), Color.White
            );

            _spriteBatch.DrawString
            (
                bigArial, "About Komaru Platformer",
                new Vector2(200, 100), Color.White
            );

            _spriteBatch.DrawString
            (
                bigArial, "Made by Obed",
                new Vector2(400, 250), Color.White
            );

            _spriteBatch.DrawString
            (
                bigArial, "Powered by MonoGame",
                new Vector2(250, 320), Color.White
            );

            _spriteBatch.Draw
            (
                monoGameTexture, new Rectangle
                (
                    525, 410, 150, 150
                ), Color.White
            );

            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}
