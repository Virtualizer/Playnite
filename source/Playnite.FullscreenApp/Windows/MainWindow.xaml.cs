using Playnite.Controls;
using Playnite.FullscreenApp.Markup;
using Playnite.FullscreenApp.Themes.Fullscreen.Default.CustomControls;
using Playnite.Windows;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace Playnite.FullscreenApp.Windows
{

    public class MainWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new MainWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        Timer myTimer;
        DispatcherTimer screenSaverTimer;
        int graceCounter = 0;
        int graceCounterTarget = Convert.ToInt32(ConfigurationManager.AppSettings["GraceCounterTarget"]);
        int graceCounterKey = 0;
        int graceCounterKeyTarget = Convert.ToInt32(ConfigurationManager.AppSettings["GraceCounterKeyTarget"]);
        int screenSaverTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ScreenSaverTimerInterval"]);
        int screenSaverTimerSnowInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ScreenSaverTimerSnowInterval"]);
        DateTime? graceStartTime = null;

        public MainWindow() : base()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            screenSaverTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(screenSaverTimerInterval) };
            screenSaverTimer.Tick += (s, arg) => Snow();
            screenSaverTimer.Start();

            myTimer = new Timer(1500);
            myTimer.AutoReset = false;
            myTimer.Elapsed += delegate { MouseExt.SafeOverrideCursor(Cursors.None); }; //Hide cursor
        }

        private void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
        {
            //if (graceStartTime != null)
            //{
            //    DateTime actualTime = DateTime.Now;
            //    var diff = actualTime - graceStartTime;
            //    if (diff.HasValue && diff.Value.Seconds > 5)
            //    {
            //        graceCounter = 0;
            //        return;
            //    }
            //}

            if (graceCounter > graceCounterTarget)
            {
                if (e.MouseDevice != null)
                {
                    myTimer.Stop();
                    Mouse.OverrideCursor = null; //Show cursor
                    myTimer.Start();
                }

                screenSaverTimer.Stop();
                RemoveSnow();
                ViewBox.Opacity = 1;
                ViewBox.IsHitTestVisible = true;
                screenSaverTimer.Interval = TimeSpan.FromSeconds(screenSaverTimerInterval);
                screenSaverTimer.Start();
                graceCounter = 0;
                graceCounterKey = 0;
            }
            else
            {
                //graceStartTime = DateTime.Now;
                graceCounter++;
            }
        }

        private void LayoutRoot_KeyDown(object sender, KeyEventArgs e)
        {
            if (graceCounterKey > graceCounterKeyTarget)
            {
                myTimer.Stop();
                Mouse.OverrideCursor = null; //Show cursor
                myTimer.Start();

                screenSaverTimer.Stop();
                RemoveSnow();
                ViewBox.Opacity = 1;
                ViewBox.IsHitTestVisible = true;
                screenSaverTimer.Interval = TimeSpan.FromSeconds(screenSaverTimerInterval);
                screenSaverTimer.Start();
                graceCounterKey = 0;
                graceCounter = 0;
            }
            else
            {
                //graceStartTime = DateTime.Now;
                graceCounterKey++;
            }
        }

        private void RemoveSnow()
        {
            int intTotalChildren = LayoutRoot.Children.Count - 1;
            for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
            {
                if (LayoutRoot.Children[intCounter].GetType() == typeof(SnowFlake))
                {
                    SnowFlake ucCurrentChild = (SnowFlake)LayoutRoot.Children[intCounter];
                    LayoutRoot.Children.Remove(ucCurrentChild);
                }
            }
        }

        readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private void Snow()
        {
            if (ViewBox.Opacity != 0.1)
            {
                ViewBox.Opacity = 0.1;
                ViewBox.IsHitTestVisible = false;
            }

            var xAmount = _random.Next(-500, (int)LayoutRoot.ActualWidth - 100);
            var yAmount = -100;
            var s = _random.Next(5, 15) * 0.1;
            var rotateAmount = _random.Next(0, 270);

            RotateTransform rotateTransform = new RotateTransform(rotateAmount);
            ScaleTransform scaleTransform = new ScaleTransform(s, s);
            TranslateTransform translateTransform = new TranslateTransform(xAmount, yAmount);

            var flake = new SnowFlake
            {
                RenderTransform = new TransformGroup
                {
                    Children = new TransformCollection { rotateTransform, scaleTransform, translateTransform }
                },

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            RenderOptions.SetBitmapScalingMode(flake, BitmapScalingMode.LowQuality);
            LayoutRoot.Children.Add(flake);

            Duration duration = new Duration(TimeSpan.FromSeconds(_random.Next(1, 4)));

            xAmount += _random.Next(100, 500);
            var xAnimation = GenerateAnimation(xAmount, duration, flake, "RenderTransform.Children[2].X");

            yAmount += (int)(LayoutRoot.ActualHeight + 100 + 100);
            var yAnimation = GenerateAnimation(yAmount, duration, flake, "RenderTransform.Children[2].Y");

            rotateAmount += _random.Next(90, 360);
            var rotateAnimation = GenerateAnimation(rotateAmount, duration, flake, "RenderTransform.Children[0].Angle");

            Storyboard story = new Storyboard();
            story.Completed += (sender, e) => LayoutRoot.Children.Remove(flake);
            story.Children.Add(xAnimation);
            story.Children.Add(yAnimation);
            story.Children.Add(rotateAnimation);
            flake.Loaded += (sender, args) => story.Begin();

            screenSaverTimer.Stop();
            screenSaverTimer.Interval = TimeSpan.FromMilliseconds(screenSaverTimerSnowInterval);
            screenSaverTimer.Start();
        }

        private static DoubleAnimation GenerateAnimation(int x, Duration duration, SnowFlake flake, string propertyPath)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = x,
                Duration = duration
            };
            Storyboard.SetTarget(animation, flake);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
            return animation;
        }


    }
}
