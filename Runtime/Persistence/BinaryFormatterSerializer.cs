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

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Infohazard.Sequencing {
    public class BinaryFormatterSerializer : Serializer {
        private BinaryFormatter _formatter = new BinaryFormatter();
        
        public override void Write(Stream stream, object data) {
            _formatter.Serialize(stream, data);
        }

        public override bool Read<T>(Stream stream, out T data) {
            object result = _formatter.Deserialize(stream);
            if (result is T resultT) {
                data = resultT;
                return true;
            } else {
                data = default;
                return false;
            }
        }

        public override object ObjectToIntermediate(object data) {
            return data;
        }

        public override bool IntermediateToObject<T>(object intermediate, out T data) {
            if (intermediate is T t) {
                data = t;
                return true;
            } else {
                data = default;
                return false;
            }
        }
    }
}