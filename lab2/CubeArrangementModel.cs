using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    internal class CubeArrangementModel
    {
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the solving animation should run or it should be frozen.
        /// </summary>
        public bool SolvingAnimationEnabled { get; set; } = false;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the center cube is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double CenterCubeScale { get; private set; } = 1;

        /// <summary>
        /// The speed of the rotation of the cube.
        /// </summary>
        public float RotationSpeed { get; set; } = 3f;

        /// <summary>
        /// The threshold value for the rotation. If the rotation is less than this value, the cube is considered to be rotated.
        /// </summary>
        public float Threshold => RotationSpeed / 100;

        /// <summary>
        /// The string representation of the axes.
        /// </summary>
        private string[] AxesStr = new string[] { "X", "Y", "Z" };

        internal void AdvanceTime(double deltaTime, List<MyCubeModel> cubes)
        {
            if (AnimationEnabled)
            {
                if (!RotateCubes(deltaTime, cubes))
                {
                    AnimationEnabled = false;
                }
            }

            if (SolvingAnimationEnabled)
            {
                Time += deltaTime;
                CenterCubeScale = 0.1 * Math.Sin(3 * Time) + 0.9;
            }
        }

        private void HandleCubeRotation(MyCubeModel cube, double deltaTime, string axes)
        {
            float diff = cube.goalRotation[axes] - cube.actualRotation[axes];
            cube.actualRotation[axes] += (float)deltaTime * RotationSpeed * Math.Sign(diff);

            if (Math.Abs(diff) < Threshold)
            {
                cube.goalRotation[axes] = 0;
                cube.actualRotation[axes] = 0;
                cube.needsRotation[axes] = false;
                cube.rotationHistory.Add(cube.IsPositiveRotation ? axes : "-" + axes);
            }
        }

        private bool RotateCubes(double deltaTime, List<MyCubeModel> cubes)
        {

            bool isRotating = false;
            foreach (var cube in cubes)
            {
                foreach (var axes in AxesStr)
                {
                    if (cube.needsRotation[axes])
                    {
                        isRotating = true;
                        HandleCubeRotation(cube, deltaTime, axes);
                        break;
                    }
                }
            }
            return isRotating;
        }
    }
}
