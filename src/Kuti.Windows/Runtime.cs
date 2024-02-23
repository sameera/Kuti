using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuti.Windows
{
    public class Runtime
    {
        private readonly Dictionary<Type, Func<Runtime, object>> _factories;
        private readonly Dictionary<Type, object> _instances;

        public static Runtime Uninitialized = new Runtime();
        public static Runtime Current { get; private set; } = Uninitialized;

        public Runtime(Dictionary<Type, Func<Runtime, object>>? factories = null)
        {
            Current = this;

            _instances = new Dictionary<Type, object>();
            _factories = factories ?? new Dictionary<Type, Func<Runtime, object>>();
        }

        public void Register<T>(Func<Runtime, T> factory) where T: notnull => _factories.Add(typeof(T), r => factory(r));
        public void Register<T>(Func<T> factory) where T: notnull => _factories.Add(typeof(T), _ => factory());


        public T GetInstance<T>()
        {
            if (_instances.ContainsKey(typeof(T))) return (T)_instances[typeof(T)];

            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException(typeof(T).Name + " has no registered factory.");

            object instance = _factories[typeof(T)].Invoke(this);
            if (instance == null || instance.GetType().IsAssignableTo(typeof(T)) == false)
            {
                throw new InvalidCastException($"Factory registered for ${typeof(T)} produced a ${instance?.GetType().Name ?? "null"} which is not assignable to ${typeof(T).Name}");
            }
            _instances[typeof(T)] = instance;

            return (T)instance;
        }
    }
}
