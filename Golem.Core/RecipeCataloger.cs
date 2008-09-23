using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using Golem.Core;

namespace Golem.Core
{

    public class RecipeCatalogue
    {
        public RecipeCatalogue(List<LoadedAssemblyInfo> found)
        {
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecipeCataloger
    {
        //loads assemblies
        //building tree of recipes
        //reflecting on types

        private readonly string[] _searchPaths;
        private readonly List<LoadedAssemblyInfo> _loadedAssemblies;
        
        public RecipeCataloger(params string[] searchPaths)
        {
            _searchPaths = searchPaths;
            _loadedAssemblies = new List<LoadedAssemblyInfo>();
        }
        
        public ReadOnlyCollection<LoadedAssemblyInfo> LoadedAssemblies { get { return _loadedAssemblies.AsReadOnly(); } }
        

        /// <summary>
        /// Queries loaded assembly info to list all assemblies examined
        /// </summary>
        public ReadOnlyCollection<Assembly> AssembliesExamined
        {
            get
            {
                return (from la in _loadedAssemblies select la.Assembly).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Queries loaded assembly info to list the associated recipes with each
        /// </summary>
        public ReadOnlyCollection<Recipe> Recipes
        { 
            get
            {
                return (from la in _loadedAssemblies from r in la.FoundRecipes select r).ToList().AsReadOnly();
            }
        }
 
        /// <summary>
        /// Lists assemblies that contained recipes
        /// </summary>
        public List<LoadedAssemblyInfo> LoadedAssembliesContainingRecipes
        { 
            get
            {
                return (from la in _loadedAssemblies where la.FoundRecipes.Count > 0 select la).ToList();
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<Recipe> CatalogueRecipes()
        {
            var fileSearch = new FileSearch(_searchPaths);
            fileSearch.BuildFileList();

            PreLoadAssembliesToPreventAssemblyNotFoundError(
                fileSearch.FoundAssemblyFiles.ToArray()
                );
            
            ExtractRecipesFromPreLoadedAssemblies();
            return Recipes.ToArray();
        }

        
        
        private void PreLoadAssembliesToPreventAssemblyNotFoundError(FileInfo[] assemblyFiles)
        {   
            foreach (var file in assemblyFiles)
                //loading the core twice is BAD because "if(blah is RecipeAttribute)" etc will always fail
                if( ! file.Name.StartsWith("Golem.Core") && ! LoadedAssemblies.Any(la=>la.File.FullName == file.FullName))
                {
                    try
                    {
                        var i =  new LoadedAssemblyInfo
                                {
                                    Assembly = Assembly.LoadFrom(file.FullName),
                                    File = file
                                };
                        _loadedAssemblies.Add(i);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Ooops: " + e.Message);
                    }
                }

            
        }

        private void ExtractRecipesFromPreLoadedAssemblies()
        {
            foreach(var la in LoadedAssemblies)
                try
                {
                    foreach (var type in la.Assembly.GetTypes())
                        ExtractRecipesFromType(type, la);
                }
                catch (ReflectionTypeLoadException e)
                {
//                    foreach(var ex in e.LoaderExceptions)
//                        Console.WriteLine("Load Exception: " + ex.Message);
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

            

            var recipeAtt = atts[0] as RecipeAttribute;

            //throw if bad case. Should cast fine (if not, then might indicate 2 of the same assembly is loaded)
            if (recipeAtt == null)
                throw new Exception("Casting error for RecipeAttribute. Same assembly loaded more than once?");

            return recipeAtt;
        }

        
        private void ExtractRecipesFromType(Type type, LoadedAssemblyInfo la)
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

        private static void CreateDependentTasks(Type type, TaskAttribute taskAttribute, Task task)
        {
            foreach(string methodName in taskAttribute.After)
            {
                var dependee = type.GetMethod(methodName);
                if(dependee == null) throw new Exception(String.Format("No dependee method {0}",methodName));
                task.DependsOnMethods.Add(dependee);
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