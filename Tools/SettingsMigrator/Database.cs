/***
 * Database.cs
 * 
 * Contains the glue between the properties of the Settings class and
 * the underlying SQLite database.
 * 
 * Takes care of creating, initializing, updating and loading the
 * data in the SQLite database file.
 *	
 * * * * * * * * *
 * 
 * Copyright 2013 Simplare
 * 
 * This code is part of the Stoffi Music Player Project.
 * Visit our website at: stoffiplayer.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version
 * 3 of the License, or (at your option) any later version.
 * 
 * See stoffiplayer.com/license for more information.
 ***/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

using System.Data.SQLite;
//using Mono.Data.Sqlite;

namespace Stoffi.Tools.Migrator
{
	/// <summary>
	/// Describes an SQLite database.
	/// </summary>
	public class Database
	{
		#region Members
		private string dbConnection;
		private SQLiteConnection cnn;
		#endregion

		#region Constructor
		public Database(string filename)
		{
			if (!File.Exists(filename))
				SQLiteConnection.CreateFile(filename);
			dbConnection = "uri=file:"+filename;
			cnn = new SQLiteConnection(dbConnection);
			cnn.Open();
		}
		public static string[] Split(string source, char separator)
		{
			char[] toks = new char[2] { '\"', separator };
			char[] quot = new char[1] { '\"' };
			int n = 0;
			List<string> ls = new List<string>();
			string s;

			while (source.Length > 0)
			{
				n = source.IndexOfAny(toks, n);
				if (n == -1) break;
				if (source[n] == toks[0])
				{
					//source = source.Remove(n, 1);
					n = source.IndexOfAny(quot, n + 1);
					if (n == -1)
					{
						//source = "\"" + source;
						break;
					}
					n++;
					//source = source.Remove(n, 1);
				}
				else
				{
					s = source.Substring(0, n).Trim();
					if (s.Length > 1 && s[0] == quot[0] && s[s.Length - 1] == s[0])
						s = s.Substring(1, s.Length - 2);

					source = source.Substring(n + 1).Trim();
					if (s.Length > 0) ls.Add(s);
					n = 0;
				}
			}
			if (source.Length > 0)
			{
				s = source.Trim();
				if (s.Length > 1 && s[0] == quot[0] && s[s.Length - 1] == s[0])
					s = s.Substring(1, s.Length - 2);
				ls.Add(s);
			}

			string[] ar = new string[ls.Count];
			ls.CopyTo(ar, 0);

			return ar;
		}
		#endregion

		#region Destructor
		~Database()
		{
			if (cnn != null)
				try { cnn.Close(); }
				catch { }
		}
		#endregion

		#region Methods

		public DataTable ExecuteReader(string sql)
		{
			DataTable dt = new DataTable();
			try
			{
				var cmd = new SQLiteCommand(cnn);
				cmd.CommandText = sql;
				var reader = cmd.ExecuteReader();
				dt.Load(reader);
				reader.Close();
			}
			catch (Exception e)
			{
				U.L(LogLevel.Error, "Settings", "Error executing query in database: " + e.Message);
				throw e;
			}
			return dt;
		}

		public int ExecuteNonQuery(string sql)
		{
			var fails = 0;
			var maxAttempts = 5;
			while (fails < maxAttempts)
			{
				try
				{
					var cmd = new SQLiteCommand(cnn);
					cmd.CommandText = sql;
					var rowsUpdated = cmd.ExecuteNonQuery();
					return rowsUpdated;
				}
				catch (Exception e)
				{
					U.L(LogLevel.Error, "Settings", "Error executing non-query in database: " + e.Message);
				}
				fails++;
			}
			return 0;
		}

		public string ExecuteScalar(string sql)
		{
			try
			{
				var cmd = new SQLiteCommand(cnn);
				cmd.CommandText = sql;
				var value = cmd.ExecuteScalar();
				if (value != null)
					return value.ToString();
			}
			catch (Exception e)
			{
				U.L(LogLevel.Error, "Settings", "Error executing scalar in database: " + e.Message);
				throw e;
			}
			return "";
		}

		public DataTable Select(string sql)
		{
			return ExecuteReader(sql);
		}

		public int Insert(string table, Dictionary<string, string> data)
		{
			var columns = String.Join(", ", from d in data select d.Key);
			var values = String.Join(", ", from d in data select d.Value);
			return ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", table, columns, values));
		}

		public int BulkInsert(string table, IEnumerable<Dictionary<string, string>> data)
		{
			if (data.Count() == 0)
				return 0;
			
			var columns = String.Join(", ", from d in data.ElementAt(0) select d.Key);
			List<string> rows = new List<string>();
			foreach (var d in data)
			{
				rows.Add("("+String.Join(", ", from x in d select x.Value)+")");
			}
			var values = String.Join(", ", rows);
			return ExecuteNonQuery(String.Format("insert into {0}({1}) values {2};", table, columns, values));
		}

		public int LastID(string table)
		{
			var data = ExecuteReader(String.Format("select max(rowid) as id from {0};", table));
			if (data.Rows.Count == 0)
				return -1;
			return Convert.ToInt32(data.Rows[0]["id"].ToString());
		}

		public int Update(string table, Dictionary<string, string> data, string filter)
		{
			var vals = "";
			if (data.Count > 0)
				vals = String.Join(",", from d in data select String.Format("{0}={1}", d.Key, d.Value));
			return ExecuteNonQuery(String.Format("update {0} set {1} where {2};", table, vals, filter));
		}

