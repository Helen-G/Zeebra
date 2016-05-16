using System.Web.Http;
using AFT.RegoV2.Infrastructure.Attributes;
using Log4Net.Async;

namespace AFT.RegoV2.GameApi.Controllers
{
    [ForceJsonFormatter]
    public class RootController : ApiController
    {
        [Route("")]
        public string[] Get()
        {
            return new[] {"REGO 2 Game API"};
        }
    }
    // read Notes in https://github.com/cjbhaines/Log4Net.Async as to why this exists
    internal class DummyRingBuffer : RingBuffer<int> { public DummyRingBuffer() : base(1) {} }
}