using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Utilities
{
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Creates a file inside of the given directory with the given name.
        /// </summary>
        public static void WriteAllText(this FileInfo file, string text)
        {
            File.WriteAllText(file.FullName, text);
        }

        /// <summary>
        /// Creates a file inside of the given directory with the given name.
        /// </summary>
        public static FileInfo CreateSubFile(DirectoryInfo directory, string fileName)
        {
            string filePath = Path.Combine(directory.FullName, fileName);
            File.Create(filePath);
            return new FileInfo(filePath);
        }

        /// <summary>
        /// Creates a WriteableBitmap image from the given path to image.
        /// </summary>
        public static WriteableBitmap CreateWriteableBitmap(string path, UriKind uriKind = UriKind.Absolute)
        {
            if (uriKind == UriKind.Absolute)
            {
                Uri imageUri = new(path, UriKind.Absolute);
                BitmapImage bitmapImage = new(imageUri);
                return new WriteableBitmap(bitmapImage);
            }
            else
            {
                path = path.Replace("\\", "/");
                Uri imageUri = new("pack://application:,,,/" + path, UriKind.Absolute);
                BitmapImage bitmapImage = new(imageUri)
                {
                    CreateOptions = BitmapCreateOptions.None
                };
                return new WriteableBitmap(bitmapImage);

            }
        }

        /// <summary>
        /// Removes all files and folders from the given directory.
        /// </summary>
        public static void ClearDirectory(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        /// <summary>
        /// Removes the directory with the given name and all of its contents.
        /// </summary>
        public static void RemoveDirectory(this DirectoryInfo directoryInfo, string directoryName)
        {
            new DirectoryInfo(Path.Combine(directoryInfo.FullName, directoryName)).Delete(true);
        }

        /// <summary>
        /// Returns the last created file inside of the given directory.
        /// </summary>
        /// <returns>The last created file if one exists; Otherwise null.</returns>
        public static FileInfo LastCreatedFile(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo.GetFiles().Length == 0) return null;
            return directoryInfo.GetFiles()
             .OrderByDescending(f => f.LastWriteTime)
             .First();
        }

        /// <summary>
        /// Returns the last created directory inside of the given directory.
        /// </summary>
        /// <returns>The last created directory if one exists; Otherwise null.</returns>
        public static DirectoryInfo LastCreatedDirectory(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo.GetDirectories().Length == 0) return null;
            return directoryInfo.GetDirectories()
             .OrderByDescending(f => f.LastWriteTime)
             .First();
        }

        /// <summary>
        /// Reads and returns all text inside of the given file.
        /// </summary>
        /// <returns>The ready text.</returns>
        public static string ReadAllText(this FileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        /// <summary>
        /// Crates a file inside the given <paramref name="directoryInfo"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to create.</param>
        /// <returns>The created file.</returns>
        public static FileInfo CreateSubfile(this DirectoryInfo directoryInfo, string fileName)
        {
            File.Create(Path.Combine(directoryInfo.FullName, fileName)).Close();
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        /// <summary>
        /// Deletes all files and directories inside of the given <paramref name="directoryInfo"/>.
        /// </summary>
        public static void DeleteAllSubs(this DirectoryInfo directoryInfo)
        {
            directoryInfo.DeleteAllSubfiles();
            directoryInfo.DeleteAllSubdirectories();
        }

        /// <summary>
        /// Deletes all directories inside of the given <paramref name="directoryInfo"/>.
        /// </summary>
        public static void DeleteAllSubdirectories(this DirectoryInfo directoryInfo)
        {
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                directory.Delete(true);
            }
        }

        /// <summary>
        /// Deletes all files inside of the given <paramref name="directoryInfo"/>.
        /// </summary>
        /// <param name="matchingExtensions">Specify an extension to only delete files with that extension.</param>
        public static void DeleteAllSubfiles(this DirectoryInfo directoryInfo, params string[] matchingExtensions)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (matchingExtensions.Length == 0)
                    file.Delete();
                else if (matchingExtensions.Contains(file.Extension))
                    file.Delete();
            }
        }

        /// <summary>
        /// Deletes all files and directories inside of the given directory except a file/directory with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of file or directory to omit.</param>
        public static void DeleteAllSubsExcept(this DirectoryInfo directoryInfo, string fileName)
        {
            directoryInfo.DeleteAllSubdirectoriesExcept(fileName);
            directoryInfo.DeleteAllSubdirectoriesExcept(fileName);
        }

        /// <summary>
        /// Deletes all files inside of the given directory except the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of file to omit.</param>
        public static void DeleteAllSubfilesExcept(this DirectoryInfo directoryInfo, string fileName)
        {
            // ! Not tested
            string[] filePaths = Directory.GetFiles(directoryInfo.FullName);
            foreach (string filePath in filePaths)
            {
                if (new FileInfo(filePath).Name != fileName)
                    File.Delete(filePath);
            }
        }

        /// <summary>
        /// Deletes all directories inside of the given directory except the directory with the given <paramref name="directoryName"/>.
        /// </summary>
        /// <param name="directoryName">The name of directory to omit.</param>
        public static void DeleteAllSubdirectoriesExcept(this DirectoryInfo directoryInfo, string directoryName)
        {
            string[] directoryPaths = Directory.GetDirectories(directoryInfo.FullName);
            foreach (string directoryPath in directoryPaths)
            {
                if (new DirectoryInfo(directoryPath).Name != directoryName)
                    Directory.Delete(directoryPath);
            }
        }

        /// <summary>
        /// Deletes the subdirectory inside of the <paramref name="directoryInfo"/> with the given name.
        /// </summary>
        /// <param name="directoryName">The name of the directory to delete.</param>
        public static void DeleteSubdirectory(this DirectoryInfo directoryInfo, string directoryName)
        {
            Directory.Delete(Path.Combine(directoryInfo.FullName, directoryName));
        }

        /// <summary>
        /// Deletes the file inside of the <paramref name="directoryInfo"/> subfile with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        public static void DeleteSubfile(this DirectoryInfo directoryInfo, string fileName)
        {
            File.Delete(Path.Combine(directoryInfo.FullName, fileName));
        }

        /// <summary>
        /// Checks if the directory inside <paramref name="directoryInfo"/> with the given <paramref name="directoryName"/> exists.
        /// </summary>
        /// <param name="directoryName">The name of the directory to look for.</param>
        /// <returns>True if the directory exists, false if not.</returns>
        public static bool SubdirectoryExists(this DirectoryInfo directoryInfo, string directoryName)
        {
            return Directory.Exists(Path.Combine(directoryInfo.FullName, directoryName));
        }

        /// <summary>
        /// Checks if the file inside <paramref name="directoryInfo"/> with the given <paramref name="fileName"/> exists.
        /// </summary>
        /// <param name="fileName">The name of the file to look for.</param>
        /// <returns>True if the file exists, false if not.</returns>
        public static bool SubfileExists(this DirectoryInfo directoryInfo, string fileName)
        {
            return File.Exists(Path.Combine(directoryInfo.FullName, fileName));
        }

        /// <summary>
        /// Checks if the file inside <paramref name="directoryInfo"/> with the given <paramref name="fileName"/> prefix exists.
        /// </summary>
        /// <param name="prefix">The prefix of the file to look for.</param>
        /// <returns>True if the file exists, false if not.</returns>
        public static bool SubfileExists_Prefix(this DirectoryInfo directoryInfo, string prefix)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (file.Name.StartsWith(prefix))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the subfile with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to return.</param>
        /// <returns>The file with the given name.</returns>
        public static FileInfo SubFile(this DirectoryInfo directoryInfo, string fileName)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        /// <summary>
        /// Returns the subdirectory with the given <paramref name="directoryName"/>.
        /// </summary>
        /// <param name="directoryName">The name of the directory to return.</param>
        /// <returns>The directory with the given name.</returns>
        public static DirectoryInfo SubDirectory(this DirectoryInfo directoryInfo, string directoryName)
        {
            return new DirectoryInfo(Path.Combine(directoryInfo.FullName, directoryName));
        }

        /// <summary>
        /// Returns the first subfile that matches the given <paramref name="prefix"/>.
        /// </summary>
        /// <param name="prefix">The prefix of the file to look for.</param>
        /// <returns>The file that matches the prefix.</returns>
        public static FileInfo Subfile_Prefix(this DirectoryInfo directoryInfo, string prefix)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (file.Name.StartsWith(prefix))
                    return file;
            }
            return null;
        }

        /// <summary>
        /// Returns the a list of subfiles that match the given <paramref name="prefix"/>.
        /// </summary>
        /// <param name="prefix">The prefix of the file to look for.</param>
        /// <returns>A list of files that match the prefix.</returns>
        public static List<FileInfo> Subfiles_Prefix(this DirectoryInfo directoryInfo, string prefix)
        {
            List<FileInfo> matchingFiles = new();
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (file.Name.StartsWith(prefix))
                    matchingFiles.Add(file);
            }
            return matchingFiles;
        }

        /// <summary>
        /// Checks if the given file is empty.
        /// </summary>
        public static bool IsEmpty(this FileInfo file)
        {
            return file.Length == 0;
        }

        /// <summary>
        /// Returns a list of names of all subdirectories of the given directory.
        /// </summary>
        public static List<string> GetSubDirectoriesNames(this DirectoryInfo directory)
        {
            return Directory.GetDirectories(directory.FullName).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Returns a list of names of all subfiles of the given directory.
        /// </summary>
        public static List<string> GetSubFilesNames(this DirectoryInfo directory)
        {
            return Directory.GetFiles(directory.FullName).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Returns a BitmapImage from the given path in the project.
        /// </summary>
        /// <param name="imagePath">The path to the image in the project. Only specify the path inside of the project.</param>
        public static BitmapImage GetProjectBitmapImage(string imagePath)
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Uri uriSource = new($@"pack://application:,,,/{appName};component/{imagePath}");
            return new BitmapImage(uriSource);
        }

        /// <summary>
        /// Checks if the given file is locked (being used by a process).
        /// </summary>
        /// <returns>True if the file is in use, false if not.</returns>
        public static bool IsLocked(this FileInfo file)
        {
            try
            {
                File.OpenWrite(file.FullName).Close();
                return false;
            }
            catch (Exception) { return true; }
        }

        /// <summary>
        /// Returns the name of the file without the extension.
        /// </summary>
        public static string NoExtensionName(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.Name);
        }
    }
}
