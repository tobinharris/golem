using System;
using System.Collections.Generic;
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
            var finder = new RecipeFinder(Environment.CurrentDirectory);
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
        }
    }
}
