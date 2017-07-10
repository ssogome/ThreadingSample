using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void GetReading()
        {
            Task.Delay(msDelay).Wait();
            var randomValue = randomNumberGenerator.NextDouble();

            string debugString = string.Format("{0} | {1} : {2}", DateTime.Now.ToString(timeFormat), debugInfoPrefix, randomValue.ToString(numberFormat));

            Debug.WriteLine(debugString);
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

        }

        private void ThreadPoolTimerButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
