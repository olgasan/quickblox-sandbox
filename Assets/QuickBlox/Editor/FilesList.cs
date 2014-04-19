using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class FilesList : EditorWindow 
{	
	static QuickBloxBlobManager manager;
    static QuickBloxConection connectionContext;

    
    private bool IsReset = false;
	private Vector2 ScrollPosition;  
	
	private string ElementsPerPageStr = "10";
	private int ElementsPerPage = 10;
	
    private const float LetterWidth = 7.0f;
    private const float RowHeight = 15f;
	private const float BtnHeight = 20f;
	

    [MenuItem ("CloudStorage/Storage",false,4)]
    static void Init () 
	{
		manager = QuickBloxBlobManager.Current;		
		connectionContext = QuickBloxConection.Current;
		manager.Update();
        EditorWindow.GetWindow(typeof(FilesList));
    }    
	void OnGUI ()
	{		
		if(!Settings.CheckPlatform()) return;		
		this.Repaint();		
		#region Contexts Checkers
		if(connectionContext == null)
		{
			connectionContext = QuickBloxConection.Current;
			return;	
		}		
		if(manager == null)
		{
			manager = QuickBloxBlobManager.Current;
			manager.per_page = ElementsPerPage;
		}		
		if(connectionContext.user == null) return;				
		if(LogIn.NeedUpdate)
		{
			manager.Update();
			LogIn.NeedUpdate = false;
		}
		#endregion		
		try
		{
		    this.Repaint();
			
		    if(connectionContext == null || manager == null) return;
		    if(connectionContext.session == null || connectionContext.user == null) return; 			
			
			#region Top Control Panel
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal();
			GUILayoutOption[] commonBtnLayoutOptions = { GUILayout.MinHeight(BtnHeight), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };					
			if(GUILayout.Button("Update",commonBtnLayoutOptions))
			{		
				manager.Update();
			}			
			GUILayout.Space(5);			
			if(GUILayout.Button("First",commonBtnLayoutOptions))
			{			
				if(manager.current_page!=1)
				{
					manager.current_page = 1;
					manager.Update();
				}				
			}			
			if(GUILayout.Button("Previous",commonBtnLayoutOptions))
			{			
				if(manager.current_page > 1)
			    {
				    manager.current_page--;
				    manager.Update();
			    }
			}
			//Current page of all
			GUILayout.Label(string.Format("{0}/{1}", manager.current_page, this.GetTotalPages()), EditorStyles.boldLabel, GUILayout.ExpandWidth(false));			
			if(GUILayout.Button("Next",commonBtnLayoutOptions))
			{			
				if(manager.current_page < this.GetTotalPages())
			    {
				    manager.current_page++;
				    manager.Update();
			    }
			}			
			if(GUILayout.Button("Last",commonBtnLayoutOptions))
			{			
				if(manager.current_page!=this.GetTotalPages())
				{
					manager.current_page = this.GetTotalPages();
					manager.Update();
				}				
			}			
			GUILayout.Space(10);			
			//Show X entries of Y, where X = current elements per page, Y = number of elements
			GUILayout.Label("Show",GUILayout.ExpandWidth(false));				
			ElementsPerPageStr = GUILayout.TextField(ElementsPerPageStr,3,GUILayout.ExpandWidth(false));
			GUILayout.Label(string.Format("entries of {0}", manager.total_entries), GUILayout.ExpandWidth(false));			
			if(int.TryParse(ElementsPerPageStr, out ElementsPerPage) && ElementsPerPage>0)			
				manager.per_page = ElementsPerPage;						
			else			
				ElementsPerPageStr = manager.per_page.ToString();			
			EditorGUILayout.EndHorizontal();		
			#endregion						
		
		    if(manager.IsListLoaded)
		    {
                ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);			    
                int[] maxLengths = GetMaxLengtsOfFields(manager.Blobs);
				this.CreateListHeaders(maxLengths);
				for(int i=0; i<manager.Blobs.Count;++i)
				{
					this.CreateListElement(manager.Blobs[i], maxLengths);
				    if(this.IsReset)
				    {
					    this.IsReset = false;
                        GUILayout.EndScrollView();
					    this.Repaint();
					    return;				
				    }				    
				}                
                GUILayout.EndScrollView();			
		    }
		    else
		    {			    
			    GUILayout.Label ("Loading...", EditorStyles.boldLabel);	
		    }		
		}
		catch
		{
			
        }	
	}
	
	private int[] GetMaxLengtsOfFields(List<Blob> blobs)
	{
		int[] maxLengths = { 0, 0, 0, 0, 0, 0};
		manager.Blobs.ForEach(delegate(Blob blob)
		{
			if (blob.Id.ToString().Length > maxLengths[0]) maxLengths[0] = blob.Id.ToString().Length;			
			if (blob.Name.Length > maxLengths[1]) maxLengths[1] = blob.Name.Length;
			if (blob.ContentType.Length > maxLengths[2]) maxLengths[2] = blob.ContentType.Length;
			int fileSizeLength = GetFileSizeKbStr(blob.Size).Length;
			if (fileSizeLength > maxLengths[3]) maxLengths[3] = fileSizeLength;				
			if (blob.CreatedAt.ToShortDateString().Length > maxLengths[4]) maxLengths[4] = blob.CreatedAt.ToShortDateString().Length;
			if(blob.Tags!=null)
				if (blob.Tags.Length > maxLengths[5]) maxLengths[5] = blob.Tags.Length;
		});	   
		return maxLengths;
	}
	
	private string GetFileSizeKbStr(uint filesize)
	{
		float size = filesize / 1024.0f;
		if (size < 1) size = 1.0F;
		return string.Format("{0:f}", size);
	}
	
	private int GetTotalPages()
	{
		return (manager.total_entries % manager.per_page) > 0 ? ((int)(manager.total_entries / manager.per_page) + 1) : (int)(manager.total_entries / manager.per_page);
	}
	
	private void CreateListHeaders(int[] maxLengths)
	{
		GUILayout.BeginHorizontal();
		GUILayoutOption[] commonLayoutOptions = { GUILayout.MinHeight(RowHeight), GUILayout.MinWidth(0), GUILayout.ExpandHeight(false),GUILayout.ExpandWidth(false) };
		float approx = 0.5f;
		commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[0]<2?2:(maxLengths[0]+approx)) * LetterWidth), 2);
		GUILayout.Label("ID", commonLayoutOptions);
		commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[1]<4?4:(maxLengths[1]+approx)) * LetterWidth), 2);
		GUILayout.Label("Name", commonLayoutOptions);
		commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[2]<4?4:(maxLengths[2]+approx)) * LetterWidth), 2);
		GUILayout.Label("Type", commonLayoutOptions);	
		commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[3]<8?8:(maxLengths[3]+approx)) * LetterWidth), 2);
		GUILayout.Label("Size(kb)", commonLayoutOptions);	
		commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[4]+approx) * LetterWidth), 2);
		GUILayout.Label("Date", commonLayoutOptions);	
		GUILayout.EndHorizontal();
	}
	
	private void CreateListElement(Blob blob, int[] maxLengths)
	{
		try
		{					
			GUILayout.Space(5);
			
	  	    Rect layoutArea = EditorGUILayout.BeginHorizontal();
			layoutArea.height+=3;
			GUI.Box(layoutArea,GUIContent.none);
		    
            GUILayoutOption[] commonLayoutOptions = { GUILayout.MinHeight(RowHeight), GUILayout.ExpandWidth(false), GUILayout.MinWidth(0), GUILayout.ExpandHeight(false) };
            
			commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[0]+1) * LetterWidth), 2);
            GUILayout.Label(blob.Id.ToString(), commonLayoutOptions);
            
			commonLayoutOptions.SetValue(GUILayout.MaxWidth(maxLengths[1] * LetterWidth), 2);
            GUILayout.Label(blob.Name, commonLayoutOptions);
            
			commonLayoutOptions.SetValue(GUILayout.MaxWidth(maxLengths[2] * LetterWidth), 2);
            GUILayout.Label(blob.ContentType, commonLayoutOptions);
            
			commonLayoutOptions.SetValue(GUILayout.MaxWidth((maxLengths[3]+1) * LetterWidth), 2);
            GUILayout.Label(GetFileSizeKbStr(blob.Size), commonLayoutOptions);	
			
            commonLayoutOptions.SetValue(GUILayout.MaxWidth(maxLengths[4] * LetterWidth), 2);
            GUILayout.Label(blob.CreatedAt.ToShortDateString(), commonLayoutOptions);	
			
			commonLayoutOptions.SetValue(GUILayout.MaxWidth(maxLengths[5] * LetterWidth), 2);
            GUILayout.Label(blob.Tags, commonLayoutOptions);		 
		
		    if(blob.IsDeleting || blob.IsDownloading)
		    {
			    if(blob.IsDeleted)
			    {
				    manager.Blobs.Remove(blob);
				    this.IsReset = true;
				    EditorGUILayout.EndHorizontal();
				    return;
			    }				
			    if(blob.IsDownloaded)
			    {					
				    string path = "Assets/StreamingAssets";
				    if(!Directory.Exists(path)) Directory.CreateDirectory(path);
				    path+= string.Format("/QuickBlox/AppId{0}",connectionContext.Settings.ApplicationId);
				    if(!Directory.Exists(path)) Directory.CreateDirectory(path);
				    path+= "/" + blob.Name;
				    blob.Save(path);
				    AssetDatabase.ImportAsset(path);					
			    }			
                GUILayout.Label(blob.IsDownloading ? blob.Progress.ToString() + "%" : "operation is in progress...");			    
		    }
		    else
		    {
                commonLayoutOptions.SetValue(GUILayout.MinWidth(10 * LetterWidth), 2);
                if (GUILayout.Button("Delete", commonLayoutOptions))
			    {
				    blob.Delete();
			    }
			    if(blob.IsFileSave)
			    {
                    commonLayoutOptions.SetValue(GUILayout.MinWidth(12 * LetterWidth), 2);
                    if (GUILayout.Button("View", commonLayoutOptions))
				    {
					    Object ob = AssetDatabase.LoadMainAssetAtPath(blob.FilePath);
					    EditorGUIUtility.PingObject(ob);
				    }
			    }
			    else
			    {
                    commonLayoutOptions.SetValue(GUILayout.MinWidth(12 * LetterWidth), 2);
                    if (GUILayout.Button("Download", commonLayoutOptions))
				    {
					    blob.Download();
				    }
			    }
		    }									
		    EditorGUILayout.EndHorizontal();					
		}
		catch
		{
			
        }
	}	
}