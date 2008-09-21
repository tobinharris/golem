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
            var finder = new RecipeSearch(Environment.CurrentDirectory);
            var found = finder.FindRecipesInFiles();

            if(args.Length > 0)
            {
                if(args[0].ToLower() == "-t")
                {
                    ShowList(found);
                    return;
                }

                var parts = args[0].Split(':');
                var runner = new TaskRunner(finder);
                
                if(parts.Length == 2)
                    runner.Run(parts[0],parts[1]);
                else
                    Console.WriteLine("Error: don't know what to do with that. \n\nTry: golem -t\n\n...to see commands.");
            }
            else
            {
                Console.WriteLine("Golem (Beta) - Your friendly executable .NET build tool.");
                ShowList(found);
            }
            
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
