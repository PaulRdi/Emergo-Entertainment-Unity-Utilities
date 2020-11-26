using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    /// <summary>
    /// Wrapper class for messages.
    /// </summary>
    public class Message
    {
        public string type;
        public object[] data;

        public Message(string type_, params object[] data_)
        {
            type = type_;
            data = data_;
        }

        /// <summary>
        /// Returns the first instance of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool GetData<T> (out T data) where T : class
        {
            data = default;
            foreach(object obj in this.data)
            {
                if (!(obj is T))
                    continue;

                data = obj as T;
                return true;
            }
            return false;
        }
    }
}
