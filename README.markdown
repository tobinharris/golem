Golem .NET Build Tool
======================================================================

By Tobin Harris [http://www.tobinharris.com](http://www.tobinharris.com)

About Golem
-----------

Golem is a simple build tool for Microsoft .NET 3.5. 
It lets you specify build tasks in regular c# code.
You can run those in various ways: from the command line, a GUI runner, or from within visual studio. 

Advantages include:

	* Create custom tasks and recipes in good old c# or VB.NET code. 
	* Flexible. Create tasks for any purpose (testing, build, deployment, nhibernate, db, documentation, code metrics, reporting etc) 
	* No learning new build languages or tools	
	* Quick and easy to write
	* Tasks are easy to run. Invoke from the command line or GUI runner.	
	* Share recipes between projects
	* Share recipes with the community		

Golem is currently under development by Tobin Harris, and was inspired by Ruby Rake.

Build Recipes & Tasks
---------------------

If you've used a unit testing framework, then Golem code will be familiar. 

Just as NUnit has Test Fixtures and Tests, Golem has Build Recipes and Tasks.

Here is a Golem recipe in c# code:

	//
	//include this anywhere in your solution
	//
	
	[Recipe(Description="Database Build Tasks")]
	public class DbRecipe
	{
		[Task(Description="Drop the development database"]
		public void Drop()
		{
			//some regular c# code drop the db
			//...load and execute the SQL drop script
			
			Console.WriteLine("Database Schema Dropped");
			
		}
		[Task(Description="Create the development database")	
		public void Create()
		{
			//some regular c# code to create the database			
			//...load and execute the SQL create script
			
			Console.WriteLine("Database Schema Created");			
		}
		
		[Task(Description="Drop, create and populat the development database", After=new[]{"Drop","Create"})
		public void Reset()
		{
			//some regular code to populate data (Drop and Create called automatically)
			//...load and execute the insert SQL script
			
			Console.WriteLine("Sample Dev Data Loaded");			
		}
	}

Build your solution, and then type this at the command line:

    cd c:\Code\FooBar
	golem -T 
	
...which lists available build commands:

	Golem (Beta) 2008
	Your friendly executable .NET build tool.

	golem db:drop				  # Drop the development database
	golem db:create               # Create the development database
	golem db:reset				  # Drop, create and populat the development database

You could now type:

	> golem db:reset

... to quickly drop, creat and populate your development database. Cool huh?

The Sky is the Limit
--------------------

Writing your own build tasks is cool, but you find it better to reuse ones already published in the community.
Watch out for some cool build tasks in the making...

	golem nhib:mappings           # Lists NHibernate classes and associated tables
	golem nhib:reset              # Drops and recreates db objects for the current environment using NHibernate mapping
	golem test:all                # Runs all NUnit tests in the test solution
	golem build:all               # Runs MSBuild on the solution
	golem swiss:guid              # Generates a new GUID
	golem swiss:secret            # Generates a cryptographically secure secret.
	golem mvc:routes              # List ASP.NET MVC routes and controllers
	golem mvc:scaffold            # Creates an ASP.NET MVC controller and view with given name
	golem mvc:checklinks          # Checks for broken links in current running web site
	golem stress:db               # Runs custom stress test against dev DB and outputs report
	golem stress:web              # Runs custom stress test against web site and outputs report
	golem ndepend:all             # Generate all stats for the project as HTML file and show in browser
	golem ndepend:critical        # Generate critical fixes report from NDepend
	golem config:reset            # Clears config files and replaces them with master copy
	golem config:init             # Copies config files needed to correct places
	golem config:clear            # Deletes copies of Windsor and Hibernate config files from projects	
	golem recipes:list            # List available community recipes on public server
	golem recipes:install         # Install a community recipe for use in this project
	golem recipes:submit          # Submit a recipe to the community
	golem system:update           # Update to latest version of Rakish. Pass VERSION for specific version.
	golem iis:restart             # Restart local IIS (Use IP=X.X.X.X for remote
	golem iis:backupconfigs       # Backup IIS configuration files (into project folder here)
		



