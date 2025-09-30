namespace Cardevil.Core
{
    public interface IInitializableFromVariableContainer<TEnum> where TEnum : System.Enum
    {
        public void InitializeFromVariableContainer(DataStructure.VariableContainer<TEnum> container);
    }
    public interface IInitializableFromVariableContainer
    {
        public void InitializeFromVariableContainer(DataStructure.VariableContainer container);
    }
}