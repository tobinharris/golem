using System;
using System.Collections.Generic;
using System.IO;

namespace Golem.Core
{
    public class FileSearch
    {
        private string[] startDirs;
        public List<FileInfo> FoundAssemblyFiles = new List<FileInfo>();
        
        public FileSearch()
        {
            startDirs = new[] { Environment.CurrentDirectory };    
        }

        public FileSearch(params string[] startDirs)
        {
            this.startDirs = startDirs;
        }

        public void BuildFileList()
        {
            FoundAssemblyFiles.Clear();
            
            foreach (var startDir in startDirs)
            {
                FileInfo[] dlls = FindFilesExcludingDuplicates(startDir);
                FoundAssemblyFiles.AddRange(dlls);
            }
        }

        private FileInfo[] FindFilesExcludingDuplicates(string startDir)
        {
            //if a file, then return
            var tmp = new FileInfo(startDir);
            if( tmp.Exists )
            {
                return new[]{tmp};
            }

            var found = new DirectoryInfo(startDir)
                .GetFiles("*.dll", SearchOption.AllDirectories);

            var valid = new List<FileInfo>();

            foreach (var fileInfo in found)
                if (!fileInfo.Directory.FullName.Contains("\\obj\\") && ! FoundAssemblyFiles.Contains(fileInfo))
                    valid.Add(fileInfo);

            return valid.ToArray();

        }

    }
}