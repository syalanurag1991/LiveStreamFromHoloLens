using UnityEngine;

namespace CustomStreaming
{

    /// A wrapper struct for images and their associated timestamps.
    public struct Frame
    {
        /// Representation of RGBA colors in 32 bit format.
        /// Each color component is a byte value with a range from 0 to 255.
        /// Components(r, g, b) define a color in RGB color space. Alpha component(a) defines transparency - alpha of 255 is completely opaque, alpha of zero is completely transparent.
        public Color32[] rgba;

        /// The timestamp of the frame (in seconds). Can be used as an identifier of the frame.  If you use Time.timeScale to pause and use the same time units then you will not be able to process frames while paused.
        public float timestamp;

        /// Width of the frame. Value has to be greater than zero
        public int w;

        /// Height of the frame. Value has to be greater than zero
        public int h;

        /// Orientation of the frame
        /// Note : enum values match http://sylvana.net/jpegcrop/exif_orientation.html
        public enum Orientation
        {
            /// Image's 0th row is at the top and 0th column is on the left side.
            Upright = 1,
            
            /// Image's 0th row is on the left side and 0th column is at the bottom.
            CW_90 = 8,

            /// Image's 0th row is at the bottom and 0th column is on the right side.
            CW_180 = 3,

            /// Image's 0th row is on the right side and 0th column is at the top.
            CW_270 = 6
        }

        /// Orientation of the frame
        public Orientation orientation;

        /// Representation of RGBA colors in 32 bit format.
        /// Each color component is a byte value with a range from 0 to 255.
        /// Components(r, g, b) define a color in RGB color space. Alpha component(a) defines transparency - alpha of 255 is completely opaque, alpha of zero is completely transparent.
        /// <param name="rgba">Representation of RGBA colors in 32 bit format.</param>
        /// <param name="width">Width of the frame. Value has to be greater than zero</param>
		/// <param name="height">Height of the frame. Value has to be greater than zero</param>
        /// <param name="orientation">Orientation of the frame.</param>
        /// <param name="timestamp">The timestamp of the frame (in seconds). Can be used as an identifier of the frame.  If you use Time.timeScale to pause and use the same time units then you will not be able to process frames while paused.</param>
        public Frame(Color32[] rgba, int width, int height, Orientation orientation, float timestamp)
        {
            this.w = width;
            this.h = height;
            this.rgba = rgba;
            this.orientation = orientation;
            this.timestamp = timestamp;
        }

        /// Representation of RGBA colors in 32 bit format.  The orientation of the image must be upright.  For a rotated image, use the alternate constructor which allows specification of the frame orientation.
        /// Each color component is a byte value with a range from 0 to 255.
        /// Components(r, g, b) define a color in RGB color space. Alpha component(a) defines transparency - alpha of 255 is completely opaque, alpha of zero is completely transparent.
        /// <param name="rgba">Representation of RGBA colors in 32 bit format.</param>
        /// <param name="width">Width of the frame. Value has to be greater than zero</param>
        /// <param name="height">Height of the frame. Value has to be greater than zero</param>
        /// <param name="timestamp">The timestamp of the frame (in seconds). Can be used as an identifier of the frame.  If you use Time.timeScale to pause and use the same time units then you will not be able to process frames while paused.</param>
        public Frame(Color32[] rgba, int width, int height, float timestamp) : this(rgba, width, height, Frame.Orientation.Upright, timestamp)
        {
        }
    }
}
