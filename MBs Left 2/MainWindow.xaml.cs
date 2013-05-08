using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MBs_Left_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly TimerEx _timerEx = new TimerEx();
        private UserTrafficStatistic _userTrafficStatistic;

        private const int SecondsInMinute = 60;

        public MainWindow()
        {
            InitializeComponent();
            SetWindowPosition();
            ClearLabels();

            InitializeUserTrafficStatistic(Properties.Settings.Default.UserPhoneNamber, Properties.Settings.Default.UserPass);

            InitializeMainBehavior();
            InitializeContextMenuBehavior();
            InitializeProgressBarBehavior(ProgressBar);

            ApplicationFirsStart();


            Properties.Settings.Default.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName == "UserPhoneNamber")
                    {
                        _userTrafficStatistic.SetUserData(Properties.Settings.Default.UserPhoneNamber, Properties.Settings.Default.UserPass);
                    }
                };
        }

        private void ApplicationFirsStart()
        {
            if (Properties.Settings.Default.UserPhoneNamber.Length == 0)
            {
                ShowWindowToUserDataInput();
            }
        }

        private void SetWindowPosition()
        {
            MainWindowForm.Left = Properties.Settings.Default.Left;
            MainWindowForm.Top = Properties.Settings.Default.Top;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Left = MainWindowForm.Left;
            Properties.Settings.Default.Top = MainWindowForm.Top;

            Properties.Settings.Default.AutoUpdateEnable = MenuItem_AutoUpdate.IsChecked;
            Properties.Settings.Default.AutoUpdateInterval = int.Parse(MenuItem_AutoUpdateTime.Text);
            Properties.Settings.Default.Opacity = MenuItem_OpacitySlider.Value / 100;
            Properties.Settings.Default.OpacitySliderValue = MenuItem_OpacitySlider.Value;
            Properties.Settings.Default.TopMost = MainWindowForm.Topmost;

            Properties.Settings.Default.Save();
        }

        private void InitializeContextMenuBehavior()
        {
            MenuItem_GetStatictic.Click += delegate
            {
                GetAndSetStatisticToLabels();
            };

            MenuItem_Exit.Click += delegate
            {
                Application.Current.Shutdown();
            };

            MenuItem_AutoUpdate.Checked += delegate
            {
                _timerEx.Start();
            };

            MenuItem_AutoUpdate.Unchecked += delegate
                {
                    _timerEx.Stop();
                };

            MenuItem_AutoUpdateTime.TextChanged += delegate
                {
                    int parseResult;
                    if (int.TryParse(MenuItem_AutoUpdateTime.Text, out parseResult))
                    {
                        _timerEx.Stop();
                        _timerEx.TotalIntervalInSec = parseResult * SecondsInMinute;
                        if (MenuItem_AutoUpdate.IsChecked)
                        {
                            _timerEx.Start();
                        }
                    }
                };

            MenuItem_TopMost.Checked += delegate
                {
                    MainWindowForm.Topmost = true;
                };

            MenuItem_TopMost.Unchecked += delegate
                {
                    MainWindowForm.Topmost = false;
                };

            MenuItem_SetUserData.Click += delegate
                {
                    ShowWindowToUserDataInput();
                };
            /*
            //Language
            MenuItem_English.Click += delegate
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = 
                        System.Globalization.CultureInfo.GetCultureInfo("en-US");

                    InitializeComponent();
                };*/
        }

        private void InitializeUserTrafficStatistic(string userPhone, string userPass)
        {
            _userTrafficStatistic = new UserTrafficStatistic(userPhone, userPass);
        }

        private void InitializeMainBehavior()
        {
            _userTrafficStatistic.StatisticLoaded += delegate
            {
                ClearLabels();

                Dispatcher.Invoke(delegate
                {
                    if (MenuItem_AutoUpdate.IsChecked)
                    {
                        _timerEx.Start();
                    }
                });
            };

            _userTrafficStatistic.StatisticLoadingStart += delegate
            {
                _timerEx.Stop();
            };

            _timerEx.TatalIntervalElapsed += delegate
            {
                GetAndSetStatisticToLabels();
            };
        }

        private void InitializeProgressBarBehavior(ProgressBar progressBar)
        {
            _userTrafficStatistic.StatisticLoadingStart += delegate
            {
                SetProgressBarStyleToMarquee(progressBar);
                //if (!progressBar.Dispatcher.CheckAccess())
                //{
                //    ThreadSafeCallToControlDelegate progressBarThreadSafeCallDelegate = SetProgressBarStyleToMarquee;
                //    progressBar.Dispatcher.Invoke(progressBarThreadSafeCallDelegate, progressBar);
                //}
                //else
                //{
                //    progressBar.IsIndeterminate = true;
                //}
            };

            _userTrafficStatistic.StatisticLoaded += delegate
            {
                SetProgressBarStyleToBlock(progressBar);
                //if (!progressBar.Dispatcher.CheckAccess())
                //{
                //    ThreadSafeCallToControlDelegate progressBarThreadSafeCallDelegate = SetProgressBarStyleToBlock;
                //    progressBar.Dispatcher.Invoke(progressBarThreadSafeCallDelegate, progressBar);
                //}
                //else
                //{
                //    progressBar.IsIndeterminate = false;
                //    progressBar.Value = progressBar.Maximum;
                //}
            };

            _timerEx.Elapsed += delegate
            {
                ProgressBarDecrement(progressBar);
                //if (!progressBar.Dispatcher.CheckAccess())
                //{
                //    ThreadSafeCallToControlDelegate progressBarThreadSafeCallDelegate = ProgressBarDecrement;
                //    progressBar.Dispatcher.Invoke(progressBarThreadSafeCallDelegate, progressBar);
                //}
                //else
                //{
                //    ProgressBarDecrement(progressBar);
                //}
            };

            _timerEx.TotalIntervalChanged += delegate
            {
                SetProgressBarMaximum(progressBar, _timerEx.TotalIntervalInSec);
            };

            _timerEx.TatalIntervalElapsed += delegate
            {
                ChargeProgressBar(progressBar);
                //if (!progressBar.Dispatcher.CheckAccess())
                //{
                //    ThreadSafeCallToControlDelegate progressBarThreadSafeCallDelegate = ChargeProgressBar;
                //    progressBar.Dispatcher.Invoke(progressBarThreadSafeCallDelegate, progressBar);
                //}
                //else
                //{
                //    progressBar.Value = progressBar.Maximum;
                //}
            };

            _timerEx.Started += delegate
            {
                ChargeProgressBar(progressBar);
            };
        }

        #region ProgressBar Funcrions

            private void ProgressBarDecrement(ProgressBar progressBar)
            {
                Dispatcher.Invoke(delegate
                    {
                        const double epsilon = 0;
                        if (Math.Abs(progressBar.Value - 0) > epsilon)
                    {
                        progressBar.Value--;
                    }
                    });
            }

            private void ChargeProgressBar(ProgressBar progressBar)
            {
                Dispatcher.Invoke(delegate
                {
                    progressBar.Value = progressBar.Maximum;
                });
            }

            private void SetProgressBarStyleToMarquee(ProgressBar progressBar)
            {
                Dispatcher.Invoke(delegate
                {
                    progressBar.IsIndeterminate = true;
                });
            }

            private void SetProgressBarStyleToBlock(ProgressBar progressBar)
            {
                Dispatcher.Invoke(delegate
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = progressBar.Maximum;
                });
            }

            private void SetProgressBarMaximum(ProgressBar progressBar, int maximum)
        {
            Dispatcher.Invoke(delegate
            {
                progressBar.Maximum = maximum;
            });
        }

        #endregion


        private void ClearLabels()
        {
            Dispatcher.Invoke(delegate { Label_TrafficLeft.Content = @"0000"; });
            Dispatcher.Invoke(delegate { Label_TotalTraffic.Content = @"0000"; });
            Dispatcher.Invoke(delegate { Label_CurrentSession.Content = @"0000"; });
            Dispatcher.Invoke(delegate { Label_Saldo.Content = @"0000"; });

            //if (!Label_TrafficLeft.Dispatcher.CheckAccess())
            //{
            //    ThreadSafeCallToLabelDelegate threadSafeCallToLabelDelegate = ClearLabels;
            //    Label_TrafficLeft.Dispatcher.Invoke(threadSafeCallToLabelDelegate);
            //}
            //else
            //{
            //    Label_TrafficLeft.Content = @"0000";
            //}

            //if (!Label_TotalTraffic.Dispatcher.CheckAccess())
            //{
            //    ThreadSafeCallToLabelDelegate threadSafeCallToLabelDelegate = ClearLabels;
            //    Label_TotalTraffic.Dispatcher.Invoke(threadSafeCallToLabelDelegate);
            //}
            //else
            //{
            //    Label_TotalTraffic.Content = @"0000";
            //}

            //if (!Label_CurrentSession.Dispatcher.CheckAccess())
            //{
            //    ThreadSafeCallToLabelDelegate threadSafeCallToLabelDelegate = ClearLabels;
            //    Label_CurrentSession.Dispatcher.Invoke(threadSafeCallToLabelDelegate);
            //}
            //else
            //{
            //    Label_CurrentSession.Content = @"0000";
            //}

            //if (!Label_Saldo.Dispatcher.CheckAccess())
            //{
            //    ThreadSafeCallToLabelDelegate threadSafeCallToLabelDelegate = ClearLabels;
            //    Label_Saldo.Dispatcher.Invoke(threadSafeCallToLabelDelegate);
            //}
            //else
            //{
            //    Label_Saldo.Content = @"0000";
            //}

            //Dont work work WPF - thread unsafe
            //Label_TrafficLeft.Content = @"0000";
            //Label_TotalTraffic.Content = @"0000";
            //Label_CurrentSession.Content = @"0000";
            //Label_Saldo.Content = @"0000";
        }

        private async void GetAndSetStatisticToLabels()
        {
            bool inetConectionEnabled = ConnectionChecker.IsConnectedToInternet();
            //bool inetconectionEnabled = CheckInetConection();

            if (inetConectionEnabled)
            {
                TrafficStatictic trafficStatictic = await _userTrafficStatistic.GetPageContentAsync();

                Dispatcher.Invoke(delegate { Label_TrafficLeft.Content = trafficStatictic.TrafficLeft; });
                Dispatcher.Invoke(delegate { Label_TotalTraffic.Content = trafficStatictic.TotalTraffic; });
                Dispatcher.Invoke(delegate { Label_CurrentSession.Content = trafficStatictic.CurrentSession; });
                Dispatcher.Invoke(delegate { Label_Saldo.Content = trafficStatictic.Saldo; });
            }
            else
            {
                MessageBox.Show(Properties.Resources.CheckInternetConnectionMessage);
            }

            //Dont work work WPF - thread unsafe
            //Label_TrafficLeft.Content = trafficStatictic.TrafficLeft;
            //Label_TotalTraffic.Content = trafficStatictic.TotalTraffic;
            //Label_CurrentSession.Content = trafficStatictic.CurrentSession;
            //Label_Saldo.Content = trafficStatictic.Saldo;
        }

        private static string GetVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }

            return "Debug";
        }

        private void ShowWindowToUserDataInput()
        {
            WindowToUserDataInput windowToUserDataInput = new WindowToUserDataInput();
            windowToUserDataInput.Top = this.Top - (windowToUserDataInput.Height - this.Height) / 2;
            windowToUserDataInput.Left = this.Left - (windowToUserDataInput.Width - this.Width) / 2;
            windowToUserDataInput.Show();
        }

        //DragMove
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainWindowForm_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }

        private void MenuItem_OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindowForm.Opacity = MenuItem_OpacitySlider.Value / 100; //to precentage
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(GetVersion());
        }

    }

    struct TrafficStatictic
    {
        private readonly string _currentSession;
        private readonly string _totalTraffic;
        private readonly string _trafficLeft;
        private readonly string _saldo;

        public TrafficStatictic(string currentSession, string totalTraffic, string saldo)
            : this()
        {
            _currentSession = currentSession;
            _totalTraffic = totalTraffic;
            _saldo = saldo;
            _trafficLeft = (float.Parse(totalTraffic) - float.Parse(currentSession)).ToString(CultureInfo.InvariantCulture);
        }

        public string CurrentSession
        {
            get { return _currentSession; }
        }

        public string TotalTraffic
        {
            get { return _totalTraffic; }
        }

        public string TrafficLeft
        {
            get { return _trafficLeft; }
        }

        public string Saldo
        {
            get { return _saldo; }
        }
    }

    sealed class UserTrafficStatistic
    {
        private string _postData;
        private const string URLLogin = "https://assa.intertelecom.ua/ru/login/";

        private TrafficStatictic _trafficStatictic;

        //CONSTRUCTOR
        public UserTrafficStatistic(string userPhone, string userPass)
        {
            SetUserData(userPhone, userPass);
        }

        public void SetUserData(string userPhone, string userPass)
        {
            _postData = @"phone=" + userPhone + @"&pass=" + userPass + @"&ref_link=https%3A%2F%2Fassa.intertelecom.ua%2Fru%2Fstatistic&js=1";
        }

        public async Task<TrafficStatictic> GetPageContentAsync()
        {
            //Event
            OnStatisticLoadingStart();

            HttpResponseMessage response;
            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent content = new StringContent(_postData);

                //важная строка - без нее не проходит логин
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                response = await httpClient.PostAsync(URLLogin, content);
            }
            response.EnsureSuccessStatusCode();

            //проверка на правильность логина/пароля
            if (response.RequestMessage.RequestUri.AbsolutePath.Contains("statistic"))
            {
                var respContent = await response.Content.ReadAsStringAsync();
                respContent = respContent.Replace("\n", "");
                respContent = respContent.Replace("\r", "");
                respContent = respContent.Replace("\t", "");

                //Parsing
                ParseStringAndSetTrafficStats(respContent);

                //Event
                OnStatisticLoaded();

                return _trafficStatictic;
            }
            MessageBox.Show(Properties.Resources.WrongUserDataMessage);
            TrafficStatictic emptyTrafficStatictic = new TrafficStatictic("0000", "0000", "0000");

            //Event
            OnStatisticLoaded();

            return emptyTrafficStatictic;
        }

        void ParseStringAndSetTrafficStats(string stringToParse)
        {
            // нахожу на странице поле трафика текущей сессии и достаю значение через группу
            Match parsMatchTotalTraffic = Regex.Match(stringToParse, @"(?<AllT>\d*)\.?\d* по \d\d");
            if (!parsMatchTotalTraffic.Success)
            {
                MessageBox.Show("DEBUG.\n\nCan not find TotalTraffic");

                string debugString = "DEBUG.\n\nCan not find TotalTraffic" + "\nin" + stringToParse;
                string filePath = @AppDomain.CurrentDomain.BaseDirectory + @"log-" + DateTime.Now.Second + @".txt";
                File.WriteAllText(filePath, debugString);
            }

            // нахожу на странице поле трафика текущей сессии и достаю значение через группу
            Match parsMatchCurrentSession = Regex.Match(stringToParse, @"<td>Трафик МБ</td>.*?(?<currSessionT>\d{1,4})<+?");
            if (!parsMatchCurrentSession.Success)
            {
                MessageBox.Show("DEBUG.\n\nCan not find CurrentSession");
            }

            //нахожу на странице поле сальдо и достаю значение через группу
            Match parsMatchSaldo = Regex.Match(stringToParse, @"<td>Сальдо</td>.*(?<Saldo>\d{1,2}.\d{1,2}).*месяц");
            if (!parsMatchSaldo.Success)
            {
                MessageBox.Show("DEBUG.\n\nCan not find Saldo");
            }

            //заполняю структуру статистики
            _trafficStatictic = new TrafficStatictic(parsMatchCurrentSession.Groups["currSessionT"].Value,
                                                    parsMatchTotalTraffic.Groups["AllT"].Value,
                                                    parsMatchSaldo.Groups["Saldo"].Value);
        }

        #region EVENTS

        //StatisticLoaded
        public delegate void StatisticLoadedEventHandler(object sender, EventArgs e);
        public event StatisticLoadedEventHandler StatisticLoaded;
        private void OnStatisticLoaded()
        {
            StatisticLoadedEventHandler handler = StatisticLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //StatisticLoadingStart
        public delegate void StatisticLoadingStartEventHandler(object sender, EventArgs e);
        public event StatisticLoadingStartEventHandler StatisticLoadingStart;
        private void OnStatisticLoadingStart()
        {
            StatisticLoadingStartEventHandler handler = StatisticLoadingStart;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

    }

    sealed class TimerEx
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();

        private int _totalIntervalResetValue;
        private int _totalIntervalInSec;

        public int TotalIntervalInSec
        {
            get { return _totalIntervalInSec; }
            set
            {
                //!!*60
                _totalIntervalResetValue = value;
                _totalIntervalInSec = value;
                OnTotalIntervalChanged();
            }
        }

        private double TickInterval
        {
            get
            {
                return _timer.Interval;
            }
            set
            {
                _timer.Interval = value;
            }
        }

        //CONSTRUCTIOR
        public TimerEx()
        {
            //ставлю на костыли ивент Elapsed, потому что не знаю как сделать его паблик на прямую
            _timer.Elapsed += delegate
            {
                OnElapsed();
            };

            SetTotalIntervalBehavior();
            TickInterval = 1000;
        }

        private void SetTotalIntervalBehavior()
        {
            _timer.Elapsed += delegate
            {
                if (_totalIntervalInSec != 0)
                {
                    _totalIntervalInSec--;
                }
                else
                {
                    _totalIntervalInSec = _totalIntervalResetValue;

                    //event call
                    OnTatalIntervalElapsed();
                }
            };
        }

        private void ResetTotalInterval()
        {
            _totalIntervalInSec = _totalIntervalResetValue;
        }

        public void Start()
        {
            ResetTotalInterval();

            _timer.Start();

            //event call
            OnStarted();
        }

        public void Stop()
        {
            _timer.Stop();

            //event call
            OnStopped();
        }

        #region EVENTS

        //OnStarted
        public event EventHandler Started;
        private void OnStarted()
        {
            EventHandler handler = Started;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //OnStopped
        public event EventHandler Stopped;
        private void OnStopped()
        {
            EventHandler handler = Stopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //TatalIntervalElapsed
        //private delegate void EventHandler(object sender, EventArgs args);
        public event EventHandler TatalIntervalElapsed;
        private void OnTatalIntervalElapsed()
        {
            EventHandler handler = TatalIntervalElapsed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //TotalIntervalChanged
        public event EventHandler TotalIntervalChanged;
        private void OnTotalIntervalChanged()
        {
            EventHandler handler = TotalIntervalChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //Def Elapsed !!!!!!!!КОСТЫЛИ------------------
        public event EventHandler Elapsed;
        private void OnElapsed()
        {
            EventHandler handler = Elapsed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

    }

    public static class ConnectionChecker
    {
        // Импортируем функцию из стандартной dll Windows
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        // Создаем новую функцию используя API-функцию библиотеки
        public static bool IsConnectedToInternet()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }
    }

}
