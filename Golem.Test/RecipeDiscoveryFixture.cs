using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Golem.Core;
using NUnit.Framework;
using Golem.Core;

namespace Golem.Test
{
    [TestFixture]
    public class RecipeDiscoveryFixture
    {
        private RecipeSearch search;
        IList<Recipe> found;

        [SetUp]
        public void Before_Each_Test_Is_Run()
        {
            search = new RecipeSearch(Environment.CurrentDirectory + "..\\..\\..\\..\\");
            found = search.FindRecipesInFiles();   
        }

        [Test]
        public void Can_Discover_Recipes()
        {
            foreach(var r in found)
            {
                Console.WriteLine(r.Name);
            }
            Assert.AreEqual(4, found.Count);
        }

        [Test]
        public void Can_Discover_Recipe_Details()
        {
            var recipeInfo = found[1];
            Assert.AreEqual("demo", recipeInfo.Name);
            
        }

        [Test]
        public void Can_Discover_Tasks_And_Details()
        {
            var recipeInfo = found[1];
            Assert.AreEqual(3, recipeInfo.Tasks.Count);
            Assert.AreEqual("list", recipeInfo.Tasks[1].Name);
            Assert.AreEqual("List all NUnit tests in solution", recipeInfo.Tasks[1].Description);
        }

        [Test]
        public void Can_Run_Task()
        {
            var recipeInfo = found[1];
            var runner = new TaskRunner(search);
            runner.Run(recipeInfo, recipeInfo.Tasks[0]);
            Assert.AreEqual("TEST", AppDomain.CurrentDomain.GetData("TEST"));
        }

        [Test]
        public void Can_Run_Task_By_Name()
        {
            
            var runner = new TaskRunner(search);
            Locations.StartDirs = new[] {Environment.CurrentDirectory + "..\\..\\..\\..\\"};
            runner.Run("demo","list");
            Assert.AreEqual("LIST", AppDomain.CurrentDomain.GetData("TEST"));
        }

        
        [Test, Ignore]
        public void Can_Run_All_Default_Tasks()
        {
            Assert.Fail();
        }

        [Test]
        public void Can_Set_Dependencies()
        {
            var demo2 = found[0];
            Assert.AreEqual("demo2", demo2.Name);
            Assert.AreEqual(2, demo2.Tasks[0].DependsOnMethods.Count);
            Assert.AreEqual(demo2.Tasks[0].DependsOnMethods[0].Name, "Three");
        }

        [Test]
        public void Functions_Are_Called_In_Correct_Order_With_Dependencies()
        {
            var runner = new TaskRunner(search);
            runner.Run("demo2", "one");          

        }

        [Test]
        public void Can_Infer_Recipe_Category_And_Task_Name()
        {
            var runner = new TaskRunner(search);
            runner.Run("demo3", "hello");          
        }

        [Test, Ignore]
        public void Can_Override_Current_Root_Folder()
        {
            Assert.Fail();
        }

        [Test, Ignore]
        public void Can_Fetch_List_Of_Available_Recipes_From_Server()
        {
            Assert.Fail();
        }

        [Test]
        public void Recipes_Inheriting_RecipeBase_Have_Contextual_Information()
        {
            var demo4 = found[3];
            var runner = new TaskRunner(search);
            runner.Run(demo4,demo4.Tasks[0]);
        }
        
    }

    [TestFixture]
    public class FinderConfigurationFixture
    {
        [SetUp]
        public void Before_Each_Test()
        {
            if(File.Exists(Configuration.DefaultFileName))
                File.Delete(Configuration.DefaultFileName);
        }

     

        [Test]
        public void Finder_Caches_Assemblies_Containing_Recipes()
        {
            var finder = new RecipeSearch();
            finder.FindRecipesInFiles();
            Assert.AreEqual(1, finder.RecipeAssemblies.Count);
        }

        [Test,Ignore]
        public void Can_Automatically_Generate_First_Config_File()
        {
            Assert.IsFalse(File.Exists("\\" + Configuration.DefaultFileName));
            var config = new Configuration();
            Assert.IsTrue(config.IsNew);
            Assert.IsFalse(File.Exists("\\" + Configuration.DefaultFileName));

            var finder = new RecipeSearch();
            finder.FindRecipesInFiles();

            if(config.IsNew)
                config.RecipeSearchHints.AddRange(finder.FilesContainingRecipes.Select(s=>s.FullName));

            config.Save();

            var config2 = new Configuration();
            Assert.IsFalse(config2.IsNew);
            Assert.AreEqual(1, config2.RecipeSearchHints.Count);

            
        }

    }
}
