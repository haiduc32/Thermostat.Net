using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    public class TelemetryModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public DateTime Timestamp { get; set; }

        public List<Zone> Zones { get; set; }
        
        public decimal? OutsideTemperature { get; set; }

        public List<bool> Channels { get; set; }
    }

    public static class TelemetryModelExtensions
    {
        /// <summary>
        /// Merge multiple telemetries that are in a short interval of time
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TelemetryModel Merge(this IEnumerable<TelemetryModel> data)
        {
            data = data.OrderBy(x => x.Timestamp);
            //for performance reasons we're going to merge everything down into the first telemetry object in the list
            foreach (Zone z in data.First().Zones)
            {
                z.Temperature = data.Average(x => x.Zones.SingleOrDefault(y => y.Name == z.Name)?.Temperature);
            }

            data.First().OutsideTemperature = data.Average(x => x.OutsideTemperature);

            if (data.First().Channels != null)
            {
                //if at any moment the heating was ON, make it ON on the whole data set
                for (int i = 0; i < data.First().Channels.Count; i++)
                {
                    //if in the first telemetry object, the heating is ON, skip the next ones
                    if (data.First().Channels[i]) continue;

                    foreach (TelemetryModel t in data)
                    {
                        if (t.Channels.Count > i)
                        {
                            if (t.Channels[i])
                            {
                                data.First().Channels[i] = true;
                                continue;
                            }
                        }
                    }
                }
            }

            return data.First();
        }
    }
}
