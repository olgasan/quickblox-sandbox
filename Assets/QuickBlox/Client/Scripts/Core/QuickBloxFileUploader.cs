using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  Uploads a file to server.
/// </summary>
public class QuickBloxFileUploader 
{
	
	private QuickBloxConection con;
	
	private WWW CreateBlobLoader;
	private WWW ComplateLoader;
	private WWW Uploadloader;
	
	/// <summary>
	/// File entity
	/// </summary>
	public Blob blob;
	
	public PostResponse ps;
	
	/// <summary>
	/// Error message.
	/// Can be not empty if while file download no exception occurred or the server didn't rerurn an error.
	/// </summary>
	public string ErrorMessage;
	
	/// <summary>
	/// Filename
	/// </summary>
	public string Name;
	
	/// <summary>
	/// The type of the MIME.
	/// </summary>
	public string MimeType;
	
	/// <summary>
	/// File for upload to the server.
	/// </summary>
	private byte[] file;
	
	/// <summary>
	/// The tags.
	/// </summary>
	public string Tags;
	
	
	/// <summary>
	/// File upload to the server progress bar in percent.
	/// </summary>
	/// <value>
	/// 0-100
	/// </value>
	public int Progress
	{
		get
		{
			if(this.Uploadloader == null) return 0;
			return (int)(this.Uploadloader.uploadProgress*100);
		}
	}
	
	/// <summary>
	/// File upload stage which is being executed in the moment.
	/// There are three file upload stages:
	/// 1. Entity registration
	/// 2. File upload
	/// 3. Upload confirmation
	/// </summary>
	public string CurrentAtion;
	
	/// <summary>
	/// Whether the file is uploaded to the server.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is load; otherwise, <c>false</c>.
	/// </value>
	public bool IsLoaded
	{
		get
		{
			if(!string.IsNullOrEmpty(this.ErrorMessage)) return false;
			
			if(this.CreateBlobLoader == null)
			{
				this.CreateBlob();
				this.CurrentAtion =  "Preparation for file download...";
				return false;
			}
			if(!this.CreateBlobLoader.isDone) return false;
			if(!this.CheckStatus(this.CreateBlobLoader))
			{
				this.ErrorMessage = "CreateBlobLoader error!";
				return false;
			}
			
			if(this.Uploadloader == null)
			{
				this.Upload(this.file);
				this.CurrentAtion = "Send the file to the server...";
				return false;
			}
			if(!this.Uploadloader.isDone) return false;
			if(!this.CheckAmazonStatus(this.Uploadloader))
			{
				this.ErrorMessage = "Uploadloader error!";
				return false;
			}
			
			this.ps = new PostResponse(this.Uploadloader.text);
			if(!ps.IsOK)
			{
				this.ErrorMessage = "Uploadloader error!";
				return false;
			}
			
			if(this.ComplateLoader == null)
			{
				this.Complate();
				this.CurrentAtion = "Completion of the operation...";
				return false;
			}
			if(!this.ComplateLoader.isDone) return false;
			if(!this.CheckStatus(this.ComplateLoader)) 
			{
				this.ErrorMessage = "ComplateLoader error!";
				return false;
			}
			
			this.CurrentAtion = this.Name + ": " + "done";
			
			return true;
		}
	}
	
	
	/// <summary>
	/// Initializes a new instance of the <see cref="QuickBloxFileUploader"/> class.
	/// Creates an entity and uploads file to the server.
	/// </summary>
	/// <param name='filecontent'>
	/// File for upload
	/// </param>
	/// <param name='FileName'>
	/// Filename
	/// </param>
	/// <param name='FileMimeType'>
	/// File MIME type.
	/// </param>
	public QuickBloxFileUploader(byte[] filecontent, string FileName, string FileMimeType, string tags)
	{
		this.con = QuickBloxConection.Current;
		this.file = filecontent;
		this.Name = FileName;
		this.MimeType = FileMimeType;
		this.Tags = tags;
	}
	
