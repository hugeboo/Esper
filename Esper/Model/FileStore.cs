using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esper.Model
{
    internal sealed class FileStore
    {
        public enum FileType
        {
            Unknown,
            Text,
            Lua
        }

        public Encoding Encoding { get; private set; }
        public string FullSystemPath { get; private set; }

        public FileStore(string fullSystemPath)
        {
            FullSystemPath = fullSystemPath;
            Encoding = Encoding.GetEncoding(1251);
        }

        public string ReadAllText(FileStore.File file)
        {
            var s = System.IO.File.ReadAllText(
                System.IO.Path.Combine(file.Directory.SystemPath, file.Name), Encoding);
            return s;
        }

        public void SaveAllText(FileStore.File file, string text)
        {
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(file.Directory.SystemPath, file.Name), text, Encoding);
        }

        public Directory GetFullTree()
        {
            var root = new Directory()
            {
                Name = System.IO.Path.GetFileName(FullSystemPath),
                SystemPath = FullSystemPath
            };
            _GetTree(root);
            return root;
        }

        private void _GetTree(Directory root)
        {
            var files = System.IO.Directory.GetFiles(root.SystemPath).OrderBy(n => n.ToLower());
            foreach (var fn in files)
            {
                root.Files.Add(new File()
                {
                    Directory = root,
                    Name = System.IO.Path.GetFileName(fn),
                    Type = File.GetFileType(System.IO.Path.GetExtension(fn))
                });
            }

            var dirs = System.IO.Directory.GetDirectories(root.SystemPath).OrderBy(n => n.ToLower());
            foreach (var dn in dirs)
            {
                var dir = new Directory()
                {
                    ParentDirectory = root,
                    Name = System.IO.Path.GetFileName(dn),
                    SystemPath = System.IO.Path.GetFullPath(dn)
                };
                root.Directories.Add(dir);
                _GetTree(dir);
            }
        }

        public sealed class Directory
        {
            public Directory ParentDirectory { get; set; }
            public string Name { get; set; }
            public string SystemPath { get; set; }
            public List<File> Files { get; private set; } = new List<File>();
            public List<Directory> Directories { get; private set; } = new List<Directory>();

            public override bool Equals(object obj)
            {
                var other = obj as Directory;
                return other != null && other.SystemPath == SystemPath;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return SystemPath;
            }
        }

        public sealed class File
        {
            public Directory Directory { get; set; }
            public string Name { get; set; }
            public FileType Type { get; set; }

            public string GetFullPath()
            {
                if (!string.IsNullOrEmpty(Name) && Directory != null && Directory.SystemPath != null)
                {
                    return System.IO.Path.Combine(Directory.SystemPath, Name);
                }
                return null;
            }

            public static string GetExtension(FileType ftype)
            {
                switch (ftype)
                {
                    case FileType.Text:
                        return ".txt";
                    case FileType.Lua:
                        return ".lua";
                    default:
                        return null;
                }
            }

            public static FileType GetFileType(string ext)
            {
                if (string.IsNullOrEmpty(ext)) return FileType.Unknown;
                switch (ext.ToLower())
                {
                    case ".txt":
                        return FileType.Text;
                    case ".lua":
                        return FileType.Lua;
                    default:
                        return FileType.Unknown;
                }
            }

            public override bool Equals(object obj)
            {
                var other = obj as File;
                return other != null &&
                    other.Name == Name &&
                    Equals(other.Directory, Directory);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                var name = !string.IsNullOrEmpty(Name) ? Name : "NONAME";
                var dir = Directory?.SystemPath;
                return !string.IsNullOrEmpty(dir) ? System.IO.Path.Combine(dir, name) : name;
            }
        }
    }
}
