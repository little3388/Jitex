﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Jitex.Utils
{
    /// <summary>
    /// Utilities for type.
    /// </summary>
    internal static class TypeUtils
    {
        private static readonly IntPtr ObjectTypeHandle;

        static TypeUtils()
        {
            ObjectTypeHandle = typeof(object).TypeHandle.Value;
        }
        /// <summary>
        /// Get reference address from object.
        /// </summary>
        /// <param name="obj">Object to get address.</param>
        /// <returns>Reference address from object.</returns>
        public static unsafe IntPtr GetAddressFromObject(ref object obj)
        {
            TypedReference typeRef = __makeref(obj);
            return *(IntPtr*)(&typeRef);
        }

        /// <summary>
        /// Get object from a reference address.
        /// </summary>
        /// <param name="address">Reference address.</param>
        /// <returns>Object from reference.</returns>
        ///
        ///Created by: IllidanS4
        ///https://github.com/IllidanS4/SharpUtils/blob/a3b4da490537e361e6a5debc873c303023d83bf1/Unsafe/Pointer.cs#L58
        public static object GetObjectFromReference(IntPtr address)
        {
            TypedReference tr = default;
            Span<IntPtr> spanTr;

            unsafe
            {
                spanTr = new Span<IntPtr>(&tr, sizeof(TypedReference));
            }

            spanTr[0] = address;
            spanTr[1] = ObjectTypeHandle;

            return __refvalue(tr, object);
        }

        public static IntPtr GetValueAddress(IntPtr address, Type type)
        {
            Type elementType;

            if (type.IsByRef)
                elementType = type.GetElementType()!;
            else
                elementType = type;

            if (elementType.IsPrimitive)
            {
                if (type.IsByRef)
                    return address;

                address = Marshal.ReadIntPtr(address);
                return address;
            }

            if (elementType.IsValueType)
            {
                address = Marshal.ReadIntPtr(address);
                return address + IntPtr.Size;
            }

            //if (elementType.IsPrimitive && type.IsByRef)
            //    return address;

            if (type.IsByRef)
                return address;

            return Marshal.ReadIntPtr(address);
        }
    }
}