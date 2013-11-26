using System;

namespace Parser._66
{
    internal class SelectException : Exception
    {
        public SelectException(string path) : base(string.Format("Cannot select \"{0}\"", path))
        {
        }
    }
}