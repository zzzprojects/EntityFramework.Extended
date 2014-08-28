#MSBuild Community Tasks

The MSBuild Community Tasks Project is an open source project for MSBuild tasks.

##Download

The latest nightly build of can be downloaded from the following location.
http://code.google.com/p/msbuildtasks/downloads/list

The MSBuild Community Tasks  library is also available on nuget.org via package name `MSBuildTasks`.

To install MSBuildTasks, run the following command in the Package Manager Console

    PM> Install-Package MSBuildTasks
    
More information about NuGet package avaliable at
https://nuget.org/packages/MSBuildTasks

##Join Project

Please join the MSBuild Community Tasks Project and help contribute in building the tasks. 

Google Group for MSBuild Community Tasks
https://groups.google.com/d/forum/msbuildtasks

##Current Community Tasks

<table border="0" cellpadding="3" cellspacing="0" width="90%" id="tasksTable">
    <tr>
        <th align="left" width="190">
            Task
        </th>
        <th align="left">
            Description
        </th>
    </tr>
    <tr>
        <td>
            Add
        </td>
        <td>
            Add numbers
        </td>
    </tr>
    <tr>
        <td>
            AddTnsName
        </td>
        <td>
            Defines a database host within the Oracle TNSNAMES.ORA file.
        </td>
    </tr>
    <tr>
        <td>
            AppPoolController
        </td>
        <td>
            Allows control for an application pool on a local or remote machine with IIS installed. The default is to control the application pool on the local machine. If connecting to a remote machine, you can specify the and for the task to run under.
        </td>
    </tr>
    <tr>
        <td>
            AppPoolCreate
        </td>
        <td>
            Creates a new application pool on a local or remote machine with IIS installed. The default is to create the new application pool on the local machine. If connecting to a remote machine, you can specify the and for the task to run under.
        </td>
    </tr>
    <tr>
        <td>
            AppPoolDelete
        </td>
        <td>
            Deletes an existing application pool on a local or remote machine with IIS installed. The default is to delete an existing application pool on the local machine. If connecting to a remote machine, you can specify the and for the task to run under.
        </td>
    </tr>
    <tr>
        <td>
            AssemblyInfo
        </td>
        <td>
            Generates an AssemblyInfo files
        </td>
    </tr>
    <tr>
        <td>
            Attrib
        </td>
        <td>
            Changes the attributes of files and/or directories
        </td>
    </tr>
    <tr>
        <td>
            Beep
        </td>
        <td>
            A task to play the sound of a beep through the console speaker.
        </td>
    </tr>
    <tr>
        <td>
            BuildAssembler
        </td>
        <td>
            BuildAssembler task for Sandcastle.
        </td>
    </tr>
    <tr>
        <td>
            ChmBuilder
        </td>
        <td>
            ChmBuilder task for Sandcastle.
        </td>
    </tr>
    <tr>
        <td>
            ChmCompiler
        </td>
        <td>
            Html Help 1x compiler task.
        </td>
    </tr>
    <tr>
        <td>
            Computer
        </td>
        <td>
            Provides information about the build computer.
        </td>
    </tr>
    <tr>
        <td>
            DBCSFix
        </td>
        <td>
            DBCSFix task for Sandcastle.
        </td>
    </tr>
    <tr>
        <td>
            Divide
        </td>
        <td>
            Divide numbers
        </td>
    </tr>
    <tr>
        <td>
            ExecuteDDL
        </td>
        <td>
            MSBuild task to execute DDL and SQL statements.
        </td>
    </tr>
    <tr>
        <td>
            FileUpdate
        </td>
        <td>
            Replace text in file(s) using a Regular Expression.
        </td>
    </tr>
    <tr>
        <td>
            FtpCreateRemoteDirectory
        </td>
        <td>
            Creates a full remote directory on the remote server if not exists using the File Transfer Protocol (FTP). This can be one directory or a full path to create.
        </td>
    </tr>
    <tr>
        <td>
            FtpDirectoryExists
        </td>
        <td>
            Determ if a remote directory exists on a FTP server or not.
        </td>
    </tr>
    <tr>
        <td>
            FtpUpload
        </td>
        <td>
            Uploads a group of files using File Transfer Protocol (FTP).
        </td>
    </tr>
    <tr>
        <td>
            FtpUploadDirectoryContent
        </td>
        <td>
            Uploads a full directory content to a remote directory.
        </td>
    </tr>
    <tr>
        <td>
            FxCop
        </td>
        <td>
            Uses FxCop to analyse managed code assemblies and reports on their design best-practice compliance.
        </td>
    </tr>
    <tr>
        <td>
            GacUtil
        </td>
        <td>
            MSBuild task to install and uninstall asseblies into the GAC
        </td>
    </tr>
    <tr>
        <td>
            GetSolutionProjects
        </td>
        <td>
            Retrieves the list of Projects contained within a Visual Studio Solution (.sln) file
        </td>
    </tr>
    <tr>
        <td>
            HxCompiler
        </td>
        <td>
            A Html Help 2.0 compiler task.
        </td>
    </tr>
    <tr>
        <td>
            ILMerge
        </td>
        <td>
            A wrapper for the ILMerge tool.
        </td>
    </tr>
    <tr>
        <td>
            InstallAspNet
        </td>
        <td>
            Installs and register script mappings for ASP.NET
        </td>
    </tr>
    <tr>
        <td>
            InstallAssembly
        </td>
        <td>
            Installs assemblies.
        </td>
    </tr>
    <tr>
        <td>
            JSCompress
        </td>
        <td>
            Compresses JavaScript source by removing comments and unnecessary whitespace. It typically reduces the size of the script by half, resulting in faster downloads and code that is harder to read.
        </td>
    </tr>
    <tr>
        <td>
            Mail
        </td>
        <td>
            Sends an email message
        </td>
    </tr>
    <tr>
        <td>
            Merge
        </td>
        <td>
            Merge files into the destination file.
        </td>
    </tr>
    <tr>
        <td>
            Modulo
        </td>
        <td>
            Performs the modulo operation on numbers.
        </td>
    </tr>
    <tr>
        <td>
            MV
        </td>
        <td>
            Moves files on the filesystem to a new location.
        </td>
    </tr>
    <tr>
        <td>
            MRefBuilder
        </td>
        <td>
            MRefBuilder task for Sandcastle.
        </td>
    </tr>
    <tr>
        <td>
            Multiple
        </td>
        <td>
            Multiple numbers
        </td>
    </tr>
    <tr>
        <td>
            NDoc
        </td>
        <td>
            Runs the NDoc application.
        </td>
    </tr>
    <tr>
        <td>
            NUnit
        </td>
        <td>
            Run NUnit 2.4 on a group of assemblies.
        </td>
    </tr>
    <tr>
        <td>
            Prompt
        </td>
        <td>
            Displays a message on the console and waits for user input.
        </td>
    </tr>
    <tr>
        <td>
            RegexMatch
        </td>
        <td>
            Task to filter an Input list with a Regex expression. Output list contains items from Input list that matched given expression
        </td>
    </tr>
    <tr>
        <td>
            RegexReplace
        </td>
        <td>
            Task to replace portions of strings within the Input list Output list contains all the elements of the Input list after performing the Regex Replace.
        </td>
    </tr>
    <tr>
        <td>
            RegistryRead
        </td>
        <td>
            Reads a value from the Registry
        </td>
    </tr>
    <tr>
        <td>
            RegistryWrite
        </td>
        <td>
            Writes a value to the Registry
        </td>
    </tr>
    <tr>
        <td>
            RoboCopy
        </td>
        <td>
            Task wrapping the Window Resource Kit Robocopy.exe command.
        </td>
    </tr>
    <tr>
        <td>
            Sandcastle
        </td>
        <td>
            The Sandcastle task.
        </td>
    </tr>
    <tr>
        <td>
            Script
        </td>
        <td>
            Executes code contained within the task.
        </td>
    </tr>
    <tr>
        <td>
            ServiceController
        </td>
        <td>
            Task that can control a Windows service.
        </td>
    </tr>
    <tr>
        <td>
            ServiceQuery
        </td>
        <td>
            Task that can determine the status of a specified service on a target server.
        </td>
    </tr>
    <tr>
        <td>
            Sleep
        </td>
        <td>
            A task for sleeping for a specified period of time.
        </td>
    </tr>
    <tr>
        <td>
            Sound
        </td>
        <td>
            A task to play a sound from a .wav file path or URL.
        </td>
    </tr>
    <tr>
        <td>
            SqlExecute
        </td>
        <td>
            Executes a SQL command.
        </td>
    </tr>
    <tr>
        <td>
            SqlPubWiz
        </td>
        <td>
            The Database Publishing Wizard enables the deployment of SQL Server databases (both schema and data) into a shared hosting environment.
        </td>
    </tr>
    <tr>
        <td>
            Subtract
        </td>
        <td>
            Subtract numbers
        </td>
    </tr>
    <tr>
        <td>
            SvnCheckout
        </td>
        <td>
            Checkout a local working copy of a Subversion repository.
        </td>
    </tr>
    <tr>
        <td>
            SvnClient
        </td>
        <td>
            Subversion client base class
        </td>
    </tr>
    <tr>
        <td>
            SvnCommit
        </td>
        <td>
            Subversion Commit command
        </td>
    </tr>
    <tr>
        <td>
            SvnCopy
        </td>
        <td>
            Copy a file or folder in Subversion
        </td>
    </tr>
    <tr>
        <td>
            SvnExport
        </td>
        <td>
            Export a folder from a Subversion repository
        </td>
    </tr>
    <tr>
        <td>
            SvnInfo
        </td>
        <td>
            Run the "svn info" command and parse the output
        </td>
    </tr>
    <tr>
        <td>
            SvnUpdate
        </td>
        <td>
            Subversion Update command
        </td>
    </tr>
    <tr>
        <td>
            SvnVersion
        </td>
        <td>
            Summarize the local revision(s) of a working copy.
        </td>
    </tr>
    <tr>
        <td>
            TaskSchema
        </td>
        <td>
            A Task that generates a XSD schema of the tasks in an assembly.
        </td>
    </tr>
    <tr>
        <td>
            TemplateFile
        </td>
        <td>
            MSBuild task that replaces tokens in a template file and writes out a new file.
        </td>
    </tr>
    <tr>
        <td>
            TfsVersion
        </td>
        <td>
            Determines the changeset in a local Team Foundation Server workspace
        </td>
    </tr>
    <tr>
        <td>
            Time
        </td>
        <td>
            Gets the current date and time.
        </td>
    </tr>
    <tr>
        <td>
            UninstallAssembly
        </td>
        <td>
            Uninstalls assemblies.
        </td>
    </tr>
    <tr>
        <td>
            Unzip
        </td>
        <td>
            Unzip a file to a target directory.
        </td>
    </tr>
    <tr>
        <td>
            User
        </td>
        <td>
            Provides information about the build user.
        </td>
    </tr>
    <tr>
        <td>
            Version
        </td>
        <td>
            Generates version information based on various algorithms
        </td>
    </tr>
    <tr>
        <td>
            VssAdd
        </td>
        <td>
            Task that adds files to a Visual SourceSafe database.
        </td>
    </tr>
    <tr>
        <td>
            VssCheckin
        </td>
        <td>
            Task that executes a checkin against a VSS Database.
        </td>
    </tr>
    <tr>
        <td>
            VssCheckout
        </td>
        <td>
            Task that executes a checkout of files or projects against a Visual SourceSafe database.
        </td>
    </tr>
    <tr>
        <td>
            VssClean
        </td>
        <td>
            Task that can strip the source control information from a Visual Studio Solution and subprojects.
        </td>
    </tr>
    <tr>
        <td>
            VssDiff
        </td>
        <td>
            Task that records differences between the latest version of all the items in a Visual SourceSafe project and another version or label to a file.
        </td>
    </tr>
    <tr>
        <td>
            VssGet
        </td>
        <td>
            Task that retireves an item or project from a Visual SourceSafe database.
        </td>
    </tr>
    <tr>
        <td>
            VssHistory
        </td>
        <td>
            Generates an XML file containing details of all changes made to a Visual SourceSafe project or file between specified labels or dates.
        </td>
    </tr>
    <tr>
        <td>
            VssLabel
        </td>
        <td>
            Task that applies a label to a Visual SourceSafe item.
        </td>
    </tr>
    <tr>
        <td>
            VssUndoCheckout
        </td>
        <td>
            Task that undoes a checkout of files or projects against a Visual SourceSafe database.
        </td>
    </tr>
    <tr>
        <td>
            WebDirectoryCreate
        </td>
        <td>
            Creates a new web directory on a local or remote machine with IIS installed. The default is to create the new web directory on the local machine. The physical path is required to already exist on the target machine. If connecting to a remote machine, you can specify the and for the task to run under.
        </td>
    </tr>
    <tr>
        <td>
            WebDirectoryDelete
        </td>
        <td>
            Deletes a web directory on a local or remote machine with IIS installed. The default is to delete the web directory on the local machine. If connecting to a remote machine, you can specify the and for the task to run under.
        </td>
    </tr>
    <tr>
        <td>
            WebDirectoryScriptMap
        </td>
        <td>
            Sets an application mapping for a filename extension on an existing web directory.
        </td>
    </tr>
    <tr>
        <td>
            WebDirectorySetting
        </td>
        <td>
            Reads and modifies a web directory configuration setting.
        </td>
    </tr>
    <tr>
        <td>
            WebDownload
        </td>
        <td>
            Downloads a resource with the specified URI to a local file.
        </td>
    </tr>
    <tr>
        <td>
            XmlMassUpdate
        </td>
        <td>
            Performs multiple updates on an XML file
        </td>
    </tr>
    <tr>
        <td>
            XmlQuery
        </td>
        <td>
            Reads a value or values from lines of XML
        </td>
    </tr>
    <tr>
        <td>
            XmlRead
        </td>
        <td>
            Reads a value from a XML document using a XPath.
        </td>
    </tr>
    <tr>
        <td>
            XmlUpdate
        </td>
        <td>
            Updates a XML document using a XPath.
        </td>
    </tr>
    <tr>
        <td>
            Xslt
        </td>
        <td>
            A task to merge and transform a set of xml files.
        </td>
    </tr>
    <tr>
        <td>
            XslTransform
        </td>
        <td>
            XslTransform task for Sandcastle.
        </td>
    </tr>
    <tr>
        <td>
            Zip
        </td>
        <td>
            Create a zip file with the files specified.
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <span style="font-size: 8pt">&nbsp;* Items not complete or have not been released.</span>
        </td>
    </tr>
</table>
    
##Getting Started

In order to use the tasks in this project, you need to import the MSBuild.Community.Tasks.Targets files. 

If you installed the project with the msi installer, you can use the following.

    <Import Project="$(MSBuildExtensionsPath)\MSBuildCommunityTasks\MSBuild.Community.Tasks.Targets"/>

Alternatively if you want to get started with the nuget packages please add the following.
  
    <PropertyGroup>
        <MSBuildCommunityTasksPath>$(SolutionDir)\.build</MSBuildCommunityTasksPath>
    </PropertyGroup>  
 
    <Import Project="$(MSBuildCommunityTasksPath)\MSBuild.Community.Tasks.Targets" />

## License

Copyright (c) 2012, LoreSoft
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

- Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
- Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
- Neither the name of LoreSoft nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
