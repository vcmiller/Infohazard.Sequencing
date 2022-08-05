// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Infohazard.Sequencing {
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