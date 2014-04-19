using System;
using System.Xml;

/// <summary>
/// Entity that allows to download the file from the server.
/// </summary>
    public class BlobObjectAccess
    {
	
        public BlobObjectAccess(string XML)
        {
            this.Parse(XML);
        }

        #region
	
	/// <summary>
	/// Unique file identifier.
	/// </summary>
        public uint BlobId
        { get; set; }
	
	/// <summary>
	/// Reference expiration time
	/// </summary>
        public DateTime Expires
        { get; set; }
	
	/// <summary>
	/// Blob ID
	/// </summary>
        public uint Id
        { get; set; }
	
	/// <summary>
	/// Link access type
	/// </summary>
        public string ObjectAccessType
        { get; set; }
	
	/// <summary>
	/// Params. Use them for upload file
	/// </summary>
        public string Params
        { get; set; }

        #endregion


        private void Parse(string xml)
        {
		
		  if(string.IsNullOrEmpty(xml)) return;
		
            try
            {
                XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml("<root>"+ xml + "</root>");
			
                this.Id = uint.Parse(xDoc.GetElementsByTagName("id")[0].InnerText);
                this.BlobId = uint.Parse(xDoc.GetElementsByTagName("blob-id")[0].InnerText);
                //----
                this.Expires = DateTime.Parse(xDoc.GetElementsByTagName("expires")[0].InnerText);
                this.Params = xDoc.GetElementsByTagName("params")[0].InnerText;
                this.ObjectAccessType = xDoc.GetElementsByTagName("object-access-type")[0].InnerText;
            }
            catch
            {
            }
        }


    }