		public int Delete(string table, string filter)
		{
			return ExecuteNonQuery(String.Format("delete from {0} where {1};", table, filter));
		}

		public int Delete(string table)
		{
			return Delete(table, "1");
		}

		public int CreateTable(string name, string[] textFields = null, string[] integerFields = null, string[] realFields = null)
		{
			var sql = "create table if not exists " + name + " (";

			var tF = "";
			var iF = "";
			var rF = "";

			if (textFields != null)
				tF = String.Join(", ", from f in textFields select String.Format("{0} text", f));
			if (integerFields != null)
				iF = String.Join(", ", from f in integerFields select String.Format("{0} integer", f));
			if (realFields != null)
				rF = String.Join(", ", from f in realFields select String.Format("{0} real", f));

			var fieldGroups = new List<string>();
			if (tF != "")
				fieldGroups.Add(tF);
			if (iF != "")
				fieldGroups.Add(iF);
			if (rF != "")
				fieldGroups.Add(rF);

			sql += String.Join(", ", fieldGroups);
			sql += ");";
			return ExecuteNonQuery(sql);
		}

		#endregion
	}

	/// <summary>
	/// Represents a manager that takes care of all
	/// application settings.
	/// </summary>
	public static partial class SettingsManager
	{
		#region Members
		public static Database db;
		private static object saveToDatabaseLock = new object();
		private static object saveLock = new object();
		#endregion

		#region Constructor

		/// <summary>
		/// Initializes the <see cref="Stoffi.Core.Settings"/> class.
		/// </summary>
		static SettingsManager()
		{
		}

		#endregion

		#region Methods

		#region Public

		#region Converters

		/// <summary>
		/// String representation of the repeat state.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="repeat">Repeat state.</param>
		public static string RepeatToString(string repeat)
		{
			switch (repeat)
			{
				case "NoRepeat":
					return "off";

				case "RepeatAll":
					return "all";

				case "RepeatOne":
					return "one";
			}
			return "off";
		}

		/// <summary>
		/// The repeat state represented by a string.
		/// </summary>
		/// <returns>The repeat state.</returns>
		/// <param name="repeat">The string representation.</param>
		public static RepeatState StringToRepeat(string repeat)
		{
			switch (repeat)
			{
				case "off":
					return RepeatState.NoRepeat;

				case "all":
					return RepeatState.RepeatAll;

				case "one":
					return RepeatState.RepeatOne;
			}
			return RepeatState.NoRepeat;
		}

		/// <summary>
		/// String representation of the shuffle state.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="shuffle">Shuffle state.</param>
		public static string ShuffleToString(string shuffle)
		{
			return !String.IsNullOrWhiteSpace(shuffle) && shuffle.ToLower() == "true" ? "random" : "off";
		}

		/// <summary>
		/// The shuffle state represented by a string.
		/// </summary>
		/// <returns>The shuffle state.</returns>
		/// <param name="shuffle">The string representation.</param>
		public static bool StringToShuffle(string shuffle)
		{
			return shuffle == "random";
		}

		/// <summary>
		/// String representation of the media state.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="mediaState">Media state.</param>
		public static string MediaStateToString(string mediaState)
		{
			switch (mediaState)
			{
				case "Paused":
				case "Ended":
				case "Stopped":
					return "paused";

				case "Playing":
					return "playing";
			}
			return "paused";
		}

		/// <summary>
		/// The media state represented by a string.
		/// </summary>
		/// <returns>The media state.</returns>
		/// <param name="mediaState">The string representation.</param>
		public static MediaState StringToMediaState(string mediaState)
		{
			return mediaState == "playing" ? MediaState.Playing : MediaState.Paused;
		}

		/// <summary>
		/// String representation of the upgrade policy.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="upgrade">Upgrade policy.</param>
		public static string UpgradeToString(string upgrade)
		{
			switch (upgrade)
			{
				case "Automatic":
					return "auto";

				case "Manual":
					return "manual";

				case "Notify":
					return "notify";
			}
			return "auto";
		}

		/// <summary>
		/// The upgrade policy represented by a string.
		/// </summary>
		/// <returns>The upgrade policy.</returns>
		/// <param name="upgrade">The string representation.</param>
		public static UpgradePolicy StringToUpgrade(string upgrade)
		{
			switch (upgrade)
			{
				case "auto":
					return UpgradePolicy.Automatic;

				case "manual":
					return UpgradePolicy.Manual;

				case "notify":
					return UpgradePolicy.Notify;
			}
			return UpgradePolicy.Automatic;
		}

		/// <summary>
		/// String representation of the search policy.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="search">Search policy.</param>
		public static string SearchToString(string search)
		{
			switch (search)
			{
				case "Global":
					return "global";

				case "Individual":
					return "individual";

				case "Partial":
					return "partial";
			}
			return "individual";
		}

		/// <summary>
		/// The search policy represented by a string.
		/// </summary>
		/// <returns>The search policy.</returns>
		/// <param name="search">The string representation.</param>
		public static SearchPolicy StringToSearch(string search)
		{
			switch (search)
			{
				case "global":
					return SearchPolicy.Global;

				case "individual":
					return SearchPolicy.Individual;

				case "partial":
					return SearchPolicy.Partial;
			}
			return SearchPolicy.Individual;
		}

		/// <summary>
		/// String representation of the OpenAdd policy.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="openAdd">OpenAdd policy.</param>
		public static string OpenAddToString(string openAdd)
		{
			switch (openAdd)
			{
				case "DoNotAdd":
					return "none";

				case "Library":
					return "library";

				case "LibraryAndPlaylist":
					return "playlist";
			}
			return "library";
		}

