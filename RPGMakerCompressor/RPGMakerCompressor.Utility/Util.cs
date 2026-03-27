using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Utility
{
    public class Util
    {
        private static List<(string Color, string Text)> ProcessMultiColoredText(string Text)
        {
            List<(string Color, string Text)> coloredText = new List<(string Color, string Text)>();

            bool readingColor = false;

            (string Color, string Text) currentPair = ("", "");

            for (int i = 0; i < Text.Length; i++)
            {
                char currentChar = Text[i];

                // Conditional statement to start reading colors or text.
                if (currentChar == '[')
                {
                    // Add current segment.
                    if (currentPair.Text != "")
                    {
                        if (currentPair.Color == "")
                        {
                            currentPair.Color = "Default";
                        }

                        coloredText.Add(currentPair);
                        currentPair = ("", "");
                    }

                    readingColor = true;
                    continue;
                }
                else if (currentChar == ']')
                {
                    readingColor = false;
                    continue;
                }

                ref string writeTo = ref (readingColor ? ref currentPair.Color : ref currentPair.Text);

                writeTo += currentChar;
            }

            // Add last segment.
            if (currentPair.Text != "")
            {
                if (currentPair.Color == "")
                {
                    currentPair.Color = "Default";
                }

                coloredText.Add(currentPair);
                currentPair = ("", "");
            }

            return coloredText;
        }

        public static long GetFolderSize(string folderPath)
        {
            return Directory
                .EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }

        // Delete's the smaller file of the 2 files provided.
        // Returns 2 if file 2 was smaller, else returns 1 if file 1 was smaller.
        public static int KeepSmallestFile(string file1Path, string file2Path)
        {
            FileInfo file1 = new FileInfo(file1Path);
            FileInfo file2 = new FileInfo(file2Path);

            // Only replace old file if the new compressed one is smaller.
            if (file2.Length < file1.Length)
            {
                file1.Delete();
                return 2;
            }
            else
            {
                file2.Delete();
                return 1;
            }
        }
        public static void WriteMultiColored(string Text)
        {
            // Input example: [Yellow]This is yellow text! [Green]This is green text!

            List<(string Color, string Text)> parsedText = ProcessMultiColoredText(Text);

            for (int i = 0; i < parsedText.Count; i++)
            {
                (string Color, string Text) currentPair = parsedText[i];

                if (Enum.TryParse<ConsoleColor>(currentPair.Color, true, out var color))
                {
                    Console.ForegroundColor = color;
                }
                else
                {
                    Console.ResetColor();
                }

                Console.Write(currentPair.Text);
            }

            Console.ResetColor();
        }
        public static void RewriteLastLine(string Text, bool ColoredText = false)
        {
            int lastLine = Console.CursorTop - 1;

            Console.SetCursorPosition(0, lastLine);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, lastLine);

            if (ColoredText)
            {
                WriteLineMultiColored(Text);
            }
            else
            {
                Console.WriteLine(Text);
            }
        }
        public static void WriteLineMultiColored(string Text)
        {
            WriteMultiColored(Text);
            Console.Write('\n');
        }

        public static void WriteLineColored(string Text, ConsoleColor Color = ConsoleColor.White)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(Text);
            Console.ResetColor();
        }
        public static string QuestionInput(string Question, bool ColoredText = false, bool ClearConsole = false)
        {
            if (ClearConsole)
            {
                Console.Clear();
            }

            if (ColoredText)
            {
                WriteMultiColored(Question);
            }
            else
            {
                Console.Write(Question);
            }

            return Console.ReadLine();
        }
    }
}
