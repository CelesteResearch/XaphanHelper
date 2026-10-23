using System;
using System.Collections.Generic;
using Celeste.Core.Reflection.Runtime;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.XaphanHelper.Presentation.Outlines
{
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.UI_Elements.OutlinePoint")]
    public class OutlinePoint
    {
        public float x;

        public float y;

        public bool visible;

        public OutlinePoint(float x, float y, bool visible)
        {
            this.x = x;
            this.y = y;
            this.visible = visible;
        }

        public static List<OutlinePoint> GenerateSolidOutline(SourceSolidEntityState solid)
        {
            List<OutlinePoint> list = new List<OutlinePoint>();
            for (int i = (int)solid.Width / 2; (float)i < solid.Width; i++)
            {
                Math.DivRem(i, 4, out var result);
                if (result == 1 || result == 2)
                {
                    list.Add(new OutlinePoint(i, 0f, visible: true));
                }
                else
                {
                    list.Add(new OutlinePoint(i, 0f, visible: false));
                }
            }

            for (int j = 0; (float)j < solid.Height; j++)
            {
                Math.DivRem(j, 4, out var result2);
                if (result2 == 1 || result2 == 2)
                {
                    list.Add(new OutlinePoint(solid.Width - 1f, j, visible: true));
                }
                else
                {
                    list.Add(new OutlinePoint(solid.Width - 1f, j, visible: false));
                }
            }

            for (int num = (int)solid.Width; num >= 0; num--)
            {
                Math.DivRem(num, 4, out var result3);
                if (result3 == 1 || result3 == 2)
                {
                    list.Add(new OutlinePoint(num, solid.Height - 1f, visible: true));
                }
                else
                {
                    list.Add(new OutlinePoint(num, solid.Height - 1f, visible: false));
                }
            }

            for (int num2 = (int)solid.Height; num2 >= 0; num2--)
            {
                Math.DivRem(num2, 4, out var result4);
                if (result4 == 1 || result4 == 2)
                {
                    list.Add(new OutlinePoint(0f, num2, visible: true));
                }
                else
                {
                    list.Add(new OutlinePoint(0f, num2, visible: false));
                }
            }

            for (int k = 0; (float)k < solid.Width / 2f; k++)
            {
                Math.DivRem(k, 4, out var result5);
                if (result5 == 1 || result5 == 2)
                {
                    list.Add(new OutlinePoint(k, 0f, visible: true));
                }
                else
                {
                    list.Add(new OutlinePoint(k, 0f, visible: false));
                }
            }

            list.Reverse();
            return list;
        }
    }
}
