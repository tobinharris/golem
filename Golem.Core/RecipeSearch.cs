using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using Golem.Core;

namespace Golem.Core
{
    public class LoadedAssembly
    {
        public Assembly Assembly { get; set; }
        public FileInfo LoadedFrom { get; set; }
        public List<Recipe> FoundRecipes{get; set;}
        
        public LoadedAssembly()
        {
            FoundRecipes = new List<Recipe>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecipeSearch
    {
        //scanning directories 
        //loading assemblies
        //building tree of recipes
        //reflecting on types

        private readonly AssemblySearch assemblySearch;
        
        public RecipeSearch()
        {
            assemblySearch = new AssemblySearch();
        }
        
        public RecipeSearch(params string[] searchLocations)
        {
            assemblySearch = new AssemblySearch(searchLocations);
        }

        public ReadOnlyCollection<Assembly> AssembliesExamined { get; private set; }
        public List<Recipe> Recipes { get; private set; }

        public List<Assembly> RecipeAssemblies { get; private set; }
        public List<FileInfo> FilesContainingRecipes = new List<FileInfo>();

        public List<LoadedAssembly> LoadedAssemblies = new List<LoadedAssembly>();
        
        public IList<Recipe> FindRecipesInFiles()
        {
            
            assemblySearch.Scan();
            PreLoadAssembliesToPreventAssemblyNotFoundError(
                assemblySearch.FoundAssemblyFiles.ToArray()
                );

            
            ExtractAndFlag();

            AssembliesExamined = (from la in LoadedAssemblies select la.Assembly).ToList().AsReadOnly();
            
            Recipes = (from la in LoadedAssemblies from r in la.FoundRecipes select r).ToList();

            return Recipes.ToArray();
        }

        
        
        private void PreLoadAssembliesToPreventAssemblyNotFoundError(FileInfo[] assemblyFiles)
        {   
            foreach (var file in assemblyFiles)
                //loading the core twice is BAD because "if(blah is RecipeAttribute)" etc will always fail
                if( ! file.Name.StartsWith("Golem.Core") && ! LoadedAssemblies.Any(la=>la.LoadedFrom.FullName == file.FullName))
                {
                    LoadedAssemblies.Add(
                        new LoadedAssembly
                            {
                                Assembly = Assembly.LoadFrom(file.FullName),
                                LoadedFrom = file
                            }); 
                }

            
        }

        private void ExtractAndFlag()
        {
            foreach(var la in LoadedAssemblies)
                foreach (var type in la.Assembly.GetTypes())
                    ExtractRecipesFromType(type, la);
            
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

            if(RecipeAssemblies == null)
                RecipeAssemblies = new List<Assembly>();

            if (! RecipeAssemblies.Contains(type.Assembly))
                RecipeAssemblies.Add(type.Assembly);

            var recipeAtt = atts[0] as RecipeAttribute;

            //throw if bad case. Should cast fine (if not, then might indicate 2 of the same assembly is loaded)
            if (recipeAtt == null)
                throw new Exception("Casting error for RecipeAttribute. Same assembly loaded more than once?");

            return recipeAtt;
        }

        //TODO: ExtractRecipesFromType is Too long and 
        //TODO: too many IL Instructions
        //TODO: Nesting is too deep
        //TODO: Not enough comments
        //TODO: Too many variables
        //
        private void ExtractRecipesFromType(Type type, LoadedAssembly la)
        {
            //find the attribute on the assembly if there is one
            var recipeAtt = GetRecipeAttributeOrNull(type);
            
            //if not found, return
            if (recipeAtt == null) return;
            
            //create recipe details from attribute
            Recipe recipe = CreateRecipeFromAttribute(type, recipeAtt);

            //associate recipe with assembly
            la.FoundRecipes.Add(recipe);

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

       
    }
}