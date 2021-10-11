using System;

namespace P3D.Legacy.Common.Data.P3DData
{
    public abstract record P3DData
    {
        protected P3DData(in ReadOnlySpan<char> data) { }

        public abstract string ToP3DString();
    }
}