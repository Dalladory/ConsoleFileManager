using System;
using System.Collections.Generic;
using System.IO;

namespace FileManager
{
    struct BlockProperties
    {
        public int Name { get; private set; } // 30%
        public int CreationTime { get; private set; } // 25%

        public int DateOfChange { get; private set; } // 25%

        public int Type { get; private set; } // 10%

        public int Size { get; private set; } // 10%

        // X Coordinate

        public int XName { get; private set; }
        public int XCreationTime { get; private set; }

        public int XDateOfChange { get; private set; }

        public int XType { get; private set; }

        public int XSize { get; private set; }

        public int FooterOptionsSpace { get; set; }

        public BlockProperties(int consolewidth)
        {
            Name = consolewidth * 40 / 100;
            CreationTime = consolewidth * 20 / 100;
            DateOfChange = consolewidth * 20 / 100;
            Type = consolewidth * 10 / 100;
            Size = consolewidth * 10 / 100;

            XName = 3;
            XCreationTime = Name;
            XDateOfChange = Name + CreationTime;
            XType = Name + CreationTime + DateOfChange;
            XSize = Name + CreationTime + DateOfChange + Type;

            FooterOptionsSpace = consolewidth / 6;
        }
    }

    class FileMan
    {
        protected BlockProperties blockProperties;
        protected string currentDirectory = "";
        protected List<string> dirOrFileNames = new List<string>();
        protected DriveInfo[] drives = DriveInfo.GetDrives();

        protected int foldersCount;
        protected int topIndexInObjectsArr;
        protected int menuPosition;
        protected int maxObjectsCountInScreen;
        protected int objectCountInLastScreen;

        protected int consoleHeight = Console.WindowHeight;
        protected int consoleWidth = Console.WindowWidth;

        protected ConsoleColor backColorObject = ConsoleColor.Blue;
        protected ConsoleColor foreColorObject = ConsoleColor.White;

        protected ConsoleColor backColorObjectSelected = ConsoleColor.Red;
        protected ConsoleColor foreColorObjectSelected = ConsoleColor.Black;

        public FileMan()
        {
            maxObjectsCountInScreen = consoleHeight - 6;
            PrintOptions();
        }
        
        protected void SetDirOrFiles()
        {
            dirOrFileNames.Clear();
            dirOrFileNames.InsertRange(0, Directory.GetDirectories(currentDirectory));
            foldersCount = dirOrFileNames.Count;
            dirOrFileNames.InsertRange(dirOrFileNames.Count == 0 ? 0 : dirOrFileNames.Count, Directory.GetFiles(currentDirectory));
        }
        protected void SetDrives()
        {
            dirOrFileNames.Clear();
            foreach (var drive in drives)
            {
                dirOrFileNames.Add(drive.Name);
            }
            topIndexInObjectsArr = 0;
            foldersCount = drives.Length;
        }
        protected void ClearRow(int YPosition)
        {
            Console.SetCursorPosition(0, YPosition);
            Console.BackgroundColor = ConsoleColor.Black;
            for (int i = 0; i < consoleWidth; i++)
            {
                Console.Write(' ');
            }
            Console.SetCursorPosition(0, YPosition);
        }
        protected void ClearAllObjects()
        {
            for (int i = 2; i < consoleHeight - 2; i++)
            {
                ClearRow(i);
            }
        }
        protected void PrintObject(int indexObjectArr)
        {
            int top = Console.GetCursorPosition().Top;
            FileSystemInfo info;
            string type;
            string size = "";

            if (indexObjectArr < foldersCount)
            {
                type = "Folder";
                info = new DirectoryInfo(Path.Combine(currentDirectory, dirOrFileNames[indexObjectArr]));
            }
            else
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(currentDirectory, dirOrFileNames[indexObjectArr]));
                info = fileInfo;
                type = "File";
                size = FileLengthConverter(fileInfo.Length);
            }
            Console.SetCursorPosition(1, top);
            Console.Write(info.Name);

            Console.SetCursorPosition(blockProperties.XCreationTime, top);
            Console.Write(info.CreationTime);

