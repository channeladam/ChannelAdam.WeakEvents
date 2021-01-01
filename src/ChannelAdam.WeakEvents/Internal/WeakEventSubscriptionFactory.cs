//-----------------------------------------------------------------------
// <copyright file="WeakEventSubscriptionFactory.cs">
//     Copyright (c) 2017-2021 Adam Craven. All rights reserved.
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
    /// Internal factory for creating weakly referenced event subscriptions/handlers.
    /// </summary>
    internal static class WeakEventSubscriptionFactory
    {
        #region Internal Methods

        /// <summary>
        /// Creates a weak event subscription/handler, specifying a delegate method that can unsubscribe the event handler from the event.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="unsubscribeFromEventDelegate">The unsubscribe from event delegate.</param>
        /// <returns>
        /// A weak event handler.
        /// </returns>
        /// <exception cref="ArgumentNullException">If eventHandler is null.</exception>
        /// <exception cref="ArgumentException">A WeakEventSubscription can only be created on instance methods.</exception>
        internal static IWeakEventSubscription<TEventArgs> Create<TEventArgs>(EventHandler<TEventArgs> eventHandler, UnsubscribeFromEventAction<TEventArgs>? unsubscribeFromEventDelegate) where TEventArgs : EventArgs
        {
            if (eventHandler is null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            if (eventHandler.Method?.DeclaringType is null || eventHandler.Method.IsStatic || eventHandler.Target is null)
            {
                throw new ArgumentException("A WeakEventSubscription can be created on instance methods only", nameof(eventHandler));
            }

            Type type = typeof(WeakEventSubscription<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(TEventArgs));
            ConstructorInfo? constructor = type.GetConstructor(new Type[] { typeof(EventHandler<TEventArgs>), typeof(UnsubscribeFromEventAction<TEventArgs>) });

            if (constructor is null)
            {
                throw new ArgumentException("Could not get constructor for the given types");
            }

            return (IWeakEventSubscription<TEventArgs>)constructor.Invoke(new object?[] { eventHandler, unsubscribeFromEventDelegate });
        }

        #endregion
    }
}
