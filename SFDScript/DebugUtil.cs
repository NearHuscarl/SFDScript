using SFDGameScriptInterface;
using System;
using System.Collections.Generic;

namespace SFDScript.DebugUtil
{
    public partial class GameScript : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        public void OnStartup()
        {
            // System.Diagnostics.Debugger.Break();
            if (Game.GetPlayers().Length == 1) // Create another player for testing
            {
                var pos = Game.GetPlayers()[0].GetWorldPosition();
                pos.X += 20;
                Game.CreatePlayer(pos);
            }

            DebugUtil.Initialize();
        }

        public static class ScriptHelper
        {
            public static Vector2 PerpendicularClockwise(Vector2 vector2)
            {
                return new Vector2(vector2.Y, -vector2.X);
            }

            public static Vector2 PerpendicularCounterClockwise(Vector2 vector2)
            {
                return new Vector2(-vector2.Y, vector2.X);
            }

            public static Area GetAABB(IPlayer player)
            {
                var aabb = player.GetAABB();
                var sizeModifier = player.GetModifiers().SizeModifier;
                var newWidth = aabb.Width * sizeModifier * 1.75f;
                var newHeight = aabb.Height * sizeModifier * 1.25f;
                var dir = player.FacingDirection;

                aabb.Left -= (newWidth - aabb.Width) / 2;
                aabb.Right += (newWidth - aabb.Width) / 2;
                aabb.Top += (newHeight - aabb.Height) / 2;
                aabb.Bottom -= (newHeight - aabb.Height) / 2;

                if (player.FacingDirection == -1) // Facing left
                {
                    aabb.Left += 3;
                    aabb.Right += 3;
                }

                if (player.IsStrengthBoostActive)
                {
                    if (dir == 1)
                    {
                        aabb.Left -= 3;
                        aabb.Right += 2;
                        aabb.Top += 3;
                    }
                    else
                    {
                        aabb.Right += 3;
                        aabb.Left -= 2;
                        aabb.Top += 3;
                    }
                }

                return aabb;
            }

            public static void GetAimVectorInfo(IPlayer player, out Vector2 start, out Vector2 center)
            {
                var aimVector = player.AimVector;
                var playerPosition = player.GetWorldPosition();
                var direction = player.FacingDirection;

                // Center position and start vector only apply to normal size (SizeModifier = 1f). Things
                // maybe a little off on different sizes
                center = playerPosition;
                center.X += -direction * 2;
                center.Y += 9;

                start = center;
                var perpendicularVec = PerpendicularCounterClockwise(aimVector);
                var radius = Vector2.Normalize(perpendicularVec) * direction * 4;
                start += radius;
            }

            public static bool IsIntersectArea(Vector2 start, Vector2 end, Area area)
            {
                if (Intersects(start, end, area.TopLeft, area.TopRight))
                    return true;
                if (Intersects(start, end, area.TopRight, area.BottomRight))
                    return true;
                if (Intersects(start, end, area.BottomRight, area.BottomLeft))
                    return true;
                if (Intersects(start, end, area.BottomLeft, area.TopLeft))
                    return true;

                return false;
            }

            // https://stackoverflow.com/a/3746601/9449426
            // a1 is line1 start, a2 is line1 end, b1 is line2 start, b2 is line2 end
            static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
            {
                Vector2 b = a2 - a1;
                Vector2 d = b2 - b1;
                float bDotDPerp = b.X * d.Y - b.Y * d.X;

                // if b dot d == 0, it means the lines are parallel so have infinite intersection points
                if (bDotDPerp == 0)
                    return false;

                Vector2 c = b1 - a1;
                float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
                if (t < 0 || t > 1)
                    return false;

                float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
                if (u < 0 || u > 1)
                    return false;

                return true;
            }

            // Get the minimum distance between line (ab) and point p
            public static float GetMinDistance(Vector2 a, Vector2 b, Vector2 p)
            {
                var lengthSquare = (a - b).LengthSquared();
                if (lengthSquare == 0)
                    return Vector2.Distance(a, p); // a == b case

                // Consider the line extending the segment, parameterized as v = a + t (b - a) (v is a point on ab, t is a parameter)
                // (p - v) * (b - a) = 0
                // [p - a - t (b - a)] * (b - a) = 0
                // t = (p - a)(b - a) / (b - a)^2
                // We find projection of point p onto the line.
                // It falls where t = [(p-a) . (b-a)] / |b-a|^2
                // We clamp t from [0,1] to handle points outside the segment ab.
                var t = MathHelper.Clamp(Vector2.Dot(p - a, b - a) / lengthSquare, 0, 1);
                var projection = a + t * (b - a);  // Projection falls on the segment
                return Vector2.Distance(p, projection);
            }
        }

        public static class DebugUtil
        {
            private static Dictionary<string, List<IObjectText>> m_aimVectorDots = new Dictionary<string, List<IObjectText>>();
            private static Dictionary<string, List<IObjectText>> m_aabbDots = new Dictionary<string, List<IObjectText>>();
            private static IObjectText m_aimDistanceText;
            private static IPlayer m_player;

