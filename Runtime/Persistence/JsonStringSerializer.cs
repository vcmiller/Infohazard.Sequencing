using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class JsonStringSerializer : Serializer {
        [SerializeField] private Formatting _formatting;
        
        private JsonSerializer _serializer;
        
        private void Awake() {
            _serializer = new JsonSerializer {
                Formatting = _formatting,
                ContractResolver = new DefaultContractResolver {
                    IgnoreSerializableAttribute = false,
                },
                Converters = {
                    new Vector3Converter(),
                    new QuaternionConverter(),
                },
            };
        }

        public override void Write(Stream stream, object data) {
            using StreamWriter sw = new StreamWriter(stream);
            using JsonWriter writer = new JsonTextWriter(sw);
            
            _serializer.Serialize(writer, data);
        }

        public override bool Read<T>(Stream stream, out T data) {
            using StreamReader sr = new StreamReader(stream);
            using JsonReader reader = new JsonTextReader(sr);

            try {
                data = _serializer.Deserialize<T>(reader);
                return true;
            } catch (Exception ex) {
                Debug.LogException(ex);
                data = default;
                return false;
            }
        }

        public override object ObjectToIntermediate(object data) {
            return data;
        }

        public override bool IntermediateToObject<T>(object intermediate, out T data) {
            switch (intermediate) {
                case T t:
                    data = t;
                    return true;
                case JContainer container:
                    data = container.ToObject<T>(_serializer);
                    return true;
                default:
                    data = default;
                    return false;
            }
        }
    }
}