namespace UnixLauncher.Core.Logger
{
    /// <summary>
    /// Представляет объект, управляющий областью (scope) логирования, который автоматически удаляет свою запись из коллекции при завершении работы.
    /// </summary>
    /// <remarks>
    /// Класс <c>ScopeDisposable</c> используется для создания временной области логирования, которая регистрируется в общем списке областей.
    /// При вызове метода <see cref="Dispose()"/> происходит попытка удаления идентификатора области (<c>_scope</c>) из переданного списка областей (<c>_scopes</c>), что позволяет корректно завершить контекст логирования.
    /// 
    /// Обратите внимание, что данный класс следует использовать в конструкции <c>using</c> для гарантированного освобождения ресурсов и своевременного завершения области логирования.
    /// </remarks>
    internal class ScopeDisposable : IDisposable
    {
        private readonly string _scope;
        private readonly List<string> _scopes;
        private readonly object _scopesLock;
        private bool _disposed;

        public ScopeDisposable(string scope, List<string> scopes, object scopesLock)
        {
            _scope = scope;
            _scopes = scopes;
            _scopesLock = scopesLock;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_scopesLock)
            {
                _scopes.Remove(_scope);
            }

            _disposed = true;
        }
    }
}