﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace K2Field.K2NE.ServiceBroker.Helpers.PowerShell
{
    public static class PowerShellHelper
    {
        public static string RunScript(string powerShellScript, List<PowerShellVariablesDC> variablesList)
        {
            using (System.Management.Automation.PowerShell powerShellInstance = System.Management.Automation.PowerShell.Create())
            {
                //set input variables
                foreach (PowerShellVariablesDC variable in variablesList)
                {
                    powerShellInstance.Runspace.SessionStateProxy.SetVariable(variable.Name, variable.Value);
                }                    

                powerShellInstance.AddScript(powerShellScript);
                    
                // begin invoke execution on the pipeline
                Collection<System.Management.Automation.PSObject> returnValue = powerShellInstance.Invoke();

                //get input variables
                foreach (PowerShellVariablesDC variable in variablesList)
                {
                    variable.Value = powerShellInstance.Runspace.SessionStateProxy.GetVariable(variable.Name);
                }

                return GetScriptOutput(returnValue);
            }
        }

        private static string GetScriptOutput(Collection<System.Management.Automation.PSObject> psObjectValues)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (System.Management.Automation.PSObject obj in psObjectValues)
            {
                stringBuilder.AppendLine(obj.ToString());
            }
            return stringBuilder.ToString();
        }

        #region Directory interaction
        
        public static string LoadScriptByPath(string filePath)
        {
            try
            {
                // Create an instance of StreamReader to read from our file. 
                // The using statement also closes the StreamReader. 
                using (StreamReader sr = new StreamReader(filePath))
                {

                    // use a string builder to get all our lines from the file 
                    StringBuilder fileContents = new StringBuilder();

                    // string to hold the current line 
                    string curLine;

                    // loop through our file and read each line into our 
                    // stringbuilder as we go along 
                    while ((curLine = sr.ReadLine()) != null)
                    {
                        // read each line and MAKE SURE YOU ADD BACK THE 
                        // LINEFEED THAT IT THE ReadLine() METHOD STRIPS OFF 
                        fileContents.Append(curLine + "\n");
                    }

                    // call RunScript and pass in our file contents 
                    // converted to a string 
                    return fileContents.ToString();
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong. 
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("The file could not be read:");
                stringBuilder.AppendLine(e.Message);
                stringBuilder.AppendLine("\n");
                throw new Exception(stringBuilder.ToString(), e);
            }
        }

        public static Dictionary<string, string> GetFilePathsFromDirectories(string folderNames)
        {
            Dictionary<string, string> scriptFiles = new Dictionary<string, string>();

            foreach (string folderName in folderNames.Split(';'))
            {
                if (folderName.Contains(':'))
                    continue;

                string fullFolderPath = GetServiceBrokerDirectory() + folderName.Trim('.').Trim('\\');

                if (Directory.Exists(fullFolderPath))
                {
                    string[] scriptFilePaths = Directory.GetFiles(fullFolderPath, "*.ps1");

                    foreach (string scriptFilePath in scriptFilePaths)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(scriptFilePath);
                        scriptFiles.Add(fileName, scriptFilePath);
                    }
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(fullFolderPath);
                    }
                    catch (Exception e)
                    {
                        // Let the user know what went wrong. 
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("The powershell directory \"");
                        stringBuilder.AppendLine(fullFolderPath);
                        stringBuilder.AppendLine("\" could not be created: ");
                        stringBuilder.AppendLine(e.Message);
                        stringBuilder.AppendLine("\n");
                        throw new Exception(stringBuilder.ToString(), e);
                    }
                }
            }

            return scriptFiles;
        }

        //Service broker working from K2 blackpearl\Host Server\Bin folder. We are getting K2 blackpearl\ServiceBroker
        private static string GetServiceBrokerDirectory()
        {
            return Directory.GetParent(Directory.GetParent(@".").FullName).FullName + "\\ServiceBroker\\";
        }

        #endregion
    }
}
