using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Base
{
    /// <summary>
    /// Use this base class for your unit tests. 
    /// Inherits from SingleProcessTestsBase as unit tests should never spawn multiple processes.
    /// </summary>
    [Category("Unit")]
    public abstract class UnitTestsBase : SingleProcessTestsBase { }
}