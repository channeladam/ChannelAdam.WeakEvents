//-----------------------------------------------------------------------
// <copyright file="WeakEventSubscriptionFactory.cs">
//     Copyright (c) 2017-2018 Adam Craven. All rights reserved.
// </copyright>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

namespace ChannelAdam.WeakEvents.Internal
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Factory for creating weakly referenced event subscriptions/handlers.
    /// </summary>
    internal static class WeakEventSubscriptionFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a weak event subscription/handler, specifying a delegate method that can unsubscribe the event handler from the event.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="unsubscribeFromEventDelegate">The unsubscribe from event delegate.</param>
        /// <returns>
        /// A weak event handler.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">If eventHandler is null.</exception>
        /// <exception cref="System.ArgumentException">A WeakEventSubscription can only be created on instance methods.;eventHandler.</exception>
        internal static IWeakEventSubscription<TEventArgs> Create<TEventArgs>(EventHandler<TEventArgs> eventHandler, UnsubscribeFromEventAction<TEventArgs> unsubscribeFromEventDelegate) where TEventArgs : EventArgs
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            if (eventHandler.Method.IsStatic || eventHandler.Target == null)
            {
                throw new ArgumentException("A WeakEventSubscription can only be created on instance methods.", nameof(eventHandler));
            }

            IWeakEventSubscription<TEventArgs> weakEventSubscription = CreateWeakEventSubscription<TEventArgs>(eventHandler, unsubscribeFromEventDelegate);

            return weakEventSubscription;
        }

        #endregion

        #region Private Methods

        private static IWeakEventSubscription<TEventArgs> CreateWeakEventSubscription<TEventArgs>(EventHandler<TEventArgs> eventHandler, UnsubscribeFromEventAction<TEventArgs> unsubscribeFromEventDelegate) where TEventArgs : EventArgs
        {
            Type type = typeof(WeakEventSubscription<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(TEventArgs));
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(EventHandler<TEventArgs>), typeof(UnsubscribeFromEventAction<TEventArgs>) });
            return (IWeakEventSubscription<TEventArgs>)constructor.Invoke(new object[] { eventHandler, unsubscribeFromEventDelegate });
        }

        #endregion
    }
}
