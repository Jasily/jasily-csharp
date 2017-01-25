﻿using Jasily.Core;
using JetBrains.Annotations;

namespace System.Collections.Generic
{
    public static class ArrayExtensions
    {
        public static void ForEach<T>([NotNull] this T[] source, [NotNull] Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            for (var i = 0; i < source.Length; i++)
            {
                action(source[i]);
            }
        }

        public static void ForEach<T>([NotNull] this T[] source, [NotNull] Action<int, T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            for (var i = 0; i < source.Length; i++)
            {
                action(i, source[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void CheckRange<T>([NotNull] this T[] array, int offset)
            => ToSegment(array, offset);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void CheckRange<T>([NotNull] this T[] array, int offset, int count)
            => ToSegment(array, offset, count);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ArraySegment<T> ToSegment<T>([NotNull] this T[] array)
            => new ArraySegment<T>(array);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static ArraySegment<T> ToSegment<T>([NotNull] this T[] array, int offset)
            => new ArraySegment<T>(array, offset, array.Length - offset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static ArraySegment<T> ToSegment<T>([NotNull] this T[] array, int offset, int count)
            => new ArraySegment<T>(array, offset, count);

        public static TDest[] ToArray<TSource, TDest>([NotNull] this TSource[] array) where TSource : TDest
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.Length == 0) return Empty<TDest>.Array;
            var newArray = new TDest[array.Length];
            Array.Copy(array, newArray, array.Length);
            return newArray;
        }

        public static TDest[] CastToArray<TSource, TDest>([NotNull] this TSource[] array)
            where TSource : class 
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.Length == 0) return Empty<TDest>.Array;
            var newArray = new TDest[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = (TDest)(object)array[i];
            }
            return newArray;
        }

        public static TOutput[] ConvertToArray<TInput, TOutput>([NotNull] this TInput[] array,
            [NotNull] Func<TInput, TOutput> converter)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (array.Length == 0) return Empty<TOutput>.Array;
            var newArray = new TOutput[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = converter(array[i]);
            }
            return newArray;
        }
    }
}