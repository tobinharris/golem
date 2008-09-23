Golem .NET Build Tool
======================================================================

By [Tobin Harris](mailto:tobin@tobinharris.com) [http://www.tobinharris.com](http://www.tobinharris.com)

About Golem
-----------

Golem is a simple build tool for Microsoft .NET inspired by Rake and NUnit. It lets you write useful build scripts in regular c# code and run them during development. 

Here is a taste of some *possible* Golem recipes and their tasks. I have *some* of these working already. This is what you type into the command line: 

	golem test:units		# Run all unit NUnit tests in the solution 
	golem logs:clear		# Clear all log files that my app generates
	golem iis:restart		# Restarts IIS 
	golem ndepend:top10		# Print out NDepend top 10 problems report 
	golem nhib:mappings		# Summarise NHibernate mappings (class -> table)
	golem nhib:dbreset		# Drop and recreate the development database using NHibernate Mappings 
	golem stats:code		# Print out a count of LOC, classes and functions. 
	golem solution:clone	# Clone a new copy of this solution, pass in NS=newnamespace 
	golem deploy:staging	# Push solution live to staging servers

The plan is to have recipes for all sorts of useful things. Including NUnit, NHibernate, Linq 2 SQL, code metrics, log files you name it!... 

Visual Studio integration is also being considered, so that you can run tasks from there. Also, a pretty GUI is in the pipe-line too.

Why Golem?
----------

Golem lets you do a few useful things:

* Create custom tasks and recipes in good old c# or VB.NET code. 
* Create tasks for any purpose 
* ..Testing, build, deployment, nhibernate, db, documentation, code metrics, reporting etc).  
* ..If you can write it in .NET code, you can automate it with Golem!
* No learning new build languages or tools	
* Quick and easy to write
* Tasks are easy to run. Invoke from the command line or GUI runner.	
* Share recipes between projects
* Share recipes with the community		

Golem is an experiment with writing a build system, but it will mature if people like it! Other cool looking Open Source build systems include [Psake](http://code.google.com/p/psake/) which uses Powershell, [Boobs](http://code.google.com/p/boo-build-system/) which uses the Boo language, and [Rake](http://rake.rubyforge.org/) which uses Ruby.

Writing Build Recipes & Tasks
-----------------------------

If you've used a unit testing framework, then Golem will be familiar territory. 

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

Early Days
----------
**WARNING:** Golem is still in early development. I'm getting some value from it in my personal projects right now, and you can too. 
BUT, it's probably not not quite ready for use on commercial projects yet. The code base is still unstable.

Get Involved
------------
Download and try it out. If you're interested in helping with Golem, then let me know. 
Also, feel free to send your ideas, thoughts or findings to [tobin@tobinharris.com](tobin@tobinharris.com). 

Vision
------
It's important to have a vision, even if it's naive :) The vision for Golem is as follows:

* The defacto build "swiss army knife" for automating development tasks
* Visual Studio integration, command line and stand-alone runner
* Totally easy to understand and start using 
* Easy to share recipes and get huge leverage from other peoples work
* Integration with popular CI servers 
* Mono compatible
* Installs with useful ready-to-use recipes for all popular tools and technologies (ASP.NET, NUnit, MVC, Linq 2 SQL, NHibernate, NDepend, SQL Server, Documentation, Deployment etc)
* Ability to list, download and start using new recipes at the click of a button (or tap on the keybaord)
* Self updating
* Vendors write recipes and deploy them with their tools.


