using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sunshine.Tests
{
    public class PollingCheck
    {
        const int TimeSlice = 50;
        long _timeout = 3000;
        bool _check;

        public PollingCheck()
        {
        }

        public PollingCheck(long timeout)
        {
            _timeout = timeout;
        }

        public virtual bool Check { set { _check = value; } }

        public void Run()
        {

            if (_check)
            {
                return;
            }

            long timeout = _timeout;
            while (timeout > 0)
            {
                try
                {
                    Task.Delay(TimeSlice, new System.Threading.CancellationToken());
                }
                catch (Exception)
                {
                    Assert.Fail("Unexpected InterruptedException");
                }

                if (_check)
                {
                    return;
                }

                timeout -= TimeSlice;
            }

            Assert.Fail("unexpected timeout");
        }

        public static void CheckValue(string message, long timeout, Func<bool> condition)
        {
            while (timeout > 0)
            {
                if (condition())
                {
                    return;
                }
        
                Task.Delay(TimeSlice);
                timeout -= TimeSlice;
            }
        
            Assert.Fail(message);
        }
    }
}

