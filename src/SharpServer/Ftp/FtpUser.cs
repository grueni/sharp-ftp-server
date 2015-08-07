using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SharpServer
{
    [DataContract]
    public class FtpUser
    {
		 [DataMember]
		 public string UserName { get; set; }

		 [DataMember]
        public string Password { get; set; }

		 [DataMember]
		 public string HomeDir { get; set; }

		 [DataMember]
		 public string TwoFactorSecret { get; set; }

		 [DataMember]
		 public FtpUserPerSession PerSession { get; set; }

		 public bool IsAnonymous { get; set; }

		 public override String ToString() {
			 return String.Format("UserName={0} HomeDir={1} PerSession={2}", UserName, HomeDir, PerSession);
		 }
    }

	 [DataContract]
	 public class FtpUserPerSession
	 {
		 [DataMember]
		 public Boolean UniqueDirectory { get; set; }

		 [DataMember]
		 public String PostJob { get; set; }

		 [DataMember]
		 public int PostJobTimeoutInSeconds { get; set; }

		 [DataMember]
		 public String BJSJobDirectory { get; set; }

		 public override String ToString()
		 {
			 return String.Format("UniqueDirectory={0} PostJob={1} PostJobTimeoutInSeconds={2} BJSJobDirectory={3}", UniqueDirectory, PostJob, PostJobTimeoutInSeconds, BJSJobDirectory);
		 }
	 }

//	[Obsolete("This is not a real user store. It is just a stand-in for testing. DO NOT USE IN PRODUCTION CODE.")]
	public static class FtpUserStore
	{
		private static String userStore;
		private static String gHomeDir = @"p:\temp\ftphome\";

		private static List<FtpUser> _users {
			get
			{
				var users = new List<FtpUser>();
				if (!String.IsNullOrEmpty(userStore))
				{
					if (File.Exists(userStore))
					{
						users = Ftp.FtpServer.Deserialize<List<FtpUser>>(userStore);
					}
					else
					{
						users.Add(new FtpUser
						{
							UserName = "rick",
							Password = "test",
							HomeDir = gHomeDir,
							PerSession = new FtpUserPerSession { UniqueDirectory = false, BJSJobDirectory="changeme", PostJob = "changeme", PostJobTimeoutInSeconds = 1 },
							IsAnonymous = false
						});
						users.Add(new FtpUser
						{
							UserName = @"changeme",
							Password = "changeme",
							HomeDir = @"changeme",
							PerSession = new FtpUserPerSession { UniqueDirectory = false, BJSJobDirectory = "changeme", PostJob = "changeme", PostJobTimeoutInSeconds = 1 },
							IsAnonymous = false
						});
					}
					Ftp.FtpServer.Serialize<List<FtpUser>>(userStore, users);
				}
				return users;
			}
		}

		static FtpUserStore()
		{
		}

		public static FtpUser Validate(String userstore, String username, String password)
		{
			userStore = userstore;
			FtpUser user = (username.ToLower().Equals("anonymous")) ?
				new FtpUser { UserName = username, HomeDir = gHomeDir, IsAnonymous = true }
				:
				(from u in _users where u.UserName == username && u.Password == password select u).SingleOrDefault();
			;
			return user;
		}


		public static FtpUser ValidatetwoFactorCode(string username, string password, string twoFactorCode)
		{
			FtpUser user = (from u in _users where u.UserName == username && u.Password == password select u).SingleOrDefault();
			return (TwoFactor.TimeBasedOneTimePassword.IsValid(user.TwoFactorSecret, twoFactorCode)) ? user : null;
		}
	}
}
