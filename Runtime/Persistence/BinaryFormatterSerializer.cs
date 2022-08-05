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