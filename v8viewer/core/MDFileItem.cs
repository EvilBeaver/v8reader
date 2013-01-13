using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Reader.Core
{
    class MDFileItem
    {
        public MDFileItem(String FullPath)
        {
            
            FileName = FullPath;
            ShortName = Path.GetFileName(FullPath);

            if (IsDirectory(FullPath))
            {
                ElemType = ElementType.Directory;
            }
            else if (File.Exists(FullPath))
            {
                ElemType = ElementType.File;
            }
            else
            {
                throw new FileNotFoundException();
            }

        }

        public String FileName { get; private set; }
        public String Name 
        { 
            get { return ShortName; } 
        }
        
        public ElementType ElemType { get; private set; }

        public enum ElementType
        {
            File,
            Directory
        }

        private bool IsDirectory(String Path)
        {
            if(Directory.Exists(Path))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<String> ListContents()
        {
            if (ElemType == ElementType.Directory)
            {
                return Directory.EnumerateFileSystemEntries(FileName);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public StreamReader GetStream()
        {
            if (ElemType == ElementType.File)
            {
                return new StreamReader(FileName);
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        public String ReadAll()
        {
            if (ElemType == ElementType.File)
            {
                using (StreamReader rd = new StreamReader(FileName))
                {
                    return rd.ReadToEnd();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        public MDFileItem GetElement(String Name)
        {

            if (ElemType == ElementType.Directory)
            {
                return new MDFileItem(FileName + "\\" + Name);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private String ShortName;

    }
}
