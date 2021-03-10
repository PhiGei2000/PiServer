using Microsoft.AspNetCore.Mvc;
using PiServer.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace PiServer.Controllers
{
    [Route("api/climate")]
    [ApiController]
    public partial class ClimateApiController : ControllerBase
    {
#if DEBUG
        private readonly string temperatureDirectory = "temperatureData";
#else
        private readonly string temperatureDirectory = "/media/pi/HDD/temperatureData";
#endif

        [HttpGet]
        public ContentResult Index()
        {
            var data = GetData(DateTime.Today).ToArray();

            string content = GetJson(data);
            return Content(content, MediaTypeNames.Application.Json);
        }

        [HttpGet("sensors")]
        public FileStreamResult Sensors()
        {
            string sensorsFile = Path.Combine(temperatureDirectory, "sensors.json");
            FileStream fs = System.IO.File.OpenRead(sensorsFile);

            return File(fs, MediaTypeNames.Application.Json);
        }

        [HttpGet("sensors/{id:int:min(0)}")]
        public ContentResult Sensors(int id)
        {
            string sensorsFile = Path.Combine(temperatureDirectory, "sensors.json");

            using FileStream fs = System.IO.File.OpenRead(sensorsFile);
            JsonDocument doc = JsonDocument.Parse(fs);

            var sensor = from element in doc.RootElement.EnumerateArray()
                         where element.GetProperty("id").GetInt32() == id
                         select jsonSensor_Converter(element);

            return Content(GetJson(sensor.ToArray()), MediaTypeNames.Application.Json);
        }

        [HttpGet("latest")]
        public ContentResult Latest()
        {
            var data = GetLatest().ToArray();

            return Content(GetJson(data), MediaTypeNames.Application.Json);
        }

        [HttpGet("latest/{id:int:min(0)}")]
        public ContentResult Latest(int sensorId)
        {
            var data = GetLatest(sensorId).ToArray();

            return Content(GetJson(data), MediaTypeNames.Application.Json);
        }
    }
}
