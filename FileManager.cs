using System;
using System.IO;

namespace EllyGGEpochFilter
{
	public static class FileManager
	{
		public static bool ReadFile(string folder, string fileName, out string content, out string errorMessage)
		{
			content = null;

			try
			{
				using (FileStream fs = new FileStream(Path.Combine(folder, fileName.Trim()), FileMode.Open))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						content = sr.ReadToEnd();
					}
				}
			}
			catch(Exception e)
			{
				errorMessage = e.ToString();
				return false;
			}

			errorMessage = null;
			return true;
		}

		public static bool SaveFile(string folder, string fileName, string content, out string errorMessage)
		{
			try
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
			catch(Exception e)
			{
				errorMessage = e.ToString();
				return false;
			}

			errorMessage = null;
			return true;
		}
	}
}