		/// <summary>
		/// The OpenAdd policy represented by a string.
		/// </summary>
		/// <returns>The OpenAdd policy.</returns>
		/// <param name="openAdd">The string representation.</param>
		public static OpenAddPolicy StringToOpenAdd(string openAdd)
		{
			switch (openAdd)
			{
				case "none":
					return OpenAddPolicy.DoNotAdd;

				case "library":
					return OpenAddPolicy.Library;

				case "playlist":
					return OpenAddPolicy.LibraryAndPlaylist;
			}
			return OpenAddPolicy.Library;
		}

		/// <summary>
		/// String representation of the OpenPlay policy.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="openPlay">OpenPlay policy.</param>
		public static string OpenPlayToString(string openPlay)
		{
			switch (openPlay)
			{
				case "BackOfQueue":
					return "back";

				case "DoNotPlay":
					return "none";

				case "FrontOfQueue":
					return "front";

				case "Play":
					return "play";
			}
			return "back";
		}

		/// <summary>
		/// The OpenPlay policy represented by a string.
		/// </summary>
		/// <returns>The OpenPlay policy.</returns>
		/// <param name="openPlay">The string representation.</param>
		public static OpenPlayPolicy StringToOpenPlay(string openPlay)
		{
			switch (openPlay)
			{
				case "back":
					return OpenPlayPolicy.BackOfQueue;

				case "none":
					return OpenPlayPolicy.DoNotPlay;

				case "front":
					return OpenPlayPolicy.FrontOfQueue;

				case "play":
					return OpenPlayPolicy.Play;
			}
			return OpenPlayPolicy.BackOfQueue;
		}

		/// <summary>
		/// String representation of an alignment.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="alignment">Alignment.</param>
		public static string AlignmentToString(Alignment alignment)
		{
			return alignment.ToString().ToLower();
		}

		/// <summary>
		/// Get the Alignment represented by a string.
		/// </summary>
		/// <returns>The alignment.</returns>
		/// <param name="alignment">The string representation.</param>
		public static Alignment StringToAlignment(string alignment)
		{
			switch (alignment)
			{
				case "bottom":
					return Alignment.Bottom;

				case "center":
					return Alignment.Center;

				case "left":
					return Alignment.Left;

				case "middle":
					return Alignment.Middle;

				case "right":
					return Alignment.Right;

				case "top":
					return Alignment.Top;
			}
			throw new InvalidEnumArgumentException("Alignment " + alignment + " not recognized");
		}

		/// <summary>
		/// String representation of a view mode.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="viewMode">ViewMode.</param>
		public static string ViewModeToString(ViewMode viewMode)
		{
			return viewMode.ToString().ToLower();
		}

		/// <summary>
		/// Get the ViewMode represented by a string.
		/// </summary>
		/// <returns>The view mode.</returns>
		/// <param name="viewMode">The string representation.</param>
		public static ViewMode StringToViewMode(string viewMode)
		{
			switch (viewMode)
			{
				case "content":
					return ViewMode.Content;

				case "details":
					return ViewMode.Details;

				case "icons":
					return ViewMode.Icons;

				case "list":
					return ViewMode.List;

				case "tiles":
					return ViewMode.Tiles;
			}
			throw new InvalidEnumArgumentException("View mode " + viewMode + " not recognized");
		}

		/// <summary>
		/// String representation of a source type.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="sourceType">SourceType.</param>
		public static string SourceTypeToString(SourceType sourceType)
		{
			return sourceType.ToString().ToLower();
		}

		/// <summary>
		/// Get the SourceType represented by a string.
		/// </summary>
		/// <returns>The source type.</returns>
		/// <param name="sourceType">The string representation.</param>
		public static SourceType StringToSourceType(string sourceType)
		{
			switch (sourceType)
			{
				case "file":
					return SourceType.File;

				case "folder":
					return SourceType.Folder;

				case "library":
					return SourceType.Library;
			}
			throw new InvalidEnumArgumentException("Source type " + sourceType + " not recognized");
		}

		#endregion

		/// <summary>
		/// Initializes the database connection.
		/// </summary>
		/// <param name="reset">If true the database file will be reset</param>
		public static void InitializeDatabase(string filename, bool reset = false)
		{
			var folder = Path.GetDirectoryName(U.FullPath);

			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			if (reset && File.Exists(filename))
				File.Delete(filename);

			db = new Database(filename);
			CreateDatabase();
		}

