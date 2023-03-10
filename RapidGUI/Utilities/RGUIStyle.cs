using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI
{
    public static class RGUIStyle
    {
        public static GUIStyle button;
        public static GUIStyle flatButton;
        public static GUIStyle popupFlatButton;
        public static GUIStyle popup;
        public static GUIStyle darkWindow;
        public static GUIStyle alignLeftBox;

        public static GUIStyle warningLabel;
        public static GUIStyle warningLabelNoStyle;

        public static GUIStyle slider, thumb;

        // GUIStyleState.background will be null 
        // if it set after secound scene load and don't use a few frame
        // to keep textures, set it to other member. at unity2019
        public static Texture2D flatButtonTex;
        public static Texture2D popupTex;
        public static Texture2D darkWindowTexNormal;
        public static Texture2D darkWindowTexOnNormal;
        public static List<Color32> backgroundColors, borderColors, backgroundColorsHover;
        public static Texture2D tex, texh;

        static RGUIStyle()
        {
            CreateStyles();
        }

        public static void CreateStyles()
        {
            CreateButton();
            CreateFlatButton();
            CreatePopupFlatButton();
            CreatePopup();
            CreateDarkWindow();
            CreateAlignLeftBox();
            CreateWarningLabel();
            CreateWarningLabelNoStyle();
            CreateSliderLabel();
        }

        static void CreateButton()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter
            };

            style.fixedHeight = 21f;
            button = style;
        }


        static void CreateSliderLabel()
        {
            backgroundColors = new List<Color32>();
            borderColors = new List<Color32>();
            backgroundColorsHover = new List<Color32>();

            var stylethumb = new GUIStyle(GUI.skin.horizontalSliderThumb) { };
            backgroundColors.Add(new Color32(200, 200, 200, 200));
            backgroundColors.Add(new Color32(255, 255, 255, 200));
            borderColors.Add(new Color32(0, 0, 0, 255));
            tex = RectangleCreator.CreateRoundedRectangleTexture(1, 128, 56, 0, 4, 2, backgroundColors, borderColors, .75f, 0);
            stylethumb.normal.background = tex;

            backgroundColorsHover.Add(new Color32(200, 200, 200, 255));
            backgroundColorsHover.Add(new Color32(255, 255, 255, 255));
            texh = RectangleCreator.CreateRoundedRectangleTexture(1, 128, 56, 0, 4, 2, backgroundColorsHover, borderColors, .75f, 0);
            stylethumb.hover.background = texh;
            stylethumb.active.background = texh;
            stylethumb.focused.background = texh;
            thumb = stylethumb;

            var styleslider = new GUIStyle(GUI.skin.horizontalSlider) { };
            slider = styleslider;
        }

        static void CreateFlatButton()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter
            };

            var toggle = GUI.skin.toggle;
            style.normal.textColor = toggle.normal.textColor;
            style.hover.textColor = toggle.hover.textColor;

            flatButtonTex = new Texture2D(1, 1);
            flatButtonTex.SetPixels(new[] { new Color(1, 1, 1, 1) });
            flatButtonTex.Apply();
            style.hover.background = flatButtonTex;
            style.hover.textColor = Color.white;

            style.name = nameof(flatButton);
            flatButton = style;
        }

        static void CreatePopupFlatButton()
        {
            var style = new GUIStyle(flatButton)
            {
                alignment = GUI.skin.label.alignment,
                padding = new RectOffset(4, 4, 4, 6),
                name = nameof(popupFlatButton)
            };

            popupFlatButton = style;
        }

        static void CreatePopup()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset()
            };

            popupTex = new Texture2D(1, 1);
            popupTex.SetPixels(new[] { new Color(44f / 255f, 62f / 255f, 80f / 255f, 1f) });
            popupTex.Apply();

            style.normal.background =
            style.hover.background = popupTex;

            style.name = nameof(popup);
            popup = style;
        }


        public static void CreateDarkWindow()
        {
            var style = new GUIStyle(GUI.skin.window);

            style.normal.background = darkWindowTexNormal = CreateTexDark(style.normal.background, 0.5f, 1.4f);
            style.onNormal.background = darkWindowTexOnNormal = CreateTexDark(style.onNormal.background, 0.6f, 1.5f);

            style.name = nameof(darkWindow);

            darkWindow = style;
        }

        public static void CreateAlignLeftBox()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                name = nameof(alignLeftBox),
            };
            style.fixedHeight = 21f;
            style.fixedWidth = 260f;
            alignLeftBox = style;
        }

        public static Texture2D CreateTexDark(Texture2D src, float colorRate, float alphaRate)
        {
            // copy texture trick.
            // Graphics.CopyTexture(src, dst) must same format src and dst.
            // but src format can't call GetPixels().
            var tmp = RenderTexture.GetTemporary(src.width, src.height);
            Graphics.Blit(src, tmp);

            var prev = RenderTexture.active;
            RenderTexture.active = prev;

            var dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            dst.ReadPixels(new Rect(0f, 0f, src.width, src.height), 0, 0);


            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tmp);


            var pixels = dst.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
            {
                var col = pixels[i];
                col.r *= colorRate;
                col.g *= colorRate;
                col.b *= colorRate;
                col.a *= alphaRate;

                pixels[i] = col;
            }

            dst.SetPixels(pixels);
            dst.Apply();

            return dst;
        }


        private static void CreateWarningLabel()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = GUI.skin.label.alignment,
                richText = true,
                name = nameof(warningLabel)
            };

            warningLabel = style;
        }

        private static void CreateWarningLabelNoStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                name = nameof(warningLabelNoStyle)
            };

            warningLabelNoStyle = style;
        }
    }

    // code from https://stackoverflow.com/questions/52615685/create-smoothly-rounded-rectangle-with-texture2d
    public class RectangleCreator
    {
        public static Texture2D CreateRoundedRectangleTexture(int resolutionmultiplier, int width, int height, int borderThickness, int borderRadius, int borderShadow, List<Color32> backgroundColors, List<Color32> borderColors, float initialShadowIntensity, float finalShadowIntensity)
        {
            width = width * resolutionmultiplier;
            height = height * resolutionmultiplier;

            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color32[] color = new Color32[width * height];

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    switch (backgroundColors.Count)
                    {
                        case 4:
                            Color32 leftColor0 = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color32 rightColor0 = Color32.Lerp(backgroundColors[2], backgroundColors[3], ((float)y / (height - 1)));
                            color[x + width * y] = Color32.Lerp(leftColor0, rightColor0, ((float)x / (width - 1)));
                            break;
                        case 3:
                            Color32 leftColor1 = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                            Color32 rightColor1 = Color32.Lerp(backgroundColors[1], backgroundColors[2], ((float)y / (height - 1)));
                            color[x + width * y] = Color32.Lerp(leftColor1, rightColor1, ((float)x / (width - 1)));
                            break;
                        case 2:
                            color[x + width * y] = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)x / (width - 1)));
                            break;
                        default:
                            color[x + width * y] = backgroundColors[0];
                            break;
                    }

                    color[x + width * y] = ColorBorder(x, y, width, height, borderThickness, borderRadius, borderShadow, color[x + width * y], borderColors, initialShadowIntensity, finalShadowIntensity);
                }
            }

            texture.SetPixels32(color);
            texture.Apply();
            return texture;
        }

        private static Color32 ColorBorder(int x, int y, int width, int height, int borderThickness, int borderRadius, int borderShadow, Color32 initialColor, List<Color32> borderColors, float initialShadowIntensity, float finalShadowIntensity)
        {
            Rect internalRectangle = new Rect((borderThickness + borderRadius), (borderThickness + borderRadius), width - 2 * (borderThickness + borderRadius), height - 2 * (borderThickness + borderRadius));


            Vector2 point = new Vector2(x, y);
            if (internalRectangle.Contains(point)) return initialColor;

            Vector2 origin = Vector2.zero;

            if (x < borderThickness + borderRadius)
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(borderRadius + borderThickness, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(borderRadius + borderThickness, height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(borderRadius + borderThickness, y);
            }
            else if (x > width - (borderRadius + borderThickness))
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(width - (borderRadius + borderThickness), borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(width - (borderRadius + borderThickness), height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(width - (borderRadius + borderThickness), y);
            }
            else
            {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(x, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(x, height - (borderRadius + borderThickness));
            }

            if (!origin.Equals(Vector2.zero))
            {
                float distance = Vector2.Distance(point, origin);

                if (distance > borderRadius + borderThickness + 1)
                {
                    return Color.clear;
                }
                else if (distance > borderRadius + 1)
                {
                    if (borderColors.Count > 2)
                    {
                        float modNum = distance - borderRadius;

                        if (modNum < borderThickness / 2)
                        {
                            return Color32.Lerp(borderColors[2], borderColors[1], (float)((modNum) / (borderThickness / 2.0)));
                        }
                        else
                        {
                            return Color32.Lerp(borderColors[1], borderColors[0], (float)((modNum - (borderThickness / 2.0)) / (borderThickness / 2.0)));
                        }
                    }


                    if (borderColors.Count > 0)
                        return borderColors[0];
                }
                else if (distance > borderRadius - borderShadow + 1)
                {
                    float mod = (distance - (borderRadius - borderShadow)) / borderShadow;
                    float shadowDiff = initialShadowIntensity - finalShadowIntensity;
                    return DarkenColor(initialColor, ((shadowDiff * mod) + finalShadowIntensity));
                }
            }

            return initialColor;
        }

        private static Color32 DarkenColor(Color32 color, float shadowIntensity)
        {
            return Color32.Lerp(color, Color.black, shadowIntensity);
        }
    }
}