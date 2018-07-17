﻿using System;

namespace MkCoreLibrary.PlatformManagement.Infrastructure
{
    public interface IKeyValuePairStorage : IDisposable {
        void Write(string key, object obj);
        object Read<T>(string key);
        void Clear();
    }
    public interface IKeyValuePairStorage <T>: IKeyValuePairStorage
    {
        T PlatformController { get; set; }
    }

    public class ConstTypes
    {
        public static  Type Boolean = typeof(bool);
        public static Type Integer = typeof(int);
        public static Type String = typeof(string);
        public static Type Single = typeof(float);
    }
}