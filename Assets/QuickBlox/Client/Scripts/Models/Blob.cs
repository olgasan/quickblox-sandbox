using UnityEngine;
using System;
using System.IO;
using System.Xml;
using Instarest;

/// <summary>
/// Entity of the file which is uploaded to the server.
/// </summary>
public class Blob
{
	
	public Blob(string XML)
	{
		this.Parse(XML);
	}

#region Fields
	
	/// <summary>
	/// Status of the File
	/// </summary>
	public BlobStatus BStatus
	{ get; set; }
	
	/// <summary>
	/// Content type in mime format
	/// </summary>
	public string ContentType
	{ get; set; }
	
	/// <summary>
	/// Entity creation data.
	/// </summary>
	public DateTime CreatedAt
	{ get; set; }
	
	/// <summary>
	/// Last read file time
	/// </summary>
	public DateTime? LastReadAccessTs
	{ get; set; }
	
	/// <summary>
	/// Time that file will live after delete, in seconds
	/// </summary>
	public DateTime? SetCompletedAt
	{ get; set; }
	
	/// <summary>
	/// Last entity update data
	/// </summary>
	public DateTime UpdatedAt
	{ get; set; }
	
	/// <summary>
	/// Entity unique identifier on the server.
	/// </summary>
	public uint Id
	{ get; set; }
	
	/// <summary>
	/// Time that file will live after delete, in seconds
	/// </summary>
	public int Lifetime
	{ get; set; }
	
	/// <summary>
	///Filename
	/// </summary>
	public string Name
	{ get; set; }
	
	/// <summary>
	/// File’s visibility
	/// </summary>
	public bool IsPublic
	{ get; set; }
	
	/// <summary>
	/// File’s links count
	/// </summary>
	public uint RefCount
	{ get; set; }
	
	/// <summary>
	/// The size of file in bytes, readonly
	/// </summary>
	public uint Size
	{ get; private set; }
	
	/// <summary>
	/// Coma separated string with file’s tags
	/// </summary>
	public string Tags
	{ get; set; }
	
	/// <summary>
	/// File unique identifier
	/// </summary>
	public string UID
	{ get; set; }
	
	/// <summary>
	/// An instance of BlobObjectAccess
	/// </summary>
	public BlobObjectAccess BOA
	{ get; set; }

#endregion

	private Request DownloadLoader;
	private Request DeleteLoader;
	
