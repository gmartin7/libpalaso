﻿using NUnit.Framework;
using SIL.Threading;

namespace SIL.Tests.Threading
{
	[TestFixture]
	public class GlobalMutexTests
	{
		[Test]
		public void Initialize_CreatedNew_ReturnsTrue()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Unlink();
				Assert.That(mutex.Initialize(), Is.True);
			}
		}

		[Test]
		public void Initialize_Existing_ReturnsFalse()
		{
			using (var mutex1 = new GlobalMutex("test"))
			{
				mutex1.Initialize();
				using (var mutex2 = new GlobalMutex("test"))
					Assert.That(mutex2.Initialize(), Is.False);
			}
		}

		[Test]
		public void InitializeAndLock_CreatedNew_ReturnsTrue()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Unlink();
				bool createdNew;
				using (mutex.InitializeAndLock(out createdNew)) {}
				Assert.That(createdNew, Is.True);
			}
		}

		[Test]
		public void InitializeAndLock_Existing_ReturnsFalse()
		{
			using (var mutex1 = new GlobalMutex("test"))
			{
				mutex1.Initialize();
				using (var mutex2 = new GlobalMutex("test"))
				{
					bool createdNew;
					using (mutex2.InitializeAndLock(out createdNew)) {}
					Assert.That(createdNew, Is.False);
				}
			}
		}

		[Test]
		public void InitializeAndLock_Reentrancy_DoesNotBlock()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				using (mutex.InitializeAndLock())
				{
					using (mutex.Lock()) {}
				}
			}
		}

		[Test]
		public void Lock_Reentrancy_DoesNotBlock()
		{
			using (var mutex = new GlobalMutex("test"))
			{
				mutex.Initialize();
				using (mutex.Lock())
				{
					using (mutex.Lock()) {}
				}
			}
		}
	}
}
