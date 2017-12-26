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
                _Control.BeginInvoke(new Action(() =>//.SafeBeginInvoke(() =>
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
                }));
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
        //public void Do<T>(
        //    Func<T> doWork,
        //    Action<T, Exception> result,
        //    bool showWaitText = true,
        //    string waitText = null,
        //    bool callEndWaitAfterResult = true,
        //    bool logDbgException = true)
        //{
        //    Action<T, Exception> resultProc = (t, ex) =>
        //    {
        //        if (ex != null && logDbgException)
        //            _Logger?.LogDbgException(ex);

        //        _Control.SafeBeginInvoke(() =>
        //        {
        //            if (callEndWaitAfterResult)
        //            {
        //                try
        //                {
        //                    result(t, ex);
        //                }
        //                finally
        //                {
        //                    if (showWaitText && _EndWait != null)
        //                        _EndWait();
        //                }
        //            }
        //            else
        //            {
        //                if (showWaitText && _EndWait != null)
        //                    _EndWait();

        //                result(t, ex);
        //            }
        //        });
        //    };

        //    try
        //    {
        //        if (showWaitText && _BeginWait != null)
        //            _BeginWait(waitText);

        //        _SeqPerformer.AddTask(() =>
        //        {
        //            Utilities.CultureHelper.ResetCurrentThreadLanguage();
        //            try
        //            {
        //                var t = doWork();
        //                resultProc(t, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                resultProc(default(T), ex);
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        resultProc(default(T), ex);
        //    }
        //}
    }
}
