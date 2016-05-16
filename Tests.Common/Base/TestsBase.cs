using System;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Base
{
    [TestFixture]
    public abstract class TestsBase
    {
        [TestFixtureSetUp]
        public void BeforeAllSetUp()
        {
            try
            {
                BeforeAll();
            }
            catch (Exception ex)
            {
                BeforeAllFailed(ex);
                throw;
            }
        }

        public virtual void BeforeAll()
        {
        }

        public virtual void BeforeAllFailed(Exception ex) 
        {
        }

        [SetUp]
        public void BeforeEachSetUp()
        {
            try
            {
                BeforeEach();
            }
            catch(Exception ex)
            {
                BeforeEachFailed(ex);
                throw;
            }
        }

        public virtual void BeforeEach()
        {
        }

        public virtual void BeforeEachFailed(Exception ex) 
        {
        }

        [TearDown]
        public virtual void AfterEach()
        {
            if (TestContext.CurrentContext.Result.Status == TestStatus.Failed)
            {
                AfterEachFailed();
            }
        }

        public virtual void AfterEachFailed()
        {
            
        }

        [TestFixtureTearDown]
        public virtual void AfterAll()
        {
        }
    }
}