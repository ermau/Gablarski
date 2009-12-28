using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Gablarski.Clients.Windows.Entities;
using System.Data.SQLite;

namespace Gablarski.Clients.Windows
{
	public static class Persistance
	{
		static Persistance()
		{
			if (Boolean.Parse (ConfigurationManager.AppSettings["useLocalDatabase"]))
				DbFile = new FileInfo ("gablarski.db");
			else
				DbFile = new FileInfo (Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData), "Gablarski\\gablarski.db"));

			var builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = DbFile.FullName;

			bool create = !DbFile.Exists;

			db = new SQLiteConnection(builder.ToString());
			db.Open();

			if (create)
				CreateDbs();
		}

		public static IEnumerable<SettingEntry> GetSettings()
		{
			using (var cmd = db.CreateCommand ("SELECT * FROM settings"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new SettingEntry (Convert.ToInt32 (reader["settingId"]))
					{
						Name = Convert.ToString (reader["settingName"]),
						Value = Convert.ToString (reader["settingValue"])
					};
				}
			}
		}

		public static void SaveOrUpdate (SettingEntry setting)
		{
			if (setting == null)
				throw new ArgumentNullException ("setting");

			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = setting.Id > 0
				                  	? "UPDATE settings SET settingName=?,settingValue=? WHERE (settingId=?)"
				                  	: "INSERT INTO settings (settingName,settingValue) VALUES (?,?)";

				cmd.Parameters.Add (new SQLiteParameter ("name", setting.Name));
				cmd.Parameters.Add (new SQLiteParameter ("value", setting.Value));

				if (setting.Id > 0)
					cmd.Parameters.Add (new SQLiteParameter ("id", setting.Id));

				cmd.ExecuteNonQuery();
			}
		}

		public static IEnumerable<ServerEntry> GetServers()
		{
			using (var cmd = db.CreateCommand ("SELECT * FROM servers"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new ServerEntry (Convert.ToInt32 (reader[0]))
					{
						Name = Convert.ToString (reader[1]),
						Host = Convert.ToString (reader[2]),
						Port = Convert.ToInt32 (reader[3]),
						ServerPassword = Convert.ToString (reader[4]),
						UserNickname = Convert.ToString (reader[5]),
						UserPhonetic = Convert.ToString (reader[6]),
						UserName = Convert.ToString (reader[7]),
						UserPassword = Convert.ToString (reader[8])
					};
				}
			}
		}

		public static void SaveOrUpdate (ServerEntry server)
		{
			if (server == null)
				throw new ArgumentNullException ("server");
			
			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = server.Id > 0 ? "UPDATE servers SET serverName=?,serverHost=?,serverPort=?,serverPassword=?,serverUserNickname=?,serverUserPhonetic=?,serverUserName=?,serverUserPassword=?" 
					: "INSERT INTO servers (serverName,serverHost,serverPort,serverPassword,serverUserNickname,serverUserPhonetic,serverUserName,serverUserPassword) VALUES (?,?,?,?,?,?,?,?)";

				cmd.Parameters.Add (new SQLiteParameter ("name", server.Name));
				cmd.Parameters.Add (new SQLiteParameter ("host", server.Host));
				cmd.Parameters.Add (new SQLiteParameter ("port", server.Port));
				cmd.Parameters.Add (new SQLiteParameter ("password", server.ServerPassword));
				cmd.Parameters.Add (new SQLiteParameter ("nickname", server.UserNickname));
				cmd.Parameters.Add (new SQLiteParameter ("phonetic", server.UserPhonetic));
				cmd.Parameters.Add (new SQLiteParameter ("username", server.UserName));
				cmd.Parameters.Add (new SQLiteParameter ("userpassword", server.UserPassword));

				if (server.Id > 0)
					cmd.Parameters.Add (new SQLiteParameter ("id", server.Id));

				cmd.ExecuteNonQuery();
			}
		}

		public static void Delete (ServerEntry server)
		{
			if (server == null)
				throw new ArgumentNullException ("server");
			if (server.Id < 1)
				throw new ArgumentException ("Can't delete a non-existant server", "server");

			using (var cmd = db.CreateCommand ("DELETE FROM servers WHERE (serverId=?)"))
			{
				cmd.Parameters.Add (new SQLiteParameter ("id", server.Id));
				cmd.ExecuteNonQuery();
			}
		}

		private static readonly DbConnection db;
		private static readonly FileInfo DbFile;

		private static void CreateDbs()
		{
			using (var cmd = db.CreateCommand ("CREATE TABLE servers (serverId integer primary key autoincrement, serverName varchar(255), serverHost varchar(255), serverPort integer, serverPassword varchar(255), serverUserNickname varchar(255), serverUserPhonetic varchar(255), serverUserName varchar(255), serverUserPassword varchar(255))"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE settings (settingId integer primary key autoincrement, settingName varchar(255), settingValue varchar(255))"))
				cmd.ExecuteNonQuery();
		}
	}
}