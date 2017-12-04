using System;
using System.Threading;
using Android.App;
using Android.OS;

namespace MobileEstimatorApp.Droid
{
    public class BackgroundJob
    {
        public static void StartBackgroundJob(
            MonitoredActivity activity, string title,
            string message, Action job, Handler handler)
        {
            var dialog = ProgressDialog.Show(activity, title, message, true, false);
            ThreadPool.QueueUserWorkItem(w => new BackgroundJob(activity, job, dialog, handler).Run());
        }

        private MonitoredActivity _activity;
        private readonly ProgressDialog _progressDialog;
        private readonly Action _job;
        private readonly Handler _handler;

        public BackgroundJob(MonitoredActivity activity, Action job,
                             ProgressDialog progressDialog, Handler handler)
        {
            _activity = activity;
            _progressDialog = progressDialog;
            _job = job;
            _handler = handler;

            activity.Destroying += (sender, e) =>
            {
                CleanUp();
                handler.RemoveCallbacks(CleanUp);
            };

            activity.Stopping += (sender, e) => progressDialog.Hide();
            activity.Starting += (sender, e) => progressDialog.Show();
        }


        public void Run()
        {
            try
            {
                _job();
            }
            finally
            {
                _handler.Post(CleanUp);
            }
        }

        private void CleanUp()
        {
            if (_progressDialog.Window != null)
            {
                _progressDialog.Dismiss();
            }
        }
    }
}