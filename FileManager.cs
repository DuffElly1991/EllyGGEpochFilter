using System;
using System.IO;

namespace EllyGGEpochFilter
{
	public static class FileManager
	{
		public static bool ReadFile(string folder, string fileName, out string content, out string errorMessage)
		{
			content = null;

			using (FileStream fs = new FileStream(Path.Combine(folder, fileName.Trim()), FileMode.Open))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					try
					{
						content = sr.ReadToEnd();
					}
					catch(Exception e)
					{
						errorMessage = e.ToString();
						return false;
					}
				}
			}

			errorMessage = null;
			return true;
		}

		public static void SaveFile(string folder, string fileName, string content)
		{
			using (FileStream fs = new FileStream(Path.Combine(folder, fileName.Trim()), FileMode.Create))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.WriteLine(content);
					sw.Flush();
				}
			}
		}
	}
}