using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KomaruPlatformer;

public class MapLoader
{
    private string[] mapInStrings;
    private char[,] map;
    private readonly Vector2 blockSize;
    public int mapHorizontalOffset = 0;
    private Block winBlock;

    public Block ReturnWinBlock
    {
        get
        {
            return winBlock;
        }
    }

    private Dictionary<char, Texture2D> charToTexture;

    public MapLoader(string mapPath, Texture2D[] textures, Vector2 blockSize)
    {
        // Getting map from map.txt
        mapInStrings = File.ReadAllLines(mapPath);

        // Converting to char array
        map = new char[mapInStrings.Length, mapInStrings[0].Length];

        // Writing map to console
        for (int y = 0; y < mapInStrings.Length; y++)
        {
            for (int x = 0; x < mapInStrings[0].Length; x++)
            {
                map[y, x] = mapInStrings[y][x];
            }
        }

        // Getting blocks textures
        charToTexture = new Dictionary<char, Texture2D>
        {
            {'@', textures[0]},
            {'%', textures[1]},
            {'#', textures[2]},
            {'&', textures[3]},
            {'*', textures[4]},
            {'$', textures[5]}
        };

        this.blockSize = blockSize;
    }

    public void DrawMap(SpriteBatch spriteBatch)
    {
        // Position where map drawer will draw block
        var drawPos = new Vector2(350 + mapHorizontalOffset, 0);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Y coordinate
        for (int y = 0; y < map.GetLength(0); y++)
        {
            // X coordinate
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // Getting char by coordinates
                char mapChar = map[y, x];

                // if space return
                if (mapChar == ' ')
                {
                    drawPos.X += blockSize.X;
                    continue;
                }

                // Getting texture from dictionary by char and drawing
                foreach ((char dictionaryChar, Texture2D texture) in charToTexture)
                {
                    spriteBatch.Draw
                    (
                        charToTexture[mapChar],
                        new Rectangle
                        (
                            (int)drawPos.X, (int)drawPos.Y,
                            (int)blockSize.X, (int)blockSize.Y
                        ),
                        Color.White
                    );
                    break;
                }

                // Updating X position
                drawPos.X += blockSize.X;
            }

            // Updating Y position
            drawPos.Y += blockSize.Y;
            drawPos.X = 350 + mapHorizontalOffset;
        }

        spriteBatch.End();
    }

    public List<Block> GetBlocks()
    {
        // List to return
        List<Block> tmpList = new List<Block>();

        // Block position
        var blockPos = new Vector2(350 + mapHorizontalOffset, 0);

        // Y coordinate
        for (int y = 0; y < map.GetLength(0); y++)
        {
            // X coordinate
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // Getting char by coordinates
                char mapChar = map[y, x];

                // Trying get texture
                Texture2D texture;
                bool canGetTexture = charToTexture.TryGetValue(mapChar, out texture);

                // If cant continue
                if (!canGetTexture)
                {
                    blockPos.X += blockSize.X;
                    continue;
                }

                // Adding to list
                if (mapChar != '*' && mapChar != '$')
                {
                    tmpList.Add(new Block(texture, blockPos, blockSize));
                }
                else if (mapChar == '*')
                {
                    winBlock = new Block(texture, blockPos, blockSize);
                }

                // Updating X position
                blockPos.X += blockSize.X;
            }

            // Updating Y position
            blockPos.Y += blockSize.Y;
            blockPos.X = 350 + mapHorizontalOffset;
        }

        return tmpList;
    }
}