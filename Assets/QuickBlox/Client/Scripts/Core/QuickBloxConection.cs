using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using Instarest;

/// <summary>
/// Class - Singletone, establishes connection with the server and executes logging in by said user.
/// Any operations depend on this class.
/// </summary>
public class QuickBloxConection 
{
	
	#region Singleton
	private static QuickBloxConection _current;
	public static QuickBloxConection Current
	{
		get
		{
			if(_current == null)
			{
				_current = new QuickBloxConection();
				return _current;
			}
			else
				return _current;
		}
	}
	//
	
	private QuickBloxConection()
	{
		try
		{
			if(Application.isPlaying)
			{
				this.Settings = new QuickBloxConectionSettings();
			}
			else if(Application.isEditor)
			{
				this.LoadSettings();
			}
			else
				this.Settings = new QuickBloxConectionSettings();
		}
		catch
		{}
	}
#endregion
	
	private bool IsOK;
	
	/// <summary>
	/// Shows that the application interacts with the server on behalf of specified user.
	/// it is impossible to work with files if this parameter == false.
	/// If parameter IsAppSignedin == false and Error is not empty. Signinig in by said user is impossible.
	/// if this parameter returnes false and Error is not empty login and password should be checked.
	/// </summary>
	public bool IsSignedin;
	
	/// <summary>
	/// Application connected to the server successfully and got token (session was created).
	/// If this field returnes false, then some connection parameters are wrong, 
	/// it is necessary to look at Error field to reveal the mistake.
	/// If this parameter is false, signinig in by said user is impossible.
	/// </summary>
	public bool IsAppSignedin;
	private bool IsConnectionOnly;
	public bool IsError;
	public string ErrorMessage;
	
	/// <summary>
	/// Positive if the application and the user signed in.
	/// Negative if there wasn't any request to server yet, the request on the server is being executed, 
	/// the server has returned an error. In the last case it is necessary to look at Error field.
	/// </summary>
	public bool IsDone
	{
		get
		{
			if(this.IsError) return false;
			if(this.IsOK) return true;
			if(this.ConectionLoader == null) return false;
			if(!this.CheckConnectionResult()) return false;
			if(this.IsConnectionOnly) return true;
			if(this.LoginLoader == null)
			{
				this.Login();
				this.IsAppSignedin = true;
				return false;
			}
	
			if(!this.CheckLoginResult()) return false;
			this.IsOK = true;
			return true;
		}
	}
	
	/// <summary>
	/// Current session.
	/// Can be not null if tha application has connected to the server successfully.
	/// </summary>
	public Session session;
	
	/// <summary>
	/// Connection settings.
	/// Parameters of this field are used for application connection to the server.
	/// </summary>
	public QuickBloxConectionSettings Settings;
	
	/// <summary>
	/// User by whoom application works.
	/// Can be not null if singing in by said user was successful.
	/// </summary>
	public User user;
	
	/// <summary>
	/// If while application connection to the server or said user singing in an exception occurred,
	/// or the server returned an error, error description will be in this field. 
	/// </summary>
	public string Error;
	
	/// <summary>
	/// Username for sign in.
	/// </summary>
	private string Username;
	private string Password;
	private Request LoginLoader;
	private Request ConectionLoader;
	
