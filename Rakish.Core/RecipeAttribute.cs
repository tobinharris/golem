using System;

namespace Rakish.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RecipeAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
            
    }
}