using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Rakish.Core
{
    public class RecipeFinder
    {
        public IList<Recipe> FindRecipesInAssemblies()
        {
            var startDir = Environment.CurrentDirectory;
            return FindRecipesInAssemblies(startDir);
        }

        public IList<Recipe> FindRecipesInAssemblies(params string[] startDirs)
        {
            var recipeClasses = new List<Recipe>();

            foreach(var startDir in startDirs)
            {
                FileInfo[] possibleContainers = GetPossibleContainers(startDir);

                foreach(var assemblyFile in possibleContainers)
                {
                    FindRecipesInAssembly(assemblyFile, recipeClasses);
                }
            }

            return recipeClasses.AsReadOnly();
        }

        private static void FindRecipesInAssembly(FileInfo file, List<Recipe> recipeClasses)
        {
            var loaded = Assembly.LoadFrom(file.FullName);
            var types = loaded.GetTypes();
            foreach(var type in types)
            {
                FindRecipeInType(type, recipeClasses);
            }
        }

        private static void FindRecipeInType(Type type, List<Recipe> recipeClasses)
        {
            var atts = type.GetCustomAttributes(true);

            foreach(var att in atts)
            {
                var recipeAtt = att as RecipeAttribute;

                if (recipeAtt == null)
                    continue;

                var recipe = new Recipe { Class = type, Name = String.IsNullOrEmpty(recipeAtt.Name) ? type.Name.Replace("Recipe","").ToLower() : recipeAtt.Name };
                recipeClasses.Add(recipe);

                foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var taskAttributes = method.GetCustomAttributes(true);
                    foreach(var taskAttribute in taskAttributes)
                    {
                        var ta = taskAttribute as TaskAttribute;
                        
                        if (ta == null)
                            continue;

                        Task t = new Task();
                        
                        if(! String.IsNullOrEmpty(ta.Name))
                        {
                            t.Name = ta.Name;
                        }
                        else
                        {
                            t.Name = method.Name.Replace("Task", "").ToLower();
                        }
                        
                        t.Method = method;
                        
                        if(! String.IsNullOrEmpty(ta.Help))
                        {
                            t.Description = ta.Help;
                        }
                        else
                        {
                            t.Description = "No description";
                        }

                        foreach(string methodName in ta.After)
                        {
                            var dependee = type.GetMethod(methodName);
                            if(dependee == null) throw new Exception(String.Format("No dependee method {0}",methodName));
                            t.DependsOnMethods.Add(dependee);
                        }

                        recipe.Tasks.Add(t);
                        
                    }
                }
            }
        }

        private FileInfo[] GetPossibleContainers(string startDir)
        {
            return new DirectoryInfo(startDir)
                .GetFiles("*.dll", SearchOption.AllDirectories);
        }
    }
}