using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    static class SetScreenColorsApp // Кастомные цвета в консоли
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;
            internal short Top;
            internal short Right;
            internal short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COLORREF
        {
            internal uint ColorDWORD;

            internal COLORREF(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }

            internal COLORREF(uint r, uint g, uint b)
            {
                ColorDWORD = r + (g << 8) + (b << 16);
            }

            internal Color GetColor()
            {
                return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
                                      (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            }

            internal void SetColor(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO_EX
        {
            internal int cbSize;
            internal COORD dwSize;
            internal COORD dwCursorPosition;
            internal ushort wAttributes;
            internal SMALL_RECT srWindow;
            internal COORD dwMaximumWindowSize;
            internal ushort wPopupAttributes;
            internal bool bFullscreenSupported;
            internal COLORREF black;
            internal COLORREF darkBlue;
            internal COLORREF darkGreen;
            internal COLORREF darkCyan;
            internal COLORREF darkRed;
            internal COLORREF darkMagenta;
            internal COLORREF darkYellow;
            internal COLORREF gray;
            internal COLORREF darkGray;
            internal COLORREF blue;
            internal COLORREF green;
            internal COLORREF cyan;
            internal COLORREF red;
            internal COLORREF magenta;
            internal COLORREF yellow;
            internal COLORREF white;
        }

        const int STD_OUTPUT_HANDLE = -11;                                        // per WinBase.h
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);    // per WinBase.h

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        // Set a specific console color to an RGB color
        // The default console colors used are gray (foreground) and black (background)
        public static int SetColor(ConsoleColor consoleColor, Color targetColor)
        {
            return SetColor(consoleColor, targetColor.R, targetColor.G, targetColor.B);
        }

        public static int SetColor(ConsoleColor color, uint r, uint g, uint b)
        {
            CONSOLE_SCREEN_BUFFER_INFO_EX csbe = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            csbe.cbSize = (int)Marshal.SizeOf(csbe);                    // 96 = 0x60
            IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);    // 7
            if (hConsoleOutput == INVALID_HANDLE_VALUE)
            {
                return Marshal.GetLastWin32Error();
            }
            bool brc = GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }

            switch (color)
            {
                case ConsoleColor.Black:
                    csbe.black = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkBlue:
                    csbe.darkBlue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGreen:
                    csbe.darkGreen = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkCyan:
                    csbe.darkCyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkRed:
                    csbe.darkRed = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkMagenta:
                    csbe.darkMagenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkYellow:
                    csbe.darkYellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Gray:
                    csbe.gray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGray:
                    csbe.darkGray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Blue:
                    csbe.blue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Green:
                    csbe.green = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Cyan:
                    csbe.cyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Red:
                    csbe.red = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Magenta:
                    csbe.magenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Yellow:
                    csbe.yellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.White:
                    csbe.white = new COLORREF(r, g, b);
                    break;
            }
            ++csbe.srWindow.Bottom;
            ++csbe.srWindow.Right;
            brc = SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public static int SetScreenColors(Color foregroundColor, Color backgroundColor)
        {
            int irc;
            irc = SetColor(ConsoleColor.Gray, foregroundColor);
            if (irc != 0) return irc;
            irc = SetColor(ConsoleColor.Black, backgroundColor);
            if (irc != 0) return irc;

            return 0;
        }
    }
    class Program
    {
        #region Подключение библиотек

        //Доступ к буферу консоли
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // Выбор шрифтра консоли
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }
        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int dwType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SetConsoleFont(IntPtr hOut, uint dwFontNum);

        #endregion

        static Random Rand = new Random();
        static SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        static void Main()
        {
            SetConsoleColor();
            Console.BackgroundColor = (ConsoleColor)0;
            Console.ForegroundColor = (ConsoleColor)15;
            SetConsoleFont("Consolas", 13);

            int steps = 0;
            int alive = 0;

            int avg = 0;
            int avgCount = 0;
            int stepsPerCount = 0;

            int fps = 0;

            int bornChance = 3;

            int aliveCount = 2;
            int bornCount = 3;
            int deathCount = 4;

            int worldH = (int)(Console.LargestWindowHeight * 0.95);
            int worldW = (int)(Console.LargestWindowWidth / 2d * 0.95);

            Console.SetWindowSize(worldW * 2, worldH + 1);
            Console.SetBufferSize(worldW * 2, worldH + 1);

            int[] world = new int[worldH * worldW];
            for (int i = 0; i < worldH; i++)
            {
                for (int j = 0; j < worldW; j++)
                {
                    world[i * worldW + j] = Rand.Next(0, 2) == 1 ? 64 : 0;
                }
            }

            DateTime start = DateTime.Now;

            while (true)
            {
                int[] temp = new int[worldH * worldW];

                for (int i = 0; i < worldH; i++)
                {
                    for (int j = 0; j < worldW; j++)
                    {
                        int nbrs = 0;

                        if ((i == 0 && j == 0 && world[(worldH - 1) * worldW + worldW - 1] == 64) || (i > 0 && j > 0 && world[(i - 1) * worldW + j - 1] == 64))
                            nbrs++;
                        if ((i == 0 && j == worldW - 1 && world[(worldH - 1) * worldW] == 64) || (i > 0 && j < worldW - 1 && world[(i - 1) * worldW + j + 1] == 64))
                            nbrs++;
                        if ((i == worldH - 1 && j == 0 && world[worldW - 1] == 64) || (i < worldH - 1 && j > 0 && world[(i + 1) * worldW + j - 1] == 64))
                            nbrs++;
                        if ((i == worldH - 1 && j == worldW - 1 && world[0] == 64) || (i < worldH - 1 && j < worldW - 1 && world[(i + 1) * worldW + j + 1] == 64))
                            nbrs++;

                        if ((i == 0 && world[(worldH - 1) * worldW + j] == 64) || (i > 0 && world[(i - 1) * worldW + j] == 64))
                            nbrs++;
                        if ((j == 0 && world[i * worldW + worldW - 1] == 64) || (j > 0 && world[i * worldW + j - 1] == 64))
                            nbrs++;
                        if ((i == worldH - 1 && world[j] == 64) || (i < worldH - 1 && world[(i + 1) * worldW + j] == 64))
                            nbrs++;
                        if ((j == worldW - 1 && world[i * worldW] == 64) || (j < worldW - 1 && world[i * worldW + j + 1] == 64))
                            nbrs++;

                        /*if (i > 0 && world[(i - 1) * worldW + j] == 64 || i == 0 && world[(worldH - 1) * worldW + j] == 64)
                            nbrs++;
                        if (i < worldH - 1 && world[(i + 1) * worldW + j] == 64 || i == worldH - 1 && world[j] == 64)
                            nbrs++;
                        if (j > 0 && world[i * worldW + j - 1] == 64 || j == 0 && world[i * worldW + worldW - 1] == 64)
                            nbrs++;
                        if (j < worldW - 1 && world[i * worldW + j + 1] == 64 || j == worldW - 1 && world[i * worldW] == 64)
                            nbrs++;

                        if (i > 0 && j > 0 && world[(i - 1) * worldW + j - 1] == 64 || i == 0 && j == 0 && world[(worldH - 1) * worldW + worldW - 1] == 64)
                            nbrs++;
                        if (i < worldH - 1 && j < worldW - 1 && world[(i + 1) * worldW + j + 1] == 64 || i == worldH - 1 && j == worldW - 1 && world[0] == 64)
                            nbrs++;
                        if (i > 0 && j < worldW - 1 && world[(i - 1) * worldW + j + 1] == 64 || i == 0 && j == worldW - 1 && world[(worldH - 1) * worldW] == 64)
                            nbrs++;
                        if (i < worldH - 1 && j > 0 && world[(i + 1) * worldW + j - 1] == 64 || i == worldH - 1 && j == 0 && world[worldW - 1] == 64)
                            nbrs++;*/


                        if (Rand.Next(0, 10000) < bornChance)
                            temp[i * worldW + j] = 64;

                        else if (world[i * worldW + j] < 64)
                        {
                            if (nbrs >= bornCount && nbrs < deathCount)
                            {
                                temp[i * worldW + j] = 64;
                                alive++;
                            }
                            else if (world[i * worldW + j] > 0)
                                temp[i * worldW + j] = world[i * worldW + j] - 1;
                        }
                        else if (world[i * worldW + j] == 64)
                        {
                            if (nbrs < aliveCount || nbrs >= deathCount)
                                temp[i * worldW + j] = 63;
                            else
                            {
                                temp[i * worldW + j] = 64;
                                alive++;
                            }
                        }
                    }
                }

                steps++;

                Console.CursorVisible = false;

                if (!h.IsInvalid)
                {
                    CharInfo[] buf = new CharInfo[worldW * 2 * worldH];
                    SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)(worldW * 2), Bottom = (short)worldH };

                    for (int i = 0; i < temp.Length; i++)
                    {
                        int tempSympol = 0;
                        int tempColor = 0;

                        if (temp[i] == 64 || temp[i] == 60 || temp[i] == 56 || temp[i] == 52 || temp[i] == 48 || temp[i] == 44 || temp[i] == 40 || temp[i] == 36 || temp[i] == 32 || temp[i] == 28 || temp[i] == 24 || temp[i] == 20 || temp[i] == 16 || temp[i] == 12 || temp[i] == 8 || temp[i] == 4)
                            tempSympol = 219;
                        else if (temp[i] == 63 || temp[i] == 59 || temp[i] == 55 || temp[i] == 51 || temp[i] == 47 || temp[i] == 43 || temp[i] == 39 || temp[i] == 35 || temp[i] == 31 || temp[i] == 27 || temp[i] == 23 || temp[i] == 19 || temp[i] == 15 || temp[i] == 11 || temp[i] == 7 || temp[i] == 3)
                            tempSympol = 178;
                        else if (temp[i] == 62 || temp[i] == 58 || temp[i] == 54 || temp[i] == 50 || temp[i] == 46 || temp[i] == 42 || temp[i] == 38 || temp[i] == 34 || temp[i] == 30 || temp[i] == 26 || temp[i] == 22 || temp[i] == 18 || temp[i] == 14 || temp[i] == 10 || temp[i] == 6 || temp[i] == 2)
                            tempSympol = 177;
                        else if (temp[i] == 61 || temp[i] == 57 || temp[i] == 53 || temp[i] == 49 || temp[i] == 45 || temp[i] == 41 || temp[i] == 37 || temp[i] == 33 || temp[i] == 29 || temp[i] == 25 || temp[i] == 21 || temp[i] == 17 || temp[i] == 13 || temp[i] == 9 || temp[i] == 5 || temp[i] == 1)
                            tempSympol = 176;

                        if (temp[i] <= 64 && temp[i] >= 61)
                            tempColor = 15;
                        else if (temp[i] <= 60 && temp[i] >= 57)
                            tempColor = 14;
                        else if (temp[i] <= 56 && temp[i] >= 53)
                            tempColor = 13;
                        else if (temp[i] <= 52 && temp[i] >= 49)
                            tempColor = 12;
                        else if (temp[i] <= 48 && temp[i] >= 45)
                            tempColor = 11;
                        else if (temp[i] <= 44 && temp[i] >= 41)
                            tempColor = 10;
                        else if (temp[i] <= 40 && temp[i] >= 37)
                            tempColor = 9;
                        else if (temp[i] <= 36 && temp[i] >= 33)
                            tempColor = 8;
                        else if (temp[i] <= 32 && temp[i] >= 29)
                            tempColor = 7;
                        else if (temp[i] <= 28 && temp[i] >= 25)
                            tempColor = 6;
                        else if (temp[i] <= 24 && temp[i] >= 21)
                            tempColor = 5;
                        else if (temp[i] <= 20 && temp[i] >= 17)
                            tempColor = 4;
                        else if (temp[i] <= 16 && temp[i] >= 13)
                            tempColor = 3;
                        else if (temp[i] <= 12 && temp[i] >= 9)
                            tempColor = 2;
                        else if (temp[i] <= 8 && temp[i] >= 5)
                            tempColor = 1;

                        buf[i * 2 + 1].Attributes = (short)(tempColor >= 2 ? (tempColor | (tempColor - 1) << 4) : 1);
                        buf[i * 2 + 1].Char.AsciiChar = (byte)tempSympol;

                        buf[i * 2].Attributes = (short)(tempColor >= 2 ? (tempColor | (tempColor - 1) << 4) : 1);
                        buf[i * 2].Char.AsciiChar = (byte)tempSympol;
                    }

                    WriteConsoleOutput(h, buf,
                        new Coord() { X = (short)(worldW * 2), Y = (short)worldH },
                        new Coord() { X = 0, Y = 0 },
                        ref rect);
                }

                Console.Title = $"FPS: {fps}     < Game Of Life >     " + $"Alive: {alive} | AVG Alive: {avg} | Gen: {steps}";

                if ((DateTime.Now - start).TotalMilliseconds >= 1000)
                {
                    fps = stepsPerCount;

                    if (stepsPerCount > 0)
                        avg = avgCount / stepsPerCount;
                    else
                        avg = alive;

                    stepsPerCount = 0;
                    avgCount = 0;
                    start = DateTime.Now;
                }
                else
                {
                    stepsPerCount++;
                    avgCount += alive;
                }

                world = temp;
                alive = 0;
                //Console.ReadKey();
            }
        }


        static void SetConsoleFont(string fontName, short fontSizeY)
        {
            unsafe
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                    newInfo.dwFontSize = new COORD(info.dwFontSize.X, fontSizeY);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }
        static void SetConsoleColor()
        {
            //Гардиент от тёмного к светлому
            SetScreenColorsApp.SetColor((ConsoleColor)0, 0, 0, 0);
            SetScreenColorsApp.SetColor((ConsoleColor)1, 0, 0, 10); 
            SetScreenColorsApp.SetColor((ConsoleColor)2, 0, 0, 20); 
            SetScreenColorsApp.SetColor((ConsoleColor)3, 10, 10, 30); 
            SetScreenColorsApp.SetColor((ConsoleColor)4, 20, 20, 40); 
            SetScreenColorsApp.SetColor((ConsoleColor)5, 30, 30, 50); 
            SetScreenColorsApp.SetColor((ConsoleColor)6, 40, 40, 60); 
            SetScreenColorsApp.SetColor((ConsoleColor)7, 50, 50, 70); 
            SetScreenColorsApp.SetColor((ConsoleColor)8, 60, 60, 80); 
            SetScreenColorsApp.SetColor((ConsoleColor)9, 70, 70, 90); 
            SetScreenColorsApp.SetColor((ConsoleColor)10, 80, 80, 100); 
            SetScreenColorsApp.SetColor((ConsoleColor)11, 90, 90, 110);
            SetScreenColorsApp.SetColor((ConsoleColor)12, 100, 100, 120);
            SetScreenColorsApp.SetColor((ConsoleColor)13, 120, 120, 130);
            SetScreenColorsApp.SetColor((ConsoleColor)14, 130, 130, 140);
            SetScreenColorsApp.SetColor((ConsoleColor)15, 255, 255, 255);
        }
    }
}
