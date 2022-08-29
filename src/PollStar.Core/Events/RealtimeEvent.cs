using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PollStar.Core.Events;

public class RealtimeEvent
{
    public string EventName { get; set; }
    public object Payload { get; set; }

    public static string FromDto<TDto>(string eventName, TDto dto)
    {
        var eventObject = new RealtimeEvent
        {
            EventName = eventName,
            Payload = dto
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.None
        };

        return JsonConvert.SerializeObject(eventObject, settings);

    }
}