namespace DynamicChecklist
{
    using System;
    using Microsoft.Xna.Framework;

    public static class MathX
    {
        public static int Clamp(int x, int min, int max)
        {
            return (x > max) ? max : ((x < min) ? min : x);
        }

        public static long Clamp(long x, long min, long max)
        {
            return (x > max) ? max : ((x < min) ? min : x);
        }

        public static float Clamp(float x, float min, float max)
        {
            return (x > max) ? max : ((x < min) ? min : x);
        }

        public static double Clamp(double x, double min, double max)
        {
            return (x > max) ? max : ((x < min) ? min : x);
        }

        public static Rectangle CenteredRectangle(int containerWidth, int containerHeight, int width, int height)
        {
            var x = containerWidth / 2 - width / 2;
            var y = containerHeight / 2 - height / 2;
            return new Rectangle(x, y, width, height);
        }

        public static Rectangle CenteredRectangle(Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width / 2;
            var y = v.Height / 2 - height / 2;
            return new Rectangle(v.X + x, v.Y + y, width, height);
        }

        /// <summary>
        /// Calculates a rectangle centered on the container, having the same width-to-height ratio as the reference and an integer scaling factor.
        /// </summary>
        /// <param name="container">A container rectangle, e.g., a window</param>
        /// <param name="contained">Some other rectangle, e.g., a sprite</param>
        /// <param name="scaleMax">Maximum scale-up factor, ideally <c>Game1.pixelZoom</c></param>
        /// <returns>Centered, scaled rectangle</returns>
        public static Rectangle CenteredScaledRectangle(Rectangle container, Rectangle contained, int scaleMax = 4)
        {
            return CenteredScaledRectangle(container, contained.Width, contained.Height, scaleMax);
        }

        /// <summary>
        /// Calculates a rectangle centered on the container, having the specified width-to-height ratio.
        /// Useful for centering a sprite within some background and having the sprite not look like utter trash.
        /// When scaling up, scales up by an integer factor so that pixel graphics remain crisp.
        /// When scaling down, only preserves aspect ratio, but powers-of-two will still be crisp (e.g., a 64x64 -> 16x16 transformation).
        /// </summary>
        /// <param name="container">The rectangle in which to center the new rectangle</param>
        /// <param name="width">Aspect width of the contained rectangle</param>
        /// <param name="height">Aspect height of the contained rectangle</param>
        /// <param name="scaleMax">Maximum scale-up factor, ideally <c>Game1.pixelZoom</c></param>
        /// <returns>Centered, scaled rectangle</returns>
        public static Rectangle CenteredScaledRectangle(Rectangle container, int width, int height, int scaleMax = 4)
        {
            if (container.Width == width && container.Height == height)
            {
                return container;
            }

            int a = height > width ? container.Height : container.Width;
            int b = height > width ? height : width;

            if (a > b)
            {
                // scale up by an integer factor to preserve crisp pixel look
                int scaleUp = a / b;
                scaleUp = Math.Max(scaleUp, scaleMax);
                width *= scaleUp;
                height *= scaleUp;
            }
            else
            {
                // This is b > a. Note that a == b is handled at the very top of the function.
                // Scaled down image is going to get interpolated anyway, so just preserve aspect ratio
                float scaleDown = a / (float)b;
                scaleDown = Math.Max(scaleDown, scaleMax);
                width = (int)(scaleDown * width); // implicit floor
                height = (int)(scaleDown * height);
            }

            return CenteredRectangle(container, width, height);
        }
    }
}