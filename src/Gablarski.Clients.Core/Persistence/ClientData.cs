// Copyright (c) 2011-2013, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Cadenza;
using Gablarski.Clients.Input;
using Tempest;

#if XAMARIN
using SQLiteConnectionStringBuilder=Mono.Data.Sqlite.SqliteConnectionStringBuilder;
using SQLiteConnection=Mono.Data.Sqlite.SqliteConnection;
using SQLiteParameter=Mono.Data.Sqlite.SqliteParameter;
#else
using System.Data.SQLite;
#endif

namespace Gablarski.Clients.Persistence
{
	public static class ClientData
	{
		public static void Setup (bool useLocalFiles)
		{
			DataDirectory = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Gablarski"));
			if (!DataDirectory.Exists)
				DataDirectory.Create();

			if (useLocalFiles) {
				KeyFile = new FileInfo ("gablarski.key");
				DbFile = new FileInfo ("gablarski.db");
			} else {
				KeyFile = new FileInfo (Path.Combine (DataDirectory.FullName, "gablarski.key"));
				DbFile = new FileInfo (Path.Combine (DataDirectory.FullName, "gablarski.db"));
			}

			#if !XAMARIN
			var builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = DbFile.FullName;

			db = new SQLiteConnection (builder.ToString());
			#else
			KeyFile = new FileInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), KeyFile.Name));
			DbFile = new FileInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), DbFile.Name));
			if (!DbFile.Exists)
				SQLiteConnection.CreateFile (DbFile.FullName);

			db = new SQLiteConnection ("Data Source=" + DbFile.FullName);
			#endif
			db.Open();
			
			CreateDbs();
		}

		sealed class RSAParametersSerializer
			: ISerializer<RSAParameters>
		{
			public static readonly RSAParametersSerializer Instance = new RSAParametersSerializer();

			public static void Serialize (IValueWriter writer, RSAParameters element)
			{
				Instance.Serialize (null, writer, element);
			}

			public static RSAParameters Deserialize (IValueReader reader)
			{
				return Instance.Deserialize (null, reader);
			}

			public void Serialize (ISerializationContext context, IValueWriter writer, RSAParameters element)
			{
				writer.WriteBytes (element.D);
				writer.WriteBytes (element.DP);
				writer.WriteBytes (element.DQ);
				writer.WriteBytes (element.Exponent);
				writer.WriteBytes (element.InverseQ);
				writer.WriteBytes (element.Modulus);
				writer.WriteBytes (element.P);
				writer.WriteBytes (element.Q);
			}

			public RSAParameters Deserialize (ISerializationContext context, IValueReader reader)
			{
				return new RSAParameters {
					D = reader.ReadBytes(),
					DP = reader.ReadBytes(),
					DQ = reader.ReadBytes(),
					Exponent = reader.ReadBytes(),
					InverseQ = reader.ReadBytes(),
					Modulus = reader.ReadBytes(),
					P = reader.ReadBytes(),
					Q = reader.ReadBytes()
				};
			}
		}

		public static Task<RSAAsymmetricKey> GetCryptoKeyAsync()
		{
			return GetCryptoKeyAsync (CancellationToken.None);
		}

		public static Task<RSAAsymmetricKey> GetCryptoKeyAsync (CancellationToken cancelToken = default(CancellationToken))
		{
			return Task.Run (() => {
				RSAAsymmetricKey key = null;
				if (KeyFile.Exists) {
					try {
						using (var stream = File.OpenRead (KeyFile.FullName)) {
							var reader = new StreamValueReader (stream);
							RSAParameters parameters = RSAParametersSerializer.Deserialize (reader);
							key = new RSAAsymmetricKey (parameters);
						}
					} catch (Exception ex) {
						Trace.TraceWarning ("Failed to read key: {0}", ex);
						KeyFile.Delete();
					}
				}

				cancelToken.ThrowIfCancellationRequested();

				if (!KeyFile.Exists) {
					var rsa = new RSACrypto();
					RSAParameters parameters = rsa.ExportKey (true);
					key = new RSAAsymmetricKey (parameters);

					cancelToken.ThrowIfCancellationRequested();
				
					using (var stream = File.OpenWrite (KeyFile.FullName)) {
						var writer = new StreamValueWriter (stream);
						RSAParametersSerializer.Serialize (writer, parameters);
					}
				}

				return key;
			}, cancelToken);
		}

		public static bool CheckForUpdates()
		{
			if (!Settings.Version.IsNullOrWhitespace())
			{
				string newVersion = Settings.Version = typeof (ClientData).Assembly.GetName().Version.ToString();
				Version version = new Version (Settings.Version);

				if (version < new Version (0, 13, 1))
				{
					Settings.TextToSpeech = "Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier";
					Settings.Version = newVersion;
					Settings.Save();

					return true;
				}

				if (version < new Version (0, 13, 4))
				{
					Settings.PlaybackDevice = "Default";
					Settings.VoiceDevice = "Default";
					Settings.Version = newVersion;
					Settings.Save();

					return true;
				}

				return false;
			}

			db.Close();
			DbFile.Delete();
			Settings.Clear();

			DbFile.Refresh();
			
			db.Open();
			CreateDbs();
			
			return true;
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

		#region Ignores
		public static void SaveOrUpdate (IgnoreEntry ignore)
		{
			if (ignore == null)
				throw new ArgumentNullException ("ignore");

			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = ignore.Id > 0
									? "UPDATE ignores SET ignoreServerId=?,ignoreUsername=? WHERE (ignoreId=?)"
									: "INSERT INTO ignores (ignoreServerId,ignoreUsername) VALUES (?,?)";

				cmd.Parameters.Add (new SQLiteParameter ("server", ignore.ServerId));
				cmd.Parameters.Add (new SQLiteParameter ("username", ignore.Username));

				if (ignore.Id > 0)
					cmd.Parameters.Add (new SQLiteParameter ("id", ignore.Id));

				cmd.ExecuteNonQuery();
			}
		}

		public static IEnumerable<IgnoreEntry> GetIgnores()
		{
			using (var cmd = db.CreateCommand ("SELECT * FROM ignores"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new IgnoreEntry (Convert.ToInt32 (reader["ignoreId"]))
					{
						ServerId = Convert.ToInt32 (reader["ignoreServerId"]),
						Username = Convert.ToString (reader["ignoreUsername"])
					};
				}
			}
		}

		public static void Delete (IgnoreEntry ignore)
		{
			if (ignore == null)
				throw new ArgumentNullException ("ignore");
			if (ignore.Id < 1)
				throw new ArgumentException ("Can't delete a non-existant ignore", "ignore");

			using (var cmd = db.CreateCommand ("DELETE FROM ignores WHERE (ignoreId=?)"))
			{
				cmd.Parameters.Add (new SQLiteParameter ("id", ignore.Id));
				cmd.ExecuteNonQuery();
			}
		}
		#endregion

		#region Servers
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

		public static Task<IEnumerable<ServerEntry>> GetServersAsync()
		{
			return Task.Run<IEnumerable<ServerEntry>> (() => GetServers().ToArray());
		}

		public static void SaveOrUpdate (ServerEntry server)
		{
			if (server == null)
				throw new ArgumentNullException ("server");
			
			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = server.Id > 0 ? "UPDATE servers SET serverName=?,serverHost=?,serverPort=?,serverPassword=?,serverUserNickname=?,serverUserPhonetic=?,serverUserName=?,serverUserPassword=? WHERE (serverId=?)" 
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
		#endregion

		#region Volume
		public static IEnumerable<VolumeEntry> GetVolumes (ServerEntry server)
		{
			if (server == null)
				throw new ArgumentNullException ("server");

			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = "SELECT * FROM volumes WHERE (volumeServerId=?)";
				cmd.Parameters.Add (new SQLiteParameter ("serverId", server.Id));
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return new VolumeEntry (Convert.ToInt32 (reader[0]))
						{
							ServerId = Convert.ToInt32 (reader[1]),
							Username = Convert.ToString (reader[2]),
							Gain = Convert.ToSingle (reader[3])
						};
					}
				}
			}
		}

		public static void SaveOrUpdate (VolumeEntry volumeEntry)
		{
			if (volumeEntry == null)
				throw new ArgumentNullException ("volumeEntry");

			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = (volumeEntry.VolumeId > 0)
									? "UPDATE volumes SET volumeServerId=?, volumeUsername=?, volumeGain=? WHERE volumeId=?"
									: "INSERT INTO volumes (volumeServerId,volumeUsername,volumeGain) VALUES (?,?,?)";

				cmd.Parameters.Add (new SQLiteParameter ("serverId", volumeEntry.ServerId));
				cmd.Parameters.Add (new SQLiteParameter ("username", volumeEntry.Username));
				cmd.Parameters.Add (new SQLiteParameter ("gain", volumeEntry.Gain));

				if (volumeEntry.VolumeId > 0)
					cmd.Parameters.Add (new SQLiteParameter ("id", volumeEntry.VolumeId));

				cmd.ExecuteNonQuery();
			}
		}

		public static void Delete (VolumeEntry volumeEntry)
		{
			if (volumeEntry == null)
				throw new ArgumentNullException ("volumeEntry");
			if (volumeEntry.VolumeId < 1)
				throw new ArgumentException ("Can't delete a non-existent server", "volumeEntry");

			using (var cmd = db.CreateCommand ("DELETE FROM volumes WHERE (volumeId=?)"))
			{
				cmd.Parameters.Add (new SQLiteParameter ("id", volumeEntry.VolumeId));
				cmd.ExecuteNonQuery();
			}
		}
		#endregion

		#region Bindings
		public static IEnumerable<CommandBindingEntry> GetCommandBindings()
		{
			using (var cmd = db.CreateCommand ("SELECT * FROM commandbindings"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new CommandBindingEntry
					{
						ProviderType = Convert.ToString (reader[0]),
						Command = (Command)Convert.ToInt32 (reader[1]),
						Input = Convert.ToString (reader[2])
					};
				}
			}
		}

		public static void Create (CommandBindingEntry bindingEntry)
		{
			if (bindingEntry == null)
				throw new ArgumentNullException ("bindingEntry");

			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = "INSERT INTO commandbindings (commandbindingProvider,commandbindingCommand,commandbindingInput) VALUES (?,?,?)";

				cmd.Parameters.Add (new SQLiteParameter ("provider", bindingEntry.ProviderType));
				cmd.Parameters.Add (new SQLiteParameter ("command", (int)bindingEntry.Command));
				cmd.Parameters.Add (new SQLiteParameter ("input", bindingEntry.Input));
				cmd.ExecuteNonQuery();
			}
		}

		public static void DeleteAllBindings ()
		{
			using (var cmd = db.CreateCommand ("DELETE FROM commandbindings"))
			{
				cmd.ExecuteNonQuery();
			}
		}
		#endregion

		public static async Task<IEnumerable<string>> GetBuddiesAsync()
		{
			using (var cmd = db.CreateCommand ("SELECT * FROM buddies"))
			using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait (false)) {
				List<string> buddies = new List<string>();
				while (await reader.ReadAsync().ConfigureAwait (false)) {
					buddies. Add(reader.GetString (0));
				}

				return buddies;
			}
		}

		public static async Task SaveBuddiesAsync (IEnumerable<string> buddies)
		{
			if (buddies == null)
				throw new ArgumentNullException ("buddies");

			HashSet<string> buddySet = new HashSet<string> (buddies);

			var currentBuddies = await GetBuddiesAsync().ConfigureAwait (false);
			buddySet.ExceptWith (currentBuddies);

			foreach (string identity in buddySet) {
				using (var cmd = db.CreateCommand ("INSERT INTO buddies (identity) VALUES (?)")) {
					cmd.Parameters.Add (new SQLiteParameter ("identity", identity));
					await cmd.ExecuteNonQueryAsync().ConfigureAwait (false);
				}
			}
		}

		private static DirectoryInfo DataDirectory;
		private static DbConnection db;
		private static FileInfo DbFile;
		private static FileInfo KeyFile;

		private static void CreateDbs()
		{
			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS servers (serverId integer primary key autoincrement, serverName varchar(255), serverHost varchar(255), serverPort integer, serverPassword varchar(255), serverUserNickname varchar(255), serverUserPhonetic varchar(255), serverUserName varchar(255), serverUserPassword varchar(255))"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS settings (settingId integer primary key autoincrement, settingName varchar(255), settingValue varchar(255))"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS volumes (volumeId integer primary key autoincrement, volumeServerId integer, volumeUsername varchar(255), volumeGain float)"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS ignores (ignoreId integer primary key autoincrement, ignoreServerId integer, ignoreUsername varchar(255))"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS commandbindings (commandbindingProvider string, commandbindingCommand integer, commandbindingInput varchar(255))"))
				cmd.ExecuteNonQuery();

			using (var cmd = db.CreateCommand ("CREATE TABLE IF NOT EXISTS buddies (identity string)"))
				cmd.ExecuteNonQuery();
		}
	}
}