	/// <summary>
	/// Check whether the file was deleted or no.
	/// The field is actual if file delete process is run.
	/// </summary>
	public bool IsDeleted
	{
		get
		{
			if(this.DeleteLoader == null) return false;
			if(this.IsDeleting)
			{
				if(this.DeleteLoader.isDone)
				{
					if(DeleteLoader.response.status == 200)
					{
						this.IsDeleting = false;
						return true;
						
					}
					else
					{
						Debug.LogError("Blob delete:" + DeleteLoader.response.status.ToString());
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			
			return false;
		}
	}
	
	/// <summary>
	/// Whether file delete process is run.
	/// </summary>
	public bool IsDeleting;
	
	/// <summary>
	/// The field is actual if file download process is run.
	/// Value in percent.
	/// </summary>
	/// <value>
	/// 0-100
	/// </value>
	public int Progress
	{
		get
		{
			if(this.DownloadLoader == null) return 0;
			if(this.DownloadLoader.response == null) return 0;
			return this.DownloadLoader.response.Progress;
		}
	}
	
	/// <summary>
	/// Runs file delete process on the server.
	/// </summary>
	public void Delete()
	{
		if(this.IsDeleted || this.IsDeleting) return;
		this.IsDeleting = true;
		
		DeleteLoader = new Request("http://api.quickblox.com/blobs/" + this.Id.ToString() + ".xml");
		DeleteLoader.AddHeader("QuickBlox-REST-API-Version","0.1.0");
		DeleteLoader.AddHeader("QB-Token", QuickBloxConection.Current.session.Token);
		DeleteLoader.Send(AcceptVerbs.DELETE);
	
	}
	
	private bool IsOk = false;
	
	/// <summary>
	/// File download from server check.
	/// Actual if download process is run. 
	/// One should pay attention to the Error field.
	/// 
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is download; otherwise, <c>false</c>.
	/// </value>
	public bool IsDownloaded
	{
		get
		{
			if(this.IsOk) return this.IsOk;
			if(this.DownloadLoader == null) return false;
			if(this.IsDownloading)
			{
				if(this.DownloadLoader.isDone)
				{
					if(DownloadLoader.response.status == 200)
					{
						this.IsDownloading = false;
						this.file = DownloadLoader.response.bytes;
						this.IsOk = true;
						return true;
					}
					else
					{
						Debug.LogError("Blob download:" + DownloadLoader.response.status.ToString());
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			
			return false;
		}
	}
	
	
	/// <summary>
	/// File download in memory process is run.
	/// </summary>
	public bool IsDownloading;
	
	/// <summary>
	/// File path, if the file was saved to the local system.
	/// </summary>
	public string FilePath;
	
	/// <summary>
	/// File uploaded from the server.
	/// </summary>
	public byte[] file;
	
	
	/// <summary>
	/// Run file download in memory process.
	/// </summary>
	public void Download()
	{
		if(this.IsDownloaded || this.IsDownloading) return;
		this.IsDownloading = true;
		
		this.DownloadLoader = new Request("http://api.quickblox.com/blobs/"+ this.UID +".xml");
		this.DownloadLoader.AddHeader("QuickBlox-REST-API-Version","0.1.0");
		this.DownloadLoader.AddHeader("QB-Token", QuickBloxConection.Current.session.Token);
		this.DownloadLoader.Send(AcceptVerbs.GET);
	}
	
	/// <summary>
	/// File was saved.
	/// </summary>
	public bool IsFileSave;
	
	/// <summary>
	/// Save file to the file system.
	/// </summary>
	/// <param name='path'>
	/// Save path.
	/// </param>
	public void Save(string path)
	{
		if(this.file == null) return;
		this.FilePath = path;
		System.IO.File.WriteAllBytes(path,this.file);
		this.IsFileSave = true;
		
	}
	
	private void Parse(string xml)
	{
		try
		{
			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml(xml);			
			this.Id = uint.Parse(xDoc.GetElementsByTagName("id")[0].InnerText);
			this.CreatedAt = DateTime.Parse(xDoc.GetElementsByTagName("created-at")[0].InnerText);
			this.UpdatedAt = DateTime.Parse(xDoc.GetElementsByTagName("updated-at")[0].InnerText);
			this.LastReadAccessTs = (string.IsNullOrEmpty(xDoc.GetElementsByTagName("last-read-access-ts")[0].InnerText) ? (DateTime?)null : DateTime.Parse(xDoc.GetElementsByTagName("last-read-access-ts")[0].InnerText));
			this.SetCompletedAt = (string.IsNullOrEmpty(xDoc.GetElementsByTagName("set-completed-at")[0].InnerText) ? (DateTime?)null : DateTime.Parse(xDoc.GetElementsByTagName("set-completed-at")[0].InnerText));
			this.Lifetime = string.IsNullOrEmpty(xDoc.GetElementsByTagName("lifetime")[0].InnerText) ? 0 : int.Parse(xDoc.GetElementsByTagName("lifetime")[0].InnerText);
			this.RefCount = string.IsNullOrEmpty(xDoc.GetElementsByTagName("ref-count")[0].InnerText) ? 0 : uint.Parse(xDoc.GetElementsByTagName("ref-count")[0].InnerText);
			this.Size = string.IsNullOrEmpty(xDoc.GetElementsByTagName("size")[0].InnerText) ? 0 : uint.Parse(xDoc.GetElementsByTagName("size")[0].InnerText);
			this.IsPublic = xDoc.GetElementsByTagName("public")[0].InnerText == "true" ? true : false;
			this.ContentType = xDoc.GetElementsByTagName("content-type")[0].InnerText;
			this.Name = xDoc.GetElementsByTagName("name")[0].InnerText;
			//this.Tags = xDoc.GetElementsByTagName("tag_list")[0].InnerText;
			
			try
			{
				XmlNodeList nodeList = xDoc.GetElementsByTagName("blob-object-access");
				if(nodeList != null && nodeList.Count > 0)
				{
					XmlNode currentNode = nodeList[0];
					if(!string.IsNullOrEmpty(currentNode.InnerXml))
					{
						this.BOA = new BlobObjectAccess(currentNode.InnerXml);
					}
				}
			}
			catch(Exception ex)
			{
				Debug.Log(ex.Message);
			}
			
			try
			{
				switch (xDoc.GetElementsByTagName("blob-status")[0].InnerText)
				{
				case "Complete":
				{
					this.BStatus = BlobStatus.Complete;
					break;
				}
				case "Locked":
				{
					this.BStatus = BlobStatus.Locked;
					break;
				}
				case "New":
				{
					this.BStatus = BlobStatus.New;
					break;
				}
				}
			}
			catch(Exception ex)
			{
				Debug.Log(ex.Message);
			}
			
			this.UID = xDoc.GetElementsByTagName("uid")[0].InnerText;
		}
		
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
	}
}