		/// <summary>
		/// Create all databases for storing settings.
		/// </summary>
		public static void CreateDatabase()
		{
			db.CreateTable("config", new string[] { "key", "value" });

			db.CreateTable("cloudLinks",
				new string[] { "provider", "picture", "names", "url", "connectUrl", "error" },
				new string[] { "cloudID", "connected", "canShare", "doShare", "canListen", "doListen", "canDonate", "doDonate", "canCreatePlaylist", "doCreatePlaylist", "identity" });
			db.CreateTable("cloudIdentities", null, new string[] { "user", "configuration", "device", "synchronize", "synchronizeConfig", "synchronizePlaylists", "synchronizeQueue", "synchronizeFiles" });

			db.CreateTable("shortcuts", new string[] { "name", "category", "keys", "profile" }, new string[] { "global" });
			db.CreateTable("shortcutProfiles", new string[] { "name", "title" }, new string[] { "protected" });

			db.CreateTable("equalizerProfiles", new string[] { "name", "bands" }, new string[] { "protected", "echo" });

			db.CreateTable("files",
				new string[] { "path", "title", "album", "artist", "genre", "url", "artUrl", "originalArtUrl", "source", "codecs", "grp" },
				new string[] { "year", "bitrate", "track", "channels", "number", "lastWrite", "lastPlayed", "userPlayCount", "globalPlayCount", "sampleRate", "processed" },
				new string[] { "length" }
			);
			db.CreateTable("radio",
				new string[] { "path", "title", "album", "artist", "genre", "url", "artUrl", "originalArtUrl", "source", "codecs", "grp" },
				new string[] { "year", "bitrate", "track", "channels", "number", "lastWrite", "lastPlayed", "userPlayCount", "globalPlayCount", "sampleRate", "processed" },
				new string[] { "length" }
			);
			db.CreateTable("bookmarks", new string[] { "track", "type" }, null, new string[] { "pos" });
			db.CreateTable("queue", new string[] { "path" }, new string[] { "number" });
			db.CreateTable("history", new string[] { "path" }, new string[] { "lastPlayed" });
			db.CreateTable("playlists",
				new string[] { "name", "ownerName", "filter" },
				new string[] { "cloudID", "ownerID", "ownerCacheTime", "listConfig" },
				new string[] { "length" });
			db.CreateTable("playlistTracks", new string[] { "path", "playlist" });

			db.CreateTable("listColumns",
				new string[] { "name", "text", "binding", "converter", "sortOn", "align" },
				new string[] { "alwaysVisible", "sortable", "visible", "config" },
				new string[] { "width" });
			db.CreateTable("listConfigurations",
				new string[] { "selection", "sorting", "filter", "mode" },
				new string[] { "numberColumn", "allowNumber", "showNumber", "numberPos", "useIcons", "acceptDrop", "canDragSort", "canClickSort", "lockSortOnNumber" },
				new string[] { "horizontalOffset", "verticalOffset", "verticalOffsetWithoutSearch", "iconSize" }
			);

			db.CreateTable("sources", new string[] { "type", "data" }, new string[] { "automatic", "ignore" });

			db.CreateTable("pluginData", null, new string[] { "plugin", "enabled", "installed" });
			db.CreateTable("pluginSettings", new string[] { "id", "type", "value", "min", "max" }, new string[] { "visible", "plugin" });
			db.CreateTable("pluginSettingPossibleValues", new string[] { "value" }, new string[] { "setting" });
			db.CreateTable("listenBuffer", new string[] { "url", "method", "track" });
		}

		#region Save to database

		/// <summary>
		/// Save a key-value pair to the database.
		/// </summary>
		/// <param name="table">Database table.</param>
		/// <param name="key">Configuration key.</param>
		/// <param name="value">Value.</param>
		public static void SaveConfig(string table, string key, string value)
		{
			var d = new Dictionary<string,string> ();
			d ["key"] = DBEncode (key);
			d ["value"] = DBEncode (value);

			// check if key exists
			var filter = String.Format ("key='{0}'", key);
			var result = db.Select(String.Format("select * from config where {0};", filter));	
			if (result.Rows.Count > 0)
				db.Update (table, d, filter);
			else
				db.Insert (table, d);
		}

		/// <summary>
		/// Save tracks to a database table.
		/// </summary>
		/// <param name="tracks">Track collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveTracks(IEnumerable<TrackData> tracks, string table)
		{
			while (true)
			{
				try
				{
					var data = (from t in tracks select CreateData(t)).ToList();
					db.BulkInsert(table, data);
					break;
				}
				catch (InvalidOperationException e) { } // collection was modified while trying to save
				catch { }
			}
		}

