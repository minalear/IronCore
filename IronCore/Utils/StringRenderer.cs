using System;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using OpenTK.Graphics;
using SharpFont;

namespace IronCore.Utils
{
    public class StringRenderer
    {
        public float LineSpacing { get; set; }
        public float BufferWidth { get; set; }

        public StringRenderer()
        {
            LineSpacing = 8f;
            BufferWidth = 800f;
        }

        public Texture2D RenderString(Face font, string text, Color4 foreColor, Color4 backColor, bool wrapText)
        {
            LineMetrics[] metrics = formatText(font, text, wrapText);

            float maxWidth = 0f, maxHeight = 0f, totalHeight = 0f;
            for (int lineIndex = 0; lineIndex < metrics.Length; lineIndex++)
            {
                LineMetrics lineMetric = metrics[lineIndex];
                maxWidth = (lineMetric.Width > maxWidth) ? lineMetric.Width : maxWidth;
                maxHeight = (lineMetric.Height > maxHeight) ? lineMetric.Height : maxHeight;

                //Check is to ensure we don't have additional extra space accumulating beneath the texture for large Heights (commas being an example)
                totalHeight += (lineIndex < metrics.Length - 1) ? lineMetric.BaseHeight + LineSpacing : lineMetric.Height;

                metrics[lineIndex] = lineMetric;
            }

            //If any dimension is 0, we can't create a bitmap
            if (maxWidth <= 0 || totalHeight <= 0)
                return null;

            //Create a new bitmap that fits the string.
            Bitmap bmp = new Bitmap((int)Math.Ceiling(maxWidth), (int)Math.Ceiling(totalHeight));

            using (var g = Graphics.FromImage(bmp))
            {
                #region Rendering Code
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingMode = CompositingMode.SourceOver;

                g.Clear((Color)backColor);

                //Draw the string into the bitmap.
                float lineOffset = 0f;
                for (int lineIndex = 0; lineIndex < metrics.Length; lineIndex++)
                {
                    LineMetrics lineMetrics = metrics[lineIndex];

                    //float xOffset = (maxWidth - lineMetrics.Width) / 2f; //Centered
                    float xOffset = 0f;
                    //float xOffset = maxWidth - lineMetrics.Width - 20f;

                    float penX = 0f, penY = 0f;
                    for (int i = 0; i < lineMetrics.Characters.Count; i++)
                    {
                        var cm = lineMetrics.Characters[i];
                        char c = cm.Character;

                        uint glyphIndex = font.GetCharIndex(c);
                        font.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                        font.Glyph.RenderGlyph(RenderMode.Normal);
                        FTBitmap ftbmp = font.Glyph.Bitmap;

                        //Underrun
                        if (penX == 0) //First character
                            penX += -(cm.BearingX);

                        //We can't draw a 0-size bitmap, but the pen position will still get advanced.
                        if (ftbmp.Width > 0 && ftbmp.Rows > 0)
                        {
                            using (Bitmap cBmp = ftbmp.ToGdipBitmap((Color)foreColor))
                            {
                                int x = (int)Math.Round(penX + cm.BearingX + xOffset);
                                int y = (int)Math.Round(penY + lineMetrics.Top - cm.BearingY + lineOffset);

                                g.DrawImageUnscaled(cBmp, x, y);
                            }
                        }

                        //Advance pen position for the next character
                        penX += cm.AdvanceX + cm.Kern;
                    }

                    lineOffset += lineMetrics.BaseHeight + LineSpacing;
                }
                #endregion
            }

            Texture2D texture = Texture2D.CreateFromBitmap(bmp);
            bmp.Dispose();

            return texture;
        }

