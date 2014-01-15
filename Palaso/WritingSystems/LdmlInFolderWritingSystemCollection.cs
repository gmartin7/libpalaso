using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class LdmlInFolderWritingSystemStore : WritingSystemStoreBase
	{
		private const string _kExtension = ".ldml";
		private string _path;

		/// <summary>
		/// Use the default repository
		/// </summary>
		public LdmlInFolderWritingSystemStore()
		{
			string p =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(p);
			p = Path.Combine(p, "WritingSystemRepository");
			Directory.CreateDirectory(p);
			PathToWritingSystems = p;
		}

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="path"></param>
		public LdmlInFolderWritingSystemStore(string path)
		{
			PathToWritingSystems = path;
		}

		public string PathToWritingSystems
		{
			get
			{
				return _path;
			}
			set
			{
				_path = value;
				if (!Directory.Exists(_path))
				{
					string parent = Directory.GetParent(_path).FullName;
					if (!Directory.Exists(parent))
					{
						throw new ApplicationException(
							"The writing system repository cannot be created because its parent folder, " + parent +
							", does not exist.");
					}
					Directory.CreateDirectory(_path);
				}
				LoadAllDefinitions();
			}
		}

		private string GetFilePath(WritingSystemDefinition ws)
		{
			return Path.Combine(PathToWritingSystems, GetFileName(ws));
		}

		private string GetFilePathFromIdentifier(string identifier)
		{
			return Path.Combine(PathToWritingSystems, GetFileNameFromIdentifier(identifier));
		}

		public string GetFileName(WritingSystemDefinition ws)
		{
			return GetFileNameFromIdentifier(ws.RFC5646);
		}

		private static string GetFileNameFromIdentifier(string identifier)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				return identifier;
			}
			return identifier + _kExtension;
		}

		public void LoadAllDefinitions()
		{
			Clear();
			foreach (string filePath in Directory.GetFiles(_path, "*.ldml"))
			{
				try
				{
					string identifier = Path.GetFileNameWithoutExtension(filePath);
					Set(LoadDefinition(identifier));
				}
				catch (Exception
#if DEBUG
					error
#endif
					)
				{
#if DEBUG
					throw new ApplicationException("problem loading " + filePath, error);
#endif
				}
			}


		   //  if (!DontAddDefaultDefinitions )
		   //  {
				 AddActiveOSLanguages();
		   //  }

//
//            if (!_dontAddDefaultDefinitions && FindAlreadyLoadedWritingSystem("en") == null)
//            {
//                WritingSystemDefinition def = new WritingSystemDefinition();
//                LdmlAdaptor adaptor = new LdmlAdaptor();
//                adaptor.FillWithDefaults("en", def);
//                this.WritingSystemDefinitions.Add(def);
//            }
		}

		private bool HaveMatchingDefinitionInTrash(string identifier)
		{
			string path = PathToWritingSystemTrash();
			path = Path.Combine(path, GetFileNameFromIdentifier(identifier));
			return File.Exists(path);
		}

		private void AddActiveOSLanguages()
		{
			if (SystemWritingSystemProvider != null)
			{
				foreach (WritingSystemDefinition ws in SystemWritingSystemProvider)
				{
					if (null == FindAlreadyLoadedWritingSystem(ws.RFC5646))
					{
						if (!HaveMatchingDefinitionInTrash(ws.RFC5646))
						{
							Set(ws);
						}
					}
				}
			}
		}

