using MediatR;
using System;

namespace Entities
{
    public class VoltageSummaryRetry : VoltageSummary
    {
        private int _retryCount;

        public int GetRetryCount()
        {
            return _retryCount;
        }

        public void Increment()
        {
            _retryCount++;
        }
    }
}


