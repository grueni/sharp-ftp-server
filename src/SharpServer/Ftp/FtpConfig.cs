using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace SharpServer.Ftp
{
	[DataContract]
	public class FtpConfig
	{

		public FtpConfig(String ftproot = null, String userStore = null, String iPAddressV4 = null, 
			int iPPortV4 = 21, String iPAddressV6 = null, int iPPortV6 = 21,
			String certificatePath=null, String certificatePassword=null)
		{
			Ftproot = ftproot;
			UserStore = userStore;
			IPAddressV4 = iPAddressV4;
			IPPortV4 = iPPortV4;
			IPAddressV6 = iPAddressV6;
			IPPortV6 = iPPortV6;
			CertificatePath = certificatePath;
			CertificatePassword = certificatePassword;
		}

		[DataMember]
		public String Ftproot { get; set; }

		[DataMember]
		public String UserStore { get; set; }

		[DataMember]
		public String IPAddressV4 { get; set; }

		[DataMember]
		public int IPPortV4 { get; set; }

		[DataMember]
		public String IPAddressV6 { get; set; }

		[DataMember]
		public int IPPortV6 { get; set; }

		/// <summary>
		/// SSL certificate
		/// </summary>
		[DataMember]
		public String CertificatePath { get; set; }

		/// <summary>
		/// SSL certificate passphrase
		/// </summary>
		[DataMember]
		public String CertificatePassword { get; set; }


		public IPEndPoint[] LocalEndPoints
		{
			get
			{
				if (_localEndPoints == null)
				{
					var v4 = (!String.IsNullOrEmpty(IPAddressV4) && !IPAddressV4.Equals("0.0.0.0")) ? IPAddress.Parse(IPAddressV4) : IPAddress.Any;
					var v6 = (!String.IsNullOrEmpty(IPAddressV6) && !IPAddressV6.Equals("0.0.0.0")) ? IPAddress.Parse(IPAddressV6) : IPAddress.IPv6Any;

					if (!String.IsNullOrEmpty(IPAddressV4) && !String.IsNullOrEmpty(IPAddressV6))
					{
						_localEndPoints = new IPEndPoint[] { new IPEndPoint(v4, IPPortV4), new IPEndPoint(v6, IPPortV6) };
					}
					else if (!String.IsNullOrEmpty(IPAddressV4))
					{
						_localEndPoints = new IPEndPoint[] { new IPEndPoint(v4, IPPortV4) };
					}
					else if (!String.IsNullOrEmpty(IPAddressV6))
					{
						_localEndPoints = new IPEndPoint[] { new IPEndPoint(v6, IPPortV6)	};
					}
				}
				return _localEndPoints;
			}
		}

		public override String ToString()
		{
			return String.Format("Ftproot={0} UserStore={1} IPAddressV4={2} IPPortV4={3} IPAddressV6={4} IPPortV6={5} CertificatePath={6}", Ftproot, UserStore, IPAddressV4, IPPortV4, IPAddressV6, IPPortV6, CertificatePath);
		}


		private IPEndPoint[] _localEndPoints;
	}
}
