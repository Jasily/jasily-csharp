﻿using System;
using System.IO;
using System.Text;
using Jasily.Interfaces.Data;
using JetBrains.Annotations;

namespace Jasily.Interfaces.Runtime.Serialization.Json
{
    public static class JsonSerializerExtensions
    {
        static JsonSerializerExtensions()
        {
            if (JasilySettings<IJsonSerializerProvider>.Value == null)
            {
                JasilySettings<IJsonSerializerProvider>.Value = new JsonSerializerProvider();
            }
        }

        public static T JsonToObject<T>(this Stream stream)
            => JasilySettings<IJsonSerializerProvider>.GetOrThrow().DeserializeToObject<T>(stream);

        public static T JsonToObject<T>([NotNull] this byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            using (var ms = new MemoryStream(bytes))
            {
                return ms.JsonToObject<T>();
            }
        }

        public static T JsonToObject<T>([NotNull] this string s, [NotNull] Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return encoding.GetBytes(s).JsonToObject<T>();
        }

        public static T JsonToObject<T>([NotNull] this string s) => JsonToObject<T>(s, Encoding.UTF8);

        public static byte[] ObjectToJson([NotNull] this object obj)
            => JasilySettings<IJsonSerializerProvider>.GetOrThrow().SerializeToBytes(obj);
    }
}