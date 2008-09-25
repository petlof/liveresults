using System;
using System.Collections.Generic;
using System.Text;

namespace WOCEmmaClient
{
    public interface IExternalSystemResultParser
    {
        void Start();
        void Stop();
        event ResultDelegate OnResult;
        event LogMessageDelegate OnLogMessage;
    }
}
