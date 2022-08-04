using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class QuaternionConverter : JsonConverter<Quaternion> {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue,
                                            JsonSerializer serializer) {
            JObject obj = JObject.Load(reader);
            return new Quaternion(obj["x"]?.Value<float>() ?? 0, obj["y"]?.Value<float>() ?? 0,
                                  obj["z"]?.Value<float>() ?? 0, obj["w"]?.Value<float>() ?? 1);
        }
    }
}