            public static void Initialize()
            {
                Events.UpdateCallback.Start(OnUpdate);
                m_player = Game.GetPlayers()[0];

                if (Game.IsEditorTest)
                {
                    m_player.GiveWeaponItem(WeaponItem.MAGNUM);
                    m_player.GiveWeaponItem(WeaponItem.SNIPER);
                    m_player.GiveWeaponItem(WeaponItem.STRENGTHBOOST);
                    var mod = m_player.GetModifiers(); mod.SizeModifier = 1f; m_player.SetModifiers(mod);
                }

                m_aimDistanceText = (IObjectText)Game.CreateObject("Text", Vector2.Zero);
                m_aimDistanceText.SetTextScale(0.75f);
                m_aimVectorDots.Add("Arc", new List<IObjectText>());
                for (var i = 0; i < 20; i++)
                {
                    m_aimVectorDots["Arc"].Add(CreateNewDot(Color.Red));
                }
                m_aimVectorDots.Add("Center", new List<IObjectText>() { CreateNewDot(Color.Blue) });

                m_aabbDots.Add("Left", new List<IObjectText>());
                m_aabbDots.Add("Top", new List<IObjectText>());
                m_aabbDots.Add("Right", new List<IObjectText>());
                m_aabbDots.Add("Bottom", new List<IObjectText>());
                for (var i = 0; i < 25; i++) m_aabbDots["Left"].Add(CreateNewDot(Color.Yellow));
                for (var i = 0; i < 16; i++) m_aabbDots["Top"].Add(CreateNewDot(Color.Yellow));
                for (var i = 0; i < 25; i++) m_aabbDots["Right"].Add(CreateNewDot(Color.Yellow));
                for (var i = 0; i < 16; i++) m_aabbDots["Bottom"].Add(CreateNewDot(Color.Yellow));
            }

            private static float GetMinDistanceFromTarget(Vector2 start, Vector2 aimVector, Area aabb)
            {
                var minDistance = float.MaxValue;
                var corners = new Vector2[] { aabb.TopLeft, aabb.TopRight, aabb.BottomLeft, aabb.BottomRight };

                foreach (var corner in corners)
                {
                    var distance = ScriptHelper.GetMinDistance(start, start + aimVector * 200, corner);
                    minDistance = Math.Min(distance, minDistance);
                }

                return minDistance;
            }

            public static void OnUpdate(float elapsed)
            {
                var players = Game.GetPlayers();
                if (players.Length < 2) return;
                var target = players[1];

                Vector2 aimVector = m_player.AimVector;
                Vector2 start;
                Vector2 center;
                ScriptHelper.GetAimVectorInfo(m_player, out start, out center);
                var aabb = ScriptHelper.GetAABB(target);
                var distance = -1f;

                if (!ScriptHelper.IsIntersectArea(start, start + m_player.AimVector * 200, aabb))
                {
                    distance = GetMinDistanceFromTarget(start, m_player.AimVector, aabb);
                }

                var pos = m_player.GetWorldPosition();
                pos.Y += 20;
                pos.X -= 20;
                m_aimDistanceText.SetText("Aim distance: " + distance);
                m_aimDistanceText.SetWorldPosition(pos);

                DrawAABB(aabb);
                DrawAimVector(start, center, aimVector);
            }

            private static void DrawAABB(Area aabb)
            {
                var height = (int)aabb.Height;
                var width = (int)aabb.Width;

                DrawLine(m_aabbDots["Left"], aabb.TopLeft, new Vector2(0, -1), gap: 0, length: height);
                DrawLine(m_aabbDots["Top"], aabb.TopLeft, new Vector2(1, 0), gap: 0, length: width);
                DrawLine(m_aabbDots["Right"], aabb.BottomRight, new Vector2(0, 1), gap: 0, length: height);
                DrawLine(m_aabbDots["Bottom"], aabb.BottomRight, new Vector2(-1, 0), gap: 0, length: width);
            }

            public static void DrawAimVector(Vector2 start, Vector2 center, Vector2 aimVector)
            {
                SetDotPosition(m_aimVectorDots["Center"][0], center);
                DrawLine(m_aimVectorDots["Arc"], start, aimVector, 4);
            }

            private static void DrawLine(List<IObjectText> dots, Vector2 start, Vector2 direction, int gap = 0, int length = -1)
            {
                gap++;
                direction.Normalize();
                if (length == -1)
                    length = dots.Count;
                else
                    length = (int)MathHelper.Clamp(length, 1, dots.Count - 1);

                for (var i = 0; i < dots.Count; i++)
                {
                    var dot = dots[i];

                    if (i <= length - 1)
                        SetDotPosition(dot, start + direction * i * gap);
                    else
                        SetDotPosition(dot, new Vector2(Game.GetCameraMaxArea().Left - 1000, 0));
                }
            }

            private static void SetDotPosition(IObjectText dot, Vector2 position)
            {
                position.X -= 2;
                position.Y += 3;

                dot.SetWorldPosition(position);
            }

            private static IObjectText CreateNewDot(Color color)
            {
                var position = new Vector2(Game.GetCameraMaxArea().Left - 1000, 0);
                var dot = (IObjectText)Game.CreateObject("Text", position);

                dot.SetText(".");
                dot.SetTextColor(color);

                return dot;
            }
        }
    }
}
