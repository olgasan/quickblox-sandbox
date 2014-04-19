using UnityEngine;
using System.Collections;

/// <summary>
/// Return appropriate Path to use for different platforms (iOS, Android, PC, Mac)
/// </summary>
public static class PathHelper 
{
	/// <summary>
	/// Gets the path.
	/// </summary>
	/// <value>
	/// The path.
	/// </value>
	public static string Path
	{
		get
		{
			switch(Application.platform)
			{
				case RuntimePlatform.IPhonePlayer:
				{
					//Your game has read+write access to /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/Documents 
					//Application.dataPath returns /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/myappname.app/Data
					string path = Application.dataPath.Substring (0, Application.dataPath.Length - 5);					
					return path.Substring(0, path.LastIndexOf('/')) + "/Documents";
				}
				case RuntimePlatform.Android:
				{
					string path = Application.persistentDataPath;	
					path = path.Substring(0, path.LastIndexOf('/'));	
					return path;
				}
				default:
				{             
					//PC, Mac, Linux, etc.
					return Application.streamingAssetsPath;
				}
			}
		}
	}
}
