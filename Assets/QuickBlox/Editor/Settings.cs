using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Xml;

public class Settings : EditorWindow 
{
	private const float BtnHeight = 30f;
	private const float BtnWidth = 150f;
	
	static QuickBloxConection con;
	
    [MenuItem("CloudStorage/Settings",false,0)]
    static void Init () 
	{
		con = QuickBloxConection.Current;
		QuickBloxCreator.CurrentDevice = _UserSettings.Target;
        EditorWindow.GetWindow(typeof(Settings));
    }
	
	
	public static bool CheckPlatform()
	{
		if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer 
			|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed
			|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.FlashPlayer)
		{
			Debug.LogError("Please switch your target platform to PC,Max, Linux Standalone or iOS and Android");
			GUILayout.Label ("Please switch your target platform to PC,Max, Linux Standalone or iOS and Android", EditorStyles.boldLabel);
			return false;
		}
		
		return true;
	}
	
   private bool IsConnection;
	
   void OnGUI ()
	{
		
		try
		{
			if(!CheckPlatform()) return;		
			
			EditorGUILayout.Space();
			GUILayout.Label ("AssetBundles creating parameters", EditorStyles.boldLabel);
				
			DrawBuildSettingPopup();
			
	        GUILayout.Label ("Application connection parameters", EditorStyles.boldLabel);
			EditorGUILayout.Space();			
			
			if(con == null)
			{
				con = QuickBloxConection.Current;
				return;
			}
				
			if(con.Settings == null)
			{
				Debug.Log("Settings error");
				return;
			}
			
			con.Settings.Endpoint =  EditorGUILayout.TextField ("Endpoint", con.Settings.Endpoint);
	        con.Settings.ApplicationId =  EditorGUILayout.TextField ("Application id", con.Settings.ApplicationId);
	        con.Settings.AuthenticationKey = EditorGUILayout.TextField ("Authentication key", con.Settings.AuthenticationKey);
	        con.Settings.AuthenticationSecret = EditorGUILayout.TextField ("Authentication secret", con.Settings.AuthenticationSecret);					
			
			if(this.IsConnection)
			{
				if(con.IsDone || con.IsError)
				{
					if(con.IsError)
					{
						Debug.LogError(con.Error);
					}
					
					this.IsConnection = false;
					return;
				}
					
				GUILayout.Label ("Sign in to QuickBlox...", EditorStyles.boldLabel);
				this.Repaint();				 
			}
			this.Repaint();			
			
			EditorGUILayout.Space();
			GUILayout.Label ("Connection status:  " + (con.IsDone? "OK":"ERROR"), EditorStyles.boldLabel);
			if(con.IsDone && !con.IsError)
			{				
				EditorGUILayout.Space();
				if(con.IsDone)
				{
					EditorGUILayout.TextField ("Date Created", con.session.CreatedDate.ToLongDateString());
					EditorGUILayout.TextField ("ID", con.session.Id.ToString());
					EditorGUILayout.TextField ("Time Updated", con.session.UpdatedTime.ToLongTimeString());
					EditorGUILayout.TextField ("Token", con.session.Token);
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Check connection", GUILayout.MinHeight(BtnHeight), GUILayout.ExpandWidth(false), GUILayout.MinWidth(BtnWidth)))
			{
				con.CheckConnection();
				this.IsConnection = true;				
			}
			GUILayout.Space(5);	
			if(GUILayout.Button("Save settings", GUILayout.MinHeight(BtnHeight), GUILayout.ExpandWidth(false), GUILayout.MinWidth(BtnWidth)))
			{
				con.SaveSettings();
				SaveUserSettings();		
			}		
			EditorGUILayout.EndHorizontal();
		}		
		catch
		{
			this.Repaint();
		}
		
		this.Repaint();
		
    }
	
	
	int i = 0;
	
	private void DrawBuildSettingPopup()
	{
		EditorGUILayout.Space();
		
		QuickBloxCreator.CurrentDevice = _UserSettings.Target;
		
		switch(QuickBloxCreator.CurrentDevice)
		{
			case BuildTarget.iPhone:
			{
				i = 0;
				break;	
			}
			case BuildTarget.StandaloneOSXIntel:
			{
				i = 1;
				break;	
			}
			case BuildTarget.Android:
			{
				i = 2;
				break;	
			}
			case BuildTarget.StandaloneWindows:
			{
				i = 3;
				break;	
			}
			case BuildTarget.StandaloneLinuxUniversal:
			{
				i = 4;
				break;	
			}
			case BuildTarget.PS3:
			{
				i = 5;
				break;	
			}
			case BuildTarget.Wii:
			{
				i = 6;
				break;	
			}
			case BuildTarget.XBOX360:
			{
				i = 7;
				break;	
			}
		default:
			{
				i = 0;
			break;
			}
		}
		
		
		
		string[] list = { "iOS", "Mac OS X", "Android", "Windows", "Linux", "PS3", "Wii", "XBOX360" };
		
		 i = EditorGUILayout.Popup("Build target",i,list);
		
	
		switch(i)
		{
			case 0:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.iPhone;
				break;	
			}
			case 1:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.StandaloneOSXIntel;
				break;	
			}
			case 2:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.Android;
				break;	
			}
			case 3:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.StandaloneWindows;
				break;	
			}
			case 4:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.StandaloneLinuxUniversal;
				break;	
			}
			case 5:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.PS3;
				break;	
			}
			case 6:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.Wii;
				break;	
			}
			case 7:
			{
				QuickBloxCreator.CurrentDevice = BuildTarget.XBOX360;
				break;	
			}
		}
		
		_UserSettings.Target = QuickBloxCreator.CurrentDevice;
		
		EditorGUILayout.Space();
		
	}
	
	
	public static string CurrentDevice
	{
		get
		{
			switch(_UserSettings.Target)
			{
			case BuildTarget.iPhone:
			{
				return "iOS";
			}
			case BuildTarget.StandaloneOSXIntel:
			{
				return	"Mac OS X";
			}
			case BuildTarget.Android:
			{
				return "Android";	
			}
			case BuildTarget.StandaloneWindows:
			{
				return "Windows";	
			}
			case BuildTarget.StandaloneLinuxUniversal:
			{
				return "Linux";	
			}
			case BuildTarget.PS3:
			{
				return "PS3";	
			}
			case BuildTarget.Wii:
			{
				return "Wii";	
			}
			case BuildTarget.XBOX360:
			{
				return "XBOX360";	
			}
			default:
			{
				return string.Empty;			
			}
		}
			
		}
	}
	
	
	
	
	private static UserSettings userSettings;
	
	public static UserSettings _UserSettings
	{
		get
		{
			try
			{
				if(userSettings != null) return userSettings;
				userSettings = LoadUserSettings();
				if(userSettings == null)
				{
					userSettings = new UserSettings();
					return userSettings;
				}
				return userSettings;
			}
			catch
			{
				userSettings = new UserSettings();
				return userSettings;
			}
		}
	}
	
	private static UserSettings LoadUserSettings()
	{
		if(!File.Exists(Application.dataPath + "/QuickBlox/Editor/UserSettings.xml")) return null;
		string content = File.ReadAllText(Application.dataPath + "/QuickBlox/Editor/UserSettings.xml");
		if(string.IsNullOrEmpty(content)) return null;
		return DeserializeObject(content) as  UserSettings;
	}
	
	
	public static void SaveUserSettings()
	{
		if(_UserSettings == null) return;
		string content = SerializeObject(_UserSettings);
		if(string.IsNullOrEmpty(content)) return;
		File.WriteAllText(Application.dataPath + "/QuickBlox/Editor/UserSettings.xml",content);
	}
	
	
	static string UTF8ByteArrayToString(byte[] characters) 
	   {      
	      UTF8Encoding encoding = new UTF8Encoding(); 
	      string constructedString = encoding.GetString(characters); 
	      return (constructedString); 
	   } 
 
   static byte[] StringToUTF8ByteArray(string pXmlString) 
	   { 
	      UTF8Encoding encoding = new UTF8Encoding(); 
	      byte[] byteArray = encoding.GetBytes(pXmlString); 
	      return byteArray; 
	   } 
 
   
   private static string SerializeObject(object pObject) 
	   { 
	      string XmlizedString = null; 
	      MemoryStream memoryStream = new MemoryStream(); 
	      XmlSerializer xs = new XmlSerializer(typeof(UserSettings)); 
	      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
	      xs.Serialize(xmlTextWriter, pObject); 
	      memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
	      XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray()); 
	      return XmlizedString; 
	   } 
 

  private static object DeserializeObject(string pXmlizedString) 
   { 
	      XmlSerializer xs = new XmlSerializer(typeof(UserSettings)); 
	      MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString)); 
	      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
		  xmlTextWriter.ToString();
	      return xs.Deserialize(memoryStream); 
   } 
	
}