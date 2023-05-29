using Newtonsoft.Json;
// ***********************************************************************
//    Assembly       : Ken Zhang
//    Created          : 2023-05-01
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeel.ca.SocketProject
{
    public class JSONHelper
    {
        /// <summary>
        /// *nstance object convert to JSON string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string EntityToJSON<T>(T x)
        {
            string result;

            try
            {
                result = JsonConvert.SerializeObject(x);
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;

        }

        /// <summary>
        /// JSON string convert to instance object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JSONToEntity<T>(string json)
        {
            T t;
            try
            {
                t = (T)JsonConvert.DeserializeObject(json, typeof(T));
            }
            catch (Exception)
            {
                t = default;
            }

            return t;              
        }

    }
}
