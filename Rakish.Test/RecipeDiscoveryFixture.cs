using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rakish.Core;

namespace Rakish.Test
{
    [TestFixture]
    public class RecipeDiscoveryFixture
    {
        [Test]
        public void CanDiscoverRecipesAndTasksInAssembly()
        {
            var finder = new RecipeFinder();
            finder.FindRecipesInAssemblies();
        }
    }
}
