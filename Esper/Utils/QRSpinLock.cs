using System;
using System.Threading;

namespace Utilities
{
    /// <summary>
    /// Makes using a System.Threading.SpinLock a little bit more convenient.
    /// </summary>
    public class QRSpinLock
    {
        /// <summary>
        /// Performs specified code under the lock
        /// </summary>
        public void DoLocked(
            Action code
        )
        {
            bool bLockTaken = false;
            try
            {
                m_SpinLock.Enter(ref bLockTaken);
                if (!bLockTaken)
                {
                    throw new ApplicationException("Couldn`t take a SpinLock");
                }
                code();
            }
            finally
            {
                if (bLockTaken)
                {
                    m_SpinLock.Exit();
                }
            }
        }

        SpinLock m_SpinLock = new SpinLock();
    }
}
