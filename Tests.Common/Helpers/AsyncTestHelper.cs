using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public static class AsyncTestHelper
    {
        public static async Task<TException> ThrowsAsync<TException>(Func<Task> function) where TException : Exception
        {
            var exceptionThrown = false;

            try
            {
                await function();
            }
            catch (TException exception)
            {
                Assert.That(typeof(TException), Is.EqualTo(typeof(TException)));
                return exception;
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Assert.That(ex.GetType(), Is.EqualTo(typeof(TException)));
            }

            Assert.That(exceptionThrown, Is.True);
            return null;
        }
    }
}
