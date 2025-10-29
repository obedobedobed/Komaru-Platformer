using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KomaruPlatformer;

public class Player : Sprite
{
    private bool looksRight = true;
    private Direction direction = Direction.No;
    public readonly int speed;
    private float jumpForce;
    private readonly float originalJumpForce;
    private Vector2 playerPos;
    private readonly Vector2 originalPlayerPos;
    private const int SPEED_MOD = 25;
    private const float GRAVITY = -9.81f;
    private const float FRAME_TIME = 0.2f;
    private const float JUMP_TIME = 0.5f;
    private float timeToFrame = FRAME_TIME;
    private float timeToJumpEnd = JUMP_TIME;
    private float gravityMod = 0.1f;
    private readonly float originalGravityMod;
    public int currentFrame { get; private set; } = 0;
    public SpriteEffects flip { get; private set; }
    private bool isGrounded = false;
    private bool isJumping = false;
    public List<Block> blocksList = new List<Block>();
    private const float STEP_SOUND_TIME = 0.5f;
    private float timeToStepSound = 0;
    private SoundEffectInstance stepsSoundInstance;
    private SoundEffectInstance jumpSoundInstance;
    private bool pressedRLastFrame = false;
    private Game1 game;

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

    public Player(Texture2D[] frames, Vector2 position, Vector2 scale, int speed, int jumpForce,
    SoundEffect[] sounds, List<Block> blocksList, Game1 game) : base(frames, position, scale)
    {
        this.speed = speed;
        this.jumpForce = jumpForce;
        this.game = game;

        originalJumpForce = jumpForce;

        playerPos = position;
        originalPlayerPos = position;

        this.blocksList = blocksList;

        originalGravityMod = gravityMod;

        flip = (!looksRight) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        stepsSoundInstance = sounds[0].CreateInstance();
        jumpSoundInstance = sounds[1].CreateInstance();
        jumpSoundInstance.Volume = 0.4f;
    }

    public void Update(GameTime gameTime)
    {
        Gravity(gameTime);
        Animation(gameTime);
        GetInput(gameTime);
        if (isJumping) Jump(gameTime);

        position = playerPos;

        if (game.winBlock.rectangle.Intersects(rectangle))
        {
            game.congratulations = true;
        }
    }

    private void GetInput(GameTime gameTime)
    {
        direction = Direction.No;

        KeyboardState keyboardState = Keyboard.GetState();

        // Checking for A or D pressed and changing direction
        if (keyboardState.IsKeyDown(Keys.D))
        {
            looksRight = true;
            direction = Direction.Right;
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            looksRight = false;
            direction = Direction.Left;
        }

        // Moving if direction isnt NO
        if (direction != Direction.No)
        {
            Move(gameTime);
        }
        else timeToStepSound = 0;

        // Checking for SPACE pressed and jumping
        if (keyboardState.IsKeyDown(Keys.Space) && isGrounded)
        {
            jumpSoundInstance.Play();
            isJumping = true;
            isGrounded = false;
        }

        // Restart
        if (keyboardState.IsKeyDown(Keys.R) && !pressedRLastFrame)
        {
            gravityMod = originalGravityMod;
            pressedRLastFrame = true;
            playerPos = originalPlayerPos;
            game.mapHorizontalOffset = 0;
        }
        else if (keyboardState.IsKeyUp(Keys.R))
        {
            pressedRLastFrame = false;
        }
    }

    private void Move(GameTime gameTime)
    {
        // Checking for flip
        CheckAndFlip();

        int toMove = 0;

        // Checking direction
        switch (direction)
        {
            case Direction.Right:
                toMove = (int)(speed * SPEED_MOD * gameTime.ElapsedGameTime.TotalSeconds);
                break;
            case Direction.Left:
                toMove = (int)(speed * SPEED_MOD * gameTime.ElapsedGameTime.TotalSeconds) * -1;
                break;
        }

        // Making tmp rectangle to check collisions
        Rectangle tmpRect = new Rectangle
        (
            (int)playerPos.X + toMove, (int)playerPos.Y,
            (int)scale.X, (int)scale.Y
        );

        // Checking collisions
        foreach (Block block in blocksList)
        {
            if (block.intersectionCollider.Intersects(tmpRect))
            {
                toMove = 0;
            }
        }

        // Sounds
        if (isGrounded)
        {
            if (timeToStepSound <= 0)
            {
                stepsSoundInstance.Play();
                stepsSoundInstance.Pitch = Random.Shared.Next(8, 14) / 10;
                timeToStepSound = STEP_SOUND_TIME;
            }
            else timeToStepSound -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Moving
        game.mapHorizontalOffset += toMove * -1;
    }

    private void Animation(GameTime gameTime)
    {
        if (!isGrounded)
        {
            // Jump
            currentFrame = 2;
        }
        else
        {
            if (currentFrame == 2) currentFrame = 0;

            if (direction == Direction.No)
            {
                // Idle
                currentFrame = 0;
            }
            else if(direction != Direction.No)
            {
                // Run
                if (timeToFrame <= 0)
                {
                    timeToFrame = FRAME_TIME;
                    switch (currentFrame)
                    {
                        case 0: currentFrame = 1; break;
                        case 1: currentFrame = 0; break;
                    }
                }
                else timeToFrame -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        texture = frames[currentFrame];
    }

    private void Gravity(GameTime gameTime)
    {
        // Calculating move
        int toMove = (int)(GRAVITY * SPEED_MOD * gravityMod * gameTime.ElapsedGameTime.TotalSeconds);

        // Checking for jumping
        if (isJumping) return;

        bool tmpIsGrounded = false;

        // Checking for collisions with ground
        foreach (Block block in blocksList)
        {
            if (block.groundCollider.Intersects(rectangle))
            {
                tmpIsGrounded = true;
                toMove = 0;
                break;
            }
        }

        isGrounded = tmpIsGrounded;

        if (isGrounded)
        {
            gravityMod = originalGravityMod;
            return;
        }

        // Moving
        playerPos.Y -= toMove;
        gravityMod += 0.03f;
    }
    
    private void Jump(GameTime gameTime)
    {
        // Calculating move
        int toMove = (int)(jumpForce * SPEED_MOD * gameTime.ElapsedGameTime.TotalSeconds);
        jumpForce -= 0.3f;

        // Moving
        playerPos.Y -= toMove;

        // Checking for jump time
        if (timeToJumpEnd <= 0)
        {
            isJumping = false;
            timeToJumpEnd = JUMP_TIME;
            jumpForce = originalJumpForce;
        }
        else timeToJumpEnd -= (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    private void CheckAndFlip()
    {
        if (direction == Direction.Left) looksRight = false;
        if (direction == Direction.Right) looksRight = true;

        flip = (!looksRight) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }
}