            Console.SetCursorPosition(blockProperties.XDateOfChange, top);
            Console.Write(info.CreationTime);

            Console.SetCursorPosition(blockProperties.XType, top);
            Console.Write(type);

            Console.SetCursorPosition(blockProperties.XSize, top);
            Console.Write(size);
        }
        protected void PrintObjectsOneScreen()
        {
            if (dirOrFileNames.Count == 0) return;
            ClearAllObjects();
            Console.SetCursorPosition(1, 2);
            SetConsoleColorObjectSelected();
            PrintObject(topIndexInObjectsArr);
            menuPosition = 0;
            SetConsoleColorObject();
            objectCountInLastScreen = 1;
            for (int i = topIndexInObjectsArr + 1, j = 0; j < maxObjectsCountInScreen && i < dirOrFileNames.Count; i++, j++)
            {
                Console.WriteLine();
                PrintObject(i);
                ++objectCountInLastScreen;
            }
        }
        protected void PrintOptions()
        {
            //hide scrollbar and cursor
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.CursorVisible = false;
            blockProperties = new BlockProperties(consoleWidth);

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.SetCursorPosition(blockProperties.XName, 0);
            Console.Write("Name");

            Console.SetCursorPosition(blockProperties.XCreationTime, 0);
            Console.Write("Creation Time");

            Console.SetCursorPosition(blockProperties.XDateOfChange, 0);
            Console.Write("Date of Change");

            Console.SetCursorPosition(blockProperties.XType, 0);
            Console.Write("Type");

            Console.SetCursorPosition(blockProperties.XSize, 0);
            Console.Write("Size");

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(1, consoleHeight - 1);
            Console.Write("1 - Delete");

            Console.SetCursorPosition(blockProperties.FooterOptionsSpace, consoleHeight - 1);
            Console.Write("2 - New File");

            Console.SetCursorPosition(blockProperties.FooterOptionsSpace * 2, consoleHeight - 1);
            Console.Write("3 - New Folder");

            Console.SetCursorPosition(blockProperties.FooterOptionsSpace * 3, consoleHeight - 1);
            Console.Write("4 - Rename");

            Console.SetCursorPosition(blockProperties.FooterOptionsSpace * 4, consoleHeight - 1);
            Console.Write("5 - Copy");

            Console.SetCursorPosition(blockProperties.FooterOptionsSpace * 5, consoleHeight - 1);
            Console.Write("6 - Move");

            Console.SetCursorPosition(0, 2);
        }
        protected void SetConsoleColorObject()
        {
            Console.BackgroundColor = backColorObject;
            Console.ForegroundColor = foreColorObject;
        }
        protected void SetConsoleColorObjectSelected()
        {
            Console.BackgroundColor = backColorObjectSelected;
            Console.ForegroundColor = foreColorObjectSelected;
        }
        protected string FileLengthConverter(long bytes)
        {
            if (bytes <= 1024)
            {
                return bytes + "BT";
            }

            double kilobytes = Math.Round(bytes / 1024f);

            if (kilobytes > 0 && kilobytes <= 1024)
            {
                return kilobytes.ToString() + "KB";
            }

            double megabytes = Math.Round(bytes / 1024f / 1024f);
            if (megabytes > 0 && megabytes <= 1024)
            {
                return megabytes.ToString() + "MB";
            }

            double gigabytes = Math.Round(bytes / 1024f / 1024f / 1024f);
            if (gigabytes > 0 && gigabytes <= 1024)
            {
                return gigabytes.ToString() + "GB";
            }

            return Math.Round(bytes / 1024f / 1024f / 1024f / 1024f).ToString() + "TB";
        }
        protected void PrintMessage(string msg, bool clearObjects, int x, int y)
        {
            if (clearObjects)
                ClearAllObjects();
            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(msg);
        }
        protected string GetNewObjectName()
        {
            Console.SetCursorPosition(2, consoleHeight - 2);
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("New Name: ");
            string name = Console.ReadLine();
            SetConsoleColorObject();
            return name;
        }
    }
}