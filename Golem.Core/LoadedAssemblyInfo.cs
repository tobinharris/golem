using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Golem.Core
{
    public class LoadedAssemblyInfo
    {
        public Assembly Assembly { get; set; }
        public FileInfo File { get; set; }
        public List<Recipe> FoundRecipes{get; set;}
        
        public LoadedAssemblyInfo()
        {
            FoundRecipes = new List<Recipe>();
        }
    }
}