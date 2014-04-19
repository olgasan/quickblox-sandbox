using System;
using System.Xml;

  
/// <summary>
/// User's entity.
/// </summary>

    public class User
    {
        
 
        public int id
        { get; private set; }

    /// <summary>
    /// User’s login.
    /// </summary>
        public string Username
        { get;  set; }

       /// <summary>
       /// User’s password.
       /// </summary>
        public string Password
        { get; set; }

      /// <summary>
      ///User’s email.
      /// </summary>
        public string Email
        { get; set; }

      
	/// <summary>
	/// Registration date.
	/// </summary>
        public DateTime? CreatedDate
        {
            get;
            private set;
        }

    
	/// <summary>
	/// Last request date.
	/// </summary>
        public DateTime? LastRequestDate
        {
            get;
            private set;
        }

       
	/// <summary>
	/// Last change date.
	/// </summary>
        public DateTime? UpdatedDate
        {
            get;
            private set;
        }

        
	/// <summary>
	/// ID of User in external system.
	/// </summary>
        public int? ExternalUserId
        { get; set; }

        
	/// <summary>
	/// ID of User in Twitter.
	/// </summary>
        public string TwitterId
        { get; set; }

/// <summary>
/// ID of User in Facebook.
/// </summary>
        public string FacebookId
        { get; set; }

	/// <summary>
	/// User’s full name.
	/// </summary>
        public string FullName
        { get;  set; }


	/// <summary>
	/// User’s phone.
	/// </summary>
        public string Phone
        { get; set; }
	
	
	/// <summary>
	/// User’s website.
	/// </summary>
        public string Website
        { get; set; }


	
	
	/// <summary>
	/// Creates a user according the scheme.
	/// </summary>
        public User(string Xml)
        {
            this.Parse(Xml);

        }
 
        #region Methods 
         
   
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.Username) ? string.Empty : Username;
        }

        private void Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new Exception("Content error");
		
            try
            {
                XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml(xml);
						
                this.id = int.Parse(xDoc.GetElementsByTagName("id")[0].InnerText);
                this.Username = xDoc.GetElementsByTagName("login")[0].InnerText;
				this.Email = xDoc.GetElementsByTagName("email")[0].InnerText;
				this.FullName = xDoc.GetElementsByTagName("full-name")[0].InnerText;	
								
				try {
					this.ExternalUserId = int.Parse(xDoc.GetElementsByTagName("external-user-id")[0].InnerText);				
					this.FacebookId = xDoc.GetElementsByTagName("facebook-id")[0].InnerText;
					this.TwitterId = xDoc.GetElementsByTagName("twitter-id")[0].InnerText;
				} catch {
					
				}				this.CreatedDate = DateTime.Parse(xDoc.GetElementsByTagName("created-at")[0].InnerText);
                this.UpdatedDate = (string.IsNullOrEmpty(xDoc.GetElementsByTagName("updated-at")[0].InnerText) ? (DateTime?)null : DateTime.Parse(xDoc.GetElementsByTagName("updated-at")[0].InnerText));
            }
            catch
            {
                throw new Exception("Content error");
            }
        }
        #endregion
      
    }

