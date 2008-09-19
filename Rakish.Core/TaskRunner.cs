using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rakish.Core
{
    public class TaskRunner
    {
        public void Run(Recipe recipe, Task task)
        {
            var recipeInstance = Activator.CreateInstance(recipe.Class);
            
            task.Method.Invoke(recipeInstance, null);
        }

        public void Run(string recipeName, string taskName)
        {
            var finder = new RecipeFinder();
            var found = finder.FindRecipesInAssemblies();
            
            foreach(var r in found)
            {
                if(r.Name.ToLower() == recipeName.ToLower())
                {
                    foreach(var t in r.Tasks)
                    {
                        if(t.Name.ToLower() == taskName.ToLower())
                        {
                            foreach(var methodInfo in t.DependsOnMethods)
                            {
                                Run(r, r.GetTaskForMethod(methodInfo));
                            }
                            Run(r, t);
                            return;
                        }
                    }
                }
            }
        }

//        public RunManifest BuildRunManifest(string recipeName, string taskName)
//        {
//            var manifest = new RunManifest();
//            var finder = new RecipeFinder();
//            var found = finder.FindRecipesInAssemblies();
//
//            foreach (var r in found)
//            {
//                if (r.Name.ToLower() == recipeName.ToLower())
//                {
//                    foreach (var t in r.Tasks)
//                    {
//                        if (t.Name.ToLower() == taskName.ToLower())
//                        {
//                            foreach(var d in t.DependsOnMethods)
//                            {
//                                manifest.Add(null);
//                            }
//                            manifest.Add(t);
//                            
//                        }
//                    }
//                }
//            }
//            return manifest;
//        }


    }

    public class RunManifest
    {

        public OrderedDictionary ToRun;
        public void Add(Task t)
        {
            if(! ToRun.Contains(t))
            {
                ToRun.Add(t,new RunManifestItem{Task=t});
            }
        }
        public RunManifest()
        {
            ToRun = new OrderedDictionary();
        }

        public Task TaskAt(int position)
        {
            return (Task) ToRun[position];
        }
    }
    public class RunManifestItem
    {
        public Task Task;
        public bool HasRun;
    }
}