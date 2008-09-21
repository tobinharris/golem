using System;
using System.Collections.Generic;
using System.IO;

namespace Golem.Core
{
    public class AssemblySearch
    {
        private string[] startDirs;
        public List<FileInfo> FoundAssemblyFiles = new List<FileInfo>();
        
        public AssemblySearch()
        {
            startDirs = new[] { Environment.CurrentDirectory };    
        }

        public AssemblySearch(params string[] startDirs)
        {
            this.startDirs = startDirs;
            if (startDirs == null)
                startDirs = new[]{Environment.CurrentDirectory};
        }

        public void Scan()
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