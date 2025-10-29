using System;
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
    private Player player;
    private List<Block> blocksList = new List<Block>();
    public Block winBlock { get; private set; }
    private MapLoader mapLoader;

    // Text
    private SpriteFont arial;
    private const string TUTORIAL_TEXT_MOVING = "Use A & D for move and SPACE for j ump";
    private const string TUTORIAL_TEXT_RESTART = "Press R for restart";
    private const string CONGRATULATIONS_TEXT = "Congratulations!";
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
        IsMouseVisible = true;
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

        // Adding blocks
        var blocksTexturesArray = new Texture2D[6];

        for (int i = 0; i < blocksTexturesArray.Length; i++)
        {
            blocksTexturesArray[i] = Content.Load<Texture2D>($"Blocks/Block{i}");
        }

        // Loading map
        mapLoader = new MapLoader
        (
            mapPath: "map.txt",
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
            position: new Vector2(_graphics.PreferredBackBufferWidth / 2 - 50, 100),
            scale: new Vector2(100, 100),
            speed: 10, jumpForce: 15,
            sounds: playerSounds,
            blocksList: blocksList,
            game: this
        );

        // Loading the font
        arial = Content.Load<SpriteFont>("Fonts/Arial");

        // Loading sounds
        song = Content.Load<Song>("Sounds/Song");
        congratulationsSoundInstance = Content.Load<SoundEffect>("Sounds/Congratulations").CreateInstance();
        congratulationsSoundInstance.Volume = 0.5f;

        // Starting song playing
        MediaPlayer.Volume = 0.25f;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        // Checking for learned tutorial
        if (Keyboard.GetState().IsKeyDown(Keys.A) ||
            Keyboard.GetState().IsKeyDown(Keys.D) ||
            Keyboard.GetState().IsKeyDown(Keys.Space))
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
        
        // Checking for congratulations and plating sound
        if (congratulations && !playedCongrSound)
        {
            playedCongrSound = true;
            congratulationsSoundInstance.Play();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

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
            _spriteBatch.DrawString(arial, TUTORIAL_TEXT_MOVING, new Vector2(300, 20), Color.White);
        }

        if (underMap)
        {
            _spriteBatch.DrawString(arial, TUTORIAL_TEXT_RESTART, new Vector2(470, 20), Color.White);
        }

        if (congratulations)
        {
            _spriteBatch.DrawString(arial, CONGRATULATIONS_TEXT, new Vector2(500, 20), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
