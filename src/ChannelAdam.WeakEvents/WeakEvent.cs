//-----------------------------------------------------------------------
// <copyright file="WeakEvent.cs">
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

namespace ChannelAdam.WeakEvents
{
    using ChannelAdam.WeakEvents.Abstractions;
    using ChannelAdam.WeakEvents.Internal;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For use by an object that publishes events (in a way that is a little less conventional than using the 'event' keyword).
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <remarks>
    /// This approach:
    /// - forces all subscribers of the event to be WeakEventHandlers
    /// - the subscriber does not need to be concerned with creating WeakEventHandlers and passing in an unsubscribe action
    /// - enables automatic unsubscription of the event handler, when the weak reference subscriber is garbage collected
    /// - enables subscribers to unsubscribe from the event
    /// - provides a thread-safe mechanism for subscribing, unsubscribing and invoking the event.
    /// </remarks>
    public class WeakEvent<TEventArgs> : IWeakEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly object syncRoot = new();
        private readonly Dictionary<string, IWeakEventSubscription<TEventArgs>> weakEventSubscriptions = new();

        private EventHandler<TEventArgs>? myEventHandler;

        #region Private Events

        private event EventHandler<TEventArgs> MyEvent
        {
            add
            {
                string key = MakeDictionaryKeyFromDelegate(value);

                lock (this.syncRoot)
                {
                    if (this.weakEventSubscriptions.ContainsKey(key))
                    {
                        return;
                    }

                    IWeakEventSubscription<TEventArgs> weakEventHandler = WeakEventSubscriptionFactory.Create<TEventArgs>(value, (args) => this.Unsubscribe(args));

                    this.weakEventSubscriptions.Add(key, weakEventHandler);
                    this.myEventHandler += weakEventHandler.EventHandler;
                }
            }

            remove
            {
                string key = MakeDictionaryKeyFromDelegate(value);

                lock (this.syncRoot)
                {
                    if (!this.weakEventSubscriptions.ContainsKey(key))
                    {
                        return;
                    }

                    var weakEventHandler = this.weakEventSubscriptions[key];

                    this.myEventHandler -= weakEventHandler.EventHandler;
                    this.weakEventSubscriptions.Remove(key);
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Performs an explicit conversion from <see cref="WeakEvent{TEventArgs}"/> to <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <param name="weakEvent">The weak event.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator EventHandler<TEventArgs>?(WeakEvent<TEventArgs> weakEvent)
        {
            if (weakEvent == null)
            {
                return null;
            }

            return weakEvent.myEventHandler;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// An alternative to the explicit operator for converting from <see cref="WeakEvent{TEventArgs}"/> to <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <param name="weakEvent">The weak event.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public EventHandler<TEventArgs>? ToEventHandler(WeakEvent<TEventArgs> weakEvent)
        {
            if (weakEvent == null)
            {
                return null;
            }

            return weakEvent.myEventHandler;
        }

        /// <summary>
        /// Subscribes the specified event handler to the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void Subscribe(EventHandler<TEventArgs> eventHandler)
        {
            if (eventHandler is null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            this.MyEvent += eventHandler;
        }

        /// <summary>
        /// Unsubscribes the specified event handler from the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void Unsubscribe(EventHandler<TEventArgs> eventHandler)
        {
            if (eventHandler is null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            this.MyEvent -= eventHandler;
        }

        /// <summary>
        /// Invokes the specified event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="TEventArgs"/> instance containing the event data.</param>
        public void Invoke(object? sender, TEventArgs eventArgs)
        {
            EventHandler<TEventArgs>? handler;

            lock (this.syncRoot)
            {
                handler = this.myEventHandler;
            }

            handler?.Invoke(sender, eventArgs);
        }

        #endregion

        #region Private Methods

        private static string MakeDictionaryKeyFromDelegate(EventHandler<TEventArgs> eventHandler)
        {
            return string.Format("{0}-{1}", eventHandler.Target?.GetHashCode(), eventHandler.GetHashCode());
        }

        #endregion
    }
}
