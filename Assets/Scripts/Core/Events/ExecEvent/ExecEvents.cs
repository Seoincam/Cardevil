namespace Cardevil.Core.Events.ExecEvent
{
    public class TestExecEventArgs : ExecEventArgs<TestExecEventArgs>
    {
        public int Value;

        public override void Clear()
        {
            base.Clear();
            Value = 0;
        }
    }
}