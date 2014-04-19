using System;
using System.Linq;
using System.Xml;
using UnityEngine;

/// <summary>
/// Session entity
/// </summary>
  public class Session
    {
     
	/// <summary>
	/// Applicarion identifier
	/// </summary>
        public int ApplicationId
        { get; private set; }
	
	/// <summary>
	/// Connection date
	/// </summary>
        public DateTime CreatedDate
        { get; private set; }

       /// <summary>
       ///Device identifier
       /// </summary>
        public int? DeviceId
        { get; private set; }

        public int Id
        { get; private set; }

	/// <summary>
	/// Unique Random Value. Requests with the same timestamp and same value for nonce parameter can not be send twice.
	/// </summary>
        public int Nonce
        { get; private set; }
	
	/// <summary>
	///Unique auto generated sequence of numbers which identify API User as the legitimate user of our system. 
	///It is used in relatively short periods of time and can be easily changed. 
	///We grant API Users some rights after authentication and check them based on this token.
	/// </summary>
        public string Token
        { get; private set; }
	
	/// <summary>
	/// Unix Timestamp. It shouldn’t be differ from time provided by NTP more than 10 minutes. 
	/// We suggest you synchronize time on your devices with NTP service.
	/// </summary>
        public int TS
        { get; private set; }
	
	/// <summary>
	/// Last executed operation date.
	/// </summary>
        public DateTime UpdatedTime
        { get; private set; }

	/// <summary>
	/// User identifier
	/// </summary>
        public int? UserId
        { get; private set; }
	
	/// <summary>
	/// Session creation according the scheme.
	/// </summary>
	/// <param name='Scheme'>
	/// Scheme xml.
	/// </param>
        public Session(string Scheme)
        {
            if (string.IsNullOrEmpty(Scheme))
                throw new Exception("Scheme not valid");
		
            try
            {
				XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml(Scheme);
                this.Id = int.Parse(xDoc.GetElementsByTagName("id")[0].InnerText);
                this.ApplicationId = int.Parse(xDoc.GetElementsByTagName("application-id")[0].InnerText);
                this.Nonce = int.Parse(xDoc.GetElementsByTagName("nonce")[0].InnerText);
                this.TS = int.Parse( xDoc.GetElementsByTagName("ts")[0].InnerText);
                //----
                this.CreatedDate = DateTime.Parse(xDoc.GetElementsByTagName("created-at")[0].InnerText);
                this.UpdatedTime = DateTime.Parse(xDoc.GetElementsByTagName("updated-at")[0].InnerText);
                this.Token = xDoc.GetElementsByTagName("token")[0].InnerText;
            }
            catch
            {
                throw new Exception("Scheme not valid");
            }
        }


    }

