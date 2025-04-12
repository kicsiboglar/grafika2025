using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3_1
{
    internal class CubeArrangementModel
    {
        
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabled { get; set; } = false;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the tub is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double TubScale { get; private set; } = 1;

        internal void AdvanceTime(double deltaTime)
        {
            // we do not advance the simulation when animation is stopped
            if (!AnimationEnabled)
                return;

            // set a simulation time
            Time += deltaTime;

            // lets produce an oscillating scale in time
            TubScale = 1 + 0.2 * Math.Sin(1.5 * Time);
        }
    }
}
