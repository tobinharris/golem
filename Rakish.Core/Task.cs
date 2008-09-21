using System;
using System.Collections.Generic;
using System.Reflection;

namespace Golem.Core
{
    public class Task
    {
        
        public string Name { get; set; }
        public string Description { get; set; }
        
        
        public MethodInfo Method { get; set; }

        public IList<MethodInfo> DependsOnMethods { get; private set; }

        public Task()
        {
            DependsOnMethods = new List<MethodInfo>();
        }
    }
}