using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Rakish.Core
{
    public class RecipeFinder
    {
        public void FindRecipesInAssemblies()
        {
            var startDir = Environment.CurrentDirectory;
            FindRecipesInAssemblies(startDir);
        }

        public void FindRecipesInAssemblies(params string[] startDirs)
        {
            var recipeClasses = new List<Type>();

            foreach(var startDir in startDirs)
            {
                var possibleContainers = 
                    new DirectoryInfo(startDir)
                        .GetFiles("*Core.dll", SearchOption.AllDirectories);
            
                foreach(var file in possibleContainers)
                {
                    var loaded = Assembly.LoadFile(file.FullName);
                    Console.WriteLine(file.FullName);
                
                    var types = loaded.GetTypes();
                
                    foreach(var type in types)
                    {
                        var atts = type.GetCustomAttributes(false);

                        foreach(var att in atts)
                        {
                            if(Convert.ToString(att) == "Rakish.Core.RecipeAttribute")
                            {
                                recipeClasses.Add(type);
                                Console.WriteLine("\t{0}", att);
                            }
                        }
                    }
                }
            }
        }
    }
}