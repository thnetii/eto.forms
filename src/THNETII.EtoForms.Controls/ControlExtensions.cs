using System;
using System.Collections.Generic;
using System.Text;
using Eto;
using Eto.Forms;

using Microsoft.Extensions.DependencyInjection;

namespace THNETII.EtoForms.Controls
{
    public static class ControlExtensions
    {
        public static IServiceProvider? GetServiceProvider(this Control control)
        {
            _ = control ?? throw new ArgumentNullException(nameof(control));

            return control.Properties.GetServiceProvider();
        }

        public static IServiceProvider? GetServiceProvider(this PropertyStore store)
        {
            _ = store ?? throw new ArgumentNullException(nameof(store));

            return store.TryGetValue(typeof(IServiceProvider), out var obj)
                ? obj as IServiceProvider
                : null;
        }

        public static void SetServiceProvider(this Control control,
            IServiceProvider serviceProvider)
        {
            _ = control ?? throw new ArgumentNullException(nameof(control));

            SetServiceProvider(control.Properties, serviceProvider);
        }

        public static void SetServiceProvider(this PropertyStore store,
            IServiceProvider serviceProvider)
        {
            _ = store ?? throw new ArgumentNullException(nameof(store));

            store.Set(typeof(IServiceProvider), serviceProvider);
            if (!(serviceProvider is null))
                store.Set(serviceProvider.GetType(), serviceProvider);
        }

        public static T? GetOrRequestService<T>(this Control control)
            where T : class
        {
            _ = control ?? throw new ArgumentNullException(nameof(control));

            return GetOrRequestService<T>(control.Properties);
        }

        public static T? GetOrRequestService<T>(this PropertyStore props)
            where T : class
        {
            _ = props ?? throw new ArgumentNullException(nameof(props));

            T? service = props.TryGetValue(typeof(T), out object instance)
                ? instance as T
                : default;

            return service ?? props.GetServiceProvider()?.GetService<T>();
        }
    }
}