		/// <summary>
		/// Save a single track to a database table.
		/// </summary>
		/// <param name="track">Track.</param>
		/// <param name="table">Database table.</param>
		public static void SaveTrack(TrackData track, string table)
		{
			var data = CreateData (track);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save track references to a database table.
		/// </summary>
		/// <param name="tracks">Track collection.</param>
		/// <param name="table">Database table.</param>
		/// <param name="fieldsToCopy">The fields to copy into the table in addition to the path.</param>
		public static void SaveTrackReferences(IEnumerable<TrackData> tracks, string table, string[] fieldsToCopy = null)
		{
			foreach (TrackData track in tracks)
				SaveTrackReference (track, table, fieldsToCopy);
		}

		/// <summary>
		/// Save a reference to a single track in a database table.
		/// </summary>
		/// <param name="track">Track.</param>
		/// <param name="table">Database table.</param>
		/// <param name="fieldsToCopy">The fields to copy into the table in addition to the path.</param>
		public static void SaveTrackReference(TrackData track, string table, string[] fieldsToCopy = null)
		{
			var data = new Dictionary<string,string> ();
			data ["path"] = DBEncode(track.Path);

			if (fieldsToCopy != null)
			{
				foreach (var field in fieldsToCopy)
				{
					switch (field)
					{
					case "lastPlayed":
						data [field] = DBEncode (track.LastPlayed);
						break;

					case "number":
						data [field] = DBEncode (track.Number);
						break;
					}
				}
			}

			db.Insert (table, data);
		}

		/// <summary>
		/// Save bookmarks to a database table.
		/// </summary>
		/// <param name="bookmarks">Bookmark collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveBookmarks(IList<Tuple<string, string, double>> bookmarks, string table)
		{
			foreach (var bookmark in bookmarks)
				SaveBookmark (bookmark, table);
		}

		/// <summary>
		/// Save a single bookmark to a database table.
		/// </summary>
		/// <param name="bookmark">A tuple with track path, type and position of bookmark.</param>
		/// <param name="table">Database table.</param>
		public static void SaveBookmark(Tuple<string, string, double> bookmark, string table)
		{
			var data = new Dictionary<string,string> ();
			data ["track"] = DBEncode (bookmark.Item1);
			data ["type"] = DBEncode (bookmark.Item2);
			data ["pos"] = DBEncode (bookmark.Item3);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save list view configuration to a database table.
		/// </summary>
		/// <param name="configs">List view configuration collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveListConfigurations(IEnumerable<ViewDetailsConfig> configs, string table)
		{
			foreach (var config in configs)
				SaveListConfiguration (config, table);
		}

		/// <summary>
		/// Save a single list view configuration to a database table.
		/// </summary>
		/// <param name="config">A list view configuration.</param>
		/// <param name="table">Database table.</param>
		public static void SaveListConfiguration(ViewDetailsConfig config, string table)
		{
			if (config == null) return;
			var data = CreateData (config);
			SaveColumn (config.NumberColumn, "listColumns", -1);
			data ["numberColumn"] = DBEncode (db.LastID ("listColumns"));
			db.Insert (table, data);
			var id = db.LastID (table);
			SaveColumns (config.Columns, "listColumns", id);
		}

		/// <summary>
		/// Save list view columns to a database table.
		/// </summary>
		/// <param name="columns">Cloud identity collection.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the config row.</param>
		public static void SaveColumns(IEnumerable<ViewDetailsColumn> columns, string table, int parentID)
		{
			var data = (from x in columns select CreateData(x)).ToList();
			foreach (var x in data) {
				x.Add("config", DBEncode(parentID));
			}
			db.BulkInsert(table, data);
		}

		/// <summary>
		/// Save a single list view column to a database table.
		/// </summary>
		/// <param name="column">A list view column.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the config row.</param>
		public static void SaveColumn(ViewDetailsColumn column, string table, int parentID)
		{
			var data = CreateData (column);
			data ["config"] = DBEncode (parentID);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save shortcut profiles to a database table.
		/// </summary>
		/// <param name="profiles">Shortcut profile collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveShortcutProfiles(IEnumerable<KeyboardShortcutProfile> profiles, string table)
		{
			foreach (var profile in profiles)
				SaveShortcutProfile (profile, table);
		}

		/// <summary>
		/// Save a single shortcut profile to a database table.
		/// </summary>
		/// <param name="profile">A shortcut profile.</param>
		/// <param name="table">Database table.</param>
		public static void SaveShortcutProfile(KeyboardShortcutProfile profile, string table)
		{
			var data = CreateData (profile);
			db.Insert (table, data);
			SaveShortcuts (profile.Shortcuts, "shortcuts", db.LastID(table));
		}

		/// <summary>
		/// Save shortcuts to a database table.
		/// </summary>
		/// <param name="shortcuts">Shortcut collection.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the profile row.</param>
		public static void SaveShortcuts(IEnumerable<KeyboardShortcut> shortcuts, string table, int parentID)
		{
			var data = (from x in shortcuts select CreateData(x)).ToList();
			foreach (var x in data) { x["profile"] = DBEncode(parentID); }
			db.BulkInsert(table, data);
		}

		/// <summary>
		/// Save a single shortcut to a database table.
		/// </summary>
		/// <param name="shortcut">A shortcut.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the profile row.</param>
		public static void SaveShortcut(KeyboardShortcut shortcut, string table, int parentID)
		{
			var data = CreateData (shortcut);
			data ["profile"] = DBEncode (parentID);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save equalizer profiles to a database table.
		/// </summary>
		/// <param name="profiles">Equalizer profile collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveEqualizerProfiles(IEnumerable<EqualizerProfile> profiles, string table)
		{
			foreach (var profile in profiles)
				SaveEqualizerProfile (profile, table);
		}

		/// <summary>
		/// Save a single equalizer profile to a database table.
		/// </summary>
		/// <param name="profile">A equalizer profile.</param>
		/// <param name="table">Database table.</param>
		public static void SaveEqualizerProfile(EqualizerProfile profile, string table)
		{
			var data = CreateData (profile);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save cloud identities to a database table.
		/// </summary>
		/// <param name="identities">Cloud identity collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveCloudIdentities(IEnumerable<CloudIdentity> identities, string table)
		{
			foreach (var identity in identities)
				SaveCloudIdentity (identity, table);
		}

		/// <summary>
		/// Save a single cloud identity to a database table.
		/// </summary>
		/// <param name="identity">A cloud identity.</param>
		/// <param name="table">Database table.</param>
		public static void SaveCloudIdentity(CloudIdentity identity, string table)
		{
			var data = CreateData (identity);
			db.Insert (table, data);
			SaveCloudLinks (identity.Links, "cloudLinks", db.LastID(table));
		}

		/// <summary>
		/// Save cloud links to a database table.
		/// </summary>
		/// <param name="links">Cloud link collection.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the cloud identity row.</param>
		public static void SaveCloudLinks(IEnumerable<Link> links, string table, int parentID)
		{
			foreach (var link in links)
				SaveCloudLink (link, table, parentID);
		}

		/// <summary>
		/// Save a single cloud link to a database table.
		/// </summary>
		/// <param name="link">A cloud link.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the cloud identity row.</param>
		public static void SaveCloudLink(Link link, string table, int parentID)
		{
			var data = CreateData (link);
			data ["identity"] = DBEncode (parentID);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save plugin meta data to a database table.
		/// </summary>
		/// <param name="pluginData">Meta data collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveMetadata(IEnumerable<PluginSettings> data, string table)
		{
			foreach (var d in data)
				SaveMetadata (d, table);
		}

		/// <summary>
		/// Save a single plugin to a database table.
		/// </summary>
		/// <param name="pluginData">The plugin meta data.</param>
		/// <param name="table">Database table.</param>
		public static void SaveMetadata(PluginSettings pluginData, string table)
		{
			var data = CreateData (pluginData);
			db.Insert (table, data);
			SavePluginSettings (pluginData.Settings, "pluginSettings", db.LastID(table));
		}

		/// <summary>
		/// Save settings to a database table.
		/// </summary>
		/// <param name="bookmarks">Setting collection.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the plugin data row.</param>
		public static void SavePluginSettings(IEnumerable<Stoffi.Plugins.Setting> settings, string table, int parentID)
		{
			foreach (var setting in settings)
				SavePluginSetting (setting, table, parentID);
		}

		/// <summary>
		/// Save a single plugin setting to a database table.
		/// </summary>
		/// <param name="setting">The plugin setting.</param>
		/// <param name="table">Database table.</param>
		/// <param name="parentID">The ID of the plugin data row.</param>
		public static void SavePluginSetting(Stoffi.Plugins.Setting setting, string table, int parentID)
		{
			var data = CreateData (setting);
			data ["plugin"] = DBEncode (parentID);
			db.Insert (table, data);

			var id = db.LastID(table);
			foreach (var v in setting.PossibleValues)
			{
				data.Clear ();
				data ["plugin"] = DBEncode (id);
				data ["value"] = DBEncode (v.ToString ());
			}
		}

		/// <summary>
		/// Save sources to a database table.
		/// </summary>
		/// <param name="sources">Source collection.</param>
		/// <param name="table">Database table.</param>
		public static void SaveSources(IEnumerable<SourceData> sources, string table)
		{
			var data = (from x in sources select CreateData(x)).ToList();
			db.BulkInsert(table, data);
		}

		/// <summary>
		/// Save a single source to a database table.
		/// </summary>
		/// <param name="source">A source where to scan for files.</param>
		/// <param name="table">Database table.</param>
		public static void SaveSource(SourceData source, string table)
		{
			var data = CreateData (source);
			db.Insert (table, data);
		}

		/// <summary>
		/// Save playlists to a database table.
		/// </summary>
		/// <param name="playlists">Playlist collection.</param>
		/// <param name="table">Database table.</param>
		public static void SavePlaylists(IEnumerable<PlaylistData> playlists, string table)
		{
			foreach (var playlist in playlists)
				SavePlaylist (playlist, table);
		}

		/// <summary>
		/// Save a single playlist to a database table.
		/// </summary>
		/// <param name="playlist">A playlist.</param>
		/// <param name="table">Database table.</param>
		public static void SavePlaylist(PlaylistData playlist, string table)
		{
			var data = CreateData (playlist);
			db.Insert (table, data);
			var rowid = db.LastID (table);

			data.Clear ();
			SaveListConfiguration (playlist.ListConfig, "listConfigurations");
			data ["listConfig"] = DBEncode (db.LastID("listConfigurations"));
			db.Update (table, data, String.Format("rowid={0}", rowid));

			var d = new List<Dictionary<string, string>>();
			foreach (var t in playlist.Tracks)
			{
				var x = new Dictionary<string, string>();
				x ["path"] = DBEncode(t.Path);
				x ["playlist"] = DBEncode (rowid);
				d.Add(x);
			}
			db.BulkInsert("playlistTracks", d);
		}

		/// <summary>
		/// Save buffer of listen submissions to a database table.
		/// </summary>
		/// <param name="buffer">Listen submission buffer.</param>
		/// <param name="table">Database table.</param>
		public static void SaveListenBuffer(Dictionary<string, Tuple<string, string>> buffer, string table)
		{
			foreach (var listen in buffer)
			{
				var data = new Dictionary<string,string> ();
				data ["url"] = DBEncode (listen.Key);
				data ["method"] = DBEncode (listen.Value.Item1);
				data ["track"] = DBEncode (listen.Value.Item2);
				db.Insert (table, data);
			}
		}

		#endregion

		#region Encode for database

		/// <summary>
		/// Create data for an SQL update or insert query.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public static Dictionary<string, string> CreateData<T>(T obj)
		{
			var data = new Dictionary<string, string>();

			if (typeof(T) == typeof(TrackData))
			{
				var track = obj as TrackData;
				data["album"] = DBEncode(track.Album);
				data["artist"] = DBEncode(track.Artist);
				data["artUrl"] = DBEncode(track.ArtURL);
				data["bitrate"] = DBEncode(track.Bitrate);
				data["channels"] = DBEncode(track.Channels);
				data["codecs"] = DBEncode(track.Codecs);
				data["genre"] = DBEncode(track.Genre);
				data["grp"] = DBEncode(track.Group);
				data["lastPlayed"] = DBEncode(track.LastPlayed);
				data["lastWrite"] = DBEncode(track.LastWrite);
				data["length"] = DBEncode(track.Length);
				data["number"] = DBEncode(track.Number);
				data["originalArtUrl"] = DBEncode(track.OriginalArtURL);
				data["userPlayCount"] = DBEncode(track.PlayCount);
				data["processed"] = DBEncode(track.Processed);
				data["sampleRate"] = DBEncode(track.SampleRate);
				data["source"] = DBEncode(track.Source);
				data["title"] = DBEncode(track.Title);
				data["track"] = DBEncode(track.TrackNumber);
				data["url"] = DBEncode(track.URL);
				data["globalPlayCount"] = DBEncode(track.Views);
				data["year"] = DBEncode(track.Year);
				data["path"] = DBEncode(track.Path);
			}
			else if (typeof(T) == typeof(ViewDetailsConfig))
			{
				var config = obj as ViewDetailsConfig;
				data["acceptDrop"] = DBEncode(config.AcceptFileDrops);
				data["filter"] = DBEncode(config.Filter);
				data["allowNumber"] = DBEncode(config.HasNumber);
				data["horizontalOffset"] = DBEncode(config.HorizontalScrollOffset);
				data["iconSize"] = DBEncode(config.IconSize);
				data["canClickSort"] = DBEncode(config.IsClickSortable);
				data["canDragSort"] = DBEncode(config.IsDragSortable);
				data["showNumber"] = DBEncode(config.IsNumberVisible);
				data["lockSortOnNumber"] = DBEncode(config.LockSortOnNumber);
				data["mode"] = DBEncode(ViewModeToString(config.Mode));
				data["numberPos"] = DBEncode(config.NumberIndex);
				data["selection"] = DBEncode(String.Join(";", config.SelectedIndices));
				data["sorting"] = DBEncode(String.Join(";", config.Sorts));
				data["useIcons"] = DBEncode(config.UseIcons);
				data["verticalOffset"] = DBEncode(config.VerticalScrollOffset);
				data["verticalOffsetWithoutSearch"] = DBEncode(config.VerticalScrollOffsetWithoutSearch);
			}
			else if (typeof(T) == typeof(ViewDetailsColumn))
			{
				var column = obj as ViewDetailsColumn;
				data["align"] = DBEncode(AlignmentToString(column.Alignment));
				data["binding"] = DBEncode(column.Binding);
				data["converter"] = DBEncode(column.Converter);
				data["alwaysVisible"] = DBEncode(column.IsAlwaysVisible);
				data["sortable"] = DBEncode(column.IsSortable);
				data["visible"] = DBEncode(column.IsVisible);
				data["name"] = DBEncode(column.Name);
				data["sortOn"] = DBEncode(column.SortField);
				data["text"] = DBEncode(column.Text);
				data["width"] = DBEncode(column.Width);
			}
			else if (typeof(T) == typeof(KeyboardShortcutProfile))
			{
				var profile = obj as KeyboardShortcutProfile;
				data["protected"] = DBEncode(profile.IsProtected);
				data["name"] = DBEncode(profile.Name);
			}
			else if (typeof(T) == typeof(KeyboardShortcut))
			{
				var shortcut = obj as KeyboardShortcut;
				data["category"] = DBEncode(shortcut.Category);
				data["global"] = DBEncode(shortcut.IsGlobal);
				data["keys"] = DBEncode(shortcut.Keys);
				data["name"] = DBEncode(shortcut.Name);
			}
			else if (typeof(T) == typeof(EqualizerProfile))
			{
				var eqProfile = obj as EqualizerProfile;
				data["echo"] = DBEncode(eqProfile.EchoLevel);
				data["protected"] = DBEncode(eqProfile.IsProtected);
				data["name"] = DBEncode(eqProfile.Name);
				data["bands"] = DBEncode(String.Join(";", eqProfile.Levels));
			}
			else if (typeof(T) == typeof(CloudIdentity))
			{
				var identity = obj as CloudIdentity;
				data["configuration"] = DBEncode(identity.ConfigurationID);
				data["device"] = DBEncode(identity.DeviceID);
				data["synchronize"] = DBEncode(identity.Synchronize);
				data["synchronizeConfig"] = DBEncode(identity.SynchronizeConfig);
				data["synchronizeFiles"] = DBEncode(identity.SynchronizeFiles);
				data["synchronizePlaylists"] = DBEncode(identity.SynchronizePlaylists);
				data["synchronizeQueue"] = DBEncode(identity.SynchronizeQueue);
				data["user"] = DBEncode(identity.UserID);
			}
			else if (typeof(T) == typeof(Link))
			{
				var link = obj as Link;
				data["canCreatePlaylist"] = DBEncode(link.CanCreatePlaylist);
				data["canDonate"] = DBEncode(link.CanDonate);
				data["canListen"] = DBEncode(link.CanListen);
				data["canShare"] = DBEncode(link.CanShare);
				data["connected"] = DBEncode(link.Connected);
				data["connectUrl"] = DBEncode(link.ConnectURL);
				data["doCreatePlaylist"] = DBEncode(link.DoCreatePlaylist);
				data["doDonate"] = DBEncode(link.DoDonate);
				data["doListen"] = DBEncode(link.DoListen);
				data["doShare"] = DBEncode(link.DoShare);
				data["error"] = DBEncode(link.Error);
				data["cloudID"] = DBEncode(link.ID);
				data["names"] = DBEncode(String.Join("\n", link.Names));
				data["picture"] = DBEncode(link.Picture);
				data["provider"] = DBEncode(link.Provider);
				data["url"] = DBEncode(link.URL);
			}
			else if (typeof(T) == typeof(PluginSettings))
			{
				var pluginData = obj as PluginSettings;
				data["enabled"] = DBEncode(pluginData.Enabled);
				data["installed"] = DBEncode(pluginData.Installed);
				data["plugin"] = DBEncode(pluginData.PluginID);
			}
			else if (typeof(T) == typeof(Stoffi.Plugins.Setting))
			{
				var setting = obj as Stoffi.Plugins.Setting;
				data["id"] = DBEncode(setting.ID);
				data["visible"] = DBEncode(setting.IsVisible);
				data["max"] = DBEncode(setting.Maximum.ToString());
				data["min"] = DBEncode(setting.Minimum.ToString());
				data["type"] = DBEncode(setting.SerializedType);
				data["value"] = DBEncode(setting.SerializedValue);
			}
			else if (typeof(T) == typeof(SourceData))
			{
				var source = obj as SourceData;
				data["automatic"] = DBEncode(source.Automatic);
				data["data"] = DBEncode(source.Data);
				data["ignore"] = DBEncode(source.Ignore);
				data["type"] = DBEncode(SourceTypeToString(source.Type));
			}
			else if (typeof(T) == typeof(PlaylistData))
			{
				var playlist = obj as PlaylistData;
				data["filter"] = DBEncode(playlist.Filter);
				data["cloudID"] = DBEncode(playlist.ID);
				data["name"] = DBEncode(playlist.Name);
				data["ownerCacheTime"] = DBEncode(playlist.OwnerCacheTime);
				data["ownerID"] = DBEncode(playlist.OwnerID);
				data["ownerName"] = DBEncode(playlist.OwnerName);
				data["length"] = DBEncode(playlist.Time);
			}

			return data;
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(string x)
		{
			x = x ?? "";
			return String.Format("'{0}'", x.Replace("\'", "\'\'"));
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(bool x)
		{
			return x ? "1" : "0";
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(ulong x)
		{
			return x.ToString("0", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(long x)
		{
			return x.ToString("0", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(uint x)
		{
			return x.ToString("0", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(int x)
		{
			return x.ToString("0", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(double x)
		{
			return x.ToString("0.0000", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a string safe for storing in the database.
		/// </summary>
		/// <returns>A string representation safe for database storage.</returns>
		/// <param name="x">The value to store.</param>
		public static string DBEncode(DateTime x)
		{
			long l = 0;
			try
			{
				if (x.Year > 1600)
					l = x.ToFileTimeUtc();
			}
			catch
			{
			}
			return DBEncode(l);
		}

		#endregion

		#region Select from database

		/// <summary>
		/// Get the specified object's rowid in a given table.
		/// </summary>
		/// <param name="table">Table.</param>
		/// <param name="obj">Object to look for.</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <returns>The rowid if found, otherwise -1.</returns>
		public static int GetID<T>(string table, T obj, NewSettings settings)
		{
			if (obj == null)
				return -1;

			DataTable result;
			if (typeof(T) == typeof(ViewDetailsConfig))
			{
				var config = obj as ViewDetailsConfig;
				var configKey = "";
				if (config == settings.FileListConfig)
					configKey = "fileListConfig";
				else if (config == settings.QueueListConfig)
					configKey = "queueListConfig";
				else if (config == settings.HistoryListConfig)
					configKey = "historyListConfig";
				else if (config == settings.RadioListConfig)
					configKey = "radioListConfig";
				else if (config == settings.SoundCloudListConfig)
					configKey = "soundCloudListConfig";
				else if (config == settings.YouTubeListConfig)
					configKey = "youTubeListConfig";
				else if (config == settings.JamendoListConfig)
					configKey = "jamendoListConfig";

				if (configKey != "")
				{
					result = db.Select(String.Format("select value from config where key='{0}';", configKey));
					if (result.Rows.Count > 0)
						return Convert.ToInt32(result.Rows[0]["value"]);
				}
				else
				{
					foreach (var playlist in settings.Playlists)
					{
						if (config == playlist.ListConfig)
						{
							result = db.Select(String.Format("select listConfig from playlists where name='{0}' and cloudID={1};", playlist.Name, playlist.ID));
							if (result.Rows.Count > 0)
								return Convert.ToInt32(result.Rows[0]["listConfig"]);
						}
					}
				}
			}
			else if (typeof(T) == typeof(TrackData))
			{
				var track = obj as TrackData;
				result = db.Select(String.Format("select rowid from {0} where path='{1}';", table, track.Path));
				if (result.Rows.Count > 0)
					return Convert.ToInt32(result.Rows[0]["rowid"]);
			}
			else if (typeof(T) == typeof(EqualizerProfile))
			{
				var profile = obj as EqualizerProfile;
				result = db.Select(String.Format("select rowid from {0} where name='{1}';", table, profile.Name));
				if (result.Rows.Count > 0)
					return Convert.ToInt32(result.Rows[0]["rowid"]);
			}
			else if (typeof(T) == typeof(KeyboardShortcutProfile))
			{
				var profile = obj as KeyboardShortcutProfile;
				result = db.Select(String.Format("select rowid from {0} where name='{1}';", table, profile.Name));
				if (result.Rows.Count > 0)
					return Convert.ToInt32(result.Rows[0]["rowid"]);
			}
			else if (typeof(T) == typeof(PlaylistData))
			{
				var playlist = obj as PlaylistData;
				result = db.Select(String.Format("select rowid from {0} where name='{1}' and cloudID = {2};", table, playlist.Name, playlist.ID));
				if (result.Rows.Count > 0)
					return Convert.ToInt32(result.Rows[0]["rowid"]);
			}
			else if (typeof(T) == typeof(SourceData))
			{
				var source = obj as SourceData;
				result = db.Select(String.Format("select rowid from {0} where type='{1}' and data='{2}';", table, SourceTypeToString(source.Type), source.Data));
				if (result.Rows.Count > 0)
					return Convert.ToInt32(result.Rows[0]["rowid"]);
			}
			return -1;
		}

		#endregion

		#region Event handlers

		#endregion

		#endregion

		#endregion

	}
}

