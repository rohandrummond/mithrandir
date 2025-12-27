using System.Text.Json.Serialization;

namespace mithrandir.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
  Active,
  Revoked
}