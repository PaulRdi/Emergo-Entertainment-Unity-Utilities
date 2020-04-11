using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    /// <summary>
    /// A priority for when the Method invoked by a message should be handled by the queue.
    /// The higher the priority is the earlier a message gets handled.
    /// </summary>
    public enum MethodPriority
    {
        /// <summary>
        /// The default priority of a message. Ordering will then be done by whichever message got added to the queue first.
        /// </summary>
        Default = 10,
        /// <summary>
        /// A priority for filtering messages before they get handled. 
        /// </summary>
        Filter = 1000,
        /// <summary>
        /// The earliest possible priority
        /// </summary>
        PreFilter = 1001,
        /// <summary>
        /// Late Priorty for updating the UI
        /// </summary>
        UpdateUI = 5,
        /// <summary>
        /// Priority which comes after default time and after UI priority
        /// </summary>
        AfterUI = 3
    }
}