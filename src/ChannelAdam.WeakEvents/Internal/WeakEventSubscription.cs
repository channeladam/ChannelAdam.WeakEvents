//-----------------------------------------------------------------------
// <copyright file="WeakEventSubscription.cs">
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

    /// <summary>
    /// A delegate method to unsubscribe from an event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="eventHandler">The event handler.</param>
    internal delegate void UnsubscribeFromEventAction<TEventArgs>(EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs;

    /// <summary>
    /// Similar concept to a <see cref="WeakReference"/> but for an event subscription/handler,
    /// so that having an event subscription/handler does not prevent the subscriber from being garbage collected.
    /// </summary>
    /// <typeparam name="TDelegateInstanceObject">The type of the delegate instance object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    internal class WeakEventSubscription<TDelegateInstanceObject, TEventArgs> : IWeakEventSubscription<TEventArgs>
        where TDelegateInstanceObject : class
        where TEventArgs : EventArgs
    {
        #region Fields

        private readonly WeakReference weakReferenceToEventHandlerTarget;
        private readonly UnboundDelegateEventHandler unboundDelegateEventHandler;
        private UnsubscribeFromEventAction<TEventArgs>? unsubscribeFromEventAction;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventSubscription{TDelegateInstanceObject, TEventArgs}"/> class.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public WeakEventSubscription(EventHandler<TEventArgs> eventHandler) : this(eventHandler, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventSubscription{TDelegateInstanceObject, TEventArgs}"/> class.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="unsubscribeFromEventAction">The delegate method that unsubscribes from the event.</param>
        public WeakEventSubscription(EventHandler<TEventArgs> eventHandler, UnsubscribeFromEventAction<TEventArgs>? unsubscribeFromEventAction)
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            // Hold a weak reference to the target of the event handler - not the event handler itself, which would cause a strong reference.
            this.weakReferenceToEventHandlerTarget = new WeakReference(eventHandler.Target);

            // Create an unbound delegate to the MethodInfo for the event handler - not the event handler itself, which would cause a strong reference.
            this.unboundDelegateEventHandler = (UnboundDelegateEventHandler)Delegate.CreateDelegate(typeof(UnboundDelegateEventHandler), null, eventHandler.Method);

            // The actual event handler is a method in this class - not the event handler from the other object.
            this.EventHandler = this.InvokeEventHandler;

            this.unsubscribeFromEventAction = unsubscribeFromEventAction;
        }

        #endregion Constructors

        #region Delegates

        /// <summary>
        /// An unbound delegate for an event handler.
        /// </summary>
        /// <param name="instanceObject">The instance object.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="TEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// An unbound delegate allows you to pass an instance of the type whose function you want to call when the delegate is called.
        /// See also http://msdn.microsoft.com/en-us/library/ms177195(v=vs.100).aspx.
        /// </remarks>
        private delegate void UnboundDelegateEventHandler(TDelegateInstanceObject instanceObject, object? sender, TEventArgs eventArgs);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the event handler.
        /// </summary>
        /// <value>
        /// The event handler.
        /// </value>
        public EventHandler<TEventArgs> EventHandler { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="WeakEventSubscription{TDelegateInstanceObject, TEventArgs}"/> to <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <param name="weakEventHandler">The weak event handler.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator EventHandler<TEventArgs>?(WeakEventSubscription<TDelegateInstanceObject, TEventArgs> weakEventHandler)
        {
            if (weakEventHandler == null)
            {
                return null;
            }

            return weakEventHandler.EventHandler;
        }

        /// <summary>
        /// Provides an alternative to the implicit operator for converting from <see cref="WeakEventSubscription{TDelegateInstanceObject, TEventArgs}"/> to <see cref="EventHandler{TEventArgs}"/>.
        /// </summary>
        /// <param name="weakEventHandler">The weak event handler.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static EventHandler<TEventArgs>? ToEventHandler(WeakEventSubscription<TDelegateInstanceObject, TEventArgs> weakEventHandler)
        {
            if (weakEventHandler == null)
            {
                return null;
            }

            return weakEventHandler.EventHandler;
        }

        #endregion

        #region Private Methods

        private void InvokeEventHandler(object? sender, TEventArgs e)
        {
            if (this.weakReferenceToEventHandlerTarget.Target is TDelegateInstanceObject instanceObject)
            {
                this.unboundDelegateEventHandler.Invoke(instanceObject, sender, e);
            }
            else if (this.unsubscribeFromEventAction != null)
            {
                // If instance object from the weak reference has been garbage collected, then unsubscribe from the event.
                try
                {
                    this.unsubscribeFromEventAction.Invoke(this.EventHandler);
                }
                finally
                {
                    this.unsubscribeFromEventAction = null;
                }
            }
        }

        #endregion
    }
}
