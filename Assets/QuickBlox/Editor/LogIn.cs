using UnityEngine;
using UnityEditor;
using System.Collections;

public class LogIn : EditorWindow 
{
	public static bool NeedUpdate;
	
	static QuickBloxConection con;
	static UserSettings userSettinge;
    
	static string username = "";
	static string password = "";
	
	private const float BtnHeight = 30f;
	private const float BtnWidth = 150f;
	
    [MenuItem ("CloudStorage/Log In",false,2)]
    static void Init () 
	{
		userSettinge = Settings._UserSettings;
		username = userSettinge.Username;
		password =	userSettinge.Password;
		con = QuickBloxConection.Current;
        EditorWindow.GetWindow(typeof(LogIn));
    }
    
	private bool IsConnection;

   void OnGUI ()
	{
		
		if(!Settings.CheckPlatform()) return;
		
		if(userSettinge == null)
		{
			userSettinge = Settings._UserSettings;
			username = userSettinge.Username;
			password =	userSettinge.Password;
			if(string.IsNullOrEmpty(password))
			{
				password = string.Empty;
			}
			return;
		}
		
		if(con == null)
		{
			con = QuickBloxConection.Current;
			this.Repaint();
		}
		
		try
		{
			GUILayout.Space(20);	
			if(this.IsConnection)
			{
				if(con.IsDone || con.IsError)
				{
					this.IsConnection = false;
					NeedUpdate = true;
				}
				GUILayout.Label ("Sign in to QuickBlox...", EditorStyles.boldLabel);
				this.Repaint();
			}
			if(con.IsSignedin)
			{
				EditorGUILayout.TextField("ID",con.user.id.ToString());				
				EditorGUILayout.TextField ("Login", con.user.Username);
				EditorGUILayout.TextField ("Full Name", con.user.FullName);
				EditorGUILayout.TextField ("Email", con.user.Email);
				EditorGUILayout.TextField ("Date Created", con.user.CreatedDate.Value.ToLongDateString());
				if(con.user.ExternalUserId!=null)
					EditorGUILayout.TextField ("External ID", con.user.ExternalUserId.ToString());
				if(!string.IsNullOrEmpty(con.user.FacebookId))
					EditorGUILayout.TextField ("Facebook", con.user.FacebookId);
				if(!string.IsNullOrEmpty(con.user.TwitterId))
					EditorGUILayout.TextField ("Twitter", con.user.TwitterId);
			}
			//GUILayout.Space(20);
			EditorGUILayout.Space();
			username = EditorGUILayout.TextField("Username", username);
			EditorGUILayout.Space();
			password = EditorGUILayout.PasswordField("Password", password);
			EditorGUILayout.Separator();
			userSettinge.Save = EditorGUILayout.Toggle("Remember me",userSettinge.Save);
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Sign In Now", GUILayout.MinHeight(BtnHeight), GUILayout.ExpandWidth(false), GUILayout.MinWidth(BtnWidth)))
			{
				con.Connect(username,password);
				this.IsConnection = true;
				if(userSettinge.Save)
				{
					userSettinge.Username = username;
					userSettinge.Password = password;
				}
				else
				{
					userSettinge.Username = string.Empty;
					userSettinge.Password = string.Empty;
				}
				Settings.SaveUserSettings();
			}
		}
		catch
		{
			
		}		
		this.Repaint();		
    }
	
	
}