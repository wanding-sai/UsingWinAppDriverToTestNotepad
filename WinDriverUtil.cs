using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadTest
{
    public class WinDriverUtil
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static WindowsDriver<WindowsElement> _desktopSession = null;
        /// <summary>
        /// Url to communicate with Windows Application Driver
        /// </summary>
        public const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";

        private static uint _remainCount = 0;
        private const uint _refreshCount = 7;
        private static readonly TimeSpan _commandTimeOut = new TimeSpan(0, 1, 30);
        /// <summary>
        /// Obtain current desktop session
        /// </summary>
        public static WindowsDriver<WindowsElement> DesktopSession
        {
            get
            {
                if (_remainCount == 0)
                {
                    // Dispose the old session
                    _desktopSession?.Dispose();
                    _remainCount = _refreshCount;
                    var desktopDesiredCapabilities = new DesiredCapabilities();
                    desktopDesiredCapabilities.SetCapability("app", "Root");
                    // Assign new session
                    _desktopSession = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl),
                        desktopDesiredCapabilities, _commandTimeOut);
                }

                --_remainCount;
                return _desktopSession;
            }
        }
        /// <summary>
        /// Shot the session part screen and save it with full file name
        /// </summary>
        /// <param name="session">Session you want to shot</param>
        /// <param name="fullFileName">File name for image</param>
        public static void ShotScreen(WindowsDriver<WindowsElement> session, string fullFileName)
        {
            session.GetScreenshot().SaveAsFile(fullFileName, ScreenshotImageFormat.Png);
        }
    }
}
