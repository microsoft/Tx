using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace RxWinforms
{
    // This sample illustrates the original usage of Reactive Extensions for event-driven UI
    // Good book with more examples is "Programming Reactive Extensions and LINQ" http://www.apress.com/9781430237471
    public partial class Form1 : Form
    {
        IDisposable _subscription;
        public Form1()
        {
            InitializeComponent();

            var pen = new Pen(Color.Black, 2);

            var points = Observable.FromEventPattern<MouseEventArgs>(panel1, "MouseMove")
                              .Where(m => m.EventArgs.Button == MouseButtons.Left)
                              //.Sample(TimeSpan.FromMilliseconds(100))   // Un-comment this to pass only 1 event per 100 ms
                              .Select(m => new Point(m.EventArgs.X, m.EventArgs.Y));

            _subscription = points.Subscribe(p =>
            {
                panel1.CreateGraphics().FillRectangle(pen.Brush, p.X, p.Y, 5, 5);
            });
        }
    }
}
