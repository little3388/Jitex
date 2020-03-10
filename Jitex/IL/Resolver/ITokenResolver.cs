﻿using System;
using System.Reflection;

namespace Jitex.IL.Resolver
{
    /// <summary>
    /// Token resolver.
    /// </summary>
    /// <remarks>
    /// Created by jnm2
    /// https://stackoverflow.com/a/35711376
    /// </remarks>
    public interface ITokenResolver
    {
        MemberInfo ResolveMember(int token);
        Type ResolveType(int token);
        FieldInfo ResolveField(int token);
        MethodBase ResolveMethod(int token);
        byte[] ResolveSignature(int token);
        string ResolveString(int token);
    }
}