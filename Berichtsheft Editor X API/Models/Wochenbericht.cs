using System.Text.Json.Serialization;

namespace Berichtsheft_Editor_X_API.Models
{
    public class Wochenbericht
    {
        [JsonPropertyName("kalenderWoche")]
        public string KalenderWoche { get; set; }
        [JsonPropertyName("jahr")]
        public string Jahr { get; set; }
        [JsonPropertyName("bericht")]
        public string Bericht { get; set; }
    }
}
