using System.Xml.Linq;
using OrderFlow.Models;

namespace OrderFlow.Services
{
    public class XmlReportBuilder
    {
        public XDocument BuildReport(IEnumerable<Order> orders)
        {
            var now = DateTime.Now;

            var report =
                new XDocument(
                    new XElement("report",
                        new XAttribute("generated", now),

                        new XElement("summary",
                            new XAttribute("totalOrders", orders.Count()),
                            new XAttribute("totalRevenue", orders.Sum(o => o.TotalAmount))
                        ),

                        new XElement("byStatus",
                            orders.GroupBy(o => o.Status)
                                  .Select(g =>
                                      new XElement("status",
                                          new XAttribute("name", g.Key),
                                          new XAttribute("count", g.Count()),
                                          new XAttribute("revenue", g.Sum(o => o.TotalAmount))
                                      )
                                  )
                        ),

                        new XElement("byCustomer",
                            orders.GroupBy(o => o.Customer)
                                  .Select(g =>
                                      new XElement("customer",
                                          new XAttribute("id", g.Key.Id),
                                          new XAttribute("name", g.Key.FullName),
                                          new XAttribute("isVip", g.Key.IsVip),

                                          new XElement("orderCount", g.Count()),
                                          new XElement("totalSpent", g.Sum(o => o.TotalAmount)),

                                          new XElement("orders",
                                              g.Select(o =>
                                                  new XElement("orderRef",
                                                      new XAttribute("id", o.Id),
                                                      new XAttribute("total", o.TotalAmount)
                                                  )
                                              )
                                          )
                                      )
                                  )
                        )
                    )
                );

            return report;
        }

        public async Task SaveReportAsync(XDocument report, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await Task.Run(() => report.Save(path));
        }

        public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(string path, decimal threshold)
        {
            if (!File.Exists(path))
                return Enumerable.Empty<int>();

            var doc = await Task.Run(() => XDocument.Load(path));

            var ids = doc.Descendants("orderRef")
                         .Where(x => (decimal)x.Attribute("total") > threshold)
                         .Select(x => (int)x.Attribute("id"));

            return ids;
        }
    }
}