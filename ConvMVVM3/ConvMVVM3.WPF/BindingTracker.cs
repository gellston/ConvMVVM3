using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Tracks active bindings on dependency objects to prevent memory leaks.
    /// Automatically clears bindings when controls are unloaded.
    /// </summary>
    public static class BindingTracker
    {
        private static readonly ConditionalWeakTable<DependencyObject, List<BindingExpressionBase>>
            _activeBindings = new ConditionalWeakTable<DependencyObject, List<BindingExpressionBase>>();

        /// <summary>
        /// Tracks a binding expression for a dependency object.
        /// </summary>
        /// <param name="control">The dependency object that has the binding.</param>
        /// <param name="binding">The binding expression to track.</param>
        public static void TrackBinding(DependencyObject control, BindingExpressionBase binding)
        {
            if (control == null || binding == null) return;

            if (!_activeBindings.TryGetValue(control, out var bindings))
            {
                bindings = new List<BindingExpressionBase>();
                _activeBindings.Add(control, bindings);
            }

            bindings.Add(binding);
        }

        /// <summary>
        /// Clears all tracked bindings for a dependency object.
        /// </summary>
        /// <param name="control">The dependency object to clear bindings for.</param>
        public static void ClearAllBindings(DependencyObject control)
        {
            if (control == null) return;

            if (_activeBindings.TryGetValue(control, out var bindings))
            {
                foreach (var binding in bindings)
                {
                    // Use BindingOperations to clear the binding
                    if (binding is BindingExpression bindingExpr)
                    {
                        var property = bindingExpr.TargetProperty;
                        BindingOperations.ClearBinding(control, property);
                    }
                }
                _activeBindings.Remove(control);
            }
        }

        /// <summary>
        /// Gets the number of tracked bindings for a dependency object.
        /// </summary>
        /// <param name="control">The dependency object.</param>
        /// <returns>The number of tracked bindings.</returns>
        public static int GetBindingCount(DependencyObject control)
        {
            if (control == null) return 0;

            if (_activeBindings.TryGetValue(control, out var bindings))
            {
                return bindings.Count;
            }

            return 0;
        }
    }
}