using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	public abstract class IWritingSystemStoreTests
	{
		private IWritingSystemStore _storeUnderTest;
		private WritingSystemDefinition _writingSystem;

		public IWritingSystemStore StoreUnderTest
		{
			get
			{
				if (_storeUnderTest == null)
				{
					throw new InvalidOperationException("StoreUnderTest must be set before the tests are run.");
				}
				return _storeUnderTest;
			}
			set { _storeUnderTest = value; }
		}

		public abstract IWritingSystemStore CreateNewStore();

		[SetUp]
		public virtual void SetUp()
		{
			_writingSystem = new WritingSystemDefinition();
			StoreUnderTest = CreateNewStore();
		}

		[TearDown]
		public virtual void TearDown() { }

		[Test]
		public void SetTwoDefinitions_CountEquals2()
		{
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			WritingSystemDefinition ws2 = new WritingSystemDefinition();
			ws2.ISO = "two";
			StoreUnderTest.Set(ws2);

			Assert.AreEqual(2, StoreUnderTest.Count);
		}

		[Test]
		public void CreateNewDefinition_CountEquals0()
		{
			StoreUnderTest.CreateNew();
			Assert.AreEqual(0, StoreUnderTest.Count);
		}

		[Test]
		public void CreateNewDefinitionThenSet_CountEquals1()
		{
			StoreUnderTest.Set(StoreUnderTest.CreateNew());
			Assert.AreEqual(1, StoreUnderTest.Count);
		}

		[Test]
		public void SetDefinitionTwice_OnlySetOnce()
		{
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, StoreUnderTest.Count);
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.StoreID = _writingSystem.StoreID;
			StoreUnderTest.Set(ws);
			Assert.AreEqual(1, StoreUnderTest.Count);
		}

		[Test]
		public void SetDefinitionTwice_UpdatesStore()
		{
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, StoreUnderTest.Count);
			Assert.AreNotEqual("one font", _writingSystem.DefaultFontName);
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "one";
			ws1.DefaultFontName = "one font";
			ws1.StoreID = _writingSystem.StoreID;
			StoreUnderTest.Set(ws1);
			WritingSystemDefinition ws2 = StoreUnderTest.Get("one");
			Assert.AreEqual("one font", ws2.DefaultFontName);
		}

		[Test]
		public void Exists_FalseThenTrue()
		{
			Assert.IsFalse(StoreUnderTest.Exists("one"));
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			Assert.IsTrue(StoreUnderTest.Exists("one"));
		}

		[Test]
		public void Remove_CountDecreases()
		{
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			Assert.AreEqual(1, StoreUnderTest.Count);
			StoreUnderTest.Remove(_writingSystem.StoreID);
			Assert.AreEqual(0, StoreUnderTest.Count);
		}

		[Test]
		public void NewerThanEmpty_ReturnsNoneNewer()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			StoreUnderTest.Set(ws1);

			IWritingSystemStore store = CreateNewStore();
			int count = 0;
			foreach (WritingSystemDefinition ws in store.WritingSystemsNewerIn(StoreUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanOlder_ReturnsOneNewer()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			StoreUnderTest.Set(ws1);

			IWritingSystemStore store = CreateNewStore();
			WritingSystemDefinition ws2 = StoreUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			store.Set(ws2);

			int count = 0;
			foreach (WritingSystemDefinition ws in store.WritingSystemsNewerIn(StoreUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(1, count);
		}

		[Test]
		public void NewerThanNewer_ReturnsNoneNewer()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			StoreUnderTest.Set(ws1);

			IWritingSystemStore store = CreateNewStore();
			WritingSystemDefinition ws2 = StoreUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 16);
			store.Set(ws2);

			int count = 0;
			foreach (WritingSystemDefinition ws in store.WritingSystemsNewerIn(StoreUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void NewerThanCheckedAlready_ReturnsNoneNewer()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			ws1.DateModified = new DateTime(2008, 1, 15);
			StoreUnderTest.Set(ws1);

			IWritingSystemStore store = CreateNewStore();
			WritingSystemDefinition ws2 = StoreUnderTest.MakeDuplicate(ws1);
			ws2.DateModified = new DateTime(2008, 1, 14);
			store.Set(ws2);
			store.LastChecked("en", new DateTime(2008, 1, 16));

			int count = 0;
			foreach (WritingSystemDefinition ws in store.WritingSystemsNewerIn(StoreUnderTest.WritingSystemDefinitions))
			{
				count++;
			}
			Assert.AreEqual(0, count);
		}

		[Test]
		public void CanStoreVariants_CountTwo()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition();
			ws1.ISO = "en";
			Assert.AreEqual("en", ws1.RFC5646);
			WritingSystemDefinition ws2 = ws1.Clone();
			ws2.Variant = "latn";
			Assert.AreEqual("en-latn", ws2.RFC5646);

			StoreUnderTest.Set(ws1);
			Assert.AreEqual(1, StoreUnderTest.Count);
			StoreUnderTest.Set(ws2);
			Assert.AreEqual(2, StoreUnderTest.Count);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void StoreTwoOfSame_Throws()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition("foo");
			WritingSystemDefinition ws2 = new WritingSystemDefinition("foo");
			StoreUnderTest.Set(ws1);
			StoreUnderTest.Set(ws2);
		}

		[Test]
		public void CanSetAfterSetting_True()
		{
			StoreUnderTest.Set(_writingSystem);
			Assert.IsTrue(StoreUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetNewWritingSystem_True()
		{
			Assert.IsTrue(StoreUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetSecondNew_False()
		{
			StoreUnderTest.Set(_writingSystem);
			_writingSystem = StoreUnderTest.CreateNew();
			Assert.IsFalse(StoreUnderTest.CanSet(_writingSystem));
		}

		[Test]
		public void CanSetUnchangedDuplicate_False()
		{
			_writingSystem.ISO = "one";
			StoreUnderTest.Set(_writingSystem);
			Assert.IsFalse(StoreUnderTest.CanSet(StoreUnderTest.MakeDuplicate(_writingSystem)));
		}

		[Test]
		public void CanSetNull_False()
		{
			Assert.IsFalse(StoreUnderTest.CanSet(null));
		}

		[Test]
		public void MakeDuplicate_ReturnsNewObject()
		{
			StoreUnderTest.Set(_writingSystem);
			WritingSystemDefinition ws2 = StoreUnderTest.MakeDuplicate(_writingSystem);
			Assert.AreNotSame(_writingSystem, ws2);
		}

		[Test]
		public void MakeDuplicate_FieldsAreEqual()
		{
			WritingSystemDefinition ws1 = new WritingSystemDefinition("iso", "script", "region", "variant", "language",
																	  "abbrev", false);
			ws1.Keyboard = "keyboard";
			ws1.NativeName = "native name";
			ws1.DefaultFontName = "font";
			ws1.VersionDescription = "description of this version";
			ws1.VersionNumber = "1.0";
			StoreUnderTest.Set(ws1);
			WritingSystemDefinition ws2 = StoreUnderTest.MakeDuplicate(ws1);
			Assert.AreEqual(ws1.ISO, ws2.ISO);
			Assert.AreEqual(ws1.Script, ws2.Script);
			Assert.AreEqual(ws1.Region, ws2.Region);
			Assert.AreEqual(ws1.Variant, ws2.Variant);
			Assert.AreEqual(ws1.LanguageName, ws2.LanguageName);
			Assert.AreEqual(ws1.Abbreviation, ws2.Abbreviation);
			Assert.AreEqual(ws1.RightToLeftScript, ws2.RightToLeftScript);
			Assert.AreEqual(ws1.Keyboard, ws2.Keyboard);
			Assert.AreEqual(ws1.NativeName, ws2.NativeName);
			Assert.AreEqual(ws1.DefaultFontName, ws2.DefaultFontName);
			Assert.AreEqual(ws1.VersionDescription, ws2.VersionDescription);
			Assert.AreEqual(ws1.VersionNumber, ws2.VersionNumber);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void GetNull_Throws()
		{
			StoreUnderTest.Get(null);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Get_NotInStore_Throws()
		{
			StoreUnderTest.Get("I sure hope this isn't in the store.");
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NewerInNull_Throws()
		{
			StoreUnderTest.WritingSystemsNewerIn(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NewerInNullDefinition_Throws()
		{
			WritingSystemDefinition[] list = new WritingSystemDefinition[] {null};
			StoreUnderTest.WritingSystemsNewerIn(list);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SetNull_Throws()
		{
			StoreUnderTest.Set(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void MakeDuplicateNull_Throws()
		{
			StoreUnderTest.MakeDuplicate(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void RemoveNull_Throws()
		{
			StoreUnderTest.Remove(null);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Remove_NotInStore_Throws()
		{
			StoreUnderTest.Remove("This isn't in the store!");
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void GetNewStoreIDWhenSet_Null_Throws()
		{
			StoreUnderTest.GetNewStoreIDWhenSet(null);
		}

		[Test]
		public void GetNewStoreIDWhenSet_ReturnsSameStoreIDAsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("ws1");
			string newID = StoreUnderTest.GetNewStoreIDWhenSet(ws);
			StoreUnderTest.Set(ws);
			Assert.AreEqual(ws.StoreID, newID);
		}
	}
}