	/// <summary>
	/// Method executes application connection to the server and in the successful response case signs in by said user.
	/// If errors occur while this process, they will be displayed in the Error field.
	/// Parameter Settings should be filled correctly.
	/// </summary>
	/// <param name='Username'>
	/// Username which is used to sign in.
	/// Should not be empty. An user with the said username must exist.
	/// </param>
	/// <param name='Password'>
	/// Password for sign in.
	/// Password must correspond to the username.
	/// </param>
	public void Connect(string Username, string Password)
	{
		if(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
		{
			this.IsError = true;
			return;
		}
		
		this.Username = Username;
		this.Password = Password;
		if(!this.IsAppSignedin)
		{
			this.ConectionLoader = null;
		}
		this.LoginLoader = null;
		IsOK = false;
		this.session = null;
		this.user = null;
		this.IsSignedin = false;
		this.IsConnectionOnly = false;
		this.IsError = false;
		this.ErrorMessage = string.Empty;
		this.Connection();
	}
	
	
	private void Login()
	{
		LoginLoader = new Request(this.Settings.Endpoint + "login.xml");
		LoginLoader.SetHeader("QuickBlox-REST-API-Version","0.1.0");
		LoginLoader.SetHeader("QB-Token",this.session.Token);
        LoginLoader.AddFormField("login", this.Username);
        LoginLoader.AddFormField("password", this.Password);
		LoginLoader.Send(AcceptVerbs.POST);
	}
	
	
	
	private bool CheckLoginResult()
	{
		if(LoginLoader.isDone)
		{
			if(LoginLoader.response.status == 202)
			{
				try
				{
					this.user = new User(LoginLoader.response.Text);
					this.IsSignedin = true;
					return true;
				}
				catch(Exception ex)
				{
					Debug.LogError(LoginLoader.response.Text);
					this.user = null;
					this.Error = ex.Message;
					this.IsError = true;
					return false;
				}
			}
			else
			{
				Debug.LogError("PLogin or Password incorrect");
				this.Error = "PLogin or Password incorrect";
				this.IsError = true;
				this.user = null;
				return false;
			}
		}
		else
		{
			
			this.user = null;
			return false;
		}
	}
	
	/// <summary>
	/// Connection to the server is being executed. 
	/// Used only to check connection.
	/// Settings parameter must be filled in correctly.
	/// </summary>
	public void CheckConnection()
	{
		if(this.Settings == null)
		{
			this.IsError = true;
			return;
		}
		
		this.LoginLoader = null;
		this.IsConnectionOnly = true;
		this.IsAppSignedin = false;
		IsOK = false;
		this.user = null;
		this.IsSignedin = false;
		this.session = null;
		this.IsError = false;
		this.ErrorMessage = string.Empty;
		this.Connection();
	}
	
	private void Connection()
	{
		if(this.Settings == null)
		{
			this.session = null;
			return;
		}
		
		if(string.IsNullOrEmpty(this.Settings.ApplicationId)
			|| string.IsNullOrEmpty(this.Settings.AuthenticationKey)
			|| string.IsNullOrEmpty(this.Settings.AuthenticationSecret)
			|| string.IsNullOrEmpty(this.Settings.Endpoint))
		{
			this.session = null;
			return;
		}
		
		ConectionLoader = new Request(this.Settings.Endpoint + "auth.xml");
        int randomResult = UnityEngine.Random.Range(0,1000);
		
        ConectionLoader.AddFormField("application_id", Settings.ApplicationId);
        ConectionLoader.AddFormField("auth_key", Settings.AuthenticationKey);
        ConectionLoader.AddFormField("nonce", randomResult.ToString());

		
		int ts = (int)((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); 

        ConectionLoader.AddFormField("timestamp", ts.ToString());
        byte[] key = Encoding.UTF8.GetBytes(Settings.AuthenticationSecret);

        StringBuilder signature = new StringBuilder();
        signature.Append("application_id");
        signature.Append("=");
        signature.Append(Settings.ApplicationId);
        signature.Append("&");
        signature.Append("auth_key");
        signature.Append("=");
        signature.Append(Settings.AuthenticationKey);
        signature.Append("&");

        signature.Append("nonce");
        signature.Append("=");
        signature.Append(randomResult);
        signature.Append("&");
        signature.Append("timestamp");
        signature.Append("=");
        signature.Append(ts.ToString());
        
        ConectionLoader.AddFormField("signature", Encode(signature.ToString(),key));
		ConectionLoader.Send(AcceptVerbs.POST);
		
	}
	
	private bool CheckConnectionResult()
	{
		if(ConectionLoader.isDone)
		{
			try
			{
				if(ConectionLoader.response.status == 201)
				{
					try
					{
						this.session = new Session(ConectionLoader.response.Text);
						return true;
					}
					catch(Exception ex)
					{
						Debug.LogError(ConectionLoader.response.Text);
						this.session = null;
						this.Error = ex.Message;
						this.IsError = true;
						return false;
					}
				}
				else
				{
					
					this.session = null;
					Debug.LogError("Connection error");
					this.Error = "Connection error";
					this.IsError = true;
					return false;
				}
			}
			catch(Exception ex)
			{
				this.session = null;
					Debug.LogError("Connection error");
					this.Error = ex.Message;
					this.IsError = true;
					return false;
			}
		}
		else
		{
			this.session = null;
			return false;
		}
			
	}
	
	private string Encode(string input, byte[] key)
        {
           HMACSHA1 myhmacsha1 = new HMACSHA1(key);
           byte[] byteArray = Encoding.UTF8.GetBytes(input);
           MemoryStream stream = new MemoryStream(byteArray);
           return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
       }
	
	#region Settings
	
	/// <summary>
	/// Saves parameters in the file system.
	/// </summary>
	public void SaveSettings()
	{
		if(this.Settings == null) return;
		string content = this.SerializeObject(this.Settings);
		if(string.IsNullOrEmpty(content)) return;
		File.WriteAllText(Application.dataPath + "/QuickBlox/Editor/Settings.xml",content);
	}
	
	/// <summary>
	/// Downloads settings from the file system.
	/// </summary>
	public void LoadSettings()
	{
		if(!File.Exists(Application.dataPath + "/QuickBlox/Editor/Settings.xml"))
		{
			this.Settings = new QuickBloxConectionSettings();
			return;
		}
		string content = File.ReadAllText(Application.dataPath + "/QuickBlox/Editor/Settings.xml");
		if(string.IsNullOrEmpty(content))
		{
			this.Settings = new QuickBloxConectionSettings();
			return;
		}
		
		this.Settings = this.DeserializeObject(content) as QuickBloxConectionSettings;
	}
	
	
	string UTF8ByteArrayToString(byte[] characters) 
	   {      
	      UTF8Encoding encoding = new UTF8Encoding(); 
	      string constructedString = encoding.GetString(characters); 
	      return (constructedString); 
	   } 
 
   byte[] StringToUTF8ByteArray(string pXmlString) 
	   { 
	      UTF8Encoding encoding = new UTF8Encoding(); 
	      byte[] byteArray = encoding.GetBytes(pXmlString); 
	      return byteArray; 
	   } 
 
   
   private string SerializeObject(object pObject) 
	   { 
	      string XmlizedString = null; 
	      MemoryStream memoryStream = new MemoryStream(); 
	      XmlSerializer xs = new XmlSerializer(typeof(QuickBloxConectionSettings)); 
	      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
	      xs.Serialize(xmlTextWriter, pObject); 
	      memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
	      XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray()); 
	      return XmlizedString; 
	   } 
 

  private object DeserializeObject(string pXmlizedString) 
   { 
	      XmlSerializer xs = new XmlSerializer(typeof(QuickBloxConectionSettings)); 
	      MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString)); 
	      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
		  xmlTextWriter.ToString();
	      return xs.Deserialize(memoryStream); 
   } 
	
	
	#endregion
	

}


