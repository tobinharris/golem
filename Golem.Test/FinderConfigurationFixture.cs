using System;
using System.IO;
using System.Linq;
using Golem.Core;
using NUnit.Framework;

namespace Golem.Test
{
    [TestFixture]
    public class FinderConfigurationFixture
    {
        [SetUp]
        public void Before_Each_Test()
        {
            if(File.Exists(Configuration.DEFAULT_FILE_NAME))
                File.Delete(Configuration.DEFAULT_FILE_NAME);
        }

     

        [Test]
        public void Finder_Caches_Assemblies_Containing_Recipes()
        {
            var cataloger = new RecipeCataloger(Environment.CurrentDirectory);
            cataloger.CatalogueRecipes();
            Assert.AreEqual(1, cataloger.LoadedAssembliesContainingRecipes.Count);
        }

        [Test]
        public void Can_Automatically_Generate_First_Config_File()
        {
            Assert.IsFalse(Configuration.ConfigFileExists);
            
            var config = new Configuration();
            
            Assert.IsTrue(config.IsNew);
            Assert.IsTrue(Configuration.ConfigFileExists);

            var cataloger = new RecipeCataloger(Environment.CurrentDirectory);
            cataloger.CatalogueRecipes();

            if(config.IsNew)
                config.SearchPaths.AddRange(
                    cataloger.LoadedAssembliesContainingRecipes.Select(la=>la.Assembly.FullName)
                    );

            config.Save();

            var config2 = new Configuration();
            Assert.IsFalse(config2.IsNew);
            Assert.AreEqual(1, config2.SearchPaths.Count);
        }

        [Test]
        public void Cataloger_Uses_Search_Locations()
        {
            var config = new Configuration();
            var cataloger = new RecipeCataloger(Environment.CurrentDirectory);
            cataloger.CatalogueRecipes();
            config.SearchPaths.AddRange(cataloger.LoadedAssembliesContainingRecipes.Select(la => la.File.FullName));
            Assert.Greater(cataloger.AssembliesExamined.Count, 1);

            var cataloger2 = new RecipeCataloger(config.SearchPaths.ToArray());
            cataloger2.CatalogueRecipes();
            Assert.AreEqual(1, cataloger2.AssembliesExamined.Count);

        }

    }
}