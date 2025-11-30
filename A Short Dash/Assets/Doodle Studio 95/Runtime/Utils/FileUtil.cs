using System.IO;

namespace DoodleStudio95 
{
	public static class FileUtil
	{
		public static void WriteAllTextMkdirs(string path, string text, bool readOnly = false)
		{
			var dirName = Path.GetDirectoryName(path);
			if (File.Exists(dirName))
				throw new System.Exception("trying to make directory but a file exists instead: " + dirName);
			if (!Directory.Exists(dirName))
				Directory.CreateDirectory(dirName);

			if (File.Exists(path))
				MarkReadOnly(path, false);	
			File.WriteAllText(path, text);
			MarkReadOnly(path, readOnly);
		}

		public static void MarkReadOnly(string path, bool readOnly)
		{
			var currentAttribs = File.GetAttributes(path);
			var attributes = readOnly
				? (currentAttribs | FileAttributes.ReadOnly)
				: (currentAttribs & ~FileAttributes.ReadOnly);

			File.SetAttributes(path, attributes);
		}

		public static bool IsReadOnly(string path)
		{
			var currentAttribs = File.GetAttributes(path);
			return (currentAttribs & FileAttributes.ReadOnly) != 0;
		}
	}
}