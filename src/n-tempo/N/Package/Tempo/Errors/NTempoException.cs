using System;

namespace N.Package.Tempo.Errors
{
    public class NTempoException : Exception
    {
        public NTempoError Error { get; set; }
        
        public NTempoException(NTempoError error) : base(error.ToString())
        {
            Error = error;
        }
    }
}