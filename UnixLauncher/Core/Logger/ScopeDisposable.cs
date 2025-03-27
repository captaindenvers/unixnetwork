using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    public class ScopeDisposable : IDisposable
    {
        private readonly string _scope;
        private readonly List<string> _scopes;
        private bool _disposed;

        public ScopeDisposable(string scope, List<string> scopes)
        {
            _scope = scope;
            _scopes = scopes;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_scopes.Contains(_scope) && _disposed)
            {
                if (disposing)
                {
                    // Удаляем текущий скоп только при явном вызове Dispose
                    _scopes.Remove(_scope);
                }

                _disposed = true;
            }
        }
    }
}
