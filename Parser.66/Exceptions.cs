using System;

namespace Parser._66
{
    internal class EmptyResponseException : Exception
    {
        public EmptyResponseException(string uriString) 
            : base(string.Format("Cannot get response from \"{0}\"", uriString))
        {
        }
    }
    internal class SelectException : Exception
    {
        public SelectException(string path)
            : base(string.Format("Cannot select \"{0}\"", path))
        {
        }
    }
}