	/// <summary>
	/// First stage - entity creation.
	/// </summary>
	private void CreateBlob()
	{
			WWWForm CreateBlobform = new WWWForm();
			System.Collections.Hashtable CreateBlobheaders = CreateBlobform.headers;
			CreateBlobheaders["QuickBlox-REST-API-Version"] = "0.1.0";
			CreateBlobheaders["QB-Token"] = con.session.Token;
	        CreateBlobform.AddField("blob[content_type]", this.MimeType);
	        CreateBlobform.AddField("blob[name]", this.Name);
			CreateBlobform.AddField("blob[multipart]", "0");
			CreateBlobform.AddField("blob[public]","true");		
			CreateBlobform.AddField("blob[tag_list]",this.Tags);
			CreateBlobLoader = new WWW(con.Settings.Endpoint + "blobs.xml",CreateBlobform.data,CreateBlobheaders);
	}
	
	/// <summary>
	/// Second stage - upload to the server.
	/// </summary>
	private void Upload(byte[] ass)
	{
		    this.blob = new Blob(this.CreateBlobLoader.text);
			WWWForm Uploadform = new WWWForm();
			string[] amazonParams = this.blob.BOA.Params.Trim().Split('&');
			Dictionary<string,string> field = new Dictionary<string, string>();
			string[] url = amazonParams[0].Split('?');
			string[] param1 = url[1].Split('=');
			field[param1[0]] = WWW.UnEscapeURL(param1[1], System.Text.Encoding.ASCII);
			
			foreach(var t in amazonParams)
			{
				if(t.Contains("http")) continue;
				string[] temp = t.Split('=');
				field[temp[0]] = WWW.UnEscapeURL(temp[1], System.Text.Encoding.ASCII);
			}
			
			Uploadform.AddField("AWSAccessKeyId",field["AWSAccessKeyId"]);
			Uploadform.AddField("Policy",field["Policy"]);
			Uploadform.AddField("Signature",field["Signature"]);
			Uploadform.AddField("key",field["key"]);
			Uploadform.AddField("Content-Type",field["Content-Type"]);
			Uploadform.AddField("acl",field["acl"]);
			Uploadform.AddField("success_action_status",field["success_action_status"]);
			Uploadform.AddBinaryData("file",ass,blob.Name,this.MimeType);
			
			Uploadloader = new WWW(url[0],Uploadform.data,Uploadform.headers);
	}
	
	/// <summary>
	///Third stage - file upload confirmation.
	/// </summary>
	private void Complate()
	{
		ps = new PostResponse(Uploadloader.text);
		WWWForm ComplateForm = new WWWForm();
		System.Collections.Hashtable ComplateHeaders = ComplateForm.headers;
		ComplateHeaders["QuickBlox-REST-API-Version"] = "0.1.0";
		ComplateHeaders["QB-Token"] = con.session.Token;
	    ComplateForm.AddField("blob[size]", file.Length.ToString());
		ComplateLoader = new WWW("http://api.quickblox.com/blobs/"+ blob.Id.ToString() +"/complete.xml",ComplateForm.data,ComplateHeaders);
	}
	
	
	private bool CheckAmazonStatus(WWW loader)
	{
		if(loader == null) return false;
		if(!loader.isDone) return false;
		if(!string.IsNullOrEmpty(loader.error))
		{
			this.ErrorMessage = loader.error;
			Debug.LogError(loader.error);
			return false;
		}
		
		return true;
		
	}
	
	
	private bool CheckStatus(WWW loader)
	{
		if(loader == null) return false;
		if(!loader.isDone) return false;
		if(!string.IsNullOrEmpty(loader.error))
		{
			this.ErrorMessage = loader.error;
			Debug.LogError(loader.error);
			return false;
		}
		
		string Status = loader.responseHeaders["STATUS"];
		if(string.IsNullOrEmpty(Status))
		{
			this.ErrorMessage = "Status not found";
			Debug.LogError("Status not found");
			return false;
		}
		Status = Status.Trim().ToLower();
		if(Status == "202 accepted"
			|| Status == "201 created" 
			|| Status == "201" 
			|| Status == "200" 
			|| Status == "202"
			|| Status == "200 ok")
		{
			return true;
		}
		else
		{
			this.ErrorMessage = loader.error;
			Debug.LogError(loader.error);
			return false;
		}
	}
	
	
}
