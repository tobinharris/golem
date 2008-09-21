using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using Golem.Core;

namespace Golem.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeSearch
    {
        
        //scanning directories 
        //loading assemblies
        //building tree of recipes
        //reflecting on types

        private string[] startDirs;

        public RecipeSearch()
        {
            this.startDirs = new string[]{Environment.CurrentDirectory};
        }
        

        
        public RecipeSearch(params string[] startDirs) : this()
        {
            this.startDirs = startDirs;

            if(startDirs == null)
            {
                Locations.StartDirs = new string[] { Environment.CurrentDirectory };
                this.startDirs = Locations.StartDirs;
            }

            
        }

        public ReadOnlyCollection<Assembly> AllAssembliesFound { get; private set; }
        public ReadOnlyCollection<Recipe> AllRecipesFound { get; private set; }

        public List<Assembly> AssembliesContainingRecipes { get; private set; }
        public List<FileInfo> FilesContainingRecipes = new List<FileInfo>();
        
        //TODO: Too many variables
        public IList<Recipe> FindRecipesInFiles()
        {
            var recipeClasses = new List<Recipe>();
            
            //TODO: Fix this
            Locations.StartDirs = startDirs;

            foreach (var startDir in startDirs)
            {
                FileInfo[] dlls = FindAllDlls(startDir);
                var loadedAssemblies = PreLoadAssembliesToPreventAssemblyNotFoundError(dlls);

                foreach(var assembly in loadedAssemblies)
                    FindRecipesInAssembly(assembly, recipeClasses);

                AllAssembliesFound = loadedAssemblies.AsReadOnly();
                AllRecipesFound = recipeClasses.AsReadOnly();
            }

            return recipeClasses.AsReadOnly();
        }

        
        
        private List<Assembly> PreLoadAssembliesToPreventAssemblyNotFoundError(FileInfo[] dllFile)
        {
            var loaded = new List<Assembly>();
            
            foreach (var dll in dllFile)
                //loading the core twice is BAD because "if(blah is RecipeAttribute)" etc will always fail
                if(! dll.Name.StartsWith("Golem.Core") && ! FilesContainingRecipes.Any(file=>file.FullName == dll.FullName))
                {
                    loaded.Add(Assembly.LoadFrom(dll.FullName));
                    FilesContainingRecipes.Add(dll);
                }

            return loaded;
        }

        private void FindRecipesInAssembly(Assembly loaded, List<Recipe> recipeClasses)
        {
            try
            {
                var types = loaded.GetTypes();

                foreach (var type in types)
                    FindRecipeInType(type, recipeClasses);
            }
            catch(ReflectionTypeLoadException ex)
            {
                foreach(var e in ex.LoaderExceptions)
                {
                    //Console.WriteLine(e.Message);
                }
                
            }

          
            
        }


        public RecipeAttribute GetRecipeAttributeOrNull(Type type)
        {
            //get recipe attributes for type 
            var atts = type.GetCustomAttributes(typeof(RecipeAttribute), true);

            //should only be one per type
            if (atts.Length > 1)
                throw new Exception("Expected only 1 recipe attribute, but got more");

            //return if none, we'll skip this class
            if (atts.Length == 0)
                return null;

            if(AssembliesContainingRecipes == null)
                AssembliesContainingRecipes = new List<Assembly>();

            if (! AssembliesContainingRecipes.Contains(type.Assembly))
                AssembliesContainingRecipes.Add(type.Assembly);

            var recipeAtt = atts[0] as RecipeAttribute;

            //throw if bad case. Should cast fine (if not, then might indicate 2 of the same assembly is loaded)
            if (recipeAtt == null)
                throw new Exception("Casting error for RecipeAttribute. Same assembly loaded more than once?");

            return recipeAtt;
        }

        //TODO: FindRecipeInType is Too long and 
        //TODO: too many IL Instructions
        //TODO: Nesting is too deep
        //TODO: Not enough comments
        //TODO: Too many variables
        //
        private void FindRecipeInType(Type type, List<Recipe> manifest)
        {
            //find the attribute on the assembly if there is one
            var recipeAtt = GetRecipeAttributeOrNull(type);
            
            //if not found, return
            if (recipeAtt == null) return;
            
            //create recipe details from attribute
            Recipe recipe = CreateRecipeFromAttribute(type, recipeAtt);

            //add to manifest
            manifest.Add(recipe);

            //trawl through and add the tasks
            AddTasksToRecipe(type, recipe);
            
        }

        private static Recipe CreateRecipeFromAttribute(Type type, RecipeAttribute recipeAtt)
        {
            return new Recipe
                       {
                           Class = type,
                           Name = String.IsNullOrEmpty(recipeAtt.Name) ? (type.Name == "RecipesRecipe" ? "recipes" : type.Name.Replace("Recipe", "").ToLower()) : recipeAtt.Name,
                           Description = ! String.IsNullOrEmpty(recipeAtt.Description) ? recipeAtt.Description : null
                       };
        }

        private static void AddTasksToRecipe(Type type, Recipe recipe)
        {
            //loop through methods in class
            foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly ))
            {
                //get the custom attributes on the method
                var foundAttributes = method.GetCustomAttributes(typeof(TaskAttribute), false);
                
                if(foundAttributes.Length > 1)
                    throw new Exception("Should only be one task attribute on a method");

                //if none, skp to the next method
                if (foundAttributes.Length == 0)
                    continue;
                
                var taskAttribute = foundAttributes[0] as TaskAttribute;
                
                if (taskAttribute == null)
                    throw new Exception("couldn't cast TaskAttribute correctly, more that one assembly loaded?");

                //get the task based on attribute contents
                Task t = CreateTaskFromAttribute(method, taskAttribute);

                //build list of dependent tasks
                CreateDependentTasks(type, taskAttribute, t);

                //add the task to the recipe
                recipe.Tasks.Add(t);
            }
        }

        private static void CreateDependentTasks(Type type, TaskAttribute taskAttribute, Task t)
        {
            foreach(string methodName in taskAttribute.After)
            {
                var dependee = type.GetMethod(methodName);
                if(dependee == null) throw new Exception(String.Format("No dependee method {0}",methodName));
                t.DependsOnMethods.Add(dependee);
            }
        }

        private static Task CreateTaskFromAttribute(MethodInfo method, TaskAttribute ta)
        {
            Task t = new Task();
                    
            if(! String.IsNullOrEmpty(ta.Name))
                t.Name = ta.Name;
            else
                t.Name = method.Name.Replace("Task", "").ToLower();
                    
                    
            t.Method = method;
                    
            if(! String.IsNullOrEmpty(ta.Description))
                t.Description = ta.Description;
            else
                t.Description = "";
            return t;
        }

        private FileInfo[] FindAllDlls(string startDir)
        {
            
            var found = new DirectoryInfo(startDir)
                .GetFiles("*.dll", SearchOption.AllDirectories);

            var deduped = new List<FileInfo>();
            
            foreach(var fileInfo in found)
                if(! fileInfo.Directory.FullName.Contains("\\obj\\"))
                    deduped.Add(fileInfo);
            
            return deduped.ToArray();


        }
    }
}