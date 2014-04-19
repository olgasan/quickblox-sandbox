using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Instarest;

/// <summary>
/// Quick blox BLOB manager.
/// Class Singletone, that gets file list, page per page from the server for the current user.
/// The class QuickBloxConection should not be null to work correctly.
/// The connection with the server should be established as well.
/// </summary>
public class QuickBloxBlobManager
{
	
	private Request listLoader;
	private QuickBloxConection con;
	private List<Blob>_Blobs;
	
	
#region Singleton
	private static QuickBloxBlobManager _current;
	public static QuickBloxBlobManager Current
	{
		get
		{
			if(_current == null)
			{
				_current = new QuickBloxBlobManager();
				return _current;
			}
			else
				return _current;
		}
	}
	
	
	private QuickBloxBlobManager()
	{
		this.con = QuickBloxConection.Current;
		this.Update();
	}
#endregion
	
	
	/// <summary>
	/// Determines whether the user list is downloaded or not.
	/// Can be true if there is a positive response from the server and the result is parsed successfully.
	/// Can be false if there wasn't any request to server, the request on the server is being executed, 
	/// there is a negative response from the server, 
	/// and if there is a positive response from the server but an exeption in response processing occured
	/// </summary>
	public bool IsListLoaded
	{
		get
		{
			if(this.listLoader == null) return false;
			
			if(!this.IsUpdate)
				return listLoader.isDone;
			else
			{
				if(this.listLoader.isDone)
				{
					this.IsUpdate = false;
					try
					{
						XmlDocument xDoc = new XmlDocument();
						xDoc.LoadXml(this.listLoader.response.Text);
						XmlElement root = xDoc.DocumentElement;
						
						this.current_page = int.Parse(root.Attributes["current_page"].InnerText);
						this.per_page = int.Parse(root.Attributes["per_page"].InnerText);
						this.total_entries = int.Parse(root.Attributes["total_entries"].InnerText);
						
						List<Blob> temp = new List<Blob>();
						foreach(XmlNode t in xDoc.GetElementsByTagName("blob"))
						{
							Blob blob = new Blob("<blob>"+ t.InnerXml + "</blob>");
							temp.Add(blob);
						}
						
						this._Blobs = temp ;
						//Debug.Log("OK");
					}
					catch(Exception ex)
					{
						this.LoadListErrorMessage = ex.Message;
						return true;
					}
					
					return true;
				}
				else
				{
					return listLoader.isDone;
				}
			}
			
		}
	}
	
	/// <summary>
	/// An available to the user file list that the server returned as a response for a request.
	/// Can be not null or not empty if IsListLoad == true.
	/// Depends on following parameters: current_page, per_page, total_entries.
	/// Can be empty if the user hasn't downloaded any files on server.
	/// </summary>
	public List<Blob> Blobs
	{
		get
		{
			return _Blobs;
		}
	}
	
	
	/// <summary>
	/// Required request's parameter.
	/// Current page.
	/// Parameter should be greater than null
	/// </summary>
	public int current_page = 1;
	
	/// <summary>
	/// Required parameter.
	/// Quantity of files on the page.
	/// Parameter should be greater than null or will be equal 10 by default.
	/// </summary>
	public int per_page = 100;
	
	/// <summary>
	/// Total count of files that are available to the user (the files that were uploaded by the user on the server)
	/// This parameter is known after first request to the server.
	/// </summary>
	public int total_entries = 1;
	
	/// <summary>
	/// Error message while file list is downloaded from the server.
	/// Can be not empty if the server returned an error or an exception occurred on the stage of server data processing.
	/// </summary>
	public string LoadListErrorMessage;
	
	
	private bool IsUpdate;
	
	
	/// <summary>
	/// File list update - Blobs.
	/// Sends a request to the server for getting the list of files, available to the user.
	/// Following parameters are used in request: current_page, per_page, total_entries.
	/// </summary>
	public void Update()
	{
		if(!this.con.IsDone) return;
		this.IsUpdate = true;		
		
		listLoader = new Request("http://api.quickblox.com/blobs.xml");
		listLoader.AddHeader("QuickBlox-REST-API-Version","0.1.0");
		listLoader.AddHeader("QB-Token",con.session.Token);
		listLoader.AddFormField("page",this.current_page.ToString());
		listLoader.AddFormField("per_page",this.per_page.ToString());
		listLoader.Send(AcceptVerbs.GET);
		
	}
	
	
	
	
}
