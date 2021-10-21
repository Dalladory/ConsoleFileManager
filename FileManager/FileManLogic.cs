using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;

namespace FileManager
{
    class FileManLogic : FileMan
    {
        private bool isCopied;
        private bool isMoved;
        private bool isDirectory;

        private string objectPath;

        public void Logic()
        {
            SetDrives();
            PrintObjectsOneScreen();
            ConsoleKeyInfo keyInfo;

            while (true)
            {
                Console.Title = currentDirectory + $"        Folders: {foldersCount}   Files: {dirOrFileNames.Count - foldersCount}";


                if (dirOrFileNames.Count == 0)
                {
                    PrintMessage("Folder is empty", true, 1, 2);
                }

                keyInfo = Console.ReadKey();
                ClearRow(consoleHeight - 2);

                ResizeFix();

                if (dirOrFileNames.Count == 0 && (keyInfo.Key == ConsoleKey.UpArrow ||
                    keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.Enter ||
                    keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.D4 ||
                    keyInfo.Key == ConsoleKey.D5 || keyInfo.Key == ConsoleKey.D6)) { continue; }


                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        {
                            MoveUp();
                        }; break;
                    case ConsoleKey.DownArrow:
                        {
                            MoveDown();
                        }; break;
                    case ConsoleKey.Enter:
                        {
                            MoveForvard();
                        }; break;
                    case ConsoleKey.Backspace:
                        {
                            MoveBack();
                        }; break;

                    case ConsoleKey.D1:
                        {
                            Delete();
                        }; break;
                    case ConsoleKey.D2:
                        {
                            NewFile();
                        }; break;
                    case ConsoleKey.D3:
                        {
                            NewFolder();
                        }; break;
                    case ConsoleKey.D4:
                        {
                            Rename();
                        }; break;
                    case ConsoleKey.D5:
                        {
                            CopyAndMove(true);
                        }; break;
                    case ConsoleKey.D6:
                        {
                            CopyAndMove(false);
                        }; break;
                    case ConsoleKey.Spacebar:
                        {
                            try
                            {
                                if (isCopied)
                                {
                                    CopyStep2();
                                }

                                if (isMoved)
                                {
                                    MoveStep2();
                                }

                                SetDirOrFiles();
                                topIndexInObjectsArr = 0;
                                PrintObjectsOneScreen();
                            }
                            catch (Exception ex)
                            {
                                PrintMessage(ex.Message, false, 1, consoleHeight - 2);
                            }
                            
                        }; break;
                }
                ResizeFix();
            }
        }

        private void MoveUp()
        {
            if (menuPosition - 1 >= 0)
            {
                ClearRow(menuPosition + 2);
                SetConsoleColorObject();
                PrintObject(topIndexInObjectsArr + menuPosition);

                --menuPosition;
                ClearRow(menuPosition + 2);
                SetConsoleColorObjectSelected();

                PrintObject(topIndexInObjectsArr + menuPosition);
            }
            else
            {
                if (topIndexInObjectsArr > 0)
                {
                    topIndexInObjectsArr = topIndexInObjectsArr - maxObjectsCountInScreen - 1 >= 0 ? topIndexInObjectsArr - maxObjectsCountInScreen - 1 : 0;
                    PrintObjectsOneScreen();
                }
            }
        }
        private void MoveDown()
        {
            if (menuPosition + 1 <= maxObjectsCountInScreen && topIndexInObjectsArr + menuPosition + 1 < dirOrFileNames.Count)
            {
                ClearRow(menuPosition + 2);
                SetConsoleColorObject();
                PrintObject(topIndexInObjectsArr + menuPosition);

                ++menuPosition;
                ClearRow(menuPosition + 2);
                SetConsoleColorObjectSelected();

                PrintObject(topIndexInObjectsArr + menuPosition);
            }
            else
            {
                if (topIndexInObjectsArr + menuPosition < dirOrFileNames.Count - 1)
                {
                    topIndexInObjectsArr = topIndexInObjectsArr + objectCountInLastScreen < dirOrFileNames.Count ? topIndexInObjectsArr + objectCountInLastScreen : dirOrFileNames.Count - 1;
                    PrintObjectsOneScreen();
                }
            }
        }
        private void MoveForvard()
        {
            currentDirectory = dirOrFileNames[topIndexInObjectsArr + menuPosition];

            try
            {
                if (topIndexInObjectsArr + menuPosition < foldersCount)
                {
                    SetDirOrFiles();
                    topIndexInObjectsArr = 0;
                    PrintObjectsOneScreen();
                }
                else
                {
                    using (Process fileOpener = new Process())
                    {
                        fileOpener.StartInfo.FileName = "explorer";
                        fileOpener.StartInfo.Arguments = dirOrFileNames[topIndexInObjectsArr + menuPosition];
                        fileOpener.Start();
                    }
                    currentDirectory = Directory.GetParent(currentDirectory).FullName;
                }
            }
            catch (Exception ex)
            {
                currentDirectory = Directory.GetParent(currentDirectory).FullName;
                SetDirOrFiles();
                PrintMessage(ex.Message, false, 2, consoleHeight - 2);
            }

        }
        private void MoveBack()
        {
            if (Directory.GetLogicalDrives().Contains(currentDirectory))
            {
                SetDrives();
            }
            else
            {
                currentDirectory = Directory.GetParent(currentDirectory).FullName;
                topIndexInObjectsArr = 0;
                SetDirOrFiles();
            }
            PrintObjectsOneScreen();
        }

