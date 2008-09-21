using System;

namespace Golem.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RecipeAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
            
    }
}