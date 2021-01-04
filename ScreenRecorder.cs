using NLog;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotepadTest
{
    public class ScreenRecorder
    {
        private Recorder recorder;
        private bool isRecording;
        private bool isCompleted;
        private string videoPath;
        private ScreenRecordStatus status;
        private string error = string.Empty;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ScreenRecorder(string videopath)
        {
            recorder = Recorder.CreateRecorder();
            recorder.OnRecordingComplete += Recorder_OnRecordingComplete;
            recorder.OnRecordingFailed += Recorder_OnRecordingFailed;
            recorder.OnStatusChanged += Recorder_OnStatusChanged;
            this.videoPath = videopath;
        }

        /// <summary>
        /// Create screen record object
        /// </summary>
        /// <param name="videopath">Video file saved path</param>
        /// <returns>ScreenRecorder object</returns>
        public static ScreenRecorder CreateRecorder(string videopath)
        {
            if (File.Exists(videopath))
            {
                File.Delete(videopath);
            }

            ScreenRecorder screenRecorder = new ScreenRecorder(videopath);
            return screenRecorder;
        }

        /// <summary>
        /// Status of recording
        /// </summary>
        public ScreenRecordStatus Status
        {
            get
            {
                return this.status;
            }
        }
        /// <summary>
        /// Error of recording
        /// </summary>
        public string Error
        {
            get
            {
                return this.error;
            }
        }

        /// <summary>
        /// Is completed
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        /// <summary>
        /// Start recording
        /// </summary>
        public void StartRecording()
        {
            recorder.Record(this.videoPath);
            int time = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                time++;
                if (time >= 180)
                {
                    break;
                }
            }
            while (this.Status != ScreenRecordStatus.Recording);
        }

        public void StartRecording(ScreenRecorder screenRecorder, int tryCount = 5)
        {
            try
            {
                int tryIndex = 1;
                do
                {
                    _logger.Info($"Try Count {tryIndex}: The status of recorder is " + screenRecorder.Status);
                    if (screenRecorder.Status == ScreenRecordStatus.Fail ||
                        (screenRecorder.Status == ScreenRecordStatus.Idle && tryIndex > 1))
                    {
                        _logger.Info($"Try Count {tryIndex}: Re-recording");
                        screenRecorder = ScreenRecorder.CreateRecorder(screenRecorder.videoPath);
                        screenRecorder.StartRecording();
                    }
                    else if (screenRecorder.Status != ScreenRecordStatus.Recording)
                    {
                        _logger.Info($"Try Count {tryIndex}: Start recording");
                        screenRecorder.StartRecording();
                    }

                    _logger.Info($"Try Count {tryIndex}: Now recorder status is " + recorder.Status);
                    tryIndex++;
                } while (tryIndex <= tryCount && screenRecorder.Status != ScreenRecordStatus.Recording);
            }
            catch (Exception ex)
            {
                _logger.Info("Exception occurs when start recording.");
                _logger.Info(ex.Message);
            }
        }


        /// <summary>
        /// End recording
        /// </summary>
        public void EndRecording()
        {
            recorder.Stop();
            // Waiting for 10 minutes max
            SpinWait.SpinUntil(() => isRecording == false, 600000);
        }

       

        private void Recorder_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            isRecording = false;
            isCompleted = true;
        }

        private void Recorder_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            this.error = e.Error;
            this.status = ScreenRecordStatus.Fail;
            isRecording = false;
        }

        private void Recorder_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            switch (e.Status)
            {
                case RecorderStatus.Idle:
                    this.status = ScreenRecordStatus.Idle;
                    break;
                case RecorderStatus.Recording:
                    isRecording = true;
                    this.status = ScreenRecordStatus.Recording;
                    break;
                case RecorderStatus.Paused:
                    this.status = ScreenRecordStatus.Paused;
                    break;
                case RecorderStatus.Finishing:
                    this.status = ScreenRecordStatus.Finishing;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Enum for recording status
        /// </summary>
        public enum ScreenRecordStatus : int
        {
            Idle = 0,
            Recording = 1,
            Paused = 2,
            Finishing = 3,
            Fail = -1
        }
    }
}

