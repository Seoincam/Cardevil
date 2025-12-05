namespace Cardevil.Events.ExecEvents
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