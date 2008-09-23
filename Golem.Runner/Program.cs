using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Golem.Core;
using Golem.Core;

namespace Golem.Runner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Golem (Beta) 2008\nYour friendly executable .NET build tool. \n");

            IList<Recipe> found;
            RecipeCataloger finder = BuildCatalog(out found);
            
            if(args.Length > 0)
            {
                if(args[0] == "-T")
                {
                    
                    ShowList(found);
                    return;
                }
                else if(args[0].ToLower() == "-?")
                {
                    Console.WriteLine("Help: \n");
                    Console.WriteLine("golem -T   # List build tasks");
                    Console.WriteLine("golem -?   # Show this help");
                    Console.WriteLine("golem -?   # Show this help");
                    return;
                }

                var parts = args[0].Split(':');
                var runner = new TaskRunner(finder);
                
                if(parts.Length == 2)
                    runner.Run(parts[0],parts[1]);
                else
                {
                    Console.WriteLine("Type golem -? for help, or try one of the following tasks:\n");
                    ShowList(found);
                }
            }
            else
            {
                
                ShowList(found);
            }
            
        }

        private static RecipeCataloger BuildCatalog(out IList<Recipe> found)
        {
            RecipeCataloger finder;
            
            var config = new Configuration();

            if (config.SearchPaths.Count > 0)
                finder = new RecipeCataloger(config.SearchPaths.ToArray());
            else
            {
                Console.WriteLine("Scanning directories for Build Recipes (could take a while)...");
                finder = new RecipeCataloger(Environment.CurrentDirectory);
            }

            found = finder.CatalogueRecipes();

            if(config.SearchPaths.Count == 0)
            {
                config.SearchPaths.AddRange(finder.LoadedAssemblies.Select(s=>s.File.FullName));
                config.Save();
            }
            return finder;
        }

        private static void ShowList(IList<Recipe> found)
        {
            foreach(var recipe in found)
            {
                //Console.WriteLine("\n{0}\n",!String.IsNullOrEmpty(recipe.Description) ? recipe.Description : recipe.Name);
                foreach(var task in recipe.Tasks)
                {
                    var start = "golem " + recipe.Name + ":" + task.Name;
                    Console.WriteLine(start.PadRight(30) +"# " + task.Description);
                }
            }

            if (found.Count == 0)
                Console.WriteLine("No recipes found under {0}", new DirectoryInfo(Environment.CurrentDirectory).Name);
        }
    }
}
