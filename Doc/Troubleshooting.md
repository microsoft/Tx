# Troubleshooting Tx

There are two ways of getting the Tx source code:

* Click on the SOURCE CODE tab, and then on the Download button
* Using [Git](http://git-scm.com/book/en/Getting-Started-Git-Basics) as source control:
	* Install [Github for Windows](http://windows.github.com/)
	* launch the Github Shell, and type: **git clone https://git01.codeplex.com/tx**

The first method is a quick way to take first look at Tx, or debug a problem. 
The second allows you to pull subsequent changes, see the history, etc.

## LINQPad Driver

In Visual Studio:

* open Source\Tx.sln
* set Tx.LinqPad as startup project
* in this project properties, Debug, 
** configure "Start external program": C:\git\tx\References\LinqPad\LINQPad.exe 

## ETW Type Generation

In Visual Studio:

* open Source\Tx.sln
* set EtwEventTypeGen as startup project
* in this project properties, Debug, Command line arguments, point to your manifest

Example:  /m:myManifest.man 

