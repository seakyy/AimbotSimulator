// AimbotDemo.xaml.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AimbotSimulator
{
    public partial class MainWindow : Window
    {
        private List<Ellipse> enemies = new List<Ellipse>();
        private Random rnd = new Random();
        private DispatcherTimer moveTimer;
        private DispatcherTimer aimbotTimer;
        private bool aimbotEnabled = false;
        private bool smoothAim = false;
        private bool triggerBot = false;
        private bool autoReset = false;
        private bool rageMode = false;
        private Stopwatch rageTimer = new Stopwatch();

        private const double DefaultFov = 200;
        private double currentFov = DefaultFov;
        private Ellipse fovCircle;

        // Control panel components
        private StackPanel controlPanel;
        private Slider fovSlider;
        private Slider speedSlider;
        private CheckBox autoResetCheck;
        private CheckBox rageCheck;
        private Button resetButton;
        private Button toggleControlPanelButton;
        private TextBlock logBlock;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            InitFovOverlay();
            InitEnemies();
            InitTimers();
            InitControlPanel();

            this.KeyDown += ToggleFeatures;
        }

        private void InitFovOverlay()
        {
            fovCircle = new Ellipse
            {
                Width = currentFov * 2,
                Height = currentFov * 2,
                Stroke = Brushes.Yellow,
                StrokeThickness = 1
            };
            GameCanvas.Children.Add(fovCircle);
        }

        private void InitEnemies()
        {
            for (int i = 0; i < 5; i++)
            {
                var enemy = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Red
                };
                enemy.MouseDown += (s, e) => { ((Ellipse)s).Fill = Brushes.Green; };
                Canvas.SetLeft(enemy, rnd.Next(100, 700));
                Canvas.SetTop(enemy, rnd.Next(100, 500));
                enemies.Add(enemy);
                GameCanvas.Children.Add(enemy);
            }
        }

        private void InitTimers()
        {
            moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            moveTimer.Tick += (s, e) => MoveEnemies();
            moveTimer.Start();

            aimbotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            aimbotTimer.Tick += (s, e) => RunAimbot();
            aimbotTimer.Start();
        }

        private void InitControlPanel()
        {
            controlPanel = new StackPanel
            {
                Background = Brushes.Gray,
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10),
                Visibility = Visibility.Collapsed
            };

            toggleControlPanelButton = new Button { Content = "☰", Width = 30, Height = 30, Margin = new Thickness(5) };
            toggleControlPanelButton.Click += (s, e) =>
            {
                controlPanel.Visibility = controlPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            };

            fovSlider = new Slider { Minimum = 50, Maximum = 500, Value = currentFov, Margin = new Thickness(5) };
            fovSlider.ValueChanged += (s, e) =>
            {
                currentFov = fovSlider.Value;
                fovCircle.Width = currentFov * 2;
                fovCircle.Height = currentFov * 2;
            };

            speedSlider = new Slider { Minimum = 100, Maximum = 2000, Value = 800, Margin = new Thickness(5) };
            speedSlider.ValueChanged += (s, e) => { moveTimer.Interval = TimeSpan.FromMilliseconds(speedSlider.Value); };

            autoResetCheck = new CheckBox { Content = "Auto Reset Green", Margin = new Thickness(5) };
            autoResetCheck.Checked += (s, e) => autoReset = true;
            autoResetCheck.Unchecked += (s, e) => autoReset = false;

            rageCheck = new CheckBox { Content = "Rage Mode", Margin = new Thickness(5) };
            rageCheck.Checked += (s, e) => { rageMode = true; rageTimer.Restart(); };
            rageCheck.Unchecked += (s, e) => { rageMode = false; rageTimer.Reset(); };

            resetButton = new Button { Content = "Reset to Red", Margin = new Thickness(5) };
            resetButton.Click += (s, e) => ResetEnemies();

            logBlock = new TextBlock { Margin = new Thickness(5), Foreground = Brushes.White };

            controlPanel.Children.Add(new Label { Content = "FOV Size" });
            controlPanel.Children.Add(fovSlider);
            controlPanel.Children.Add(new Label { Content = "Spawn Speed" });
            controlPanel.Children.Add(speedSlider);
            controlPanel.Children.Add(autoResetCheck);
            controlPanel.Children.Add(rageCheck);
            controlPanel.Children.Add(resetButton);
            controlPanel.Children.Add(logBlock);

            GameCanvas.Children.Add(toggleControlPanelButton);
            GameCanvas.Children.Add(controlPanel);
        }

        private void MoveEnemies()
        {
            foreach (var enemy in enemies)
            {
                Canvas.SetLeft(enemy, rnd.Next(50, (int)(GameCanvas.ActualWidth - 50)));
                Canvas.SetTop(enemy, rnd.Next(50, (int)(GameCanvas.ActualHeight - 50)));
            }
        }

        private void RunAimbot()
        {
            if (!aimbotEnabled) return;

            Point cursor = GetCursorPosition();

            if (rageMode)
            {
                foreach (var enemy in enemies)
                {
                    if (((SolidColorBrush)enemy.Fill).Color == Colors.Red)
                    {
                        Point center = GetScreenCenter(enemy);
                        SetCursorInstantly(center);
                        enemy.Fill = Brushes.Green;
                    }
                }

                if (enemies.All(e => ((SolidColorBrush)e.Fill).Color == Colors.Green))
                {
                    rageTimer.Stop();
                    logBlock.Text = $"[RAGE COMPLETE] Time: {rageTimer.ElapsedMilliseconds}ms";
                }

                return;
            }

            Point closestTarget = GetClosestTarget(cursor, out Ellipse targetEnemy);

            if (closestTarget != default)
            {
                if (smoothAim)
                    MoveCursorSmooth(cursor, closestTarget);
                else
                    SetCursorInstantly(closestTarget);

                if (triggerBot && targetEnemy != null && ((SolidColorBrush)targetEnemy.Fill).Color == Colors.Red)
                {
                    targetEnemy.Fill = Brushes.Green;
                }
            }

            UpdateFovOverlay(cursor);

            if (autoReset && enemies.All(e => ((SolidColorBrush)e.Fill).Color == Colors.Green))
            {
                ResetEnemies();
            }
        }

        private Point GetClosestTarget(Point from, out Ellipse closestEnemy)
        {
            double minDist = double.MaxValue;
            Point closest = default;
            closestEnemy = null;

            foreach (var enemy in enemies)
            {
                Point center = GetScreenCenter(enemy);
                double dist = Distance(from, center);

                if (dist < currentFov && dist < minDist)
                {
                    minDist = dist;
                    closest = center;
                    closestEnemy = enemy;
                }
            }

            return closest;
        }

        private void ResetEnemies()
        {
            foreach (var enemy in enemies)
            {
                enemy.Fill = Brushes.Red;
            }
        }

        private double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private Point GetScreenCenter(Ellipse enemy)
        {
            double x = Canvas.GetLeft(enemy) + enemy.Width / 2;
            double y = Canvas.GetTop(enemy) + enemy.Height / 2;
            return GameCanvas.PointToScreen(new Point(x, y));
        }

        private void ToggleFeatures(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) aimbotEnabled = !aimbotEnabled;
            if (e.Key == Key.F2) smoothAim = !smoothAim;
            if (e.Key == Key.F3) triggerBot = !triggerBot;
            if (e.Key == Key.Escape) Close();
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private Point GetCursorPosition()
        {
            GetCursorPos(out POINT point);
            return new Point(point.X, point.Y);
        }

        private unsafe void SetCursorInstantly(Point target)
        {
            int* px = stackalloc int[1];
            int* py = stackalloc int[1];
            *px = (int)target.X;
            *py = (int)target.Y;
            SetCursorPos(*px, *py);
        }

        private void MoveCursorSmooth(Point from, Point to)
        {
            double dx = (to.X - from.X) / 5;
            double dy = (to.Y - from.Y) / 5;
            SetCursorPos((int)(from.X + dx), (int)(from.Y + dy));
        }

        private void UpdateFovOverlay(Point cursor)
        {
            Point relative = GameCanvas.PointFromScreen(cursor);
            fovCircle.Width = currentFov * 2;
            fovCircle.Height = currentFov * 2;
            Canvas.SetLeft(fovCircle, relative.X - currentFov);
            Canvas.SetTop(fovCircle, relative.Y - currentFov);
        }
    }
}
