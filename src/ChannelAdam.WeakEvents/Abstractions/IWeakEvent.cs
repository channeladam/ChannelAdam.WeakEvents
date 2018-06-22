//-----------------------------------------------------------------------
// <copyright file="IWeakEvent.cs">
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

namespace ChannelAdam.WeakEvents.Abstractions
{
    using System;

    /// <summary>
    /// Interface for an event that enforces a weak reference to the event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public interface IWeakEvent<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Subscribes the specified event handler to the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void Subscribe(EventHandler<TEventArgs> eventHandler);

        /// <summary>
        /// Unsubscribes the specified event handler from the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void Unsubscribe(EventHandler<TEventArgs> eventHandler);
    }
}
