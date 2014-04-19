using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

public class Uploader
{
	
	static QuickBloxConection con;
	
	[MenuItem("CloudStorage/Upload", false,3)]
    static void Execute()
    {
		
		con = QuickBloxConection.Current;
		
		if(con == null || con.user == null)
		{
			Debug.LogError("NO Connection");
			return;
		}
		
		if(!con.IsSignedin) 
		{
			Debug.LogError("NO Connection");
			return;
		}
		
		
		Object[] selectedObjects = Selection.GetFiltered(typeof (Object), SelectionMode.DeepAssets);
		if(selectedObjects != null && selectedObjects.Length > 0)
		{
	        foreach (Object selectedObject in selectedObjects)
	        {
				try
				{
					string path = AssetDatabase.GetAssetPath(selectedObject);
					if(string.IsNullOrEmpty(path)) continue;
					byte[] ass = System.IO.File.ReadAllBytes(path);
					if(ass == null) continue;
					if(ass.Length == 0) continue;
					QuickBloxFileUploader file = new QuickBloxFileUploader(ass,selectedObject.name + Path.GetExtension(path), MimeType.GetMIMEType(path.ToLower()), Settings.CurrentDevice);
					
					while(!file.IsLoaded)
					{
						if(!string.IsNullOrEmpty(file.ErrorMessage)) break;
						EditorUtility.DisplayProgressBar(selectedObject.name,file.CurrentAtion,file.Progress);
					}
					
					EditorUtility.ClearProgressBar();
				}
				catch
				{
					
				}
			}
			LogIn.NeedUpdate = true;
		}
		else
		{
			Debug.LogError("Need selected assets");
		}
	}
}
