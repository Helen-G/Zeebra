using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LukeSkywalker.IPNetwork;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public static class IpRegulationRangeHelper
    {
        private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }

        private static IEnumerable<string> ExpandIpAddressRangeDash(string address)
        {
            var segments = address.Split('.');

            var expanded = new List<List<string>>();

            foreach (var segment in segments)
            {
                var segmentRange = new List<string>();
                var range = segment.Split('-');
                switch (range.Count())
                {
                    case 2:
                        for (var i = Convert.ToInt32(range.First()); i <= Convert.ToInt32(range.Last()); i++)
                        {
                            segmentRange.Add(i.ToString(CultureInfo.InvariantCulture));
                        }
                        break;
                    case 1:
                        segmentRange.Add(segment);
                        break;
                }

                expanded.Add(segmentRange);
            }

            var product = CartesianProduct(expanded);

            return product.Select(segs => string.Join(".", segs));
        }

        public static IEnumerable<string> ExpandIpAddressRangeSubnet(string address)
        {
            var addressSubnet = address.Split('/');

            if (addressSubnet.Count() == 1)
            {
                return new[] { address };
            }

            var network = IPNetwork.Parse(address);

            return IPNetwork.ListIPAddress(network).Select(ip => ip.ToString());
        }

        public static IEnumerable<string> ExpandIpAddressRange(string address)
        {
            return address.Contains("-")
                ? ExpandIpAddressRangeDash(address)
                : ExpandIpAddressRangeSubnet(address);
        }
    }
}