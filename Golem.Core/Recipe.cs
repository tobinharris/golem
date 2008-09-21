using System;
using System.Collections.Generic;
using System.Reflection;

namespace Golem.Core
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Task> Tasks { get; private set; }
        public Type Class{ get; set;}

        public Recipe()
        {
            Tasks = new List<Task>();
        }

        public Task GetTaskForMethod(MethodInfo methodInfo)
        {
            foreach(Task t in Tasks)
            {
                if (t.Method != null && t.Method.Name == methodInfo.Name)
                {
                    return t;
                }
            }
            return null;
        }
    }
}