using System;
using System.Windows.Forms;

namespace Utilities
{
    public sealed class BackgroundWorker2
    {
        readonly SequentialPerformer2 _SeqPerformer;
        readonly Control _Control;
        readonly Action<string> _BeginWait;
        readonly Action _EndWait;

        public BackgroundWorker2(Control control)
            : this(control, null, null)
        {
        }

        public BackgroundWorker2(Control control, Action<string> beginWait, Action endWait)
        {
            _SeqPerformer = new SequentialPerformer2();
            _Control = control;
            _BeginWait = beginWait;
            _EndWait = endWait;
        }

        public void Do(
            Action doWork, 
            Action<Exception> result, 
            bool showWaitText = true, 
            string waitText = null)
        {
            Func<object> f = () =>
            {
                doWork();
                return null;
            };
            this.Do<object>(f, (o, e) => result(e), showWaitText, waitText);
        }

        public void Do<T>(
            Func<T> doWork,
            Action<T, Exception> result,
            bool showWaitText = true,
            string waitText = null)
        {
            Action<T, Exception> resultProc = (t, ex) =>
            {
                SafeBeginInvoke(() =>
                {
                    try
                    {
                        result(t, ex);
                    }
                    finally
                    {
                        if (showWaitText && _EndWait != null)
                            _EndWait();
                    }
                });
            };

            try
            {
                if (showWaitText && _BeginWait != null)
                    _BeginWait(waitText);

                _SeqPerformer.AddTask(() =>
                {
                    //Utilities.CultureHelper.ResetCurrentThreadLanguage();
                    try
                    {
                        var t = doWork();
                        resultProc(t, null);
                    }
                    catch (Exception ex)
                    {
                        resultProc(default(T), ex);
                    }
                });
            }
            catch (Exception ex)
            {
                resultProc(default(T), ex);
            }
        }

        private bool SafeBeginInvoke(Action action)
        {
            var c = _Control;
            if (c == null || !c.IsHandleCreated || c.IsDisposed) return false;
            try
            {
                if (c.InvokeRequired)
                {
                    c.BeginInvoke(action);
                }
                else
                {
                    action();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