        private void Delete()
        {
            try
            {
                if (topIndexInObjectsArr + menuPosition < foldersCount)
                {
                    Directory.Delete(Path.Combine(currentDirectory, dirOrFileNames[topIndexInObjectsArr + menuPosition]), true);
                }
                else
                {
                    File.Delete(Path.Combine(currentDirectory, dirOrFileNames[topIndexInObjectsArr + menuPosition]));
                }
                SetDirOrFiles();
                topIndexInObjectsArr = 0;
                PrintObjectsOneScreen();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message, false, 2, consoleHeight - 2);
            }
        }
        private void NewFile()
        {
            try
            {
                string newFilePath = Path.Combine(currentDirectory, GetNewObjectName());
                if(File.Exists(newFilePath))
                {
                    PrintMessage("File name already axist", false, 2, consoleHeight - 2);
                    return;
                }

                File.Create(newFilePath).Close();
                SetDirOrFiles();
                PrintObjectsOneScreen();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message, false, 2, consoleHeight - 2);
            }
        }
        private void NewFolder()
        {
            try
            {
                string newFolderPath = Path.Combine(currentDirectory, GetNewObjectName());

                if (Directory.Exists(newFolderPath))
                {
                    PrintMessage("Folder name already axist", false, 2, consoleHeight - 2);
                    return;
                }

                Directory.CreateDirectory(newFolderPath);

                SetDirOrFiles();
                PrintObjectsOneScreen();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message, false, 2, consoleHeight - 2);
            }
        }
        private void Rename()
        {
            try
            {
                string newObjectName = GetNewObjectName();

                if (topIndexInObjectsArr + menuPosition < foldersCount)
                {
                    FileSystem.RenameDirectory(dirOrFileNames[topIndexInObjectsArr + menuPosition], newObjectName);
                }
                else
                {
                    FileSystem.RenameFile(dirOrFileNames[topIndexInObjectsArr + menuPosition], newObjectName);
                }

                topIndexInObjectsArr = 0;
                SetDirOrFiles();
                PrintObjectsOneScreen();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message, false, 2, consoleHeight - 2);
            }
        }
        private void CopyAndMove(bool isCopy)
        {
            if(isCopy) { isCopied = true; }
            else { isMoved = true; }
            
            objectPath = dirOrFileNames[topIndexInObjectsArr + menuPosition];
            if (topIndexInObjectsArr + menuPosition < foldersCount) { isDirectory = true; }
            else { isDirectory = false; }
            PrintMessage("Go to the desired folder and press the Space", false, 2, consoleHeight - 2);
        }
        private void CopyStep2()
        {
            isCopied = false;
            string resultPath = Path.Combine(currentDirectory, Path.GetFileName(objectPath));

            if (isDirectory)
            {
                FileSystem.CopyDirectory(objectPath, resultPath, true);
            }
            else
            {
                FileSystem.CopyFile(objectPath, resultPath);
            }
        }
        private void MoveStep2()
        {
            isMoved = false;
            string resultPath = Path.Combine(currentDirectory, Path.GetFileName(objectPath));

            if (isDirectory)
            {
                FileSystem.MoveDirectory(objectPath, resultPath, true);
            }
            else
            {
                FileSystem.MoveFile(objectPath, resultPath);
            }
        }
        private void ResizeFix()
        {
            if (consoleHeight != Console.WindowHeight || consoleWidth != Console.WindowWidth)
            {
                consoleHeight = Console.WindowHeight;
                consoleWidth = Console.WindowWidth;
                if (consoleHeight < 11)
                {
                    Console.SetWindowSize(Console.WindowWidth, 10);
                    consoleHeight = 10;
                }
                if (consoleWidth < 121)
                {
                    Console.SetWindowSize(120, Console.WindowHeight);
                    consoleWidth = 120;
                }
                maxObjectsCountInScreen = consoleHeight - 6;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();
                PrintOptions();
                PrintObjectsOneScreen();
            }
        }
    }
}