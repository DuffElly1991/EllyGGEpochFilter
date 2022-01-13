using System;
using System.Collections.Generic;
using System.Threading;

namespace EllyGGEpochFilter
{
    class Program
    {
		static void Main(string[] args)
        {
			//We don't use args, so if there are any arguments then show message and exit
			if(args.Length > 0)
			{
				Console.WriteLine("Error: unexpected arguments. Use Settings.xml instead");
				return;
			}
			
			Console.WriteLine("Attempting to read settings file \"Settings.xml\"");

			string settingsFileContent = null;
			string errorMessage = null;
			if(!FileManager.ReadFile("", "Settings.xml", out settingsFileContent, out errorMessage))
			{
				//Error occured reading file
				Console.WriteLine("Error occured when trying to read file \"Settings.xml\"");
				Console.WriteLine(errorMessage);

				return;
			}

			XmlManager settingsFileXml = new XmlManager("  ");
			settingsFileXml.loadXmlFromFileContents(settingsFileContent);
			
			xmlNode current = settingsFileXml.getXml();
			if(current == null)
			{
				Console.WriteLine("Error: xml structure is empty");
				return;
			}

			if(current.label != "Settings")
			{
				Console.WriteLine("Error: Settings file did not have the expected root node \"Settings\"");
				return;
			}

			if(current.children == null)
			{
				Console.WriteLine("Error: Settings file is empty");
				return;
			}

			foreach(xmlNode x in current.children)
			{
				if(x.label == "LastEpochFilterFolder")
				{
					current = x;
					break;
				}
			}

			if(current.label != "LastEpochFilterFolder")
			{
				Console.WriteLine("Error: Settings file did not contain expected entry \"LastEpochFilterFolder\"");
				return;
			}
			string filterFolder = current.value;

			current = current.parent;


			Console.WriteLine("Successfully read in settings file \"Settings.xml\"");

			string informationGathererFileName = "EllyGG Information Gatherer.xml";
			if(!FileManager.ReadFile(filterFolder, informationGathererFileName, out settingsFileContent, out errorMessage))
			{
				//Error occured, so check the other location 
				if(!FileManager.ReadFile("", informationGathererFileName, out settingsFileContent, out errorMessage))
				{
					//Error occured reading file
					Console.WriteLine("Error occured when trying to read file \"" + informationGathererFileName);
					Console.WriteLine(errorMessage);

					return;
				}
			}

			settingsFileXml = new XmlManager();
			settingsFileXml.loadXmlFromFileContents(settingsFileContent);
			if(!Filter.LoadGatheredInformationFromXML(settingsFileXml, out errorMessage))
			{
				Console.WriteLine("Error occured when trying to process file \"" + informationGathererFileName);
				Console.WriteLine(errorMessage);

				return;
			}

			Console.WriteLine("Successfully read in settings file \"" + informationGathererFileName + "\"");

			string filterSettingsFileName = "EllyGG Settings.xml";
			Console.WriteLine("Starting filter builder for filter settings file \"" + filterSettingsFileName + "\" in folder \"" + filterFolder + "\"");

			string previousFile = "";
			
			while(true)
			{
				if(!FileManager.ReadFile(filterFolder, filterSettingsFileName, out settingsFileContent, out errorMessage))
				{
					//Error occured reading file
					Console.WriteLine("Error occured when trying to read file \"" + filterSettingsFileName + "\" in folder \"" + filterFolder + "\"");
					Console.WriteLine(errorMessage);

					return;
				}

				//Check if file has been changed since last run
				//This is done instead of using last modified time as that is not guaranteed to be updated
				//If first time then will pass check as comparing against empty string
				if(previousFile != settingsFileContent)
				{
					XmlManager filterSettingsXml = new XmlManager("  ");
					filterSettingsXml.loadXmlFromFileContents(settingsFileContent);
					
					Console.WriteLine("Successfully read in filter settings file");

					List<(string name, string xml)> filters = new List<(string name, string xml)>();
					if(Filter.LoadFilterSpecificationsFromSettingsXML(filterSettingsXml, out errorMessage, out filters))
					{
						foreach((string name, string xml) filter in filters)
						{
							if(!FileManager.SaveFile(filterFolder, filter.name, filter.xml, out errorMessage))
							{
								Console.WriteLine("Error occured when trying to write file \"" + filter.name + "\" in folder \"" + filterFolder + "\"");
								Console.WriteLine(errorMessage);
							}
						}
						Console.WriteLine("Successfully wrote filters");
					}
					else
					{
						Console.WriteLine("Error occured when writing filters: " + errorMessage);
						return;
					}
				}

				previousFile = settingsFileContent;

				Thread.Sleep(250);
			}
        }
    }
}