//        /// <summary>
//        /// useful for unit tests
//        /// </summary>
//        public bool DontAddDefaultDefinitions { get; set; }
//
		/// <summary>
		/// Provides writing systems from a repository that comes, for example, with the OS
		/// </summary>
		public IEnumerable<WritingSystemDefinition> SystemWritingSystemProvider { get; set; }

		private WritingSystemDefinition FindAlreadyLoadedWritingSystem(string rfc4646)
		{
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if(ws.RFC5646 == rfc4646 )
					return ws;
			}
			return null;
		}

		public WritingSystemDefinition LoadDefinition(string identifier)
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			LdmlAdaptor adaptor = new LdmlAdaptor();
			string filePath = GetFilePathFromIdentifier(identifier);
			if (File.Exists(filePath))
			{
				adaptor.Read(filePath, ws);
				ws.StoreID = identifier;
				ws.Modified = false;
			}
			else
			{
				if (identifier.ToLower() == "en-latn")
				{
					//??? Why is the default necessary here? I think a good default for en would already be present.
					adaptor.FillWithDefaults("en-Latn", ws);
				}
			}
			return ws;
		}

		public void SaveDefinition(WritingSystemDefinition ws)
		{
			string incomingFileName = GetFileNameFromIdentifier(ws.StoreID);
			Set(ws);
			string writingSystemFileName = GetFileName(ws);
			string writingSystemFilePath = GetFilePath(ws);
			MemoryStream oldData = null;
			if (!ws.Modified && File.Exists(writingSystemFilePath))
			{
				return; // no need to save (better to preserve the modified date)
			}
			if (!String.IsNullOrEmpty(incomingFileName))
			{
				string previousFilePath = Path.Combine(PathToWritingSystems, incomingFileName);
				if (File.Exists(previousFilePath))
				{
					// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
					try
					{
						oldData = new MemoryStream(File.ReadAllBytes(previousFilePath), false);
					}
					catch {}
					if (writingSystemFileName != incomingFileName)
					{
						// What to do?  Assume that the UI has already checked for existing, asked, and allowed the overwrite.
						File.Delete(previousFilePath); //!!! Should this be move to trash?
					}
				}
			}
			LdmlAdaptor adaptor = new LdmlAdaptor();
			adaptor.Write(writingSystemFilePath, ws, oldData);

			ws.Modified = false;
		}

		public string FilePathToWritingSystem(WritingSystemDefinition ws)
		{
			return Path.Combine(PathToWritingSystems, GetFileName(ws));
		}

		override public void Remove(string identifier)
		{
			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(GetFilePathFromIdentifier(identifier)) && Exists(identifier))
			{
				WritingSystemDefinition ws = Get(identifier);
				SaveDefinition(ws);
			}

			if (File.Exists(GetFilePathFromIdentifier(identifier)))
			{
				Directory.CreateDirectory(PathToWritingSystemTrash());
				string destination = Path.Combine(PathToWritingSystemTrash(), GetFileNameFromIdentifier(identifier));
				//clear out any old on already in the trash
				if (File.Exists(destination))
				{
					File.Delete(destination);
				}
				File.Move(GetFilePathFromIdentifier(identifier), destination);
			}
			base.Remove(identifier);
		}

		private string PathToWritingSystemTrash()
		{
			return Path.Combine(_path, "trash");
		}

		override public void Save()
		{
			//delete anything we're going to delete first, to prevent loosing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
			List<string> idsToRemove = new List<string>();
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if (ws.MarkedForDeletion)
				{
					idsToRemove.Add(ws.StoreID);//nb: purposefully not removing from our list, for fear of leading to bugs in the UI. If we did this, we'd want to require the UI to reload the WS list after a save.
				}
			}
			foreach (string id in idsToRemove)
			{
				Remove(id);
			}

			// make a copy and then go through that list - SaveDefinition calls Set which
			// may delete and then insert the same writing system - which would change WritingSystemDefinitions
			// and not be allowed in a foreach loop
			List<WritingSystemDefinition> allDefs = new List<WritingSystemDefinition>();
			foreach (WritingSystemDefinition ws in WritingSystemDefinitions)
			{
				if (CanSet(ws))
				{
					allDefs.Add(ws);
				}
			}
			foreach (WritingSystemDefinition ws in allDefs)
			{
				SaveDefinition(ws);
				if (!ws.Modified)
				{
					OnChangeNotifySharedStore(ws);
				}
			}
		}

		public override void Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			// Renaming files on Set keeps the file names consistent with StoreID which is changed in the base.
			// This allows us to avoid creating duplicate files and to preserve LDML data which is not used
			// in palaso.
			string oldFileName = GetFileNameFromIdentifier(ws.StoreID);
			string oldFilePath = string.IsNullOrEmpty(oldFileName) ? string.Empty : Path.Combine(PathToWritingSystems, oldFileName);
			string oldID = ws.StoreID;
			base.Set(ws);
			if (oldID == ws.StoreID || string.IsNullOrEmpty(oldFileName) || !File.Exists(oldFilePath))
			{
				return;
			}
			string writingSystemFilePath = GetFilePath(ws);
			File.Move(oldFilePath, writingSystemFilePath);
		}
	}
}