        private LineMetrics measureText(Face face, string line)
        {
            var measuredChars = new List<CharMetrics>();
            float stringWidth = 0; //the measured width of the string
            float stringHeight = 0; //the measured height of the string
            float top = 0f, bottom = 0f;
            float overrun = 0f;
            float baseHeight = 0f;

            //Measure the size of the string before rendering it.
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                uint glyphIndex = face.GetCharIndex(c);
                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

                //Set glyph metrics (https://www.freetype.org/freetype2/docs/tutorial/step2.html)
                float gAdvanceX = face.Glyph.Metrics.HorizontalAdvance.ToSingle();
                float gBearingX = face.Glyph.Metrics.HorizontalBearingX.ToSingle();
                float gBearingY = face.Glyph.Metrics.HorizontalBearingY.ToSingle();
                float gWidth = face.Glyph.Metrics.Width.ToSingle();

                //Negative bearing could cause clipping of first while positive bearing could cause whitespace
                if (stringWidth == 0) //First character
                    stringWidth += -(gBearingX);

                //Accumulate overrun which could cause clipping at the right side of characters
                if (gBearingX + gWidth > 0 || gAdvanceX > 0)
                {
                    overrun -= Math.Max(gBearingX + gWidth, gAdvanceX);
                    if (overrun <= 0f) overrun = 0f;
                }
                overrun += (gBearingX == 0 && gWidth == 0) ? 0 : gBearingX + gWidth - gAdvanceX;

                //Add overrun if we're at the last letter of the string
                if (i == line.Length - 1)
                    stringWidth += overrun;

                float gTop = face.Glyph.Metrics.HorizontalBearingY.ToSingle();
                float gBottom = (face.Glyph.Metrics.Height - face.Glyph.Metrics.HorizontalBearingY).ToSingle();

                if (gTop > top)
                    top = gTop;
                if (gBottom > bottom)
                    bottom = gBottom;

                //Calculate kerning for next character (if any)
                float kern = 0f;
                if (face.HasKerning && i < line.Length - 1)
                {
                    char next = line[i + 1];
                    kern = face.GetKerning(glyphIndex, face.GetCharIndex(next), KerningMode.Default).X.ToSingle();
                }

                //Check for baseHeight
                if (gBearingY > baseHeight)
                    baseHeight = gBearingY;

                //Update string measurements
                stringWidth += gAdvanceX + kern;

                measuredChars.Add(new CharMetrics(c, gAdvanceX, gBearingX, gBearingY, gWidth, kern));
            }

            stringHeight = top + bottom;

            return new LineMetrics(measuredChars, stringWidth, stringHeight, top, bottom, baseHeight);
        }
        private LineMetrics[] formatText(Face face, string text, bool wrapText)
        {
            List<LineMetrics> lineMetrics = new List<LineMetrics>();

            //Remove all \r characters, since we use \n to seperate lines
            text = text.Replace("\r", string.Empty);

            //Split the current text by newlines which act as paragraphs
            string[] lines = text.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string[] words = lines[lineIndex].Split(' ');
                if (words.Length == 0) continue;

                //Go through each individual word and measure the new line (currentText + newWord) to see if it is less than the buffer width
                StringBuilder lineText = (words.Length > 1) ? new StringBuilder(words[0] + " ") : new StringBuilder(words[0]);
                LineMetrics currentLine = measureText(face, lineText.ToString());
                for (int wordIndex = 1; wordIndex < words.Length; wordIndex++)
                {
                    string newText = words[wordIndex];
                    if (wordIndex < words.Length - 1)
                        newText += " ";

                    LineMetrics testLine = measureText(face, lineText + newText);
                    if (testLine.Width <= BufferWidth || !wrapText)
                    {
                        currentLine = testLine;
                        lineText.Append(newText);
                    }
                    else
                    {
                        lineMetrics.Add(currentLine);
                        lineText = new StringBuilder(newText);
                    }
                }
                lineMetrics.Add(currentLine);
            }

            return lineMetrics.ToArray();
        }

        private class CharMetrics
        {
            public char Character { get; set; }
            public float AdvanceX { get; set; }
            public float BearingX { get; set; }
            public float BearingY { get; set; }
            public float Width { get; set; }
            public float Kern { get; set; }

            public CharMetrics(char ch, float advanceX, float bearingX, float bearingY, float width, float kern)
            {
                Character = ch;
                AdvanceX = advanceX;
                BearingX = bearingX;
                BearingY = bearingY;
                Width = width;
                Kern = kern;
            }

            public override string ToString()
            {
                return $"{Character} - ({AdvanceX}, ({BearingX}, {BearingY}), {Width}, {Kern})";
            }
        }
        private class LineMetrics
        {
            public List<CharMetrics> Characters { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
            public float BaseHeight { get; set; }

            public float Top { get; set; }
            public float Bottom { get; set; }

            public LineMetrics(List<CharMetrics> characters, float width, float height, float top, float bottom, float baseHeight)
            {
                Characters = characters;
                Width = width;
                Height = height;
                Top = top;
                Bottom = bottom;
                BaseHeight = baseHeight;
            }
        }
    }
}
