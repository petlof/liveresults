﻿
namespace LiveResults.Model
{
    public interface IExternalSystemResultParser
    {
        void Start();
        void Stop();
        event ResultDelegate OnResult;
        event LogMessageDelegate OnLogMessage;
        event RadioControlDelegate OnRadioControl;
    }
}
