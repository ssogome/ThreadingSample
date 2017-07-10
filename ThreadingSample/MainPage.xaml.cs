using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ThreadingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Random randomNumberGenerator = new Random();
        private const int msDelay = 200;

        private const string debugInfoPrefix = "Random value";
        private const string numberFormat = "F2";
        private const string timeFormat = "HH:mm:fff";

        private const string timerStopLabel = "Stop";
        private const string timerStartLabel = "Start";

        private TimeSpan timeSpanZero = TimeSpan.FromMilliseconds(0);
        private TimeSpan timeSpanDelay = TimeSpan.FromMilliseconds(msDelay);

        private Timer timer;
        private ThreadPoolTimer threadPoolTimer;

        private bool isTimerActive = false;
        private bool isThreadPoolTImerActive = false;

        private SynchronizationContext synchronizationContext;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeTimer();

            synchronizationContext = SynchronizationContext.Current;
        }

        private void DisplayReadingValueUsingSynchronizationContext(double value)
        {
            var sendOrPostCallback = new SendOrPostCallback((arg) =>
            {
                ProgressBar.Value = Convert.ToInt32(value);
            });

            synchronizationContext.Post(sendOrPostCallback, value);
        }
        private async void DisplayReadingValue(double value)
        {
            if (Dispatcher.HasThreadAccess)
            {
                ProgressBar.Value = Convert.ToInt32(value);
            }
            else
            {
                var dispatcherHandler = new DispatchedHandler(() => { DisplayReadingValue(value); });
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, dispatcherHandler);
            }
        }

        private void InitializeTimer()
        {
            if(timer != null)
            {
                return;
            }
            else
            {
                var timerCallback = new TimerCallback((arg) => { GetReading(); });
                timer = new Timer(timerCallback, null, Timeout.InfiniteTimeSpan, timeSpanDelay);
            }

        }

        private void UpdateButtonLabel(Button button, bool isTimerActive)
        {
            if(button != null)
            {
                var buttonLabel = button.Content as string;
                if(buttonLabel != null)
                {
                    if (isTimerActive)
                    {
                        buttonLabel = buttonLabel.Replace(timerStartLabel, timerStopLabel);
                    }
                    else
                    {
                        buttonLabel = buttonLabel.Replace(timerStopLabel, timerStartLabel);
                    }
                    button.Content = buttonLabel;
                }
            }
        }

        private void UpdateTimerState()
        {
            if (isTimerActive)
            {
                //Stop Timer
                timer.Change(Timeout.InfiniteTimeSpan, timeSpanDelay);
            }
            else
            {
                //Start Timer
                timer.Change(timeSpanZero, timeSpanDelay);
            }
            isTimerActive = !isTimerActive;
        }

        private void StartThreadPoolTimer()
        {
            var timerElapseHandler = new TimerElapsedHandler((arg) => { GetReading(); });
            threadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(timerElapseHandler, timeSpanDelay);
        }

        private void StopThreadPoolTImer()
        {
            if(threadPoolTimer != null)
            {
                threadPoolTimer.Cancel();
            }
        }

        private void UpdateThreadPoolTimerState()
        {
            if (isThreadPoolTImerActive)
            {
                StopThreadPoolTImer();
            }
            else
            {
                StartThreadPoolTimer();
            }
            isThreadPoolTImerActive = !isThreadPoolTImerActive;
        }

        private void GetReading()
        {
            Task.Delay(msDelay).Wait();
            var randomValue = randomNumberGenerator.NextDouble();

            string debugString = string.Format("{0} | {1} : {2}", DateTime.Now.ToString(timeFormat), debugInfoPrefix, randomValue.ToString(numberFormat));

            Debug.WriteLine(debugString);

            //ProgressBar.Value = Convert.ToInt32(randomValue * 100);
            //This is using CoreDispatcher// DisplayReadingValue(randomValue * 100);
            DisplayReadingValueUsingSynchronizationContext(randomValue * 100);
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            var action = new Action(GetReading);
            Task.Run(action);
            //Or alternatively 
            //Task task = new Task(action);
            //task.Start();
        }

        private async void ThreadPoolButton_Click(object sender, RoutedEventArgs e)
        {
            var workItemHandler = new WorkItemHandler((arg) => { GetReading(); });
            await ThreadPool.RunAsync(workItemHandler);
        }

        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimerState();
            UpdateButtonLabel(sender as Button, isTimerActive);
        }

        private void ThreadPoolTimerButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateThreadPoolTimerState();
            UpdateButtonLabel(sender as Button, isThreadPoolTImerActive);
        }
    }
}
