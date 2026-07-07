using Godot;

namespace AchtungDieSpurve.Scoreboard
{
    public class CountdownManager : Control
    {
        [Signal]
        public delegate void CountdownFinished();

        private Label _label;
        private Timer _timer;
        private int   _count;

        public override void _Ready()
        {
            PauseMode = PauseModeEnum.Process;
            _label    = GetNode<Label>("Label");
            _timer    = GetNode<Timer>("Timer");
            _timer.Connect("timeout", this, nameof(OnTimerTimeout));
            Hide();
        }

        public void StartCountdown()
        {
            _count          = 3;
            _label.Text     = "3";
            _timer.WaitTime = 1.0f;
            Show();
            _timer.Start();
        }

        private void OnTimerTimeout()
        {
            _count--;

            if (_count > 0)
            {
                _label.Text     = _count.ToString();
                _timer.WaitTime = 1.0f;
                _timer.Start();
            }
            else if (_count == 0)
            {
                _label.Text     = "GO!";
                _timer.WaitTime = 0.5f;
                _timer.Start();
            }
            else
            {
                Hide();
                EmitSignal(nameof(CountdownFinished));
            }
        }
    }
}
