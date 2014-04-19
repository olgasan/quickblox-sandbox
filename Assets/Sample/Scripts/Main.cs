using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngineInternal;
using System.IO;
using System;

public class Main : MonoBehaviour 
{
	/// <summary>
	/// The refresh button flag.
	/// </summary>
	private bool IsRefresh;
	
	/// <summary>
	/// Sample folder 
	/// /QuickBlox/AppId<ID>
	/// </summary>
    private string SampleFolder="/QuickBlox/AppId1532";
	
	/// <summary>
	/// Login from QuickBlox.
	/// </summary>
	private string UserName = "rsk";
	
	/// <summary>
	/// Password from QuickBlox.
	/// </summary>
	private string Password = "QWE510091";	
	
	/// <summary>
	/// The list of files.
	/// </summary>
    private List<WWW> Files = new List<WWW>();
	
	void Awake()
	{	
		//Load all already downloaded elements (game objects and textures)
		PreLoadFiles(this.Files);				
		
		//Connect with QuickBlox by using Login/Password of any User you have created.
		QuickBloxConection.Current.Connect(this.UserName, this.Password);	
	}
	
	IEnumerator Start()
	{        
		//Null unitll connection is established
		while(!QuickBloxConection.Current.IsDone) yield return null;				
	}	
	
	void Update()
	{
		//Put files from list to scene		
		for(int i=0; i<this.Files.Count;++i)
		{
			WWW file = this.Files[i];
			if (file.isDone)
			{				
				string fileType = MimeType.GetMIMEType(Path.GetFileName(file.url));				
				//If file is asset and not null put it to scene
				if(fileType == MimeType.MIMETypesDictionary["assetbundle"])
				{					
					if (file.assetBundle != null)											
						GameObject.Instantiate(file.assetBundle.mainAsset as GameObject);										
					this.Files.Remove(file);										
					file.Dispose();
					return;
				}
				//If file is texture (jpg) find the appropriate game object and set it as main texture of object's material
				else if(fileType == MimeType.MIMETypesDictionary["jpg"])
				{					
					if(file.texture!=null)
					{
						GameObject tmpStand = GameObject.Find("/samplestandTemp(Clone)/QuickBloxPanel");
						if(tmpStand!=null)
						{
							tmpStand.renderer.material.mainTexture = file.texture;
							this.Files.Remove(file);										
							file.Dispose();
							return;
						}
					}					
				}
				else
				{
					this.Files.Remove(file);
					file.Dispose();
					return;
				}				
			}
		}		
	}
	
	void OnGUI()
	{		
        if (!QuickBloxConection.Current.IsDone)        
        {
            GUI.Label(new Rect(0, Screen.height - 20, Screen.width, 50), "CONNECTING...");
            return;
        }
        
        float boxWidth = Screen.width / 4.0f;
        float boxX = Screen.width - boxWidth;
        GUI.Box(new Rect(boxX, 0, boxWidth, Screen.height), "");

        if (GUI.Button(new Rect(boxX + 10, Screen.height - 60, boxWidth - 20, 50), "REFRESH"))
        {
			//Send a command to download the list of available for current user files.
            QuickBloxBlobManager.Current.Update();
            IsRefresh = true;           
        }
		
		//Create a list of buttons associated with a list of available files		
        if (QuickBloxBlobManager.Current.IsListLoaded && IsRefresh)
        {
            int j = 0;
			
			//Get the list of available blobs
			List<Blob> blobsList = QuickBloxBlobManager.Current.Blobs;
			for(int i=0;i<blobsList.Count;++i)
			{
				Blob file = blobsList[i];
				if (!IsFileExists(file.Name))
                {
                    if (file.IsDownloaded)
                    {		
						//Save file to StreamingAssets Folder, Load to memory and put into List<WWW>
						LoadFile(SaveFile(file), this.Files);                       
                    }
                    else
                    {
						//Create buttons with name of files and loading progress
                        Rect blobBtnRect = new Rect(boxX + 10, j * 55+10, boxWidth - 20, 50);
						
                        if (file.IsDownloading)
						{
                            GUI.Button(blobBtnRect, string.Format("{0}%", file.Progress));
						}
                        else
						{
                            if (GUI.Button(blobBtnRect, file.Name.Split('.')[0]))
							{
								//Send a command to start to download current blob
								file.Download();
							}
						}
                    }
                    j++; 
                }    
			}		
        }                    
	}
	
	#region Helpers to work with Bundles (load, save, open)	
	/// <summary>
	/// Determines whether this file exists in the specified path.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this file exists in the specified name; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='name'>
	/// If set to <c>true</c> name.
	/// </param>
    bool IsFileExists(string name)
    {
        return File.Exists(string.Format("{0}{1}/{2}", PathHelper.Path, SampleFolder, name));        
    }
	
	/// <summary>
	/// Save the file to folder with read/write access.
	/// Folder can be different for some platforms.
	/// </summary>
	/// <returns>
	/// The path to file.
	/// </returns>
	/// <param name='item'>
	/// Item.
	/// </param>
    string SaveFile(Blob item)
    {
        if (!Directory.Exists(PathHelper.Path + SampleFolder))
            Directory.CreateDirectory(PathHelper.Path + SampleFolder);

        string filePath = string.Format("{0}{1}/{2}", PathHelper.Path, SampleFolder, item.Name);        
        File.WriteAllBytes(filePath, item.file);        
        return filePath;        
    }
	
	/// <summary>
	/// Load the file and put it to list of bundles.
	/// </summary>
	/// <param name='filepath'>
	/// Filepath.
	/// </param>
	/// <param name='bundles'>
	/// Bundles.
	/// </param>
    void LoadFile(string filepath, List<WWW> bundles)
    {		
        WWW file = new WWW("file://" + filepath);
		bundles.Add(file);		
    }
	
	/// <summary>
	/// Load files from curent StreamingAssets folder.
	/// </summary>
	/// <param name='bundles'>
	/// Bundles.
	/// </param>
    void PreLoadFiles(List<WWW> bundles)
    {
        if (!Directory.Exists(PathHelper.Path + SampleFolder))
            Directory.CreateDirectory(PathHelper.Path + SampleFolder);		
        foreach (string file in Directory.GetFiles(string.Format("{0}{1}/", PathHelper.Path, SampleFolder)))
		{
			LoadFile(file, bundles);   			               
		}
    }	
	
	
	#endregion
}

