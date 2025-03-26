
namespace UnixLauncher.Core
{
    public interface IAbstractFactory<T>
    {
        public T Create();
    }
}