using PiServer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using IOFile = System.IO.File;

namespace PiServer.Controllers
{
    public partial class ClimateApiController
    {
        private readonly Func<JsonElement, Sensor> jsonSensor_Converter = (element) => new Sensor()
        {
            ID = element.GetProperty("id").GetInt32(),
            Name = element.GetProperty("name").GetString(),
            IPAddress = IPAddress.Parse(element.GetProperty("ip").GetString())
        };

        private readonly Func<string, int, TemperatureMeasurement> jsonClimateData_Converter = (line, sensorId) =>
        {
            string[] parts = line.Split(',');
            DateTime time = DateTime.Parse(parts[0]);
            float temperature = Convert.ToSingle(parts[1], CultureInfo.InvariantCulture);

            return new TemperatureMeasurement()
            {
                SensorId = sensorId,
                Time = time,
                Temperature = temperature
            };
        };        

        private IEnumerable<ClimateData> GetData()
        {
            IEnumerable<Sensor> sensors = GetSensors();

            foreach (var sensor in sensors)
            {
                string sensorDirectory = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}");
                yield return new ClimateData()
                {
                    sensor = sensor,
                    data = Directory.GetFiles(sensorDirectory, "*_*.csv").SelectMany<string, TemperatureMeasurement>(file =>
                    {
                        using StreamReader reader = new StreamReader(IOFile.OpenRead(file));
                        string[] lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                        return lines.Select(line => jsonClimateData_Converter(line, sensor.ID));
                    })
                };
            }
        }

        public IEnumerable<ClimateData> GetData(DateTime start, DateTime end = default)
        {
            if (end == default)
            {
                end = DateTime.Now;
            }

            string sensorsFile = Path.Combine(temperatureDirectory, "sensors.json");

            using FileStream fs = IOFile.OpenRead(sensorsFile);
            JsonDocument doc = JsonDocument.Parse(fs);

            IEnumerable<Sensor> sensors = from element in doc.RootElement.EnumerateArray()
                                          select jsonSensor_Converter(element);

            foreach (var sensor in sensors)
            {
                string sensorDirectory = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}");

                int year = start.Year;
                int month = start.Month;

                ClimateData data;
                data.sensor = sensor;
                List<TemperatureMeasurement> measures = new List<TemperatureMeasurement>();

                for (; year <= end.Year; year++)
                {
                    for (; month <= end.Month; month++)
                    {
                        string filename = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}", $"{year}_{month}.csv");
                        using StreamReader reader = new StreamReader(IOFile.OpenRead(filename));

                        string[] lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        measures.AddRange(lines
                            .Select(line => jsonClimateData_Converter(line, sensor.ID))
                            .Where(measure => measure.Time >= start && measure.Time <= end));
                    }
                }

                data.data = measures;
                yield return data;
            }
        }

        private IEnumerable<ClimateData> GetLatest(int sensorId = -1)
        {
            var sensors = sensorId == -1 ? GetSensors() : GetSensors().Where(sensor => sensor.ID == sensorId);

            ClimateData getLatestMeasure(Sensor sensor)
            {
                // TODO: Don't use all the sensor data
                var sensorData = GetSensorData(sensor.ID);
                DateTime maxTime = sensorData.data.Max(measure => measure.Time);

                return new ClimateData()
                {
                    sensor = sensor,
                    data = sensorData.data.Where(measure => measure.Time == maxTime)
                };
            }

            try
            {
                return sensors.Select(getLatestMeasure);
            }
            catch (InvalidOperationException)
            {
                return new List<ClimateData>();
            }
        }

        private ClimateData GetSensorData(int sensorId)
        {
            Sensor sensor = GetSensors().First(sensor => sensor.ID == sensorId);

            string sensorDirectory = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}");

            return new ClimateData()
            {
                sensor = sensor,
                data = Directory.GetFiles(sensorDirectory, "*_*.csv").SelectMany(file =>
                {
                    using StreamReader reader = new StreamReader(IOFile.OpenRead(file));
                    string[] lines = reader.ReadToEnd().Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    return lines.Select(line => jsonClimateData_Converter(line, sensor.ID));
                })
            };
        }

        private ClimateData GetSensorData(int sensorId, DateTime start, DateTime end = default)
        {
            if (end == default)
            {
                end = DateTime.Now;
            }

            string sensorsFile = Path.Combine(temperatureDirectory, "sensors.json");

            using FileStream fs = IOFile.OpenRead(sensorsFile);
            JsonDocument doc = JsonDocument.Parse(fs);

            Sensor sensor;
            try
            {
                sensor = GetSensors().First(sensor => sensor.ID == sensorId);
            }
            catch (InvalidOperationException)
            {
                return new ClimateData();
            }

            string sensorDirectory = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}");

            int year = start.Year;
            int month = start.Month;

            ClimateData data;
            data.sensor = sensor;
            List<TemperatureMeasurement> measures = new List<TemperatureMeasurement>();

            for (; year <= end.Year; year++)
            {
                for (; month <= end.Month; month++)
                {
                    string filename = Path.Combine(temperatureDirectory, $"sensor{sensor.ID}", $"{year}_{month}.csv");
                    using StreamReader reader = new StreamReader(IOFile.OpenRead(filename));

                    string[] lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    measures.AddRange(lines
                        .Select(line => jsonClimateData_Converter(line, sensor.ID))
                        .Where(measure => measure.Time >= start && measure.Time <= end));
                }
            }

            data.data = measures;
            return data;
        }

        private IEnumerable<Sensor> GetSensors()
        {
            string sensorsFile = Path.Combine(temperatureDirectory, "sensors.json");

            using FileStream fs = IOFile.OpenRead(sensorsFile);
            JsonDocument doc = JsonDocument.Parse(fs);

            return doc.RootElement.EnumerateArray().Select(jsonSensor_Converter);
        }

        private static string GetJson(params ClimateData[] data)
        {
            using MemoryStream ms = new MemoryStream();
            Utf8JsonWriter writer = new Utf8JsonWriter(ms);

            writer.WriteStartArray();

            foreach (var sensorData in data)
            {
                writer.WriteStartObject();
                writer.WriteNumber("sensor_id", sensorData.sensor.ID);
                writer.WriteString("sensor_name", sensorData.sensor.Name);

                writer.WriteStartArray("measurements");

                foreach (var measurement in sensorData.data)
                {
                    writer.WriteStartObject();
                    writer.WriteString("time", measurement.Time);
                    writer.WriteNumber("temperature", measurement.Temperature);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetJson(params Sensor[] sensors)
        {
            using MemoryStream ms = new MemoryStream();
            Utf8JsonWriter writer = new Utf8JsonWriter(ms);

            writer.WriteStartArray();

            foreach (var sensor in sensors)
            {
                writer.WriteStartObject();
                writer.WriteNumber("sensor_id", sensor.ID);
                writer.WriteString("sensor_name", sensor.Name);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
