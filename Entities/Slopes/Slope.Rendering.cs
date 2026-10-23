// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using Celeste.Core.Mathematics;
using Celeste.Core.Rendering;
using Celeste.Graphics.Rendering;
using Celeste.Scene.Tracking;
using Color = Celeste.Core.Display.PackedColor;
using MTexture = Celeste.Graphics.Content.Atlas.AtlasEntry;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    public sealed partial class Slope
    {
        private void DrawTexture(MTexture texture, ISourceRenderContext context, Vector2 at, Vector2 origin,
            Color color)
            => SourceAtlasDrawing.Draw(context, this, texture, at, origin, color);

        private Color GetHue(Vector2 position)
        {
            float num = 280f;
            float value = (position.Length() + ((ISourceScene)Scene).SourceScene.Clock.TimeActive * 50f) % num / num;
            return HsvToColor(0.4f + ScalarMath.YoYo(value) * 0.4f, 0.4f, 0.9f);
        }

        private static Color HsvToColor(float hue, float s, float v)
        {
            int num = (int)(hue * 360f);
            float num2 = s * v;
            float num3 = num2 * (1f - Math.Abs((float)num / 60f % 2f - 1f));
            float num4 = v - num2;
            if (num < 60)
            {
                return new Color(num4 + num2, num4 + num3, num4);
            }

            if (num < 120)
            {
                return new Color(num4 + num3, num4 + num2, num4);
            }

            if (num < 180)
            {
                return new Color(num4, num4 + num2, num4 + num3);
            }

            if (num < 240)
            {
                return new Color(num4, num4 + num3, num4 + num2);
            }

            if (num < 300)
            {
                return new Color(num4 + num3, num4, num4 + num2);
            }

            return new Color(num4 + num2, num4, num4 + num3);
        }

        public void DrawSlopesTiles(ISourceRenderContext context, Vector2 Pos)
        {
            if (!UpsideDown)
            {
                if (Side == "Left")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[i]], context,
                                    Pos = Position + new Vector2(-24f + i * 16f, i * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[i + 1]], context,
                                    Pos = Position + new Vector2(-16f + i * 16f, i * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 2]], context,
                                Pos = Position + new Vector2(-8f + i * 16f, i * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 3]], context,
                                Pos = Position + new Vector2(i * 16f, i * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            DrawTexture(SlopeTextures[0 + variation[j] * 8], context,
                                Pos = Position + new Vector2(8f + j * 16f, j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[1 + variation[j] * 8], context,
                                Pos = Position + new Vector2(9f + j * 16f, j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[2 + variation[j] * 8], context,
                                Pos = Position + new Vector2(10f + j * 16f, 1f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[3 + variation[j] * 8], context,
                                Pos = Position + new Vector2(11f + j * 16f, 1f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[4 + variation[j] * 8], context,
                                Pos = Position + new Vector2(12f + j * 16f, 2f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[5 + variation[j] * 8], context,
                                Pos = Position + new Vector2(13f + j * 16f, 2f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[6 + variation[j] * 8], context,
                                Pos = Position + new Vector2(14f + j * 16f, 3f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[7 + variation[j] * 8], context,
                                Pos = Position + new Vector2(15f + j * 16f, 3f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[0 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(16f + j * 16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(17f + j * 16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(18f + j * 16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(19f + j * 16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(20f + j * 16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(21f + j * 16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(22f + j * 16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[7 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(23f + j * 16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(16f + j * 16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(17f + j * 16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(18f + j * 16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(19f + j * 16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(20f + j * 16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(21f + j * 16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(22f + j * 16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(23f + j * 16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[k]], context,
                                        Pos = Position + new Vector2(-16 + k * 8f, k * 8f), Vector2.Zero,
                                        Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[k + 1]], context,
                                    Pos = Position + new Vector2(-8 + k * 8f, k * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[k + 2]], context,
                                Pos = Position + new Vector2(k * 8f, k * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[0 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * 8f, l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * 8f, 1f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * 8f, 2f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * 8f, 3f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * 8f, 4f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * 8f, 5f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * 8f, 6f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[7 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * 8f, 7f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * 8f, l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * 8f, 1f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * 8f, 2f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * 8f, 3f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * 8f, 4f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * 8f, 5f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * 8f, 6f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * 8f, 7f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
                else if (Side == "Right")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[i]], context,
                                    Pos = Position + new Vector2(40f + i * -16f, i * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[i + 1]], context,
                                    Pos = Position + new Vector2(32f + i * -16f, i * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 2]], context,
                                Pos = Position + new Vector2(24f + i * -16f, i * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 3]], context,
                                Pos = Position + new Vector2(16f + i * -16f, i * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            DrawTexture(SlopeTextures[7 + variation[j] * 8], context,
                                Pos = Position + new Vector2(15f + j * -16f, j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[6 + variation[j] * 8], context,
                                Pos = Position + new Vector2(14f + j * -16f, j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[5 + variation[j] * 8], context,
                                Pos = Position + new Vector2(13f + j * -16f, 1f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[4 + variation[j] * 8], context,
                                Pos = Position + new Vector2(12f + j * -16f, 1f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[3 + variation[j] * 8], context,
                                Pos = Position + new Vector2(11f + j * -16f, 2f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[2 + variation[j] * 8], context,
                                Pos = Position + new Vector2(10f + j * -16f, 2f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[1 + variation[j] * 8], context,
                                Pos = Position + new Vector2(9f + j * -16f, 3f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[0 + variation[j] * 8], context,
                                Pos = Position + new Vector2(8f + j * -16f, 3f + j * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[7 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(7f + j * -16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(6f + j * -16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(5f + j * -16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(4f + j * -16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(3f + j * -16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(2f + j * -16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(1f + j * -16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[0 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(j * -16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(7f + j * -16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(6f + j * -16f, 4f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(5f + j * -16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(4f + j * -16f, 5f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(3f + j * -16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(2f + j * -16f, 6f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(1f + j * -16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(j * -16f, 7f + j * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[k]], context,
                                        Pos = Position + new Vector2(32f + k * -8f, k * 8f), Vector2.Zero,
                                        Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[k + 1]], context,
                                    Pos = Position + new Vector2(24f + k * -8f, k * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[k + 2]], context,
                                Pos = Position + new Vector2(16f + k * -8f, k * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[7 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * -8f, l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * -8f, 1f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * -8f, 2f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * -8f, 3f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * -8f, 4f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * -8f, 5f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * -8f, 6f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[0 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * -8f, 7f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * -8f, l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * -8f, 1f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * -8f, 2f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * -8f, 3f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * -8f, 4f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * -8f, 5f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * -8f, 6f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * -8f, 7f + l * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Side == "Left")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[i]], context,
                                    Pos = Position + new Vector2(-24f + i * 16f, i * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[i + 1]], context,
                                    Pos = Position + new Vector2(-16f + i * 16f, i * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 2]], context,
                                Pos = Position + new Vector2(-8f + i * 16f, i * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 3]], context,
                                Pos = Position + new Vector2(i * 16f, i * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            DrawTexture(SlopeTextures[0 + variation[j] * 8], context,
                                Pos = Position + new Vector2(8f + j * 16f, j * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[1 + variation[j] * 8], context,
                                Pos = Position + new Vector2(9f + j * 16f, j * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[2 + variation[j] * 8], context,
                                Pos = Position + new Vector2(10f + j * 16f, 1f + j * -8f + 6), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[3 + variation[j] * 8], context,
                                Pos = Position + new Vector2(11f + j * 16f, 1f + j * -8f + 6), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[4 + variation[j] * 8], context,
                                Pos = Position + new Vector2(12f + j * 16f, 2f + j * -8f + 4), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[5 + variation[j] * 8], context,
                                Pos = Position + new Vector2(13f + j * 16f, 2f + j * -8f + 4), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[6 + variation[j] * 8], context,
                                Pos = Position + new Vector2(14f + j * 16f, 3f + j * -8f + 2), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[7 + variation[j] * 8], context,
                                Pos = Position + new Vector2(15f + j * 16f, 3f + j * -8f + 2), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[0 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(16f + j * 16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(17f + j * 16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(18f + j * 16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(19f + j * 16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(20f + j * 16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(21f + j * 16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(22f + j * 16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[7 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(23f + j * 16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(16f + j * 16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(17f + j * 16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(18f + j * 16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(19f + j * 16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(20f + j * 16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(21f + j * 16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(22f + j * 16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(23f + j * 16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[k]], context,
                                        Pos = Position + new Vector2(-16 + k * 8f, k * -8f + 8), Vector2.Zero,
                                        Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[k + 1]], context,
                                    Pos = Position + new Vector2(-8 + k * 8f, k * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[k + 2]], context,
                                Pos = Position + new Vector2(k * 8f, k * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[0 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * 8f, l * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * 8f, 1f + l * -8f + 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * 8f, 2f + l * -8f + 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * 8f, 3f + l * -8f + 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * 8f, 4f + l * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * 8f, 5f + l * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * 8f, 6f + l * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[7 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * 8f, 7f + l * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * 8f, l * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * 8f, 1f + l * -8f + 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * 8f, 2f + l * -8f + 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * 8f, 3f + l * -8f + 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * 8f, 4f + l * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * 8f, 5f + l * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * 8f, 6f + l * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * 8f, 7f + l * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
                else if (Side == "Right")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[i]], context,
                                    Pos = Position + new Vector2(40f + i * -16f, i * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[i + 1]], context,
                                    Pos = Position + new Vector2(32f + i * -16f, i * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 2]], context,
                                Pos = Position + new Vector2(24f + i * -16f, i * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[i + 3]], context,
                                Pos = Position + new Vector2(16f + i * -16f, i * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            DrawTexture(SlopeTextures[7 + variation[j] * 8], context,
                                Pos = Position + new Vector2(15f + j * -16f, j * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[6 + variation[j] * 8], context,
                                Pos = Position + new Vector2(14f + j * -16f, j * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[5 + variation[j] * 8], context,
                                Pos = Position + new Vector2(13f + j * -16f, 1f + j * -8f + 6), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[4 + variation[j] * 8], context,
                                Pos = Position + new Vector2(12f + j * -16f, 1f + j * -8f + 6), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[3 + variation[j] * 8], context,
                                Pos = Position + new Vector2(11f + j * -16f, 2f + j * -8f + 4), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[2 + variation[j] * 8], context,
                                Pos = Position + new Vector2(10f + j * -16f, 2f + j * -8f + 4), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[1 + variation[j] * 8], context,
                                Pos = Position + new Vector2(9f + j * -16f, 3f + j * -8f + 2), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(SlopeTextures[0 + variation[j] * 8], context,
                                Pos = Position + new Vector2(8f + j * -16f, 3f + j * -8f + 2), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[7 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(7f + j * -16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(6f + j * -16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(5f + j * -16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(4f + j * -16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(3f + j * -16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(2f + j * -16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(1f + j * -16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[0 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(j * -16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(7f + j * -16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(6f + j * -16f, 4f + j * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(5f + j * -16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(4f + j * -16f, 5f + j * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(3f + j * -16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(2f + j * -16f, 6f + j * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(1f + j * -16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[j + 1] * 8], context,
                                    Pos = Position + new Vector2(j * -16f, 7f + j * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[k]], context,
                                        Pos = Position + new Vector2(32f + k * -8f, k * -8f + 8), Vector2.Zero,
                                        Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[k + 1]], context,
                                    Pos = Position + new Vector2(24f + k * -8f, k * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            DrawTexture(BaseTextures[5, 0 + variationInner[k + 2]], context,
                                Pos = Position + new Vector2(16f + k * -8f, k * -8f + 8), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                DrawTexture(SlopeTextures[7 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * -8f, l * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[6 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * -8f, 1f + l * -8f + 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[5 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * -8f, 2f + l * -8f + 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[4 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * -8f, 3f + l * -8f + 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[3 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * -8f, 4f + l * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[2 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * -8f, 5f + l * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[1 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * -8f, 6f + l * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[0 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * -8f, 7f + l * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                DrawTexture(SlopeTextures[64 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(15f + l * -8f, l * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[65 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(14f + l * -8f, 1f + l * -8f + 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[66 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(13f + l * -8f, 2f + l * -8f + 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[67 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(12f + l * -8f, 3f + l * -8f + 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[68 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(11f + l * -8f, 4f + l * -8f + 0), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[69 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(10f + l * -8f, 5f + l * -8f - 2), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[70 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(9f + l * -8f, 6f + l * -8f - 4), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(SlopeTextures[71 + variation[l] * 8], context,
                                    Pos = Position + new Vector2(8f + l * -8f, 7f + l * -8f - 6), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
            }
        }

        public override void Render(ISourceRenderContext context)
        {
            base.Render(context);
            if (!NoRender)
            {
                Vector2 Pos = Vector2.Zero;

                if (!UpsideDown)
                {
                    if (Side == "Left")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            DrawTexture(BaseTextures[0 + variation[0], 0], context,
                                Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[1], 0], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[0]], context,
                                    Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[1]], context,
                                    Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[1], 1], context,
                                    Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            DrawTexture(BaseTextures[4, 1], context, Pos = Position + new Vector2(-8f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[2], 0], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[2]], context,
                                    Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[3]], context,
                                    Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[2], 1], context,
                                    Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            DrawTexture(BaseTextures[4, 1], context, Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[4]], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[3], 11], context,
                                Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[4], 0], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[5], 2], context,
                                Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[5]], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[6], 11], context,
                                Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[7], 0], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 3], context, Pos = Position + new Vector2(-8f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[6]], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[8], 11], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[9], 2], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[10], 11], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 3], context, Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[5, 0 + variationInner[7]], context,
                                Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[8]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[9]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[10]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[11]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[12]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[13]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(context, Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[14]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[15]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[11], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[16]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[17]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[12], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[18]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[13], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[14], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[15], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[19]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[20]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[16], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[17], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[18], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[19], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[21]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[22]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[20], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[23]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[24]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[21], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[25]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[26]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[22], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[27]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[23], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[24], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[25], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[28]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[26], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[27], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[28], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[29], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[29]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[30], 14], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(context, Pos);
                        }
                    }
                    else if (Side == "Right")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            DrawTexture(BaseTextures[0 + variation[0], 0], context,
                                Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[1], 0], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[0]], context,
                                    Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[1]], context,
                                    Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[1], 1], context,
                                    Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            DrawTexture(BaseTextures[4, 3], context, Pos = Position + new Vector2(24f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[2], 0], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[2]], context,
                                    Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[3]], context,
                                    Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[2], 1], context,
                                    Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            DrawTexture(BaseTextures[4, 3], context, Pos = Position + new Vector2(16f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[4]], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[3], 12], context,
                                Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[4], 0], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[5], 3], context,
                                Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[5]], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[6], 12], context,
                                Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[7], 0], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 1], context, Pos = Position + new Vector2(24f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[6]], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[8], 12], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[9], 3], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[10], 12], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 1], context, Pos = Position + new Vector2(16f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[5, 0 + variationInner[7]], context,
                                Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[8]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[9]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[10]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[11]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[12]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[13]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(context, Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[14]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[15]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[11], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[16]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[17]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[12], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[18]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[13], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[14], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[15], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[19]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[20]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[16], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[17], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[18], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[19], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[21]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[22]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[20], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[23]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[24]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[21], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[25]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[26]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[22], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[27]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[23], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[24], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[25], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[28]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[26], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[27], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[28], 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[29], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[29]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[30], 13], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(context, Pos);
                        }
                    }
                }
                else
                {
                    if (Side == "Left")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            DrawTexture(BaseTextures[0 + variation[0], 1], context,
                                Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[1], 1], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[0]], context,
                                    Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[1]], context,
                                    Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[1], 0], context,
                                    Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            DrawTexture(BaseTextures[4, 0], context, Pos = Position + new Vector2(-8f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[2], 1], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[2]], context,
                                    Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[3]], context,
                                    Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[2], 0], context,
                                    Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            DrawTexture(BaseTextures[4, 0], context, Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[4]], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[3], 13], context,
                                Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[4], 1], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[5], 2], context,
                                Pos = Position + new Vector2(-8f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[5]], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[6], 13], context,
                                Pos = Position + new Vector2(-8f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[7], 1], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 2], context, Pos = Position + new Vector2(-8f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[6]], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[8], 13], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[9], 2], context,
                                Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[10], 13], context,
                                Pos = Position + new Vector2(0f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 2], context, Pos = Position + new Vector2(0f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[5, 0 + variationInner[7]], context,
                                Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[8]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[9]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[10]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[11]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[12]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[13]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(context, Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[14]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[15]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[11], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[16]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[17]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[12], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[18]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[13], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[14], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[15], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[19]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[20]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[16], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[17], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[18], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[19], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[21]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[22]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[20], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[23]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[24]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[21], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[25]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[26]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[22], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[27]], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[23], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[24], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[25], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[28]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[26], 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[27], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[28], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[29], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[29]], context,
                                        Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[4, 1], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[30], 12], context,
                                    Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(context, Pos);
                        }
                    }
                    else if (Side == "Right")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            DrawTexture(BaseTextures[0 + variation[0], 1], context,
                                Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[1], 1], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[0]], context,
                                    Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[1]], context,
                                    Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                            /*if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[1], 0], context, Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }*/
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            DrawTexture(BaseTextures[4, 2], context, Pos = Position + new Vector2(24f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[2], 1], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                DrawTexture(BaseTextures[5, 0 + variationInner[2]], context,
                                    Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[3]], context,
                                    Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }

                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                DrawTexture(BaseTextures[0 + variation[2], 0], context,
                                    Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                    Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            DrawTexture(BaseTextures[4, 2], context, Pos = Position + new Vector2(16f, 8f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[4]], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[3], 14], context,
                                Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[4], 1], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[5], 3], context,
                                Pos = Position + new Vector2(24f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[5]], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[6], 14], context,
                                Pos = Position + new Vector2(24f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[7], 1], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 0], context, Pos = Position + new Vector2(24f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[5, 0 + variationInner[6]], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            DrawTexture(BaseTextures[0 + variation[8], 14], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[0 + variation[9], 3], context,
                                Pos = Position + new Vector2(16f, 0f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[0 + variation[10], 14], context,
                                Pos = Position + new Vector2(16f, 8f), Vector2.Zero,
                                Rainbow ? GetHue(Pos) : Color.White);
                            DrawTexture(BaseTextures[4, 0], context, Pos = Position + new Vector2(16f, 0f),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            DrawTexture(BaseTextures[5, 0 + variationInner[7]], context,
                                Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8),
                                Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[8]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[9]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[10]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[11]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[12]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[13]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(context, Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[14]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[15]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[11], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[16]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[17]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[12], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[18]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[13], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[14], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[15], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[19]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[20]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[16], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[17], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[18], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[19], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[21]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[5, 0 + variationInner[22]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[20], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[23]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[24]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[21], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[25]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    DrawTexture(BaseTextures[5, 0 + variationInner[26]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[22], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[5, 0 + variationInner[27]], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[23], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[24], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[25], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[28]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[0 + variation[26], 2], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[27], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                DrawTexture(BaseTextures[0 + variation[28], 0], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[29], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    DrawTexture(BaseTextures[5, 0 + variationInner[29]], context,
                                        Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8),
                                        Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }

                                DrawTexture(BaseTextures[4, 3], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                DrawTexture(BaseTextures[0 + variation[30], 11], context,
                                    Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8),
                                    Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(context, Pos);
                        }
                    }
                }
            }
        }
    }
}
