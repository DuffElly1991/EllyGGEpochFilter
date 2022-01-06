using System;

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
				Console.WriteLine("Error: Settings file did not contain expected entry \"LastEpochFilterFolder\"");
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

			Console.WriteLine("Successfully read in settings file \"Settings.xml\"");

			string filterSettingsFileName = "EllyGG Settings.xml";
			Console.WriteLine("Attempting to read filter settings file \"" + filterSettingsFileName + "\" in folder \"" + current.value + "\"");

			if(!FileManager.ReadFile(current.value, filterSettingsFileName, out settingsFileContent, out errorMessage))
			{
				//Error occured reading file
				Console.WriteLine("Error occured when trying to read file \"" + filterSettingsFileName + "\" in folder \"" + current.value + "\"");
				Console.WriteLine(errorMessage);

				return;
			}
			
			XmlManager filterSettingsXml = new XmlManager("  ");
			filterSettingsXml.loadXmlFromFileContents(settingsFileContent);
			
			Console.WriteLine("Successfully read in filter settings file");

			Console.WriteLine(filterSettingsXml.formatXml());
        }
    }
}
