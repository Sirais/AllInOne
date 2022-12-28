using Druzil.Poe.Libs;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.Models;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.AtlasHelper;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TreeRoutine.Menu;

namespace AllInOne.Misc
{
    /// <summary>
    /// /// Found this in Plugin UnstackDecks and didnt have to reinvent. Thanks to the Creator
    /// </summary>
    internal static class ArrayExtensions
    {
        public static void Fill(this int[,] matrix, int value, SharpDX.Vector2 pos)
        {
            matrix.Fill(value, (int)pos.X, (int)pos.Y);
        }

        public static void Fill(this int[,] matrix, int value, int col, int row)
        {
            if (col < 0 || row < 0 || row >= matrix.GetLength(0) && col < matrix.GetLength(1))
            {
                matrix[row, col] = value;
            }
        }

        public static void Fill(this int[,] matrix, int value, int col, int row, int width, int height)
        {
            for (var r = row; r < row + height; r++)
            {
                for (var c = col; c < col + width; c++)
                {
                    if (r >= 0 && c >= 0 && r < matrix.GetLength(0) && c < matrix.GetLength(1))
                    {
                        matrix[r, c] = value;
                    }
                }
            }
        }

        public static bool GetNextOpenSlot(this int[,] matrix, ref Point start)
        {
            if (start.X < 0 || start.Y < 0 || start.X >= matrix.GetLength(1) || start.Y >= matrix.GetLength(0))
            {
                start.X = -1;
                start.Y = -1;
                return false;
            }

            for (; start.X < matrix.GetLength(1); start.X++)
            {
                for (; start.Y < matrix.GetLength(0); start.Y++)
                {
                    if (matrix[start.Y, start.X] == 0)
                    {
                        matrix[start.Y, start.X] = 1;
                        return true;
                    }
                }
                start.Y = 0;
            }
            start.X = -1;
            start.Y = -1;
            return false;
        }
        public static string Print(this int[,] matrix)
        {
            string p = "";
            for (int y = 0; y < matrix.GetLength(0); y++)
            {
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    p += matrix[y, x].ToString();
                }
                p += "-";
            }
            return p;
        }
    }
}

