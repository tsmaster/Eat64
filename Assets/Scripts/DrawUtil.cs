using System;
using UnityEngine;

namespace BDG
{
    public class DrawUtil
    {
        public DrawUtil ()
        {
        }

        static public void DrawSpriteOpaque (Texture2D srcTexture, Texture2D destTexture, 
            int spriteX, int spriteY, int spriteWidth, int spriteHeight,
            int destX, int destY)
        {
            for (int x = 0; x < spriteWidth; ++x) {
                int fromX = spriteX + x;
                int toX = destX + x;
                for (int y = 0; y < spriteHeight; ++y) {
                    int fromY = spriteY + y;
                    int toY = destY + y;
                    destTexture.SetPixel (toX, toY, srcTexture.GetPixel (fromX, fromY));
                }
            }
        }

        static public void DrawSpriteAlpha (Texture2D srcTexture, Texture2D destTexture,
            int spriteX, int spriteY, int spriteWidth, int spriteHeight,
            int destX, int destY)
        {
            for (int x = 0; x < spriteWidth; ++x) {
                int fromX = spriteX + x;
                int toX = destX + x;
                for (int y = 0; y < spriteHeight; ++y) {
                    int fromY = spriteY + y;
                    int toY = destY + y;

                    var fromPix = srcTexture.GetPixel (fromX, fromY);
                    if (fromPix.a < 0.5f) {
                        continue;
                    }
                    destTexture.SetPixel (toX, toY, fromPix);
                }
            }
        }

        internal static void SetPixel (Texture2D displayTexture, int px, int py, Color drawColor)
        {
            displayTexture.SetPixel (px, py, drawColor);
        }

        internal static void FillTexture (Texture2D displayTexture, Color color)
        {
            Color [] colors = new Color [64 * 64];

            for (int x = 0; x < 64; ++x) {
                for (int y = 0; y < 64; ++y) {
                    colors [64 * y + x] = color;
                }
            }
            displayTexture.SetPixels (colors);
        }
    }
}
