using System.IO;
using UnityEngine;
using UnityEditor;
using PushTheBox.Level;

namespace PushTheBox.EditorTools
{
    public static class AssetGenerator
    {
        [MenuItem("PushTheBox/Generate All Assets and Levels")]
        public static void GenerateAll()
        {
            GenerateArtSprites();
            GenerateLevelAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetGenerator] Successfully generated all sprites and level data!");
        }

        public static void GenerateArtSprites()
        {
            string artDir = "Assets/Art";
            if (!Directory.Exists(artDir))
            {
                Directory.CreateDirectory(artDir);
            }

            File.WriteAllBytes(Path.Combine(artDir, "Player.png"), CreatePlayerTexture().EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "Box.png"), CreateBoxTexture(false).EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "BoxOnTarget.png"), CreateBoxTexture(true).EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "Target.png"), CreateTargetTexture().EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "Wall.png"), CreateWallTexture().EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "Floor.png"), CreateFloorTexture().EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "Star.png"), CreateStarTexture().EncodeToPNG());
            File.WriteAllBytes(Path.Combine(artDir, "ButtonBg.png"), CreateButtonTexture().EncodeToPNG());

            AssetDatabase.Refresh();

            // Configure texture import settings as 2D Sprites
            ConfigureSpriteImport(Path.Combine(artDir, "Player.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "Box.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "BoxOnTarget.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "Target.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "Wall.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "Floor.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "Star.png"));
            ConfigureSpriteImport(Path.Combine(artDir, "ButtonBg.png"), new Vector4(24, 24, 24, 24));
        }

        private static void ConfigureSpriteImport(string path, Vector4? border = null)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                if (border.HasValue)
                {
                    importer.spriteBorder = border.Value;
                }
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreatePlayerTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color bodyColor = new Color(0.25f, 0.65f, 1f, 1f); // Vibrant Cyan/Blue
            Color bellyColor = new Color(0.65f, 0.85f, 1f, 1f);
            Color eyeWhite = Color.white;
            Color eyePupil = new Color(0.1f, 0.15f, 0.25f, 1f);
            Color cheekColor = new Color(1f, 0.5f, 0.6f, 0.6f);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        // Body gradient
                        float t = (float)y / size;
                        Color c = Color.Lerp(bodyColor * 0.85f, bodyColor, t);

                        // Lighter tummy
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(center.x, center.y - 12)) < radius * 0.55f)
                        {
                            c = Color.Lerp(c, bellyColor, 0.8f);
                        }

                        // Border rim
                        if (dist > radius - 3f)
                        {
                            c = new Color(0.1f, 0.35f, 0.65f, 1f);
                        }

                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }

            // Draw Eyes
            DrawCircle(tex, new Vector2(size * 0.36f, size * 0.58f), 10, eyeWhite);
            DrawCircle(tex, new Vector2(size * 0.64f, size * 0.58f), 10, eyeWhite);
            DrawCircle(tex, new Vector2(size * 0.38f, size * 0.58f), 5, eyePupil);
            DrawCircle(tex, new Vector2(size * 0.66f, size * 0.58f), 5, eyePupil);

            // Draw Cute Cheeks
            DrawCircle(tex, new Vector2(size * 0.25f, size * 0.44f), 7, cheekColor);
            DrawCircle(tex, new Vector2(size * 0.75f, size * 0.44f), 7, cheekColor);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBoxTexture(bool onTarget)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color baseColor = onTarget ? new Color(0.95f, 0.78f, 0.22f, 1f) : new Color(0.85f, 0.58f, 0.28f, 1f);
            Color darkPlank = onTarget ? new Color(0.80f, 0.60f, 0.12f, 1f) : new Color(0.65f, 0.42f, 0.18f, 1f);
            Color frameColor = onTarget ? new Color(1.0f, 0.92f, 0.55f, 1f) : new Color(0.50f, 0.30f, 0.12f, 1f);
            Color glowCorner = onTarget ? new Color(0.4f, 1f, 0.5f, 1f) : frameColor;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < 8 || x >= size - 8 || y < 8 || y >= size - 8;
                    bool isCross1 = Mathf.Abs(x - y) < 6;
                    bool isCross2 = Mathf.Abs(x - (size - 1 - y)) < 6;

                    if (isBorder)
                    {
                        tex.SetPixel(x, y, frameColor);
                    }
                    else if (isCross1 || isCross2)
                    {
                        tex.SetPixel(x, y, darkPlank);
                    }
                    else
                    {
                        float subtleNoise = ((x * 7 + y * 13) % 10) * 0.01f;
                        tex.SetPixel(x, y, baseColor + new Color(subtleNoise, subtleNoise, subtleNoise, 0f));
                    }
                }
            }

            // Rivets on corners
            DrawCircle(tex, new Vector2(14, 14), 4, glowCorner);
            DrawCircle(tex, new Vector2(size - 14, 14), 4, glowCorner);
            DrawCircle(tex, new Vector2(14, size - 14), 4, glowCorner);
            DrawCircle(tex, new Vector2(size - 14, size - 14), 4, glowCorner);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateTargetTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color outerRing = new Color(0.2f, 0.85f, 0.95f, 0.9f);
            Color innerGlow = new Color(0.2f, 0.85f, 0.95f, 0.3f);
            Color centerDot = new Color(0.4f, 1f, 0.8f, 1f);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist >= 44 && dist <= 54)
                    {
                        tex.SetPixel(x, y, outerRing);
                    }
                    else if (dist < 44 && dist >= 16)
                    {
                        tex.SetPixel(x, y, innerGlow);
                    }
                    else if (dist < 16)
                    {
                        tex.SetPixel(x, y, centerDot);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWallTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color stoneDark = new Color(0.22f, 0.26f, 0.34f, 1f);
            Color stoneLight = new Color(0.38f, 0.44f, 0.55f, 1f);
            Color mortar = new Color(0.12f, 0.14f, 0.18f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMortarH = y == 0 || y == 64 || y == size - 1;
                    bool isMortarV = (y > 64 && (x == 0 || x == size - 1)) || (y <= 64 && (x == 0 || x == 64 || x == size - 1));

                    if (isMortarH || isMortarV)
                    {
                        tex.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        float bevel = (x % 64 < 4 || y % 64 > 58) ? 0.08f : -0.05f;
                        Color c = Color.Lerp(stoneDark, stoneLight, 0.5f + bevel);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateFloorTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color tileColor = new Color(0.16f, 0.19f, 0.25f, 1f);
            Color bevelColor = new Color(0.20f, 0.24f, 0.31f, 1f);
            Color grooveColor = new Color(0.11f, 0.13f, 0.17f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < 2 || y < 2 || x >= size - 2 || y >= size - 2)
                    {
                        tex.SetPixel(x, y, grooveColor);
                    }
                    else if (x < 6 || y >= size - 6)
                    {
                        tex.SetPixel(x, y, bevelColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, tileColor);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateStarTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color starGold = new Color(1f, 0.85f, 0.15f, 1f);
            Color starBorder = new Color(0.9f, 0.65f, 0.05f, 1f);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x, y) - center;
                    float angle = Mathf.Atan2(pt.y, pt.x);
                    float r = pt.magnitude;

                    // 5-point star equation
                    float starR = size * 0.24f + (size * 0.18f) * Mathf.Cos(5f * angle);

                    if (r <= starR)
                    {
                        tex.SetPixel(x, y, (r > starR - 3f) ? starBorder : starGold);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static void DrawCircle(Texture2D tex, Vector2 center, float radius, Color color)
        {
            int minX = Mathf.Max(0, (int)(center.x - radius));
            int maxX = Mathf.Min(tex.width - 1, (int)(center.x + radius));
            int minY = Mathf.Max(0, (int)(center.y - radius));
            int maxY = Mathf.Min(tex.height - 1, (int)(center.y + radius));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static Texture2D CreateButtonTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color body = Color.white;
            Color border = new Color(0.85f, 0.90f, 0.98f, 1f);
            Color shadow = new Color(0.55f, 0.60f, 0.72f, 1f);

            int cornerRadius = 24;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isInside = true;
                    if (x < cornerRadius && y < cornerRadius)
                        isInside = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius)) <= cornerRadius;
                    else if (x >= size - cornerRadius && y < cornerRadius)
                        isInside = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, cornerRadius)) <= cornerRadius;
                    else if (x < cornerRadius && y >= size - cornerRadius)
                        isInside = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, size - cornerRadius - 1)) <= cornerRadius;
                    else if (x >= size - cornerRadius && y >= size - cornerRadius)
                        isInside = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, size - cornerRadius - 1)) <= cornerRadius;

                    if (!isInside)
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                    else
                    {
                        if (y < 6)
                        {
                            tex.SetPixel(x, y, shadow);
                        }
                        else if (x < 4 || x >= size - 4 || y >= size - 4)
                        {
                            tex.SetPixel(x, y, border);
                        }
                        else
                        {
                            tex.SetPixel(x, y, body);
                        }
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        public static void GenerateLevelAssets()
        {
            string folder = "Assets/ScriptableObjects/Levels";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Level 1: First Steps (5x5)
            CreateLevelAsset(folder, "Level_1", 1, "Level 1: First Steps", 5, 5,
                new Vector2Int(1, 2),
                BuildPerimeterWalls(5, 5),
                new Vector2Int[] { new Vector2Int(2, 2) },
                new Vector2Int[] { new Vector2Int(3, 2) },
                2, 4);

            // Level 2: Around the Corner (6x6)
            var walls2 = new System.Collections.Generic.List<Vector2Int>(BuildPerimeterWalls(6, 6));
            walls2.Add(new Vector2Int(2, 2));
            walls2.Add(new Vector2Int(2, 3));
            CreateLevelAsset(folder, "Level_2", 2, "Level 2: Around Corner", 6, 6,
                new Vector2Int(1, 1),
                walls2.ToArray(),
                new Vector2Int[] { new Vector2Int(3, 2) },
                new Vector2Int[] { new Vector2Int(3, 4) },
                5, 8);

            // Level 3: Double Trouble (7x7)
            var walls3 = new System.Collections.Generic.List<Vector2Int>(BuildPerimeterWalls(7, 7));
            walls3.Add(new Vector2Int(3, 3)); // Center pillar
            CreateLevelAsset(folder, "Level_3", 3, "Level 3: Double Trouble", 7, 7,
                new Vector2Int(3, 4),
                walls3.ToArray(),
                new Vector2Int[] { new Vector2Int(2, 3), new Vector2Int(4, 3) },
                new Vector2Int[] { new Vector2Int(2, 2), new Vector2Int(4, 2) },
                7, 12);

            // Level 4: Storage Room (7x7)
            var walls4 = new System.Collections.Generic.List<Vector2Int>(BuildPerimeterWalls(7, 7));
            walls4.Add(new Vector2Int(2, 2));
            walls4.Add(new Vector2Int(2, 3));
            walls4.Add(new Vector2Int(4, 3));
            walls4.Add(new Vector2Int(4, 4));
            CreateLevelAsset(folder, "Level_4", 4, "Level 4: Storage Room", 7, 7,
                new Vector2Int(3, 3),
                walls4.ToArray(),
                new Vector2Int[] { new Vector2Int(3, 4), new Vector2Int(3, 2) },
                new Vector2Int[] { new Vector2Int(1, 4), new Vector2Int(5, 2) },
                12, 18);

            // Level 5: Warehouse Master (8x8)
            var walls5 = new System.Collections.Generic.List<Vector2Int>(BuildPerimeterWalls(8, 8));
            walls5.Add(new Vector2Int(3, 3));
            walls5.Add(new Vector2Int(4, 3));
            walls5.Add(new Vector2Int(3, 4));
            walls5.Add(new Vector2Int(4, 4));
            CreateLevelAsset(folder, "Level_5", 5, "Level 5: Warehouse Master", 8, 8,
                new Vector2Int(3, 5),
                walls5.ToArray(),
                new Vector2Int[] { new Vector2Int(2, 4), new Vector2Int(5, 4), new Vector2Int(5, 2) },
                new Vector2Int[] { new Vector2Int(2, 5), new Vector2Int(5, 5), new Vector2Int(2, 2) },
                18, 26);
        }

        private static Vector2Int[] BuildPerimeterWalls(int width, int height)
        {
            var list = new System.Collections.Generic.List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                list.Add(new Vector2Int(x, 0));
                list.Add(new Vector2Int(x, height - 1));
            }
            for (int y = 1; y < height - 1; y++)
            {
                list.Add(new Vector2Int(0, y));
                list.Add(new Vector2Int(width - 1, y));
            }
            return list.ToArray();
        }

        private static void CreateLevelAsset(string folder, string fileName, int id, string name, int w, int h,
            Vector2Int playerPos, Vector2Int[] walls, Vector2Int[] boxes, Vector2Int[] targets, int stars3, int stars2)
        {
            string path = $"{folder}/{fileName}.asset";
            LevelData asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.levelId = id;
            asset.levelName = name;
            asset.width = w;
            asset.height = h;
            asset.playerPosition = playerPos;
            asset.walls = walls;
            asset.boxes = boxes;
            asset.targets = targets;
            asset.threeStarMoves = stars3;
            asset.twoStarMoves = stars2;

            EditorUtility.SetDirty(asset);
        }
    }
}
