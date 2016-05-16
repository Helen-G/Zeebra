using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using LukeSkywalker.IPNetwork;
using ServiceStack.Common.Extensions;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class IpRegulationServiceBase : MarshalByRefObject, IApplicationService
    {
        protected readonly ISecurityRepository _repository;
        protected readonly ISecurityProvider _securityProvider;
        

        private readonly string[] _localhostAliases = { "127.0.0.1", "::1", "0:0:0:0:0:0:0:1" }; 

        public IpRegulationServiceBase(
            ISecurityRepository repository, 
            ISecurityProvider securityProvider
            )
        {
            _repository = repository;
            _securityProvider = securityProvider;
        }

        public bool IsLocalhost(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            return IPAddress.IsLoopback(address);
        }

        private class RangeBounds
        {
            public RangeBounds(IPAddress lowerBound, IPAddress upperBound)
            {
                LowerBound = lowerBound;
                UpperBound = upperBound;
            }

            public IPAddress LowerBound { get; private set; }
            public IPAddress UpperBound { get; private set; }
        }

        

        private static RangeBounds GetBounds(string range)
        {
            RangeBounds result;
            if (IsIpV4(range))
            {
                if (range.Contains("-"))
                {
                    var lowerSegments = new List<string>();
                    lowerSegments.AddRange(
                        range.Split('.')
                            .Select(segment => segment.Split('-'))
                            .Select(segmentRange => segmentRange.First()));

                    var upperSegments = new List<string>();
                    upperSegments.AddRange(
                        range.Split('.')
                            .Select(segment => segment.Split('-'))
                            .Select(segmentRange => segmentRange.Last()));

                    result = new RangeBounds(IPAddress.Parse(string.Join(".", lowerSegments)),
                        IPAddress.Parse(string.Join(".", upperSegments)));
                }
                else if (range.Contains("/"))
                {
                    var network = IPNetwork.Parse(range);
                    result = new RangeBounds(network.Network, network.Broadcast);
                }
                else
                {
                    var address = IPAddress.Parse(range);
                    result = new RangeBounds(address, address);
                }
            }
            else
            {
                if (range.Contains("/"))
                {
                    //Split the string in parts for address and prefix
                    var strAddress = range.Split('/').First();
                    var strPrefix = range.Split('/').Last();

                    var iPrefix = Int32.Parse(strPrefix);
                    var ipAddress = IPAddress.Parse(strAddress);

                    //Convert the prefix length to a valid SubnetMask

                    const int iMaskLength = 128;

                    var btArray = new BitArray(iMaskLength);
                    for (var iC1 = 0; iC1 < iMaskLength; iC1++)
                    {
                        //Index calculation is a bit strange, since you have to make your mind about byte order.
                        var iIndex = (iMaskLength - iC1 - 1) / 8 * 8 + (iC1 % 8);

                        btArray.Set(iIndex, iC1 >= (iMaskLength - iPrefix));
                    }

                    var bMaskData = new byte[iMaskLength / 8];

                    btArray.CopyTo(bMaskData, 0);

                    //Create subnetmask
                    var subnetMask = new IPAddress(bMaskData);

                    //Get the IP range
                    var ipaStart = IPAddress.Parse(strAddress);
                    var ipaEnd = ipAddress.GetBroadcastAddress(subnetMask);

                    result = new RangeBounds(ipaStart, ipaEnd);
                }
                else
                {
                    var address = IPAddress.Parse(range);
                    result = new RangeBounds(address, address);
                }
            }

            return result;
        }

        private static bool IsBoundsIntersects(RangeBounds firstRange, RangeBounds secondRange)
        {
            if (firstRange.LowerBound.IsGreaterThen(firstRange.UpperBound))
            {
                throw new RegoException("IP address range lower bound can't be greater then upper");
            }

            if (secondRange.LowerBound.IsGreaterThen(secondRange.UpperBound))
            {
                throw new RegoException("IP address range lower bound can't be greater then upper");
            }

            return firstRange.UpperBound.IsGreaterOrEquals(secondRange.LowerBound) &&
                   secondRange.UpperBound.IsGreaterOrEquals(firstRange.LowerBound);
        }

        private static bool IsIpV4(string range)
        {
            return range.Contains(".");
        }

        private static bool IsIpV6(string range)
        {
            return range.Contains(":");
        }

        private static bool IsSameProtocolVersion(string firstRange, string secondRange)
        {
            return (IsIpV4(firstRange) && IsIpV4(secondRange)) 
                || (IsIpV6(firstRange) && IsIpV6(secondRange));
        }

        public bool IsRangesIntersects(string firstRange, string secondRange)
        {
            if(!IsSameProtocolVersion(firstRange, secondRange))
                return false;

            var firstBounds = GetBounds(firstRange);
            var secondBounds = GetBounds(secondRange);

            return IsBoundsIntersects(firstBounds, secondBounds);
        }
    }

    public static class IpAddressEx
    {
        public static bool IsGreaterThen(this IPAddress firstAddress, IPAddress secondAddress)
        {
            var firstBytes = firstAddress.GetAddressBytes();
            var secondBytes = secondAddress.GetAddressBytes();

            for (var i = 0; i < firstBytes.Length; i++)
            {
                if (firstBytes[i] > secondBytes[i])
                {
                    return true;
                }
                
                if (secondBytes[i] > firstBytes[i])
                {
                    return false;
                }
            }

            return false;
        }

        public static bool IsGreaterOrEquals(this IPAddress firstAddress, IPAddress secondAddress)
        {
            var firstBytes = firstAddress.GetAddressBytes();
            var secondBytes = secondAddress.GetAddressBytes();

            for (var i = 0; i < firstBytes.Length; i++)
            {
                if (firstBytes[i] > secondBytes[i])
                {
                    return true;
                }

                if (secondBytes[i] > firstBytes[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
