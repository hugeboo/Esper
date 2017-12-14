using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Esper.Model
{
    internal sealed class EsperProject
    {
        public static readonly string EsperExtension = ".esper";

        private readonly FileStore _fileStore;
        private readonly string _prjFilePath;

        public FileStore FileStore
        {
            get { return _fileStore; }
        }

        public string FullFileName
        {
            get { return _prjFilePath; }
        }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(_prjFilePath); }
        }

        public EsperProject(string prjFilePath, bool create)
        {
            if (prjFilePath == null) throw new ArgumentNullException(nameof(prjFilePath));

            var fullFilePath = Path.GetFullPath(prjFilePath);
            var dir = Path.GetDirectoryName(fullFilePath);
            var ext = Path.GetExtension(fullFilePath);

            if (ext != EsperExtension) throw new ArgumentException("Invalid extension: " + prjFilePath);

            if (!Directory.Exists(dir))
            {
                if (create)
                    Directory.CreateDirectory(dir);
                else
                    throw new DirectoryNotFoundException(dir);
            }

            if (!File.Exists(fullFilePath))
            {
                if (create)
                    File.Create(fullFilePath).Close();
                else
                    throw new FileNotFoundException(fullFilePath);
            }

            _prjFilePath = fullFilePath;
            _fileStore = new FileStore(dir);
        }
    }
}
