namespace Cardevil.UI.Flow
{
    public enum UiFlowStatus
    {
        Completed,
        Canceled
    }

    public readonly struct UiFlowResult<T>
    {
        public UiFlowStatus Status { get; }
        public T Value { get; }
        public bool IsCompleted => Status == UiFlowStatus.Completed;
        public bool IsCanceled => Status == UiFlowStatus.Canceled;

        private UiFlowResult(UiFlowStatus status, T value)
        {
            Status = status;
            Value = value;
        }

        public static UiFlowResult<T> Completed(T value)
        {
            return new UiFlowResult<T>(UiFlowStatus.Completed, value);
        }

        public static UiFlowResult<T> Canceled()
        {
            return new UiFlowResult<T>(UiFlowStatus.Canceled, default);
        }
    }
}
