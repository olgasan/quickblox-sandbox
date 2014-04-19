using System;
using System.Xml;

/// <summary>
/// File entity on Amazon.
/// </summary>
    public class PostResponse
    {

        public PostResponse(string XML)
        {
            this.Parse(XML);
        }

/// <summary>
/// Gets or sets the location.
/// </summary>
/// <value>
/// The location.
/// </value>
        public string Location
        { get; set; }

	/// <summary>
	/// Gets or sets the bucket.
	/// </summary>
	/// <value>
	/// The bucket.
	/// </value>
        public string Bucket
        { get; set; }

	/// <summary>
	/// Gets or sets the key.
	/// </summary>
	/// <value>
	/// The key.
	/// </value>
        public string Key
        { get; set; }

	/// <summary>
	/// Gets or sets the E tag.
	/// </summary>
	/// <value>
	/// The E tag.
	/// </value>
        public string ETag
        { get; set; }

	
	/// <summary>
	///How the data were processed.
	/// </summary>
	public bool IsOK
	{
		get; set;
	}

        private void Parse(string xml)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml(xml);
			
                this.Location = xDoc.GetElementsByTagName("Location")[0].InnerText;
                this.Bucket = xDoc.GetElementsByTagName("Bucket")[0].InnerText;
                this.Key = xDoc.GetElementsByTagName("Key")[0].InnerText;
                this.ETag = xDoc.GetElementsByTagName("ETag")[0].InnerText;
			
				this.IsOK = true;
            }
            catch
            {
			this.IsOK = false;
            }
        }

    }

